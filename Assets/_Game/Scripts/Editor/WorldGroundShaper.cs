using UnityEditor;
using UnityEngine;
using WPG.World;

namespace WPG.WorldEditor
{
    /// <summary>
    /// Nadaje naturalny, pofalowany kształt zapieczonemu (baked) WorldRoot.prefab — w miejscu,
    /// BEZ regenerowania świata. Zachowuje całą strukturę (obozy, gobliny, propsy, ścieżki):
    ///
    ///  1) zamienia płaski Plane GroundPlane na pofalowaną siatkę + MeshCollider,
    ///  2) OSADZA każdy obiekt na nowej powierzchni (Y = wysokość terenu w jego punkcie XZ),
    ///  3) tworzy łagodne plateau pod bazą, obozami i miejscami mocy (struktury siedzą równo),
    ///  4) zapisuje WorldGroundProfile, by runtime (gobliny itd.) użył tej samej powierzchni.
    ///
    /// Osadzanie jest ABSOLUTNE (Y = teren, a nie Y += teren), więc tool można uruchamiać
    /// wielokrotnie i ze zmienionymi parametrami — obiekty NIE podnoszą się drugi raz.
    /// Dzięki temu da się swobodnie eksperymentować z amplitudą/skalą i przebudowywać teren.
    ///
    /// Parametry zmienisz w oknie: WPG/World/Ukształtuj teren (ustawienia)…
    /// Unoszące się obiekty naprawisz: WPG/World/Napraw unoszące się obiekty.
    /// </summary>
    public static class WorldGroundShaper
    {
        const string PrefabPath = "Assets/_Game/Prefabs/World/WorldRoot.prefab";

        // Siatka terenu MUSI być assetem na dysku — runtime'owy new Mesh() nie zapisze się
        // w prefabie (SaveAsPrefabAsset wyzerowałoby referencję do fileID 0 = pusty/płaski grunt).
        const string GroundMeshPath = "Assets/_Game/Prefabs/World/WorldRoot_GroundMesh.asset";

        // Niewielkie offsety Y zachowane przy osadzaniu (jak w WorldGenerator) — przeciw z-fightingowi.
        const float PathYOffset = 0.06f;
        const float ClearingYOffset = 0.02f;

        /// <summary>Strojalne parametry terenu (te same, których używa WorldGenerator).</summary>
        public struct Settings
        {
            public int seed;
            public float amplitude;
            public float noiseScale;
            public float baseFlatRadius;
            public int resolution;

            public static Settings Default => new Settings
            {
                seed = 13579,          // ten sam co WorldBootstrap
                amplitude = 3f,        // ±3 m — łagodne, ale wyraźnie widoczne wzgórza
                noiseScale = 55f,
                baseFlatRadius = 18f,
                resolution = 160,
            };
        }

        // ------------------------------------------------------------------
        // Menu
        // ------------------------------------------------------------------

        [MenuItem("WPG/World/Ukształtuj teren (ustawienia)…")]
        public static void OpenWindow() => WorldGroundShaperWindow.Open();

        [MenuItem("WPG/World/Napraw unoszące się obiekty")]
        public static void FixFloatingMenu()
        {
            if (!PrefabExists()) return;

            if (!EditorUtility.DisplayDialog("Napraw unoszące się obiekty",
                    "Osadzić wszystkie obiekty na OBECNYM terenie WorldRoot.prefab?\n\n" +
                    "Użyje zapisanego profilu (WorldGroundProfile) — nie zmienia kształtu mapy,\n" +
                    "tylko dociska propsy/struktury do powierzchni. Bezpieczne do wielokrotnego użycia.",
                    "Napraw", "Anuluj"))
                return;

            RunOnPrefab(snapOnly: true, settings: default, label: "Napraw unoszące się obiekty");
        }

        // ------------------------------------------------------------------
        // Główne wejścia
        // ------------------------------------------------------------------

        /// <summary>Pełne ukształtowanie/przebudowa terenu z podanymi parametrami + osadzenie obiektów.</summary>
        public static void Apply(Settings settings)
        {
            if (!PrefabExists()) return;
            RunOnPrefab(snapOnly: false, settings: settings, label: "Naturalny teren");
        }

        static bool PrefabExists()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return true;
            EditorUtility.DisplayDialog("WorldRoot", $"Nie znaleziono prefabu:\n{PrefabPath}", "OK");
            return false;
        }

        static void RunOnPrefab(bool snapOnly, Settings settings, string label)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            int snapped = 0;
            bool saved = false;
            try
            {
                if (snapOnly)
                {
                    // Tryb "napraw": odtwórz teren z zapisanego profilu i tylko osadź obiekty.
                    var profile = root.GetComponentInChildren<WorldGroundProfile>(true);
                    if (profile == null)
                    {
                        EditorUtility.DisplayDialog(label,
                            "Prefab nie ma jeszcze profilu terenu (WorldGroundProfile).\n\n" +
                            "Najpierw ukształtuj teren: WPG/World/Ukształtuj teren (ustawienia)…", "OK");
                        return;
                    }
                    profile.Apply(); // konfiguruje WorldGround dokładnie tak, jak wypalona siatka
                }
                else
                {
                    ConfigureSampler(root.transform, settings);
                    ReshapeGroundPlane(root.transform, settings.resolution);
                    WriteProfile(root.transform);
                }

                snapped = SnapStructure(root.transform);

                // Brakujące/zepsute skrypty (m_Script: {fileID: 0}) blokują SaveAsPrefabAsset.
                int removed = StripMissingScripts(root);
                if (removed > 0)
                    Debug.LogWarning($"[WorldGroundShaper] Usunięto {removed} brakujących skryptów przed zapisem prefabu.");

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out saved);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            if (!saved)
            {
                Debug.LogError("[WorldGroundShaper] Zapis prefabu NIE powiódł się. Sprawdź konsolę pod kątem brakujących skryptów.");
                EditorUtility.DisplayDialog(label,
                    "Zapis prefabu NIE powiódł się.\n\n" +
                    "Najczęstsza przyczyna: brakujący/zepsuty skrypt na jakimś obiekcie w prefabie.\n" +
                    "Zajrzyj do konsoli Unity po szczegóły.", "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[WorldGroundShaper] {label}: osadzono {snapped} obiektów na powierzchni terenu.");
            EditorUtility.DisplayDialog(label,
                $"Gotowe. Osadzono {snapped} obiektów na powierzchni.\nWejdź w Play, żeby zobaczyć efekt.", "OK");
        }

        // 1) Konfiguracja samplera + plateau wykryte z FAKTYCZNYCH pozycji struktur w prefabie.
        static void ConfigureSampler(Transform root, Settings s)
        {
            WorldGround.Configure(s.seed, s.amplitude, s.noiseScale);
            WorldGround.AddFlatZone(0f, 0f, s.baseFlatRadius, s.baseFlatRadius * 0.7f, 0f);

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("Camp_"))
                    WorldGround.AddFlatZone(t.position.x, t.position.z, GuessCampRadius(t) + 3f, 8f);
                else if (t.name.StartsWith("PowerSite_"))
                    WorldGround.AddFlatZone(t.position.x, t.position.z, 6f, 6f);
            }
        }

        static float GuessCampRadius(Transform camp)
        {
            Transform zone = FindByName(camp, "CampZone");
            if (zone != null)
            {
                var sc = zone.GetComponent<SphereCollider>();
                if (sc != null) return sc.radius;
            }
            return 10f;
        }

        // 2) Zamiana płaskiego Plane na pofalowaną siatkę (świeżo budowana — nigdy się nie kumuluje).
        static void ReshapeGroundPlane(Transform root, int resolution)
        {
            Transform gp = FindByName(root, "GroundPlane");
            if (gp == null)
            {
                Debug.LogWarning("[WorldGroundShaper] Nie znaleziono GroundPlane — pomijam wymianę siatki.");
                return;
            }

            // Rozmiar w metrach = bok siatki × skala. Liczymy z FAKTYCZNEJ siatki, żeby ponowne
            // uruchomienie (gdy localScale jest już 1 po wcześniejszym kształtowaniu) nie skurczyło terenu.
            float size;
            var existingMf = gp.GetComponent<MeshFilter>();
            if (existingMf != null && existingMf.sharedMesh != null)
            {
                Vector3 b = existingMf.sharedMesh.bounds.size; // Plane=10 m, nasza siatka=metry świata
                size = Mathf.Max(b.x, b.z) * Mathf.Abs(gp.localScale.x);
            }
            else
            {
                size = Mathf.Abs(gp.localScale.x) * 10f; // wbudowany Plane ma bok 10 m
            }
            if (size < 1f) size = 300f;

            Mesh mesh = WorldGround.BuildMesh(size, resolution);

            // Zapisz mesh jako asset, inaczej zapis prefabu zgubi referencję (grunt zostałby płaski/niewidoczny).
            AssetDatabase.DeleteAsset(GroundMeshPath);
            AssetDatabase.CreateAsset(mesh, GroundMeshPath);
            Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GroundMeshPath);

            var mf = gp.GetComponent<MeshFilter>() ?? gp.gameObject.AddComponent<MeshFilter>();
            var mc = gp.GetComponent<MeshCollider>() ?? gp.gameObject.AddComponent<MeshCollider>();
            mf.sharedMesh = savedMesh;
            mc.sharedMesh = savedMesh;
            gp.localScale = Vector3.one; // siatka jest już w metrach świata
        }

        // 4) Zapis profilu na obiekcie "Ground" (lub root) — runtime go odtworzy w Awake.
        static void WriteProfile(Transform root)
        {
            Transform groundRoot = FindByName(root, "Ground") ?? root;
            var profile = groundRoot.GetComponent<WorldGroundProfile>() ?? groundRoot.gameObject.AddComponent<WorldGroundProfile>();
            WorldGround.PopulateProfile(profile);
        }

        // 3) Osadzenie obiektów na powierzchni. ABSOLUTNE (Y = teren), więc bezpieczne do ponownego uruchomienia.
        static int SnapStructure(Transform root)
        {
            int count = 0;
            foreach (Transform top in root)
            {
                string n = top.name;
                if (n == "GroundPlane" || n == "WorldBounds") continue;

                if (n == "Ground")
                {
                    foreach (Transform child in top)
                    {
                        if (child.name == "GroundPlane") continue;
                        if (child.name == "Paths")
                        {
                            foreach (Transform seg in child) count += Snap(seg, PathYOffset);
                        }
                        else if (child.name.StartsWith("Clearing_"))
                        {
                            count += Snap(child, ClearingYOffset);
                        }
                    }
                }
                else if (n == "Vegetation" || n == "Grass" || n == "FutureContent")
                {
                    foreach (Transform prop in top) count += Snap(prop);
                }
                else if (n == "DruidBase" || n.StartsWith("Camp_") || n.StartsWith("PowerSite_"))
                {
                    // Struktury siedzą na plateau — osadzamy ich root, dzieci jadą razem.
                    count += Snap(top);
                }
            }
            return count;
        }

        // Osadzenie ABSOLUTNE: ustawia Y na wysokość terenu (nie dodaje), więc kolejne uruchomienia
        // niczego nie kumulują — to lek na "podwójne podniesienie" i unoszące się obiekty.
        static int Snap(Transform t, float yOffset = 0f)
        {
            Vector3 p = t.position;
            p.y = WorldGround.GetGroundHeight(p.x, p.z) + yOffset;
            t.position = p;
            return 1;
        }

        // Usuwa komponenty z brakującym skryptem z całej hierarchii prefabu.
        static int StripMissingScripts(GameObject root)
        {
            int total = 0;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                total += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            return total;
        }

        static Transform FindByName(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindByName(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }

    /// <summary>
    /// Okno z strojalnymi parametrami terenu. Pozwala zmienić ustawienia i przebudować WorldRoot.prefab
    /// dowolną liczbę razy (osadzanie jest absolutne, więc obiekty się nie kumulują).
    /// </summary>
    public class WorldGroundShaperWindow : EditorWindow
    {
        const string Pref = "WPG.GroundShaper.";

        WorldGroundShaper.Settings _s;

        public static void Open()
        {
            var w = GetWindow<WorldGroundShaperWindow>(true, "Ukształtuj teren — WorldRoot");
            w.minSize = new Vector2(380f, 280f);
            w.Load();
            w.Show();
        }

        void Load()
        {
            var d = WorldGroundShaper.Settings.Default;
            _s.seed = EditorPrefs.GetInt(Pref + "seed", d.seed);
            _s.amplitude = EditorPrefs.GetFloat(Pref + "amplitude", d.amplitude);
            _s.noiseScale = EditorPrefs.GetFloat(Pref + "noiseScale", d.noiseScale);
            _s.baseFlatRadius = EditorPrefs.GetFloat(Pref + "baseFlatRadius", d.baseFlatRadius);
            _s.resolution = EditorPrefs.GetInt(Pref + "resolution", d.resolution);
        }

        void Save()
        {
            EditorPrefs.SetInt(Pref + "seed", _s.seed);
            EditorPrefs.SetFloat(Pref + "amplitude", _s.amplitude);
            EditorPrefs.SetFloat(Pref + "noiseScale", _s.noiseScale);
            EditorPrefs.SetFloat(Pref + "baseFlatRadius", _s.baseFlatRadius);
            EditorPrefs.SetInt(Pref + "resolution", _s.resolution);
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Zmień parametry i przebuduj teren w WorldRoot.prefab. Można uruchamiać wielokrotnie — " +
                "obiekty są osadzane absolutnie (Y = teren), więc się NIE kumulują.",
                MessageType.Info);

            EditorGUILayout.Space();

            _s.seed = EditorGUILayout.IntField(new GUIContent("Seed", "Ziarno szumu (13579 = jak WorldBootstrap)."), _s.seed);
            _s.amplitude = EditorGUILayout.Slider(new GUIContent("Amplituda (±m)", "Maks. wychylenie terenu. Łagodne wzgórza: 1–4."), _s.amplitude, 0f, 12f);
            _s.noiseScale = EditorGUILayout.Slider(new GUIContent("Skala szumu", "Większa wartość = szersze, łagodniejsze wzgórza."), _s.noiseScale, 5f, 200f);
            _s.baseFlatRadius = EditorGUILayout.Slider(new GUIContent("Płaski plac bazy (m)", "Promień równego placu wokół bazy/spawnu."), _s.baseFlatRadius, 0f, 60f);
            _s.resolution = EditorGUILayout.IntSlider(new GUIContent("Gęstość siatki", "Segmenty siatki gruntu (gęstość)."), _s.resolution, 16, 250);

            EditorGUILayout.Space();

            if (GUILayout.Button("Domyślne ustawienia"))
                _s = WorldGroundShaper.Settings.Default;

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Zastosuj / przebuduj teren", GUILayout.Height(34f)))
                {
                    Save();
                    WorldGroundShaper.Apply(_s);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Po zmianie parametrów możesz też osobno osadzić same obiekty na obecnej mapie:\n" +
                "WPG/World/Napraw unoszące się obiekty.",
                MessageType.None);

            if (GUILayout.Button("Napraw unoszące się obiekty (bez zmiany kształtu)"))
                WorldGroundShaper.FixFloatingMenu();
        }
    }
}

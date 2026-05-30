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
    ///  2) podnosi każdy obiekt o wysokość terenu w jego punkcie XZ (stary grunt = Y 0),
    ///  3) tworzy łagodne plateau pod bazą, obozami i miejscami mocy (struktury siedzą równo),
    ///  4) zapisuje WorldGroundProfile, by runtime (gobliny itd.) użył tej samej powierzchni.
    ///
    /// Po uruchomieniu zrób w Unity Play — świat jest już z prefabu, więc zmiana jest natychmiast widoczna.
    /// </summary>
    public static class WorldGroundShaper
    {
        const string PrefabPath = "Assets/_Game/Prefabs/World/WorldRoot.prefab";

        // Domyślne parametry — dopasowane do WorldGenerator (seed 13579 = ten sam co WorldBootstrap).
        const int Seed = 13579;
        const float Amplitude = 2f;
        const float NoiseScale = 55f;
        const float BaseFlatRadius = 18f;
        const int Resolution = 160;

        [MenuItem("WPG/World/Apply Natural Ground To WorldRoot Prefab")]
        public static void Apply()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                EditorUtility.DisplayDialog("WorldRoot", $"Nie znaleziono prefabu:\n{PrefabPath}", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Naturalny teren",
                    "Nadać pofalowany kształt gruntowi w WorldRoot.prefab?\n\n" +
                    $"Amplituda ±{Amplitude} m, skala {NoiseScale}.\n" +
                    "Struktura (obozy/propsy/ścieżki) zostaje zachowana — obiekty zostaną osadzone na nowej powierzchni.",
                    "Zastosuj", "Anuluj"))
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            int snapped = 0;
            try
            {
                // Zabezpieczenie przed dwukrotnym uruchomieniem (podniosłoby obiekty 2x).
                if (root.GetComponentInChildren<WorldGroundProfile>(true) != null)
                {
                    EditorUtility.DisplayDialog("Naturalny teren",
                        "Prefab ma już profil terenu (WorldGroundProfile) — wygląda na już ukształtowany.\n\n" +
                        "Aby ukształtować ponownie, przywróć płaski prefab z gita i uruchom tool jeszcze raz.", "OK");
                    return;
                }

                ConfigureSampler(root.transform);
                ReshapeGroundPlane(root.transform);
                WriteProfile(root.transform);
                snapped = SnapStructure(root.transform);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[WorldGroundShaper] Zastosowano naturalny teren do WorldRoot.prefab (osadzono {snapped} obiektów).");
            EditorUtility.DisplayDialog("Naturalny teren",
                $"Gotowe. Osadzono {snapped} obiektów na pofalowanej powierzchni.\nWejdź w Play, żeby zobaczyć efekt.", "OK");
        }

        // 1) Konfiguracja samplera + plateau wykryte z FAKTYCZNYCH pozycji struktur w prefabie.
        static void ConfigureSampler(Transform root)
        {
            WorldGround.Configure(Seed, Amplitude, NoiseScale);
            WorldGround.AddFlatZone(0f, 0f, BaseFlatRadius, BaseFlatRadius * 0.7f, 0f);

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

        // 2) Zamiana płaskiego Plane na pofalowaną siatkę.
        static void ReshapeGroundPlane(Transform root)
        {
            Transform gp = FindByName(root, "GroundPlane");
            if (gp == null)
            {
                Debug.LogWarning("[WorldGroundShaper] Nie znaleziono GroundPlane — pomijam wymianę siatki.");
                return;
            }

            float size = Mathf.Abs(gp.localScale.x) * 10f; // wbudowany Plane ma bok 10 m
            if (size < 1f) size = 300f;

            Mesh mesh = WorldGround.BuildMesh(size, Resolution);

            var mf = gp.GetComponent<MeshFilter>() ?? gp.gameObject.AddComponent<MeshFilter>();
            var mc = gp.GetComponent<MeshCollider>() ?? gp.gameObject.AddComponent<MeshCollider>();
            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            gp.localScale = Vector3.one; // siatka jest już w metrach świata
        }

        // 4) Zapis profilu na obiekcie "Ground" (lub root) — runtime go odtworzy w Awake.
        static void WriteProfile(Transform root)
        {
            Transform groundRoot = FindByName(root, "Ground") ?? root;
            var profile = groundRoot.GetComponent<WorldGroundProfile>() ?? groundRoot.gameObject.AddComponent<WorldGroundProfile>();
            WorldGround.PopulateProfile(profile);
        }

        // 3) Osadzenie obiektów na nowej powierzchni. Stary grunt = Y 0, więc dodajemy wysokość terenu.
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
                            foreach (Transform seg in child) count += Raise(seg);
                        }
                        else if (child.name.StartsWith("Clearing_"))
                        {
                            count += Raise(child);
                        }
                    }
                }
                else if (n == "Vegetation" || n == "Grass" || n == "FutureContent")
                {
                    foreach (Transform prop in top) count += Raise(prop);
                }
                else if (n == "DruidBase" || n.StartsWith("Camp_") || n.StartsWith("PowerSite_"))
                {
                    // Struktury siedzą na plateau — podnosimy ich root, dzieci jadą razem.
                    count += Raise(top);
                }
            }
            return count;
        }

        static int Raise(Transform t)
        {
            Vector3 p = t.position;
            p.y += WorldGround.GetGroundHeight(p.x, p.z);
            t.position = p;
            return 1;
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
}

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WPG.Editor
{
    /// <summary>
    /// Generator prefabów graczy dla wszystkich klas postaci.
    ///
    /// Problem: w grze widać tylko Krasnoluda (Player_Dwarf) i Wojownika (Player_Warrior),
    /// bo dla pozostałych klas nie ma prefabów, a spawner mapuje na prefab tylko te dwie klasy.
    ///
    /// To narzędzie (menu "WPG/Players/...") działa W EDYTORZE i robi to, czego nie da się
    /// zrobić bezpiecznie z poziomu plików tekstowych:
    /// 1) Bierze SPRAWDZONY prefab <c>Player_Dwarf</c> jako szablon (root + 15 komponentów
    ///    Invector/WPG + Animator + kontroler), instancjonuje go i rozpakowuje.
    /// 2) Usuwa wyłącznie wizualne dziecko (stary szkielet/mesh), wstawia model docelowego FBX
    ///    (Humanoid) jako nowe dziecko i ustawia <c>Animator.avatar</c> na avatar tego FBX.
    ///    Kontroler animacji (Humanoid retarget) zostaje ten sam co u Krasnoluda — działa dla
    ///    dowolnego rigu Humanoid, więc nie ruszamy ręcznie kości.
    /// 3) Dopasowuje skalę roota i CapsuleCollider do gabarytów nowego modelu.
    /// 4) Zapisuje <c>Player_&lt;Klasa&gt;.prefab</c> w <c>Assets/_Game/Prefabs/Players/</c>.
    /// 5) Wpina wszystkie prefaby w listę <c>classPrefabs</c> spawnera w scenie <c>World.unity</c>
    ///    (mapowanie 1:1 wszystkich wartości enuma — także Mag/Łucznik/Łotrzyk/Wędrowiec, które
    ///    współdzielą model pokrewnej klasy).
    ///
    /// Krasnolud i Wojownik NIE są modyfikowane — tylko podpinane do mapy.
    /// </summary>
    public static class PlayerPrefabBuilder
    {
        private const string TemplatePrefabPath = "Assets/_Game/Prefabs/Players/Player_Dwarf.prefab";
        private const string WarriorPrefabPath = "Assets/_Game/Prefabs/Players/Player_Warrior.prefab";
        private const string OutputDir = "Assets/_Game/Prefabs/Players";
        private const string WorldScenePath = "Assets/_Game/Scenes/World.unity";

        /// <summary>Opis jednego prefabu do zbudowania z szablonu Krasnoluda.</summary>
        private struct BuildSpec
        {
            public string prefabName;      // np. "Player_Ranger"
            public string fbxPath;         // model FBX (Humanoid)
            public float rootScale;        // skala roota (Dwarf=1.3 dla małego mesha V-Bot/Barbarian)

            public BuildSpec(string prefabName, string fbxPath, float rootScale)
            {
                this.prefabName = prefabName;
                this.fbxPath = fbxPath;
                this.rootScale = rootScale;
            }
        }

        // Modele potwierdzone w projekcie jako Humanoid (animationType: 3).
        // Skala: ludzkie modele ~1.8 m budujemy w 1.0; Barbarzyńca dzieli rodzinę mesha z Krasnoludem (1.3).
        private static readonly BuildSpec[] Specs =
        {
            new BuildSpec("Player_Ranger",
                "Assets/JC_StylizedModularCharacters/Models/SM_Ranger_Male.fbx", 1.0f),
            new BuildSpec("Player_Knight",
                "Assets/JC_LP_MedievalCharacters_LITE/Models/SM_MedievalMaleLite_01.fbx", 1.0f),
            new BuildSpec("Player_Druid",
                "Assets/URP GanzSe Free Modular Character Pack/Models/Models Update 1.1/GanzSe Free Modular Character 1_1.fbx", 1.0f),
            new BuildSpec("Player_Barbarian",
                "Assets/Meshtint Free Barbarian/FBX/Barbarian Wyder.FBX", 1.3f),
        };

        // Mapowanie KAŻDEJ klasy z enuma na nazwę prefabu (klasy bez własnego modelu
        // współdzielą model klasy pokrewnej — gameplay i tak różnicują statystyki z kreatora).
        private static readonly Dictionary<CharacterClassType, string> ClassToPrefabName =
            new Dictionary<CharacterClassType, string>
            {
                { CharacterClassType.Warrior, "Player_Warrior" },
                { CharacterClassType.Dwarf, "Player_Dwarf" },
                { CharacterClassType.Barbarian, "Player_Barbarian" },
                { CharacterClassType.Ranger, "Player_Ranger" },
                { CharacterClassType.Archer, "Player_Ranger" },     // łucznik → łowca
                { CharacterClassType.Wanderer, "Player_Ranger" },   // wędrowiec → łowca
                { CharacterClassType.Knight, "Player_Knight" },
                { CharacterClassType.Rogue, "Player_Knight" },      // łotrzyk → rycerz (alternatywnie Ranger)
                { CharacterClassType.Druid, "Player_Druid" },
                { CharacterClassType.Mage, "Player_Druid" },        // mag → GanzSe (jak druid)
            };

        [MenuItem("WPG/Players/Build Class Prefabs + Wire Spawner")]
        public static void BuildAllAndWire()
        {
            Dictionary<string, GameObject> built = BuildAllPrefabs();
            string wireReport = WireSpawner(built);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "WPG — Prefaby graczy",
                "Gotowe.\n\n" + wireReport +
                "\n\nSzczegóły w konsoli. Przetestuj: MainMenu → wybierz klasę → World.",
                "OK");
        }

        [MenuItem("WPG/Players/Build Class Prefabs (only)")]
        public static void BuildOnly()
        {
            BuildAllPrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, GameObject> BuildAllPrefabs()
        {
            var report = new StringBuilder();
            report.AppendLine("[PlayerPrefabBuilder] === Budowa prefabów klas ===");

            var result = new Dictionary<string, GameObject>();

            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePrefabPath);
            if (template == null)
            {
                Debug.LogError("[PlayerPrefabBuilder] Brak szablonu: " + TemplatePrefabPath);
                return result;
            }

            EnsureFolder(OutputDir);

            foreach (BuildSpec spec in Specs)
            {
                GameObject prefab = BuildOnePrefab(template, spec, report);
                if (prefab != null)
                    result[spec.prefabName] = prefab;
            }

            // Krasnolud i Wojownik istnieją już — dorzuć je do słownika do mapowania.
            AddExisting(result, "Player_Dwarf", TemplatePrefabPath);
            AddExisting(result, "Player_Warrior", WarriorPrefabPath);

            Debug.Log(report.ToString());
            return result;
        }

        private static GameObject BuildOnePrefab(GameObject template, BuildSpec spec, StringBuilder report)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(spec.fbxPath);
            if (model == null)
            {
                report.AppendLine($"  POMINIĘTO {spec.prefabName}: brak FBX {spec.fbxPath}");
                return null;
            }

            Avatar avatar = LoadAvatar(spec.fbxPath);
            if (avatar == null)
                report.AppendLine($"  UWAGA {spec.prefabName}: brak avatara Humanoid w {spec.fbxPath} — animacje mogą nie działać.");

            // 1) Sklonuj szablon (rozpakowany → samodzielny prefab, bez zagnieżdżenia w Dwarfie).
            GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(template);
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            root.name = spec.prefabName;

            // 2) Usuń stary wizualny model (wszystkie dzieci roota = szkielet + mesh).
            //    Logika gracza siedzi w komponentach roota, nie w dzieciach.
            Transform rootT = root.transform;
            for (int i = rootT.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(rootT.GetChild(i).gameObject);

            // 3) Wstaw nowy model jako dziecko.
            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            PrefabUtility.UnpackPrefabInstance(modelInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            modelInstance.transform.SetParent(rootT, false);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;

            // Modele FBX czasem mają na roocie własny Animator — usuwamy, sterowanie idzie z roota.
            Animator childAnim = modelInstance.GetComponent<Animator>();
            if (childAnim != null)
                Object.DestroyImmediate(childAnim);

            // 4) Skala roota + Animator avatar (kontroler zostaje z szablonu — Humanoid retarget).
            rootT.localScale = Vector3.one * spec.rootScale;

            Animator rootAnim = root.GetComponent<Animator>();
            if (rootAnim != null && avatar != null)
                rootAnim.avatar = avatar;

            // 5) Dopasuj CapsuleCollider do gabarytów nowego modelu (w przestrzeni roota).
            FitCapsule(root, modelInstance, spec.rootScale);

            // 6) Zapis prefabu.
            string outPath = $"{OutputDir}/{spec.prefabName}.prefab";
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, outPath, out bool success);
            Object.DestroyImmediate(root);

            if (success && saved != null)
            {
                report.AppendLine($"  OK {spec.prefabName} ← {System.IO.Path.GetFileName(spec.fbxPath)} (skala {spec.rootScale})");
                return saved;
            }

            report.AppendLine($"  BŁĄD zapisu {spec.prefabName} → {outPath}");
            return null;
        }

        private static void FitCapsule(GameObject root, GameObject model, float rootScale)
        {
            CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
            if (capsule == null)
                return;

            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
                return;

            Bounds worldBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                worldBounds.Encapsulate(renderers[i].bounds);

            // Gabaryty w przestrzeni LOKALNEJ roota (collider jest na roocie, więc dzielimy przez skalę).
            float scale = Mathf.Approximately(rootScale, 0f) ? 1f : rootScale;
            float height = Mathf.Max(0.5f, worldBounds.size.y / scale);
            float radius = Mathf.Clamp((Mathf.Max(worldBounds.size.x, worldBounds.size.z) / scale) * 0.5f, 0.2f, height * 0.5f);

            capsule.direction = 1; // Y-up
            capsule.height = height;
            capsule.radius = radius;
            capsule.center = new Vector3(0f, height * 0.5f, 0f);
        }

        private static Avatar LoadAvatar(string fbxPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (Object asset in assets)
            {
                if (asset is Avatar avatar)
                    return avatar;
            }
            return null;
        }

        private static void AddExisting(Dictionary<string, GameObject> dict, string name, string path)
        {
            if (dict.ContainsKey(name))
                return;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                dict[name] = prefab;
        }

        private static string WireSpawner(Dictionary<string, GameObject> built)
        {
            Scene active = SceneManager.GetActiveScene();
            bool worldAlreadyOpen = active.IsValid() && active.path == WorldScenePath;

            if (!worldAlreadyOpen && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[PlayerPrefabBuilder] Przerwano otwieranie World.unity (niezapisane zmiany). " +
                                 "Prefaby zbudowane — wpnij je ręcznie w spawnerze lub uruchom ponownie po zapisaniu sceny.");
                return "Prefaby zbudowane, ale spawner NIE wpięty (anulowano zapis bieżącej sceny).";
            }

            Scene scene = worldAlreadyOpen
                ? active
                : EditorSceneManager.OpenScene(WorldScenePath, OpenSceneMode.Single);

            CharacterClassPlayerSpawner spawner = Object.FindFirstObjectByType<CharacterClassPlayerSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("[PlayerPrefabBuilder] Nie znaleziono CharacterClassPlayerSpawner w " + WorldScenePath +
                                 ". Wpnij prefaby ręcznie w Inspectorze (pole Class Prefabs).");
                return "Nie znaleziono spawnera w scenie World — wpnij prefaby ręcznie.";
            }

            var so = new SerializedObject(spawner);

            SerializedProperty list = so.FindProperty("classPrefabs");
            list.ClearArray();

            int index = 0;
            int wired = 0;
            foreach (var pair in ClassToPrefabName)
            {
                if (!built.TryGetValue(pair.Value, out GameObject prefab) || prefab == null)
                    continue;

                list.InsertArrayElementAtIndex(index);
                SerializedProperty entry = list.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("classType").enumValueIndex = (int)pair.Key;
                entry.FindPropertyRelative("prefab").objectReferenceValue = prefab;
                index++;
                wired++;
            }

            // Pola legacy — zostaw spójne (fallback gdyby ktoś wyczyścił listę).
            if (built.TryGetValue("Player_Dwarf", out GameObject dwarf))
                so.FindProperty("dwarfPrefab").objectReferenceValue = dwarf;
            if (built.TryGetValue("Player_Warrior", out GameObject warrior))
                so.FindProperty("warriorPrefab").objectReferenceValue = warrior;

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(spawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            string msg = $"Spawner wpięty: {wired}/{ClassToPrefabName.Count} klas zmapowanych.";
            Debug.Log("[PlayerPrefabBuilder] " + msg);
            return msg;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}

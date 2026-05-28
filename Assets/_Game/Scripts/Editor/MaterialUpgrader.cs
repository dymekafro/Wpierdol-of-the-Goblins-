#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using WPG.Core;
using WPG.World;

namespace WPG.EditorTools
{
    public static class MaterialUpgraderEditor
    {
        [MenuItem("WPG/Upgrade Materials to URP (Selection)")]
        public static void UpgradeSelectedMaterials()
        {
            var mats = CollectMaterials(Selection.objects);
            if (mats.Count == 0)
            {
                EditorUtility.DisplayDialog("Material Upgrader",
                    "Zaznacz materiały lub foldery w Project, potem uruchom ponownie.", "OK");
                return;
            }

            int upgraded = 0;
            foreach (var mat in mats)
            {
                if (MaterialUpgrader.UpgradeMaterial(mat)) upgraded++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[MaterialUpgrader] Zaktualizowano {upgraded}/{mats.Count} materiałów do URP.");
        }

        [MenuItem("WPG/Fix Invector Materials (URP)")]
        public static void FixInvectorMaterials()
        {
            var folders = ResolveInvectorMaterialFolders();
            if (folders.Count == 0)
            {
                EditorUtility.DisplayDialog("Invector — materiały URP",
                    "Nie znaleziono folderu Invector w Assets/.\n\n" +
                    $"Oczekiwana ścieżka:\n{GameAssetPaths.InvectorFolder}",
                    "OK");
                return;
            }

            var fixedNames = new List<string>();
            int total = 0;
            int upgraded = 0;
            int skipped = 0;

            foreach (var folder in folders)
            {
                var guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null) continue;

                    total++;
                    if (!MaterialUpgrader.NeedsUpgrade(mat))
                    {
                        skipped++;
                        continue;
                    }

                    if (MaterialUpgrader.UpgradeMaterial(mat))
                    {
                        EditorUtility.SetDirty(mat);
                        fixedNames.Add($"{mat.name} ({mat.shader.name})");
                        upgraded++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var folderList = string.Join("\n", folders);
            Debug.Log($"[MaterialUpgrader] Invector URP: {upgraded}/{total} materiałów naprawionych w:\n{folderList}");

            EditorUtility.DisplayDialog("Invector — materiały URP",
                upgraded > 0
                    ? $"Naprawiono {upgraded} z {total} materiałów Invector.\n\nFoldery:\n{folderList}\n\n" +
                      string.Join("\n", fixedNames)
                    : total > 0
                        ? $"Przeskanowano {total} materiałów — wszystkie już na URP ({skipped} bez zmian)."
                        : "Brak plików .mat w folderze Invector.",
                "OK");
        }

        [MenuItem("WPG/Upgrade Invector Materials to URP", true)]
        private static bool HideLegacyInvectorMenu() => false;

        [MenuItem("WPG/Upgrade Invector Materials to URP")]
        public static void UpgradeInvectorMaterialsLegacy() => FixInvectorMaterials();

        [MenuItem("WPG/Upgrade All Materials to URP")]
        public static void UpgradeAllInAssets()
        {
            var guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            int upgraded = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && MaterialUpgrader.UpgradeMaterial(mat))
                {
                    EditorUtility.SetDirty(mat);
                    upgraded++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[MaterialUpgrader] Zaktualizowano {upgraded} materiałów w Assets/ (w tym Fantasy Forest).");
        }

        [MenuItem("WPG/Fix Fantasy Forest Leaf Materials")]
        public static void FixFantasyForestLeafMaterials()
        {
            var folder = MaterialUpgrader.FantasyForestMaterialsFolder;
            if (!AssetDatabase.IsValidFolder(folder))
            {
                EditorUtility.DisplayDialog("Fantasy Forest",
                    $"Nie znaleziono folderu:\n{folder}", "OK");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
            var fixedNames = new List<string>();
            int upgraded = 0;
            int skipped = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                bool isFoliage = MaterialUpgrader.IsFoliageMaterial(mat);
                bool needsUpgrade = MaterialUpgrader.NeedsUpgrade(mat);

                if (!isFoliage && !needsUpgrade)
                {
                    skipped++;
                    continue;
                }

                if (MaterialUpgrader.UpgradeMaterial(mat))
                {
                    EditorUtility.SetDirty(mat);
                    fixedNames.Add($"{mat.name} ({mat.shader.name})");
                    upgraded++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var log = new StringBuilder();
            log.AppendLine($"[Fantasy Forest] Naprawiono {upgraded} materiałów liści/foliage, pominięto {skipped}.");
            foreach (var entry in fixedNames)
                log.AppendLine($"  • {entry}");

            Debug.Log(log.ToString());
            EditorUtility.DisplayDialog("Fantasy Forest — liście",
                upgraded > 0
                    ? $"Zaktualizowano {upgraded} materiał(ów):\n{string.Join("\n", fixedNames)}"
                    : "Brak materiałów do naprawy (wszystkie już na URP Lit + Alpha Clip?).",
                "OK");
        }

        private static List<Material> CollectMaterials(Object[] selection)
        {
            var result = new HashSet<Material>();
            foreach (var obj in selection)
            {
                if (obj is Material m)
                {
                    result.Add(m);
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetDatabase.IsValidFolder(path))
                {
                    var guids = AssetDatabase.FindAssets("t:Material", new[] { path });
                    foreach (var guid in guids)
                    {
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                        if (mat != null) result.Add(mat);
                    }
                }
            }
            return new List<Material>(result);
        }

        private static List<string> ResolveInvectorMaterialFolders()
        {
            var folders = new List<string>();
            if (AssetDatabase.IsValidFolder(GameAssetPaths.InvectorFolder))
                folders.Add(GameAssetPaths.InvectorFolder);

            foreach (var guid in AssetDatabase.FindAssets("Invector"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.IsValidFolder(path)) continue;
                if (MaterialUpgrader.IsInvectorAssetPath(path) && !folders.Contains(path))
                    folders.Add(path);
            }

            return folders;
        }
    }
}
#endif

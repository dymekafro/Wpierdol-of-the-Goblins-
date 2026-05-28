using UnityEditor;
using WPG.Core;

namespace WPG.Editor
{
    [InitializeOnLoad]
    public static class AssetCatalogPlayModeLoader
    {
        static AssetCatalogPlayModeLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            GameAssetRegistry.Initialize(force: true);
            GameAssetRegistry.LogReport();
        }
    }
}

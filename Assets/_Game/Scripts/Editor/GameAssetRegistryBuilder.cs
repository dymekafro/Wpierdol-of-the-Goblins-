using UnityEditor;
using WPG.Core;

namespace WPG.Editor
{
    public static class GameAssetRegistryBuilder
    {
        [MenuItem("WPG/Rebuild Asset Catalog")]
        public static void RebuildAndLog()
        {
            GameAssetRegistry.Initialize(force: true);
            GameAssetRegistry.LogReport();
        }
    }
}

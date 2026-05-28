using UnityEngine;

namespace WPG.Core
{
    /// <summary>
    /// Fasada slotów assetów — deleguje do <see cref="GameAssetRegistry"/>.
    /// </summary>
    public static class AssetCatalog
    {
        public static GameObject TreeLarge => GameAssetRegistry.TreeLarge;
        public static GameObject TreeSmall => GameAssetRegistry.TreeSmall;
        public static GameObject Bush => GameAssetRegistry.Bush;
        public static GameObject Grass => GameAssetRegistry.Grass;
        public static GameObject Rock => GameAssetRegistry.Rock;
        public static GameObject Ruin => GameAssetRegistry.Ruin;
        public static GameObject DruidModel => GameAssetRegistry.DruidModel;
        public static GameObject GoblinModel => GameAssetRegistry.GoblinModel;
        public static GameObject GoblinElite => GameAssetRegistry.GoblinElite;
        public static GameObject Totem => GameAssetRegistry.Totem;
        public static GameObject CampFire => GameAssetRegistry.CampFire;
        public static GameObject VfxFireball => GameAssetRegistry.VfxFireball;
        public static GameObject VfxHeal => GameAssetRegistry.VfxHeal;
        public static AudioClip SfxHit => GameAssetRegistry.SfxHit;
        public static AudioClip SfxDeath => GameAssetRegistry.SfxDeath;
        public static AudioClip SfxCast => GameAssetRegistry.SfxCast;
        public static GameObject UiBarFrame => GameAssetRegistry.UiBarFrame;

        public static void Initialize() => GameAssetRegistry.Initialize();
        public static void Initialize(bool force) => GameAssetRegistry.Initialize(force);
        public static void LogReport() => GameAssetRegistry.LogReport();
    }
}

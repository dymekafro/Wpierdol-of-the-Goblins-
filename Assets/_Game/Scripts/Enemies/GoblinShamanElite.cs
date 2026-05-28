using UnityEngine;
using WPG.World;

namespace WPG.Enemies
{
    /// <summary>Elitarny goblin (Fantasy Goblin) — oboz Szamana / pierścień 3.</summary>
    public class GoblinShamanElite : GoblinStormtrooper
    {
        protected override WorldAssetPlacer.CharacterModelKind? AssetModelKind =>
            WorldAssetPlacer.CharacterModelKind.GoblinElite;

        protected override float ModelScaleMultiplier => 1.08f;

        protected override void Awake()
        {
            base.Awake();
            displayName = "Goblin Elita";
            maxHealth = 55;
            damage = 9;
            moveSpeed = 3.2f;
            attackRange = 2.2f;
            attackCooldown = 1.0f;
            scale = 1.0f;
            baseColor = new Color(0.55f, 0.25f, 0.45f);
        }

        protected override void OnFantasyGoblinAttached(WorldAssetPlacer.CharacterAttachResult attach)
        {
            ApplyHierarchyMaterialTint(baseColor, 0.42f);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using WPG.Core;

namespace WPG.UI
{
    // Placeholder drzewka Przyrody: 3 gałęzie (Ogień / Życie / Kształt), po 1 zablokowanym nodzie.
    public class NatureSkillTreePanelUI : BasePanelUI
    {
        protected override string TitlePL => "Drzewko Przyrody";
        protected override Vector2 PanelSize => new Vector2(960f, 640f);

        private Text _pointsText;

        protected override void BuildContent(RectTransform body)
        {
            var pointsHolder = UIFactory.CreatePanel(body, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -36f), new Vector2(0f, 0f), "PointsHolder");
            _pointsText = UIFactory.CreateText(pointsHolder, "Punkty Przyrody: 0",
                24, new Color(0.85f, 0.95f, 0.55f), TextAnchor.UpperCenter, "Points");

            UIFactory.CreateText(body,
                "Odblokowuj nody po zdobywaniu punktów z obozów i miejsc mocy.\n(MVP — pełna logika wkrótce)",
                18, new Color(0.65f, 0.72f, 0.58f), TextAnchor.UpperCenter, "SubHint");

            BuildBranch(body, "Ogień", new Color(1f, 0.45f, 0.15f),
                GameAssetLoader.LoadSprite(GameAssetPaths.IconFireball, GameAssetPaths.ResIconFire),
                0.17f, "Płomienny Cios", "Fireball AoE +50% (zablokowane)");
            BuildBranch(body, "Życie", new Color(0.35f, 0.9f, 0.45f),
                GameAssetLoader.LoadSprite(GameAssetPaths.IconHeal, GameAssetPaths.ResIconHeal),
                0.5f, "Korzenie Życia", "Thorns 5 dmg (zablokowane)");
            BuildBranch(body, "Kształt", new Color(0.55f, 0.75f, 1f),
                GameAssetLoader.LoadSprite(GameAssetPaths.IconMelee, GameAssetPaths.ResIconMelee),
                0.83f, "Forma Bestii", "Transformacja +30% speed (zablokowane)");
        }

        private void BuildBranch(RectTransform body, string branchName, Color accent, Sprite icon,
            float anchorX, string nodeName, string nodeDesc)
        {
            float colW = 260f;
            float xCenter = anchorX;

            var col = UIFactory.CreatePanel(body,
                BaseUIAssets.PanelSprite != null
                    ? new Color(0.08f, 0.11f, 0.08f, 0.5f)
                    : new Color(0.08f, 0.11f, 0.08f, 0.92f),
                new Vector2(xCenter, 0.5f), new Vector2(xCenter, 0.5f),
                new Vector2(-colW * 0.5f, -220f), new Vector2(colW * 0.5f, 120f), "Branch_" + branchName);

            if (BaseUIAssets.PanelSprite != null)
            {
                UIFactory.CreateImage(col, BaseUIAssets.PanelSprite, new Color(1f, 1f, 1f, 0.28f),
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "ColFrame", Image.Type.Sliced);
            }

            var titleH = UIFactory.CreatePanel(col.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -40f), new Vector2(0f, 0f), "BranchTitle");
            var title = UIFactory.CreateText(titleH, branchName, 26, accent, TextAnchor.MiddleCenter, "Title");
            title.fontStyle = FontStyle.Bold;

            // Node (locked)
            var node = UIFactory.CreatePanel(col.transform, new Color(0.06f, 0.08f, 0.06f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-100f, -90f), new Vector2(100f, 50f), "Node");

            if (BaseUIAssets.IconFrameSprite != null)
            {
                UIFactory.CreateImage(node.transform, BaseUIAssets.IconFrameSprite,
                    new Color(0.45f, 0.45f, 0.45f, 0.85f),
                    new Vector2(0f, 0.35f), new Vector2(1f, 1f),
                    new Vector2(8f, 0f), new Vector2(-8f, -8f), "NodeFrame", Image.Type.Sliced);
            }

            if (icon != null)
            {
                UIFactory.CreateImage(node.transform, icon, new Color(0.5f, 0.5f, 0.5f, 0.7f),
                    new Vector2(0.25f, 0.45f), new Vector2(0.75f, 0.95f),
                    Vector2.zero, Vector2.zero, "NodeIcon");
            }

            UIFactory.CreateText(node.transform, nodeName + "\n🔒", 18,
                new Color(0.55f, 0.58f, 0.52f), TextAnchor.LowerCenter, "NodeName");

            var descH = UIFactory.CreatePanel(col.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(8f, 8f), new Vector2(-8f, 72f), "NodeDesc");
            UIFactory.CreateText(descH, nodeDesc, 15,
                new Color(0.6f, 0.65f, 0.55f), TextAnchor.LowerCenter, "Desc");
        }

        protected override void OnShown()
        {
            if (_pointsText != null)
                _pointsText.text = "Punkty Przyrody: 0";
        }
    }
}

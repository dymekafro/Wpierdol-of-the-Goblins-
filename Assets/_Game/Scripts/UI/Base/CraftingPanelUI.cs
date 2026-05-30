using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WPG.Items;

namespace WPG.UI
{
    // Panel craftingu: 3 przepisy MVP, ikony z Modern RPG icons, przyciski Fantasy Free GUI.
    public class CraftingPanelUI : BasePanelUI
    {
        public Inventory inventory;

        protected override string TitlePL => "Crafting — Sady Ostatniego Strażnika";

        private class Row
        {
            public ItemDefinition recipe;
            public Text ingredients;
            public Button craftButton;
            public Image craftButtonImage;
        }

        private readonly List<Row> _rows = new List<Row>();

        protected override void BuildContent(RectTransform body)
        {
            var recipes = ItemDatabase.Recipes;
            float rowH = 150f;
            float gap = 16f;
            float top = -10f;

            for (int i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                float y0 = top - i * (rowH + gap);

                Color rowBg = BaseUIAssets.PanelSprite != null
                    ? new Color(0.08f, 0.12f, 0.09f, 0.55f)
                    : new Color(0.1f, 0.14f, 0.1f, 0.95f);

                var rowPanel = UIFactory.CreatePanel(body, rowBg,
                    new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(0f, y0 - rowH), new Vector2(0f, y0), "Recipe_" + recipe.id);

                if (BaseUIAssets.PanelSprite != null)
                {
                    UIFactory.CreateImage(rowPanel, BaseUIAssets.PanelSprite, new Color(1f, 1f, 1f, 0.35f),
                        Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "RowFrame", Image.Type.Sliced);
                }

                // Ikona produktu (Modern RPG icons / fallback kolor)
                BaseUIAssets.CreateItemSlotIcon(rowPanel.transform, recipe.id,
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(12f, -52f), new Vector2(76f, 52f), "ProductIcon");

                // Nazwa produktu
                var nameHolder = UIFactory.CreatePanel(rowPanel.transform, new Color(0f, 0f, 0f, 0f),
                    new Vector2(0f, 1f), new Vector2(0.68f, 1f),
                    new Vector2(88f, -42f), new Vector2(0f, -6f), "NameHolder");
                UIFactory.CreateText(nameHolder, recipe.displayNamePL, 26,
                    new Color(0.92f, 0.95f, 0.78f), TextAnchor.MiddleLeft, "Name");

                // Opis
                var descHolder = UIFactory.CreatePanel(rowPanel.transform, new Color(0f, 0f, 0f, 0f),
                    new Vector2(0f, 0f), new Vector2(0.68f, 1f),
                    new Vector2(88f, 44f), new Vector2(0f, -44f), "DescHolder");
                UIFactory.CreateText(descHolder, recipe.descriptionPL, 17,
                    new Color(0.75f, 0.78f, 0.7f), TextAnchor.UpperLeft, "Desc");

                // Składniki (rich text)
                var ingHolder = UIFactory.CreatePanel(rowPanel.transform, new Color(0f, 0f, 0f, 0f),
                    new Vector2(0f, 0f), new Vector2(0.68f, 0f),
                    new Vector2(88f, 10f), new Vector2(0f, 44f), "IngHolder");
                var ingText = UIFactory.CreateText(ingHolder, "", 18,
                    Color.white, TextAnchor.LowerLeft, "Ingredients");
                ingText.supportRichText = true;

                var btn = BaseUIAssets.CreateActionButton(rowPanel.transform, "Wytwórz",
                    () => OnCraftClicked(recipe),
                    new Vector2(0.72f, 0.5f), new Vector2(1f, 0.5f),
                    new Vector2(8f, -42f), new Vector2(-16f, 42f),
                    new Color(0.18f, 0.42f, 0.22f, 1f), new Color(0.28f, 0.62f, 0.3f, 1f), 26);

                _rows.Add(new Row
                {
                    recipe = recipe,
                    ingredients = ingText,
                    craftButton = btn,
                    craftButtonImage = btn.GetComponent<Image>()
                });
            }
        }

        private void OnCraftClicked(ItemDefinition recipe)
        {
            if (inventory == null) return;
            inventory.TryCraft(recipe);
            Refresh();
        }

        protected override void OnShown() => Refresh();

        public void Refresh()
        {
            if (inventory == null) return;

            var sb = new StringBuilder();
            foreach (var row in _rows)
            {
                sb.Clear();
                sb.Append("Wymaga:  ");
                var inputs = row.recipe.craftInputs;
                for (int i = 0; i < inputs.Length; i++)
                {
                    var input = inputs[i];
                    int have = inventory.Count(input.itemId);
                    bool ok = have >= input.amount;
                    string color = ok ? "#6fdc5a" : "#e05a5a";
                    sb.Append($"<color={color}>{ItemDatabase.DisplayName(input.itemId)} {have}/{input.amount}</color>");
                    if (i < inputs.Length - 1) sb.Append("   ");
                }
                row.ingredients.text = sb.ToString();

                bool canCraft = inventory.CanCraft(row.recipe);
                row.craftButton.interactable = canCraft;
                if (row.craftButtonImage != null)
                    row.craftButtonImage.color = canCraft
                        ? new Color(0.18f, 0.42f, 0.22f, 1f)
                        : new Color(0.2f, 0.22f, 0.2f, 0.8f);
            }
        }
    }
}

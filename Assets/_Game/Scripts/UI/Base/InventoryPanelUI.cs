using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WPG.Items;

namespace WPG.UI
{
    // Ekwipunek: siatka 16 slotów (4×4) + hotbar 1–4. Klik na consumable = użycie.
    public class InventoryPanelUI : BasePanelUI
    {
        public Inventory inventory;

        protected override string TitlePL => "Ekwipunek";
        protected override Vector2 PanelSize => new Vector2(820f, 680f);

        private class SlotView
        {
            public int index;
            public Button button;
            public Text qtyText;
            public Image bg;
            public GameObject iconRoot;
        }

        private readonly List<SlotView> _slots = new List<SlotView>();

        protected override void BuildContent(RectTransform body)
        {
            UIFactory.CreateText(body, "Kliknij miksturę, aby użyć (+40 HP). Hotbar: sloty 1–4.",
                18, new Color(0.7f, 0.78f, 0.65f), TextAnchor.UpperCenter, "Hint");

            // Hotbar 1–4 (pierwsze 4 sloty inventory)
            var hotbarLabel = UIFactory.CreatePanel(body, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -36f), new Vector2(0f, -8f), "HotbarLabel");
            UIFactory.CreateText(hotbarLabel, "Hotbar", 20,
                new Color(0.8f, 0.9f, 0.7f), TextAnchor.MiddleLeft, "Label");

            float hotY = -48f;
            float slotSize = 72f;
            float gap = 12f;
            float startX = 20f;

            for (int i = 0; i < Inventory.HotbarCount; i++)
            {
                float x = startX + i * (slotSize + gap);
                BuildSlot(body, i, x, hotY, slotSize, true, (i + 1).ToString());
            }

            // Siatka 4×4 (sloty 0–15, hotbar to te same indeksy 0–3 wizualnie osobno oznaczone)
            float gridTop = -140f;
            int cols = 4;
            for (int i = 0; i < Inventory.SlotCount; i++)
            {
                int row = i / cols;
                int col = i % cols;
                float x = startX + col * (slotSize + gap);
                float y = gridTop - row * (slotSize + gap);
                BuildSlot(body, i, x, y, slotSize, false, null);
            }
        }

        private void BuildSlot(RectTransform body, int index, float x, float y, float size, bool isHotbar, string hotbarNum)
        {
            var holder = new GameObject("Slot_" + index);
            holder.transform.SetParent(body, false);
            var rt = holder.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(x, y);

            var bg = holder.AddComponent<Image>();
            bg.color = isHotbar
                ? new Color(0.14f, 0.22f, 0.15f, 0.95f)
                : new Color(0.08f, 0.11f, 0.08f, 0.92f);

            if (BaseUIAssets.IconFrameSprite != null)
            {
                UIFactory.CreateImage(holder.transform, BaseUIAssets.IconFrameSprite, Color.white,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "Frame", Image.Type.Sliced);
            }

            var iconRoot = new GameObject("IconRoot");
            iconRoot.transform.SetParent(holder.transform, false);
            var iconRt = iconRoot.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.12f, 0.14f);
            iconRt.anchorMax = new Vector2(0.88f, 0.88f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;

            var qtyGO = new GameObject("Qty");
            qtyGO.transform.SetParent(holder.transform, false);
            var qtyRt = qtyGO.AddComponent<RectTransform>();
            qtyRt.anchorMin = new Vector2(1f, 0f);
            qtyRt.anchorMax = new Vector2(1f, 0f);
            qtyRt.pivot = new Vector2(1f, 0f);
            qtyRt.anchoredPosition = new Vector2(-4f, 4f);
            qtyRt.sizeDelta = new Vector2(40f, 22f);
            var qtyText = qtyGO.AddComponent<Text>();
            qtyText.font = UIFactory.GetFont();
            qtyText.fontSize = 16;
            qtyText.color = new Color(1f, 0.95f, 0.7f);
            qtyText.alignment = TextAnchor.LowerRight;
            qtyText.raycastTarget = false;

            if (isHotbar && !string.IsNullOrEmpty(hotbarNum))
            {
                var numHolder = UIFactory.CreatePanel(holder.transform, new Color(0f, 0f, 0f, 0f),
                    new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(2f, -22f), new Vector2(22f, -2f), "HotbarNum");
                UIFactory.CreateText(numHolder, hotbarNum, 14,
                    new Color(0.9f, 0.95f, 0.75f), TextAnchor.MiddleCenter, "Num");
            }

            var btn = holder.AddComponent<Button>();
            btn.targetGraphic = bg;
            int captured = index;
            btn.onClick.AddListener(() => OnSlotClicked(captured));

            _slots.Add(new SlotView
            {
                index = index,
                button = btn,
                qtyText = qtyText,
                bg = bg,
                iconRoot = iconRoot
            });
        }

        private void OnSlotClicked(int index)
        {
            if (inventory == null) return;
            inventory.UseSlot(index);
            Refresh();
        }

        protected override void OnShown() => Refresh();

        public void Refresh()
        {
            if (inventory == null) return;

            foreach (var view in _slots)
            {
                // Usuń poprzednią ikonę
                for (int c = view.iconRoot.transform.childCount - 1; c >= 0; c--)
                    Destroy(view.iconRoot.transform.GetChild(c).gameObject);

                var slot = inventory.GetSlot(view.index);
                if (slot == null || slot.IsEmpty)
                {
                    view.qtyText.text = "";
                    continue;
                }

                var def = slot.Definition;
                var sprite = ItemIconResolver.GetIcon(slot.itemId);
                if (sprite != null)
                {
                    UIFactory.CreateImage(view.iconRoot.transform, sprite, Color.white,
                        Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "Icon");
                }
                else if (def != null)
                {
                    UIFactory.CreateImage(view.iconRoot.transform, null, def.iconColor,
                        Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "IconFallback");
                }

                view.qtyText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
            }
        }
    }
}

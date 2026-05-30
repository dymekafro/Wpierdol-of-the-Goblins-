using UnityEngine;
using UnityEngine.UI;
using WPG.Core;
using WPG.Items;

namespace WPG.UI
{
    // Wspólne sprite'y Fantasy Free GUI ładowane raz dla paneli bazy (jak PlayerHUD / MainMenu).
    public static class BaseUIAssets
    {
        public static Sprite PanelSprite { get; private set; }
        public static Sprite ButtonSprite { get; private set; }
        public static Sprite IconFrameSprite { get; private set; }
        public static bool Loaded { get; private set; }

        public static void EnsureLoaded()
        {
            if (Loaded) return;
            Loaded = true;

            PanelSprite = GameAssetLoader.LoadSprite(GameAssetPaths.GuiPanel, GameAssetPaths.ResUiPanel);
            ButtonSprite = GameAssetLoader.LoadSprite(GameAssetPaths.GuiButton, GameAssetPaths.ResUiButton);
            IconFrameSprite = GameAssetLoader.LoadSprite(GameAssetPaths.GuiIconFrame, GameAssetPaths.ResUiIconFrame);
        }

        // Ramka slotu z ikoną itemu (Modern RPG icons) lub kolorowym wypełnieniem.
        public static Image CreateItemSlotIcon(Transform parent, string itemId, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, string name = "ItemSlot")
        {
            EnsureLoaded();

            var holder = UIFactory.CreatePanel(parent, new Color(0.05f, 0.07f, 0.05f, 0.92f),
                anchorMin, anchorMax, offsetMin, offsetMax, name);

            if (IconFrameSprite != null)
            {
                UIFactory.CreateImage(holder, IconFrameSprite, Color.white,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "Frame", Image.Type.Sliced);
            }

            var sprite = ItemIconResolver.GetIcon(itemId);
            var tint = ItemIconResolver.GetTint(itemId);

            if (sprite != null)
            {
                UIFactory.CreateImage(holder, sprite, Color.white,
                    new Vector2(0.14f, 0.18f), new Vector2(0.86f, 0.86f),
                    Vector2.zero, Vector2.zero, "Icon");
            }
            else
            {
                UIFactory.CreateImage(holder, null, tint,
                    new Vector2(0.18f, 0.22f), new Vector2(0.82f, 0.82f),
                    Vector2.zero, Vector2.zero, "IconFallback");
            }

            return holder.GetComponent<Image>();
        }

        // Przycisk fantasy (GuiButton) lub fallback kolorystyczny.
        public static Button CreateActionButton(Transform parent, string label, System.Action onClick,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            Color fallbackBase, Color fallbackHover, int fontSize = 26)
        {
            EnsureLoaded();
            if (ButtonSprite != null)
            {
                return UIFactory.CreateFantasyButton(parent, label, onClick,
                    anchorMin, anchorMax, offsetMin, offsetMax, fallbackBase, fallbackHover, fontSize);
            }
            return UIFactory.CreateButton(parent, label, fallbackBase, fallbackHover, onClick,
                anchorMin, anchorMax, offsetMin, offsetMax, fontSize);
        }
    }
}

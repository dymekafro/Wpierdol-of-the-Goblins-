using UnityEngine;
using UnityEngine.UI;
using WPG.Core;

namespace WPG.UI
{
    // Wspólny szkielet panelu modalnego bazy. Używa Fantasy Free GUI (panel, button, icon_frame)
    // z fallbackiem kolorystycznym — spójnie z PlayerHUD i MainMenuBootstrap.
    public abstract class BasePanelUI : MonoBehaviour
    {
        protected RectTransform Root;
        protected RectTransform Body;

        protected abstract string TitlePL { get; }
        protected virtual Vector2 PanelSize => new Vector2(900f, 620f);

        public bool IsOpen => Root != null && Root.gameObject.activeSelf;

        // Wołane po Hide() — BaseUIManager resetuje kursor i _activePanel.
        public System.Action OnClosed;

        public void Build(Transform canvas)
        {
            BaseUIAssets.EnsureLoaded();

            float w = PanelSize.x;
            float h = PanelSize.y;

            Root = UIFactory.CreatePanel(canvas,
                new Color(0.06f, 0.09f, 0.07f, 0.97f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-w * 0.5f, -h * 0.5f), new Vector2(w * 0.5f, h * 0.5f),
                "Panel_" + GetType().Name);

            // Tło okna — GuiPanel (sliced) lub ciemna ramka
            if (BaseUIAssets.PanelSprite != null)
            {
                var panelBg = UIFactory.CreateImage(Root, BaseUIAssets.PanelSprite, Color.white,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "PanelBg", Image.Type.Sliced);
                panelBg.transform.SetAsFirstSibling();
            }
            else
            {
                var border = UIFactory.CreateImage(Root, null, new Color(0.3f, 0.5f, 0.32f, 0.9f),
                    Vector2.zero, Vector2.one, new Vector2(-4, -4), new Vector2(4, 4), "Border");
                border.transform.SetAsFirstSibling();
            }

            // Pasek tytułu
            var titleBar = UIFactory.CreatePanel(Root.transform,
                new Color(0.12f, 0.2f, 0.13f, BaseUIAssets.PanelSprite != null ? 0.75f : 1f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -56f), new Vector2(0f, 0f), "TitleBar");

            var titleHolder = UIFactory.CreatePanel(titleBar.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(24f, 0f), new Vector2(-64f, 0f), "TitleHolder");
            UIFactory.CreateText(titleHolder, TitlePL, 30,
                new Color(0.85f, 0.95f, 0.7f), TextAnchor.MiddleLeft, "Title");

            BaseUIAssets.CreateActionButton(titleBar.transform, "×", Hide,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-52f, -22f), new Vector2(-8f, 22f),
                new Color(0.45f, 0.18f, 0.18f, 1f), new Color(0.7f, 0.28f, 0.28f, 1f), 34);

            Body = UIFactory.CreatePanel(Root.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(20f, 20f), new Vector2(-20f, -64f), "Body");

            BuildContent(Body);
            Hide();
        }

        protected abstract void BuildContent(RectTransform body);

        public virtual void Show()
        {
            if (Root == null) return;
            Root.gameObject.SetActive(true);
            Root.SetAsLastSibling();
            OnShown();
        }

        public virtual void Hide()
        {
            if (Root == null) return;
            Root.gameObject.SetActive(false);
            OnHidden();
            OnClosed?.Invoke();
        }

        public void Toggle()
        {
            if (IsOpen) Hide(); else Show();
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}

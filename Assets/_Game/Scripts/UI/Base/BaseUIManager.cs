using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using WPG.Items;
using WPG.Player;
using WPG.World;

namespace WPG.UI
{
    // Manager UI bazy druida: klawisze C/K/I/Tab, auto-zamknięcie przy wyjściu z bazy,
    // hint HUD, kursor. Jeden aktywny panel naraz.
    public class BaseUIManager : MonoBehaviour
    {
        public static BaseUIManager Instance { get; private set; }
        public static bool AnyPanelOpen => Instance != null && Instance._activePanel != null && Instance._activePanel.IsOpen;

        private Canvas _canvas;
        private GameObject _hintRoot;
        private BasePanelUI _activePanel;
        private bool _insideBase;

        private Inventory _inventoryComp;
        private CraftingPanelUI _crafting;
        private InventoryPanelUI _inventoryPanel;
        private NatureSkillTreePanelUI _skillTree;

        public void Initialize(Inventory inventory, PlayerStats stats, PlayerCombat combat)
        {
            Instance = this;
            _inventoryComp = inventory;

            BaseUIAssets.EnsureLoaded();
            UIFactory.EnsureEventSystem();

            _canvas = UIFactory.CreateScreenCanvas("Canvas_BaseUI", 8);

            if (BaseUIAssets.PanelSprite == null)
            {
                UIFactory.CreatePanel(_canvas.transform, new Color(0f, 0f, 0f, 0.15f),
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "DimOverlay");
            }

            BuildHint();
            BuildPanels();

            if (_inventoryComp != null)
                _inventoryComp.OnChanged += OnInventoryChanged;

            DruidBase.OnPlayerEnter += OnPlayerEnterBase;
            DruidBase.OnPlayerExit += OnPlayerExitBase;

            if (DruidBase.IsPlayerInsideStatic)
                OnPlayerEnterBase();
            else
                SetHintVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_inventoryComp != null)
                _inventoryComp.OnChanged -= OnInventoryChanged;
            DruidBase.OnPlayerEnter -= OnPlayerEnterBase;
            DruidBase.OnPlayerExit -= OnPlayerExitBase;
        }

        private void BuildHint()
        {
            _hintRoot = UIFactory.CreatePanel(_canvas.transform, new Color(0f, 0f, 0f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-320f, 24f), new Vector2(320f, 64f), "BaseHint").gameObject;

            if (BaseUIAssets.PanelSprite != null)
            {
                UIFactory.CreateImage(_hintRoot.transform, BaseUIAssets.PanelSprite,
                    new Color(1f, 1f, 1f, 0.45f),
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "HintFrame", Image.Type.Sliced);
            }

            UIFactory.CreateText(_hintRoot.transform,
                "C Craft  |  K Drzewko  |  I Ekwipunek",
                22, new Color(0.75f, 0.88f, 0.62f, 0.92f), TextAnchor.MiddleCenter, "HintText");

            SetHintVisible(false);
        }

        private void BuildPanels()
        {
            var craftingGO = new GameObject("CraftingPanel");
            craftingGO.transform.SetParent(_canvas.transform, false);
            _crafting = craftingGO.AddComponent<CraftingPanelUI>();
            _crafting.inventory = _inventoryComp;
            _crafting.OnClosed = () => OnPanelClosed(_crafting);
            _crafting.Build(_canvas.transform);

            var invGO = new GameObject("InventoryPanel");
            invGO.transform.SetParent(_canvas.transform, false);
            _inventoryPanel = invGO.AddComponent<InventoryPanelUI>();
            _inventoryPanel.inventory = _inventoryComp;
            _inventoryPanel.OnClosed = () => OnPanelClosed(_inventoryPanel);
            _inventoryPanel.Build(_canvas.transform);

            var treeGO = new GameObject("SkillTreePanel");
            treeGO.transform.SetParent(_canvas.transform, false);
            _skillTree = treeGO.AddComponent<NatureSkillTreePanelUI>();
            _skillTree.OnClosed = () => OnPanelClosed(_skillTree);
            _skillTree.Build(_canvas.transform);
        }

        private void OnPanelClosed(BasePanelUI panel)
        {
            if (_activePanel == panel)
            {
                _activePanel = null;
                ApplyCursorForPanels(false);
            }
        }

        private void Update()
        {
            if (!_insideBase) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame && _activePanel != null && _activePanel.IsOpen)
            {
                CloseActivePanel();
                return;
            }

            if (kb.cKey.wasPressedThisFrame) TogglePanel(_crafting);
            else if (kb.kKey.wasPressedThisFrame) TogglePanel(_skillTree);
            else if (kb.iKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame) TogglePanel(_inventoryPanel);
        }

        public static bool TryCloseActivePanel()
        {
            if (Instance == null || Instance._activePanel == null || !Instance._activePanel.IsOpen)
                return false;
            Instance.CloseActivePanel();
            return true;
        }

        private void TogglePanel(BasePanelUI panel)
        {
            if (panel == null) return;

            if (_activePanel == panel && panel.IsOpen)
            {
                CloseActivePanel();
                return;
            }

            CloseAllPanels(silent: true);
            _activePanel = panel;
            panel.Show();
            ApplyCursorForPanels(true);

            if (panel == _crafting) _crafting.Refresh();
            else if (panel == _inventoryPanel) _inventoryPanel.Refresh();
        }

        private void CloseActivePanel()
        {
            if (_activePanel != null && _activePanel.IsOpen)
                _activePanel.Hide();
            _activePanel = null;
            ApplyCursorForPanels(false);
        }

        public void CloseAllPanels(bool silent = false)
        {
            _crafting?.Hide();
            _inventoryPanel?.Hide();
            _skillTree?.Hide();
            _activePanel = null;
            if (!silent) ApplyCursorForPanels(false);
        }

        private void ApplyCursorForPanels(bool panelOpen)
        {
            if (panelOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (_insideBase)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnPlayerEnterBase()
        {
            _insideBase = true;
            SetHintVisible(true);
        }

        private void OnPlayerExitBase()
        {
            _insideBase = false;
            CloseAllPanels();
            SetHintVisible(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SetHintVisible(bool visible)
        {
            if (_hintRoot != null) _hintRoot.SetActive(visible);
        }

        private void OnInventoryChanged()
        {
            if (_crafting != null && _crafting.IsOpen) _crafting.Refresh();
            if (_inventoryPanel != null && _inventoryPanel.IsOpen) _inventoryPanel.Refresh();
        }
    }
}

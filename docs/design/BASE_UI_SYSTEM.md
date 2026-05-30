# System UI bazy druida + itemy + crafting

**Wersja:** 1.0 (2026-05-30)
**Bazuje na:** `ITEMS_COMBAT_CHESTS_DESIGN.md`, `recon/COMBAT_ITEMS_RECON.md`
**Cel:** Dać graczowi w bazie (`DruidBase`) bezpieczną strefę UI: ekwipunek, crafting i placeholder drzewka Przyrody — w pełni działające w Play Mode, budowane runtime przez kod (jak `PlayerHUD` / `UIFactory`).

---

## 1. Architektura paneli

```
BaseUIManager (MonoBehaviour na graczu)
  ├─ Canvas_BaseUI (sortOrder 8: nad HUD=5, pod Pause=10)
  ├─ hint „C Craft | K Drzewko | I Ekwipunek”  (Faza 4)
  ├─ CraftingPanelUI        : BasePanelUI   (C)
  ├─ InventoryPanelUI       : BasePanelUI   (I / Tab)
  └─ NatureSkillTreePanelUI : BasePanelUI   (K)
```

### 1.1 `BasePanelUI` (abstract, `WPG.UI`)
Wspólny szkielet okna modalnego:
- `Build(Transform canvas)` — buduje ramkę okna (ciemne tło, pasek tytułu, przycisk **×** w prawym górnym rogu, obszar `Body`), następnie woła abstrakcyjne `BuildContent(RectTransform body)`.
- `Show()` / `Hide()` / `Toggle()` / `IsOpen`.
- Hooki `OnShown()` / `OnHidden()` do odświeżania zawartości.
- Przycisk **×** woła `Hide()`. ESC obsługiwany centralnie przez `BaseUIManager` (priorytet panel > pauza).

### 1.2 `BaseUIManager` (`WPG.UI`)
- Tworzony przez `WorldBootstrap` na obiekcie gracza, `Bind(Inventory, PlayerStats, PlayerCombat)`.
- Trzyma jeden wspólny `Canvas` + trzy panele.
- Czyta klawisze **tylko gdy `DruidBase.IsPlayerInside`** (przez statyczne eventy `OnPlayerEnter`/`OnPlayerExit`).
- **Jeden panel naraz** (modalny): otwarcie panelu zamyka pozostałe.
- Zarządza kursorem: panel otwarty → kursor odblokowany/widoczny; brak paneli → zablokowany.
- Statyczne `AnyPanelOpen` — `PauseMenu` i `PlayerCombat` z niego korzystają (ESC i blokada inputu walki).
- Auto-zamknięcie wszystkich paneli przy `OnPlayerExit`.

---

## 2. Flow gracza w bazie

```
Wejście do triggera DruidBase
  → OnPlayerEnter → BaseUIManager._insideBase = true
  → na HUD pojawia się hint „C Craft | K Drzewko | I Ekwipunek”
  ▼
Naciśnięcie C / K / I (Tab)
  → otwarcie odpowiedniego panelu (pozostałe zamknięte)
  → kursor odblokowany, input walki wstrzymany
  ▼
Crafting: klik „Wytwórz” przy przepisie z wystarczającymi materiałami
  → materiały zużyte, produkt w ekwipunku, panel odświeżony
  ▼
× / ESC → zamknięcie panelu (kursor z powrotem zablokowany)
  ▼
Wyjście z triggera DruidBase
  → OnPlayerExit → CloseAll() + ukrycie hintu
```

Real-time: `Time.timeScale` **nie** spada — gracz może chodzić po bazie z otwartym panelem (zgodnie z designem).

---

## 3. Mapowanie klawiszy

| Klawisz | Akcja | Warunek |
|---------|-------|---------|
| `C` | Toggle panelu Crafting | w bazie |
| `K` | Toggle drzewka Przyrody | w bazie |
| `I` / `Tab` | Toggle ekwipunku | w bazie |
| `ESC` | Zamknij aktywny panel (priorytet nad pauzą) | panel otwarty |
| `×` (klik) | Zamknij panel | panel otwarty |
| `1`–`4` | (rezerwacja hotbar — przyszłość) | — |

Brak kolizji: kod używał dotąd LPM/E/Q (walka), E/F (interakcja), R (respawn), Space (skok), ESC (pauza). `C/K/I/Tab` były wolne.

---

## 4. Auto-zamknięcie przy wyjściu z bazy

`DruidBase` emituje statyczne `OnPlayerEnter` / `OnPlayerExit` w `OnTriggerEnter` / `OnTriggerExit` (analogicznie do istniejącego `OnGameSaved`). `BaseUIManager`:
- `OnPlayerEnter` → włącza nasłuch klawiszy + pokazuje hint,
- `OnPlayerExit` → `CloseAll()` + chowa hint + blokuje kursor.

---

## 5. Crafting MVP

3 przepisy zdefiniowane w `ItemDatabase` (item z niepustym `craftInputs` = przepis produkujący ten item):

| Produkt | Składniki | Efekt |
|---------|-----------|-------|
| Mikstura Życia (`health_potion`) | `moss_clump` ×2 | Consumable: +40 HP |
| Amulet Leśnej Many (`mana_amulet`) | `goblin_tooth` ×3, `glow_mushroom` ×1 | Relic: +2 mana regen/s |
| Różdżka Żaru (`flame_wand`) | `iron_shard` ×2, `shaman_totem_fragment` ×1 | Broń: fireball −3 many |

`CraftingPanelUI` koloruje każdy składnik: **zielony** gdy gracz ma wystarczająco, **czerwony** gdy brak. Przycisk „Wytwórz” aktywny tylko gdy wszystkie składniki spełnione (`Inventory.CanCraft`). Po kliknięciu `Inventory.TryCraft` zużywa materiały i dodaje produkt.

Bonusy relikwii/broni są w MVP pasywne — przyznawane gdy item jest w ekwipunku (`Inventory.RecomputeEquipmentBonuses`): relic dodaje `PlayerStats.relicManaRegenBonus`, broń ustawia `PlayerCombat.fireballManaDiscount`.

---

## 6. Integracja z save/load

`SaveData` zyskuje `List<ItemStackSave> { itemId, quantity }`:
- **Zapis:** `GameManager.BuildSaveData` znajduje `Inventory` w scenie i serializuje sloty (`Inventory.ToSaveList`).
- **Wczytanie:** `WorldBootstrap` po zbudowaniu gracza woła `Inventory.LoadFromSave` z `pendingLoadData.inventory`.
- Otwarte panele **nie** są zapisywane (stan UI ulotny) — tylko zawartość ekwipunku.
- **Starter items (nowa gra / pusty ekwipunek):** `moss_clump ×4`, `goblin_tooth ×3`, `glow_mushroom ×1`, `iron_shard ×2`, `shaman_totem_fragment ×1` — pozwala od razu przetestować wszystkie 3 przepisy.

---

## 7. Pliki

**Nowe (`Assets/_Game/Scripts/Items/`, `WPG.Items`):**
- `ItemDefinition.cs`, `ItemDatabase.cs`, `Inventory.cs`, `InventorySlot.cs`, `ItemIconResolver.cs`

**Nowe (`Assets/_Game/Scripts/UI/Base/`, `WPG.UI`):**
- `BaseUIAssets.cs`, `BasePanelUI.cs`, `BaseUIManager.cs`, `CraftingPanelUI.cs`, `InventoryPanelUI.cs`, `NatureSkillTreePanelUI.cs`

**Edytowane (minimalny diff):**
- `World/DruidBase.cs` — statyczne `OnPlayerEnter`/`OnPlayerExit`.
- `World/WorldBootstrap.cs` — dodanie `Inventory` + `BaseUIManager`, starter items / load.
- `Player/PlayerStats.cs` — `relicManaRegenBonus`.
- `Player/PlayerCombat.cs` — `fireballManaDiscount` + blokada inputu gdy `BaseUIManager.AnyPanelOpen`.
- `UI/PauseMenu.cs` — ESC ustępuje panelom bazy.
- `Core/SaveData.cs` — `List<ItemStackSave> inventory`.
- `Core/GameManager.cs` — serializacja ekwipunku w `BuildSaveData`.
- `Core/GameAssetPaths.cs` — `IconHerb`, `IconAmulet` dla itemów.

---

## 8. Test plan (Play Mode)

1. Nowa gra → spawn w bazie → hint widoczny → `C` → panel Crafting z 3 przepisami.
2. `×` lub `ESC` zamyka panel.
3. Wyjście z bazy → panel znika automatycznie, hint znika.
4. `I`/`Tab` → ekwipunek z 16 slotami i starter materiałami.
5. Craft Mikstury Życia (masz moss×4) → `health_potion` w ekwipunku, moss −2.
6. Klik na miksturę w ekwipunku przy obrażeniach → +40 HP, ilość −1.
7. `K` → drzewko Przyrody (placeholder, 3 gałęzie, nody zablokowane).
8. Craft Amuletu → mana regen rośnie; craft Różdżki → koszt fireballa −3.
9. Save (auto co 5 s w bazie) → restart → Kontynuuj → ekwipunek zachowany.
10. 0 errorów kompilacji.

---

## 9. Assety UI

Panele bazy korzystają z tego samego pipeline'u co `PlayerHUD` i `MainMenuBootstrap`:

| Element UI | Źródło | Loader |
|------------|--------|--------|
| Tło okna / ramki | Fantasy Free GUI `GuiPanel` | `BaseUIAssets.PanelSprite` via `GameAssetLoader` |
| Przyciski (×, Wytwórz) | Fantasy Free GUI `GuiButton` | `UIFactory.CreateFantasyButton` / `BaseUIAssets.CreateActionButton` |
| Ramka slotu itemu | Fantasy Free GUI `GuiIconFrame` | `BaseUIAssets.IconFrameSprite` |
| Hint na dole ekranu | `GuiPanel` (półprzezroczysty) | `BaseUIManager.BuildHint` |

**Ikony itemów** (`ItemIconResolver` → Modern RPG icons):

| Item id | Sprite |
|---------|--------|
| `health_potion` | `IconHeal` |
| `flame_wand`, `shaman_totem_fragment` | `IconFireball` |
| `mana_amulet` | `IconAmulet` (fallback: `IconHeal`) |
| `goblin_tooth`, `iron_shard` | `IconMelee` |
| `moss_clump`, `glow_mushroom` | `IconHerb` |

**Fallback:** gdy paczka nie jest zaimportowana (patrz `docs/ASSETS_IN_PROJECT.md`), UI używa kolorów z `ItemDefinition.iconColor` na ramce slotu — bez surowych Unity defaults tam, gdzie `UIFactory` zapewnia styl druida (ciemna zieleń, złoto-kremowe teksty).

**Drzewko Przyrody:** gałęzie używają `IconFireball`, `IconHeal`, `IconMelee` jako ikon zablokowanych nodów.

Nowe ścieżki w `GameAssetPaths.cs`: `IconHerb`, `IconAmulet` (+ aliasy Resources).


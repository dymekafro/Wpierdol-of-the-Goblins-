# Rozpoznanie: walka, itemy, broń, interakcje

**Data:** 2026-05-30  
**Agent:** explore (readonly)  
**Status:** Gotowe do projektowania systemów

---

## 1. Co już istnieje

### Walka gracza (`PlayerCombat`)
- **LPM** — melee (OverlapSphere, zasięg 2.2, cooldown 0.6s)
- **E** — fireball (15 many) *lub* interakcja gdy `HasReadyInteractable`
- **Q** — heal (25 many, +35 HP)
- Skalowanie: STR→melee, INT→spell power, END→HP, MANA→mana pool

### Wrogowie
- `GoblinStormtrooper` (30 HP, 5 dmg melee)
- `GoblinArcher` (20 HP, 8 dmg strzała, kite AI)
- `GoblinShamanElite` (55 HP, 9 dmg — bez magii support)
- `Totem` — buff sojuszników, zniszczenie → `CampState.Cleared`
- 5 obozów, stany Active → Cleared → Captured

### Interakcje
- `IInteractable` — `CampInteractable`, `PowerSite`
- `InteractionDetector` — OverlapSphere r=3.2, **E lub F**
- `DruidBase` — heal, auto-save co 5s, respawn (bez UI craft/drzewko)

### UI
- `PlayerHUD` — HP/mana, cooldowny, prompt interakcji, ekran śmierci
- **Brak:** inventory, hotbar itemów, crafting UI, skill tree UI

### Save (`SaveData`)
- Zapisywane: atrybuty, pozycja, HP/mana, stany obozów, power sites
- **Nie zapisywane:** inventory, broń, skrzynie, skill nodes

---

## 2. Co brakuje vs ARPG

| Obszar | Stan | Docelowo |
|--------|------|----------|
| Inventory | Brak | 16–24 sloty, stackowanie |
| Loot / drop | Brak | Z goblinów, skrzyń, totemów |
| Broń gracza | Staff wizualny, stałe umiejętności | Slot broni, staty, wymiana |
| Skrzynie | Brak | Interakcja E, loot table, save stanu |
| Crafting | Tylko w docs | Stacja w bazie, przepisy |
| Skill tree | Brak | Ogień/Życie/Kształt w bazie |
| Combat depth | Int dmg, brak uników | Combo, typy dmg, stagger |
| UI bazy | Brak paneli | Craft/drzewko/inventory + **X zamknij** |

---

## 3. Punkty integracji

| System | Hook |
|--------|------|
| Skrzynie | `IInteractable`, `WorldGenerator.BuildCamp`, `SaveData.openedChestIds` |
| Pickup | `GoblinBase.OnDeath`, `Totem.OnDestroyed`, `IInteractable` |
| Broń | `PlayerCombat`, `handMount`, `CharacterAnimDriver.TriggerAttack` |
| Crafting | `DruidBase` trigger, nowy `CraftingStation : IInteractable` |
| Inventory | `InvectorPlayerBuilder` / `PlayerBuilder`, `GameManager.BuildSaveData` |

---

## 4. Proponowana struktura plików

```
Assets/_Game/Scripts/
  Items/
    ItemDefinition.cs      (ScriptableObject)
    Inventory.cs
    LootTable.cs
    WorldPickup.cs
  Items/Interactables/
    ChestInteractable.cs
    CraftingStation.cs
  Combat/
    WeaponDefinition.cs    (extends ItemDefinition)
    DamageInfo.cs          (opcjonalnie, faza 2)
```

---

## 5. Kolejność implementacji

1. `ItemDefinition` + `Inventory` + HUD slotów
2. `LootTable` + drop z goblinów
3. `ChestInteractable` + save stanu
4. `CraftingStation` + 2 przepisy
5. Broń jako item modyfikujący `PlayerCombat`
6. UI bazy (craft/drzewko) z przyciskiem **X**
7. Nature skill tree

---

## Następny krok

→ `docs/design/ITEMS_COMBAT_CHESTS_DESIGN.md` — pełna specyfikacja gameplay.

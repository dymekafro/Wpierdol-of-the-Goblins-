# Projekt rozgrywki: skrzynie, itemy, broń, walka, UI bazy

**Wersja:** 1.0 (2026-05-30)  
**Bazuje na:** `GAME_CONCEPT.md`, `WORLD_DESIGN.md`, `recon/COMBAT_ITEMS_RECON.md`  
**Cel:** Dopracować pętlę „wyprawa → łup → baza → upgrade → trudniejszy obóz” tak, żeby decyzje w bazie miały znaczenie.

---

## 1. Filozofia designu

Gracz to **druid-strażnik**, nie wojownik. Walka ma być:
- **Taktyczna** — wybór broni/czarów przed wyprawą, nie spam LPM
- **Ryzykowna poza bazą** — mana i HP są ograniczone; powrót do Sady to strategia
- **Nagradzająca eksplorację** — skrzynie, miejsca mocy, drop z goblinów dają **materiały** i **relikwie**, nie tylko liczby

**Zasada:** Każdy obóz Cleared daje coś **konkretnego do ekwipunku lub craftingu**. Captured daje **stały bonus** + odblokowanie skrzyni obozowej.

---

## 2. Skrzynie na mapie

### 2.1 Typy skrzyń

| Typ | ID prefiksu | Gdzie | Kiedy dostępna | Zawartość |
|-----|-------------|-------|----------------|-----------|
| **Skrzynia obozowa** | `chest_camp_{campId}` | Przy totemie / ognisku | Po `Cleared` (loot jednorazowy) | Surowce + szansa na broń/relic |
| **Skrzynia podbita** | `chest_captured_{campId}` | W centrum podbitego obozu | Po `Captured` | Rzadsze składniki, 1× relikwia obozu |
| **Skrzynia leśna** | `chest_world_{id}` | Pobocznie między obozami | Od razu (zamknięta, E otwórz) | Mikstury, grzyby, drewno |
| **Skrzynia miejsca mocy** | `chest_power_{siteId}` | Przy Kamiennych Kręgach | Po aktywacji PowerSite | Relikwia lub punkt Przyrody |

### 2.2 Interakcja

- **E** — otwórz skrzynię (prompt: „E — Otwórz skrzynię”)
- Animacja: wieko otwarte (obrót transform lub swap mesh), dźwięk `SfxChestOpen`
- Loot: **panel mini-loot** (3–5 slotów) pojawia się na 5 s lub do kliknięcia **„Weź wszystko"**
- Po zebraniu: skrzynia pusta, wizualnie otwarta, **nie respawnuje** (save `openedChestIds`)

### 2.3 Loot table per obóz (przykład MVP)

**Pierwsza Zagroda** (`goblin_camp_first_clearing`):
| Item | Waga | Ilość |
|------|------|-------|
| `goblin_tooth` (materiał) | 60% | 2–4 |
| `moss_clump` (materiał) | 40% | 1–3 |
| `crude_dagger` (broń, common) | 15% | 1 |
| `nature_point` (waluta drzewka) | 100% | 1 |

**Korzenie Zagubione** (pierścień 2):
| Item | Waga | Ilość |
|------|------|-------|
| `glow_mushroom` | 50% | 1–2 |
| `iron_shard` | 30% | 1–2 |
| `shaman_totem_fragment` (craft rare) | 10% | 1 |
| `nature_point` | 100% | 2 |

### 2.4 Wizual MVP

- Placeholder: `Cube` brązowy 1×0.6×0.8 + emisja złota na krawędziach (loot hint)
- Docelowo: prefab z RPG Dungeon Kit / Kenney chest
- Bioluminescencja: skrzynie w pierścieniu 2+ mają delikatny niebieski point light

---

## 3. Mechanika itemów

### 3.1 Typy itemów

```csharp
enum ItemType { Material, Consumable, Weapon, Relic, Quest, NaturePoint }
enum ItemRarity { Common, Uncommon, Rare, Epic }
```

| Typ | Stack | Użycie | Przykład |
|-----|-------|--------|----------|
| **Material** | 99 | Crafting | `goblin_tooth`, `moss_clump`, `glow_mushroom` |
| **Consumable** | 20 | Hotbar 1–4, instant | Mikstura HP, eliksir many |
| **Weapon** | 1 | Slot broni, zmienia combat | `crude_dagger`, `druid_staff`, `flame_wand` |
| **Relic** | 1 | Slot pasywny (max 2) | +5% spell power, +regen many w lesie |
| **NaturePoint** | — | Auto-add do puli drzewka | Niewidoczny w inventory |

### 3.2 Inventory

- **16 slotów** na start; +4 po podbiciu 3 obozów (nagroda progresji)
- **Hotbar:** sloty 1–4 (klawisze `1`–`4`) — consumables + szybka broń
- **Waga:** brak w MVP (END wpływa na max stack materiałów w przyszłości)
- **Pickup:** E na `WorldPickup` LUB auto-collect do inventory przy otwarciu skrzyni

### 3.3 ItemDefinition (ScriptableObject)

Pola:
- `id`, `displayNamePL`, `descriptionPL`, `icon` (sprite z Modern RPG icons)
- `itemType`, `rarity`, `stackMax`
- `weaponStats` (opcjonalnie): `baseDamage`, `attackSpeed`, `range`, `damageType`
- `consumableEffect` (opcjonalnie): `healAmount`, `manaAmount`, `buffDuration`
- `relicEffect` (opcjonalnie): enum buff + wartość

### 3.4 Drop z goblinów (per kill, 30% szansa)

| Goblin | Drop |
|--------|------|
| Szturmowiec | `goblin_tooth` (1), 5% `crude_dagger` |
| Łucznik | `goblin_tooth` (1), `arrow_bundle` (craft) |
| Elita/Szaman | `shaman_totem_fragment` (1), 20% `glow_mushroom` |

Drop jako `WorldPickup` (świecąca kula) lub bezpośrednio do inventory jeśli w pobliżu.

---

## 4. Broń

### 4.1 Filozofia druida

Druid **nie jest** mistrzem miecza. Broń to **narzędzie**, nie główna tożsamość:
- **Kij druida** (start) — słabe melee, bonus do czarów (+10% spell power)
- **Sztylet goblina** (loot) — szybkie melee, niskie dmg, do dobijania
- **Różdżka płomieni** (craft) — melee = touch spell, E = fireball z mniejszym kosztem many
- **Laska życia** (craft + drzewko Życie) — melee leczy 5 HP, Q heal silniejszy

### 4.2 Tabela broni MVP

| ID | Nazwa PL | Dmg | Speed | Range | Specjalne |
|----|----------|-----|-------|-------|-----------|
| `druid_staff` | Kij Strażnika | 8 | 0.7/s | 2.0 | +10% spell power |
| `crude_dagger` | Sztylet Goblinów | 12 | 1.2/s | 1.6 | +5% crit (future) |
| `flame_wand` | Różdżka Żaru | 6 | 0.5/s | 2.5 | Fireball koszt −3 many |
| `nature_club` | Maczuga Korzeni | 18 | 0.4/s | 2.2 | Stagger (future) |

### 4.3 Wymiana broni

- **Tab** lub **I** — inventory panel
- Klik na broń → „Wyposaż" → mount prefab na `handMount` (`WorldAssetPlacer`)
- `PlayerCombat` czyta staty z `EquippedWeapon` zamiast stałych
- Animacja: ten sam trigger `Attack`, różny `swingDuration` per broń

### 4.4 Broń goblinów (wizual)

Już częściowo: `goblin_stonesword.prefab` na szturmowcu.  
Docelowo: runtime load przez `GameAssetRegistry`, nie `#if UNITY_EDITOR`.

---

## 5. System walki — rozbudowany

### 5.1 Warstwy combatu

```
[Input] → [Weapon/Spell selection] → [Wind-up] → [Active frames] → [Recovery] → [Cooldown]
                ↓                           ↓
         [Mana/Stamina cost]          [Hit detection] → [Damage calc] → [IDamageReceiver]
                                              ↓
                                    [Feedback: VFX, SFX, DamageNumber, hit flash]
```

### 5.2 Nowe mechaniki (fazy)

**Faza A — MVP+ (natychmiast po itemach):**

| Mechanika | Opis | Implementacja |
|-----------|------|---------------|
| **Broń zmienia dmg** | `finalDmg = (baseWeapon + STR×2) × modifiers` | `PlayerCombat` + `WeaponDefinition` |
| **Consumable z hotbar** | Klawisz 1–4 używa mikstury | `Inventory.UseHotbar(slot)` |
| **Hit feedback** | Flash + `SfxHit` + liczba obrażeń u gracza też | `PlayerStats` + `DamageNumber` |
| **Telegraph ataku goblina** | 0.3s „wind-up" przed dmg melee | `GoblinStormtrooper` delay |

**Faza B — po 3 obozach:**

| Mechanika | Opis |
|-----------|------|
| **Typy obrażeń** | Physical, Fire, Nature — gobliny odporności per typ (łucznik: −Fire) |
| **Stagger** | Maczuga Korzeni na 0.5s zatrzymuje goblina |
| **Unik** | Shift + DEX — i-frames 0.4s, koszt 10 many (alternatywa: cooldown 2s) |
| **Combo melee** | LPM ×3 — trzeci cios ×1.5 dmg, dłuższy recovery |

**Faza C — z drzewkiem Przyrody:**

| Gałąź | Combat bonus |
|-------|--------------|
| **Ogień** | Fireball AoE +50%, trail burn (DoT 3s) |
| **Życie** | Heal leczy sojuszników (future), thorns 5 dmg przy otrzymaniu |
| **Kształt** | Transformacja: +30% move speed, melee → pazury (15 dmg, szybkie) |

### 5.3 Balans pierwszego obozu

Gracz startowy (END 6, STR 4 po bonusach druida):
- HP ~110, melee ~13 dmg, fireball ~44 dmg
- 2 szturmowcy (30 HP) + 1 łucznik (20 HP) = 80 HP wrogów
- **Bez consumables:** trudne ale możliwe (2–3 podejścia)
- **Ze skrzynią obozową (mikstura HP):** fair first clear

### 5.4 Totem jako cel walki

- Totem: 120 HP, nie rusza się, emituje pulse co 8s (buff goblinów w promieniu)
- Strategia: zabić łucznika pierwszego → szturmowcy → totem
- Po zniszczeniu: VFX explosion + **gwarantowany** `nature_point` + otwarcie skrzyni obozowej

---

## 6. UI bazy druida — panel z **X**

### 6.1 Koncepcja

Baza to **bezpieczna strefa UI**. Gracz może otworzyć panele **tylko w `DruidBase`** (trigger):

| Panel | Klawisz | Zawartość |
|-------|---------|-----------|
| **Crafting** | `C` | Lista przepisów, wymagane materiały, przycisk „Wytwórz" |
| **Drzewko Przyrody** | `K` | 3 gałęzie, nody, punkty Przyrody |
| **Ekwipunek** | `I` / `Tab` | Sloty, broń, relikwie, hotbar |
| **Save manual** | `E` na krysztale | Toast „Zapisano grę" |

### 6.2 Przycisk **X** (zamknij)

Każdy panel modalny ma:
- **Prawy górny róg:** przycisk **×** (48×48 px, hover jaśniejszy)
- **ESC** — zamyka aktywny panel (jak PauseMenu, ale priorytet: panel > pauza)
- **Klik poza panelem** — opcjonalnie zamyka (MVP: tylko X i ESC)

```
┌─────────────────────────────────────────┐
│  Crafting — Sady Ostatniego Strażnika  [×] │
├─────────────────────────────────────────┤
│  [Mikstura Życia]  wymaga: moss×2       │
│  [Amulet Many]     wymaga: tooth×3      │
└─────────────────────────────────────────┘
```

Implementacja:
- `BasePanelUI` — abstract: `Show()`, `Hide()`, `OnCloseButton()`, subskrypcja `DruidBase.IsPlayerInside`
- Wyjście ze strefy bazy → **auto-zamknij** wszystkie panele
- `Time.timeScale` **nie** spada (crafting w real-time, gracz może chodzić po bazie z otwartym inventory)

### 6.3 Crafting — przepisy MVP

| Przepis | Składniki | Efekt |
|---------|-----------|-------|
| Mikstura Życia | `moss_clump` ×2 | +40 HP (consumable) |
| Amulet Leśnej Many | `goblin_tooth` ×3, `glow_mushroom` ×1 | Relic: +2 mana regen/s |
| Różdżka Żaru | `iron_shard` ×2, `shaman_totem_fragment` ×1 | Broń: `flame_wand` |

Stacja: obiekt przy ołtarzu w bazie, prompt „C — Crafting" gdy w strefie.

---

## 7. Pętla rozgrywki (zaktualizowana)

```
BAZA (heal, save)
  │ wybór: broń, consumables, craft, drzewko [X zamknij]
  ▼
EKSPLORACJA (skrzynie leśne, power sites)
  ▼
OBIÓZ Active (walka taktyczna, drop z goblinów)
  ▼
Cleared → skrzynia obozowa + nature_point
  ▼
(powrót do bazy — przetwórz łupy)
  ▼
Captured → skrzynia podbita + stały buff obozu
  ▼
następny pierścień
```

---

## 8. Save — rozszerzenie

```csharp
// SaveData — nowe pola
public List<ItemStackSave> inventory;      // { itemId, quantity }
public string equippedWeaponId;
public List<string> equippedRelicIds;    // max 2
public List<string> openedChestIds;
public int naturePoints;
public List<string> unlockedSkillNodes;
public int[] hotbarSlots;                // itemId index per slot 0-3
```

---

## 9. Fazy implementacji (mapowanie do IMPLEMENTATION_PLAN)

| Faza | Scope | Effort |
|------|-------|--------|
| **F8a** | `ItemDefinition`, `Inventory`, `WorldPickup`, HUD 4 sloty hotbar | M |
| **F8b** | `ChestInteractable`, loot tables, save `openedChestIds` | M |
| **F8c** | `WeaponDefinition`, equip, `PlayerCombat` refactor | M |
| **F8d** | `BasePanelUI` + Crafting + **przycisk X** + ESC | M |
| **F8e** | Combat Faza A (broń dmg, telegraph, consumables) | S |
| **F8f** | `NatureSkillTreeUI` w bazie | L |
| **F8g** | Combat Faza B (typy dmg, unik, combo) | L |

**Kolejność:** F8a → F8b → F8c → F8d → F8e → F8f → F8g

---

## 10. Test plan (30 min playtest)

1. Nowa gra → baza → otwórz Crafting (`C`) → **X** zamyka panel
2. Wyjdź z bazy → panel się zamyka automatycznie
3. Pierwsza Zagroda → zabij goblinów → totem → skrzynia obozowa → loot do inventory
4. Powrót → craft Mikstury Życia → hotbar slot 1
5. Drugie podejście do obozu z miksturą → łatwiejsze
6. Podbij obóz → skrzynia captured → equip `crude_dagger` → dmg melee wyższy
7. Save → quit → Continue → inventory i otwarte skrzynie zachowane

---

## Powiązane dokumenty

- `docs/GAME_CONCEPT.md` — wizja ogólna
- `docs/WORLD_DESIGN.md` — placement skrzyń per obóz
- `docs/recon/COMBAT_ITEMS_RECON.md` — stan kodu
- `docs/plans/GOBLIN_ANIM_FIX_PLAN.md` — naprawa animacji (blokuje „żywe" gobliny)

# Mapa świata — Magiczny Ciemny Las

Jedna ciągła scena **`World.unity`**. Brak osobnych leveli. Centrum = **baza druida**; trudność rośnie pierścieniami na zewnątrz.

---

## Zasady layoutu

1. **Baza zawsze w centrum** — punkt odniesienia, respawn, save
2. **Pierścienie trudności** — im dalej od bazy, tym więcej wrogów i silniejszy skład
3. **Obozy goblinów** — główne cele; stany: Active → Cleared → Captured
4. **Miejsca mocy** — poboczne nagrody między obozami
5. **Granica lasu** — na MVP niewidzialna ściana / mgła; w przyszłości quest wyjścia

---

## ASCII mapa (orientacyjna)

```
                    [future: Przeszła Brama]
                              ↑
    ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
    ~   Pierścień 3 (trudny)                  ~
    ~       [Obóz: Cienista Polana]           ~
    ~              \                          ~
    ~    [Miejsce mocy: Kamienny Krąg]        ~
    ~               \                         ~
    ~ ~ ~ ~ ~ ~ ~ ~ ~\~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
    ~   Pierścień 2   \                       ~
    ~   [Obóz:        \  [Obóz:              ~
    ~    Mchy Żaru]     \  Korzenie           ~
    ~                    \  Zagubione]        ~
    ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~\~ ~ ~ ~ ~ ~ ~ ~ ~ ~
    ~   Pierścień 1       \                   ~
    ~   [Obóz: Pierwsza    \                  ~
    ~    Zagroda] ──────────┼── [Baza druida] ~
    ~                       /      ★ HUB      ~
    ~              [start] /                  ~
    ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ / ~ ~ ~ ~ ~ ~ ~ ~ ~
    ~   Pierścień 0 (bezpieczny) — tylko baza ~
    ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~
```

★ = spawn gracza, save, craft, drzewko Przyrody

---

## Baza druida (centrum)

| Pole | Wartość |
|------|---------|
| **ID** | `camp_druid_base` |
| **Nazwa PL** | Sady Ostatniego Strażnika |
| **Pierścień** | 0 (bezpieczny) |
| **Funkcje** | Save, heal, crafting, drzewko, respawn |

Wizual MVP: otwarta polana, świecące grzyby, prosty płot z gałęzi, ołtarz kamienny.

---

## Obozy goblinów (MVP: 3–5 lokacji)

### 1. Pierwsza Zagroda (MVP — Faza 1)

| Pole | Wartość |
|------|---------|
| **ID** | `goblin_camp_first_clearing` |
| **Nazwa PL** | Pierwsza Zagroda |
| **Pierścień** | 1 |
| **Skład MVP** | 2× szturmowiec, 1× łucznik, 1× totem |
| **Cel** | Tutorial walki i oczyszczenia |

### 2. Mchy Żaru (Faza 3)

| Pole | Wartość |
|------|---------|
| **ID** | `goblin_camp_ember_moss` |
| **Nazwa PL** | Mchy Żaru |
| **Pierścień** | 2 |
| **Skład** | 3× szturmowiec, 2× łucznik |
| **Bonus Captured** | Stały +regeneracja many w promieniu obozu |

### 3. Korzenie Zagubione (Faza 3)

| Pole | Wartość |
|------|---------|
| **ID** | `goblin_camp_lost_roots` |
| **Nazwa PL** | Korzenie Zagubione |
| **Pierścień** | 2 |
| **Skład** | 2× szturmowiec, 2× łucznik, 1× szaman (gdy gotowy) |
| **Bonus Captured** | Skrzynia craftingowa (rzadkie składniki) |

### 4. Cienista Polana (Faza 3 / trudny)

| Pole | Wartość |
|------|---------|
| **ID** | `goblin_camp_shade_glade` |
| **Nazwa PL** | Cienista Polana |
| **Pierścień** | 3 |
| **Skład** | Pełny mix + silniejszy totem |
| **Bonus Captured** | Odblokowanie skrótu do pierścienia 3 |

### 5. Leśna Nora (opcjonalny MVP+)

| Pole | Wartość |
|------|---------|
| **ID** | `goblin_camp_forest_den` |
| **Nazwa PL** | Leśna Nora |
| **Pierścień** | 1–2 |
| **Rola** | Mały obóz poboczny, szybki loot |

---

## Miejsca mocy (las)

| ID | Nazwa PL | Pierścień | Efekt MVP |
|----|----------|-----------|-----------|
| `power_site_stone_circle` | Kamienny Krąg | 2 | +1 INT (jednorazowo) |
| `power_site_glow_shrine` | Kapliczka Światłości | 1 | Odblokowanie czaru Życia (future / Faza 4) |

---

## Punkty future (oznaczone na mapie)

| ID | Nazwa PL | Opis |
|----|----------|------|
| `future_gate_faded` | Przeszła Brama | Quest wyjścia z lasu — **nie implementować w MVP** |
| `future_region_swamp` | Wschód: Bagna | Marker granicy następnego regionu |
| `future_quest_hermit` | Chata Pustelnika | NPC questowy — placeholder totem / tabliczka |

W edytorze Unity: puste GameObject + tag `FutureContent` + opis w Inspectorze.

---

## Pierścienie trudności

| Pierścień | Odległość od bazy | Zagrożenie |
|-----------|-------------------|------------|
| 0 | 0–30 m | Brak wrogów |
| 1 | 30–80 m | 1 obóz, słabi gobliny |
| 2 | 80–150 m | 2 obozy, mieszane składy |
| 3 | 150 m+ | Najtrudniejszy obóz, mgła gęstsza |

---

## Implementacja w Unity

1. Pusty parent `World_Layout` w `World.unity`
2. Dzieci: `DruidBase_Zone`, `GoblinCamp_FirstClearing`, …
3. Każdy obóz: prefab z `GoblinCamp` + `campId` zgodne z tabelą
4. Nawigacja: brak minimapy w MVP — gracz orientuje się po świetle bazy i bioluminescencji

---

## Powiązanie z kodem

| Lokacja | `campId` / komponent |
|---------|----------------------|
| Baza | `DruidBase` na strefie trigger |
| Obozy | `GoblinCamp.campId` = ID z tabel |

Zobacz też: `docs/GAME_CONCEPT.md`, `docs/MVP_ROADMAP.md`.

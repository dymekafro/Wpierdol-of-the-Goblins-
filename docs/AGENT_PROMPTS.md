# Prompty dla subagentów — Wpierdol of the Goblins

Gotowe prompty do uruchamiania w Cursorze (Task / subagent). Każdy prompt zakłada repozytorium Unity w `Assets/_Game/` i **jedną scenę** `World.unity` (otwarty las, bez leveli).

**Przed startem:** przeczytaj `docs/GAME_CONCEPT.md`, `docs/WORLD_DESIGN.md`, `docs/MVP_ROADMAP.md`.

---

## Agent: Baza druida (hub)

### Cel
Zaimplementować **bazę druida** w `World.unity`: bezpieczna strefa, save point, heal on enter, respawn, placeholdery craftingu i drzewka.

### Pliki do edycji
- `Assets/_Game/Scripts/Core/DruidBase.cs` (rozszerzenie stubu)
- Nowe: `SavePoint.cs`, `CraftingStation.cs` (w `Core/` lub `World/`)
- `Assets/_Game/Scripts/Core/GameData.cs` — ostatnia pozycja save, flaga „w bazie”
- `Assets/_Game/Scripts/Player/PlayerStats.cs` — heal / restore mana
- Scena `World.unity` — collider trigger strefy bazy (centrum mapy)

### Kryteria ukończenia
- [ ] Trigger `DruidBase`: gracz w strefie = brak spawnu wrogów (MVP: tag/layer)
- [ ] Wejście: opcjonalny heal HP/many (konfigurowalny %)
- [ ] Save point: zapis JSON do `persistentDataPath` (pozycja, atrybuty, stany obozów)
- [ ] Respawn po śmierci w punkcie spawn bazy
- [ ] Interakcja: placeholder UI craftingu i drzewka (panel lub log)
- [ ] Debug: „W bazie druida” / „Zapisano grę”

### Zależności
- **Agent System obozów** — zapis stanów `GoblinCamp`
- **Agent Drzewko Przyrody** — pełne UI drzewka w bazie
- **Agent Loot i crafting** — przepisy

---

## Agent: System obozów goblinów

### Cel
Prefab i logika obozów: stany **Active / Cleared / Captured**, totem, eventy, integracja z `GameData`.

### Pliki do edycji
- `Assets/_Game/Scripts/Enemies/GoblinCamp.cs` (rozszerzenie stubu)
- Nowe: `Totem.cs`, `CampCaptureInteractable.cs`
- `Assets/_Game/Scripts/Core/GameData.cs` — słownik `campId → CampState`
- Prefab: `Assets/_Game/Prefabs/World/GoblinCamp.prefab`
- `World.unity` — placement wg `docs/WORLD_DESIGN.md` (`goblin_camp_first_clearing`)

### Kryteria ukończenia
- [ ] `GoblinCampState`: Active, Cleared, Captured
- [ ] `OnCampStateChanged` — subskrypcja z UI / audio
- [ ] Active: spawn wrogów; Cleared: totem zniszczony, nagroda
- [ ] Captured: brak respawnu, bonus (konfig ScriptableObject lub pole)
- [ ] Totem ma HP; zniszczenie → `SetState(Cleared)`
- [ ] Interakcja w Cleared → `SetState(Captured)`
- [ ] Zapis stanu per `campId` w save

### Zależności
- **Agent Typy goblinów** — prefaby w obozie
- **Agent Baza druida** — punkty Przyrody w nagrodzie
- **Agent Loot** — skrzynia przy totemie

---

## Agent: Otwarty świat / World (placeholder streaming)

### Cel
Layout **jednej mapy** lasu w `World.unity`: teren, pierścienie trudności, mgła, bioluminescencja, granice — **bez** osobnych scen levelowych.

### Pliki do edycji
- `World.unity` — teren, światło, post-processing mgły (URP)
- Nowe (opcjonalnie): `WorldBounds.cs`, `DifficultyRing.cs` (promień od `DruidBase`)
- `docs/WORLD_DESIGN.md` — aktualizacja jeśli zmienisz placement

### Kryteria ukończenia
- [ ] Teren placeholder (mesh/plane, kolory lasu)
- [ ] Baza w centrum; pierwszy obóz w pierścieniu 1 (odległość ~50–80 m)
- [ ] Kill plane / niewidzialne ściany
- [ ] Oświetlenie: ciemny ambient + świecące point lights (grzyby)
- [ ] Placeholdery `FutureContent` na granicy (Przeszła Brama)
- [ ] **Nie** tworzyć scen `Level_01` — tylko `World`

### Zależności
- **Agent Baza druida** — pozycja centrum
- **Agent System obozów** — pozycje obozów

### Future (nie MVP)
- Chunk loading / `WorldStreamingManager` — tylko komentarz lub pusty stub

---

## Agent: System druida i transformacji

### Cel
Mana, czary Ogień/Życie, placeholder transformacji (Kształt), HUD.

### Pliki do edycji
- `Assets/_Game/Scripts/Player/PlayerSkillManager.cs`, `PlayerStats.cs`
- Nowe: `DruidSpellCaster.cs`, `DruidTransformController.cs`
- `FireballProjectile.cs`, `PlayerHUD.cs`, `SkillCooldownUI.cs`

### Kryteria ukończenia
- [ ] Mana z atrybutu MANA; regeneracja
- [ ] Min. 2 czary: obrażenia + leczenie
- [ ] Placeholder transformacji (cooldown, bonus DEX)
- [ ] HUD many i cooldownów

### Zależności
- **Agent Drzewko Przyrody** — odblokowanie czarów (MVP: hardcode OK)

---

## Agent: Drzewko Przyrody (skill tree)

### Cel
UI i logika w **bazie druida**: gałęzie Ogień / Życie / Kształt, punkty Przyrody.

### Pliki do edycji
- Nowe: `NatureSkillTree.cs`, `NatureSkillNode.cs`, `NatureSkillTreeUI.cs`
- `GameData.cs` — punkty Przyrody, odblokowane nody
- Otwarcie UI z `DruidBase` (przycisk / interakcja)

### Kryteria ukończenia
- [ ] Min. 3 nody (po 1 na gałąź)
- [ ] Spędzanie punktów z obozów
- [ ] Wpływ na `PlayerSkillManager`

### Zależności
- **Agent Baza druida**
- **Agent System druida**

---

## Agent: Typy goblinów (AI)

### Cel
Archetypy AI: szturmowiec, łucznik (+ później szaman, saper).

### Pliki do edycji
- `EnemyAI.cs` — strategia lub podklasy
- Nowe: `GoblinStormtrooperAI.cs`, `GoblinArcherAI.cs`, …
- Prefabs w `Assets/_Game/Prefabs/Enemies/`

### Kryteria ukończenia
- [ ] Szturmowiec: chase + melee
- [ ] Łucznik: dystans + projectile
- [ ] Działają w prefabie obozu testowego

### Zależności
- **Agent System obozów**

---

## Agent: Loot i crafting

### Cel
Drop, ekwipunek, crafting w bazie.

### Pliki do edycji
- `ItemData.cs`, `InventoryManager.cs`, `Chest.cs`
- Nowe: `CraftingRecipe.cs`, `CraftingManager.cs`, `CraftingUI.cs`

### Kryteria ukończenia
- [ ] Drop z goblinów
- [ ] 2 przepisy; crafting w `DruidBase`
- [ ] Relikwia ScriptableObject (opcjonalnie)

### Zależności
- **Agent Baza druida**

---

## Agent: Miejsca mocy (opcjonalny, po MVP core)

### Cel
Interaktywne punkty w lesie (atrybut, czar, relikwia).

### Pliki do edycji
- Nowe: `PowerSite.cs`, `PowerSiteInteractable.cs`
- `GameData.cs` — odwiedzone ID

### Kryteria ukończenia
- [ ] Trigger + „Naciśnij E”
- [ ] Jednorazowy bonus; zapis w GameData

### Zależności
- **Agent Otwarty świat** — placement wg `WORLD_DESIGN.md`

---

## Kolejność rekomendowana

1. **Otwarty świat** — layout `World.unity`, baza + pierwszy obóz (placeholdery)
2. **Baza druida** — trigger, heal, save, respawn
3. **Typy goblinów** → **System obozów** (Active → Cleared → Captured)
4. **System druida** / czary
5. **Loot i crafting** w bazie
6. **Drzewko Przyrody** + pełny save/load
7. Kolejne obozy (faza 3 roadmapy)
8. Miejsca mocy, regiony future

---

## Szablon ogólny

```
Pracujesz w repozytorium Unity: [ścieżka]
Projekt: Wpierdol of the Goblins — otwarty Magiczny Ciemny Las, druid vs gobliny.
Jedna mapa: World.unity. Baza w centrum, obozy goblinów w pierścieniach. NIE twórz leveli.
Przeczytaj docs/GAME_CONCEPT.md, docs/WORLD_DESIGN.md, docs/MVP_ROADMAP.md.
Konwencje: C# po angielsku, UI gracza po polsku.
Minimalny scope — działający increment.
Nie commituj bez pytania.
```

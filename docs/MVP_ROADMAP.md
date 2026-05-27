# MVP Roadmap — Wpierdol of the Goblins

Fazy pod **otwarty las**, **bazę druida** i **pętlę obozów goblinów**. Jedna scena `World.unity` — bez leveli. Odznaczaj checkboxy w miarę postępów.

---

## Faza 0: Fundament projektu

- [x] Dokumentacja koncepcyjna (`docs/GAME_CONCEPT.md`)
- [x] Mapa świata (`docs/WORLD_DESIGN.md`)
- [x] Prompty agentów (`docs/AGENT_PROMPTS.md`)
- [x] Menu główne — scena + skrypty
- [x] MainMenu — test w **Play mode** (przyciski UI nie działają w trybie edycji sceny)
- [x] `CharacterType.Druid` + bonusy atrybutów
- [x] Build Settings — sceny w buildzie
- [x] Stuby: `DruidBase.cs`, `GoblinCamp.cs`
- [ ] README projektu (opcjonalnie)

---

## Faza 1: Baza druida + pierwszy obóz w World.unity

**Cel:** Działająca pętla w jednej scenie — wyjście z bazy, dotarcie do obozu, powrót.

- [ ] `World.unity` — layout lasu placeholder (teren, mgła, bioluminescencja kolorystyczna)
- [ ] **Baza druida** — strefa trigger (`DruidBase`): bezpieczna, heal on enter, save point (placeholder log)
- [ ] Spawn gracza w bazie; respawn po śmierci w bazie
- [ ] **Jeden obóz goblinów** (`GoblinCamp`, stan Active) w pierścieniu 1
- [ ] 1 szturmowiec + 1 łucznik w obozie
- [ ] Totem z HP — zniszczenie → stan **Cleared**
- [ ] Podstawowy HUD (HP/mana)
- [ ] Test: baza → obóz → zniszczenie totemu → powrót do bazy (< 15 min)

---

## Faza 2: Podbijanie obozu + wizualna zmiana

**Cel:** Po Cleared gracz może **podbić** obóz; wizualna różnica Active vs Captured.

- [ ] Przejście `Cleared` → `Captured` (interakcja E / ritual placeholder)
- [ ] Event `OnCampStateChanged` — podpięcie pod UI / log
- [ ] Wizual: zmiana materiałów, zgaszony totem, brak respawnu wrogów
- [ ] Bonus podbicia (MVP: stały buff MANA lub unlock skrzyni)
- [ ] Zapis stanu obozu w `GameData`
- [ ] Test: drugi raz wejście do obozu — bezpieczny, bez agresji

---

## Faza 3: 2–3 obozy, różne typy goblinów

**Cel:** Więcej celów na mapie, rosnąca trudność z odległością od bazy.

- [ ] Obóz 2 i 3 wg `docs/WORLD_DESIGN.md` (pierścienie 2–3)
- [ ] Różne składy: więcej łuczników, pierwszy szaman (opcjonalnie)
- [ ] Tracker wielu obozów w `GameData` (lista ID + stan)
- [ ] Loot różnicowany per obóz
- [ ] Granice mapy / kill plane
- [ ] Test: oczyszczenie 2 obozów, podbicie 1, powrót do bazy między wyprawami

---

## Faza 4: Drzewko czarów druida

**Cel:** Progres umiejętności w bazie, powiązanie z punktami Przyrody z obozów.

- [ ] Punkty Przyrody za Cleared/Captured
- [ ] UI drzewka w bazie (Ogień / Życie / Kształt)
- [ ] Min. 3 nody (po 1 na gałąź) odblokowujące czary / transformację
- [ ] Mana + 2 czary (Ogień obrażenia, Życie heal)
- [ ] Crafting w bazie — 2 przepisy (mikstura HP, amulet MANA)
- [ ] Save/load JSON: pozycja, obozy, nody, inventory
- [ ] Menu **Kontynuuj** gdy save istnieje

---

## Faza 5+: Inne regiony (future)

Poza MVP — dokumentacja i placeholdery na mapie.

- [ ] Miejsca mocy (1–2 w lesie MVP+)
- [ ] Quest „Wyjście z lasu” — marker na granicy mapy
- [ ] Region: Bagna Cienia (ciągły obszar, nie osobny level)
- [ ] World streaming / chunki (placeholder agent)
- [ ] Pełne 4 typy goblinów, boss totem, dialogi, audio

---

## Flow scen (bez zmian levelowych)

- [x] MainMenu → CharacterSelection → AttributeAllocation → **World**
- [ ] CharacterSelection: karta **Druid** + opis po polsku
- [ ] Walidacja startu od MainMenu (`GameData`)

---

## Polish MVP (po fazie 4)

- [ ] Pauza (`GameState.Paused`)
- [ ] Ustawienia minimum (głośność, czułość)
- [ ] Balans pierwszego obozu
- [ ] Playtest 30 min: baza → 2 obozy → craft → save → kontynuuj
- [ ] Build bez błędów konsoli

---

## Jak testować po każdej fazie

| Faza | Test |
|------|------|
| 1 | Wyjście z bazy, walka w obozie, totem zniszczony, powrót, heal w bazie |
| 2 | Podbicie oczyszczonego obozu, wizualna zmiana, brak wrogów |
| 3 | Trzeci obóz dalej od bazy, trudniejszy skład |
| 4 | Punkty Przyrody → noda w drzewku → nowy czar; save i Kontynuuj |

---

## Backlog (nie blokuje MVP)

- [ ] Kenney / zewnętrzne assety lasu
- [ ] Muzyka i SFX
- [ ] Relikwie ScriptableObject
- [ ] Transformacja wilka (pełna)

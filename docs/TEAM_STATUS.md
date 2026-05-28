# TEAM STATUS — Wpierdol of the Goblins

> **Plik statusu zespołu.** Każdy agent **przed startem** czyta tę listę i sprawdza zależności. **Po skończeniu** zaznacza checkbox i dopisuje wpis w Changelog.

**Wersja:** 1.0 (initial)
**Data:** 2026-05-28
**Owner:** TEAM LEAD
**Faza projektu:** PLAN gotowy → czekamy na pierwszego agenta (A).

---

## Legenda

| Symbol | Znaczenie |
|--------|-----------|
| `[ ]` | pending |
| `[~]` | in_progress |
| `[x]` | completed |
| `[!]` | blocked (patrz sekcja Blockers) |
| `[-]` | skipped / cancelled |

---

## Tabela zbiorcza

| Faza | Opis | Agent | Status | Effort | Zależy od | Data start | Data end |
|------|------|-------|--------|--------|-----------|------------|----------|
| F1 | Kompilacja 0 errors (Unity 6 API) | Agent A | `[ ]` | S | — | — | — |
| F2 | GameAssetRegistry — single source of truth | Agent A | `[ ]` | M | F1 | — | — |
| F3 | Świat: drzewa / trawy / ścieżki / golden hour / NavMesh | Agent B | `[ ]` | L | F2 | — | — |
| F4 | Postacie: druid GanzSe + gobliny modele + Animator | Agent C | `[ ]` | L | F2 | — | — |
| F5a | VFX czarów + totem destroyed + capture ring | Agent C | `[ ]` | M | F2, F4 | — | — |
| F5b | UI Fantasy GUI + Modern RPG icons | Agent D | `[ ]` | M | F2 | — | — |
| F5c | Audio: SFX click / hit / cast / footstep / death | Agent D | `[ ]` | S | F2 | — | — |
| F6 | 5 obozów + baza (ołtarz / save point / crafting stub) | Agent B | `[ ]` | M | F3 | — | — |
| F7 | Polish + E2E test | All + TEAM LEAD | `[ ]` | M | F3, F4, F5, F6 | — | — |

---

## Szczegóły fazy

### F1 — Kompilacja & Baseline `[ ]`
**Agent:** A
**Pliki:** `Assets/_Game/Scripts/**/*.cs`, Build Settings
**Wymagania:**
- [ ] 0 errors, 0 new warnings w Editor Console
- [ ] Deprecated API fix (FindObjectOfType → FindFirstObjectByType, etc.)
- [ ] MainMenu → CharacterCreation → World przechodzi end-to-end
- [ ] Save / Settings nie regresują
**Blocker check:** nikt nie pracuje na `Assets/_Game/Scripts/Core/`.

### F2 — GameAssetRegistry `[ ]`
**Agent:** A
**Pliki nowe:** `Assets/_Game/Scripts/Core/GameAssetRegistry.cs`
**Pliki edytowane:** `GameAssetPaths.cs`, `UIFactory.cs`, `PlayerBuilder.cs`
**Wymagania:**
- [ ] API: TryLoadPrefab / TryLoadSprite / TryLoadAudio
- [ ] Ścieżki kandydackie dla 17 paczek (poza Lava Tube)
- [ ] Fallback Resources/* + null safe
- [ ] Log w bootstrap pokazuje co znaleziono
**Blocker check:** F1 musi być `[x]`.

### F3 — Świat 3D `[ ]`
**Agent:** B
**Pliki:** `WorldGenerator.cs`, `WorldBootstrap.cs`, nowe: `NavMeshBaker.cs`, `CelestialCycleBinder.cs` (opt)
**Wymagania:**
- [ ] tree_1 zamiast Cylinder+Sphere (przez Registry)
- [ ] Ścieżki teksturowane dirt01
- [ ] Golden hour ambient (Celestial preset lub manual recolor)
- [ ] NavMesh wypiekany po Generate()
- [ ] Bioluminescencja zachowana
**Blocker check:** F2 `[x]`.

### F4 — Postacie & Animator `[ ]`
**Agent:** C
**Pliki:** `PlayerBuilder.cs`, `PlayerController.cs`, `PlayerCombat.cs`, `GoblinStormtrooper.cs`, `GoblinArcher.cs`, nowe: `PlayerAnimator.cs`, `GoblinAnimator.cs`, `GoblinNav.cs`
**Wymagania:**
- [ ] Druid GanzSe model (z fallback capsule)
- [ ] Animator Speed/Cast/Hit/Die
- [ ] Goblin stormtrooper z prefab + NavMeshAgent
- [ ] Goblin archer z prefab + NavMeshAgent + LoS
**Blocker check:** F2 `[x]`. Luźno: F3 (NavMesh bake).

### F5a — VFX `[ ]`
**Agent:** C
**Pliki:** `FireballProjectile.cs`, `Totem.cs`, `GoblinCamp.cs` (minimalna edycja), nowe: `VFXSpawner.cs` (opt)
**Wymagania:**
- [ ] Fireball cast + hit VFX
- [ ] Totem explosion VFX
- [ ] Capture ring VFX
- [ ] Heal VFX
**Blocker check:** F2, F4 `[x]`.

### F5b — UI Fantasy GUI `[ ]`
**Agent:** D
**Pliki:** `UIFactory.cs`, `MainMenuBootstrap.cs`, `PlayerHUD.cs`, `PauseMenu.cs`, `SettingsMenuUI.cs`, `CharacterCreationBootstrap.cs`
**Wymagania:**
- [ ] Buttony z GuiButton sprite
- [ ] HUD z bar background + fills + ikony
- [ ] MainMenu z tłem fantasy
- [ ] Skalowanie 1080/1440/4K
**Blocker check:** F2 `[x]`.

### F5c — Audio `[ ]`
**Agent:** D
**Pliki:** `AudioPlayer.cs` (new), `PlayerCombat.cs`, `PlayerController.cs`, `FireballProjectile.cs`, `GoblinBase.cs`, `UIFactory.cs`
**Wymagania:**
- [ ] AudioPlayer static helper
- [ ] UI click, hit, cast, footstep, death podpięte
- [ ] Honoruje SettingsManager volumes
**Blocker check:** F2 `[x]`.

### F6 — Obozy + Baza `[ ]`
**Agent:** B
**Pliki:** `WorldGenerator.cs` (BuildCamp + BuildDruidBase), nowe: `SavePoint.cs`, `CraftingStation.cs` (stub)
**Wymagania:**
- [ ] 5 obozów obecnych i działających
- [ ] Palisada drewniana + bramka
- [ ] Totem 3D wizualnie ulepszony
- [ ] Baza: kamienny krąg + ołtarz + save point + crafting stub
**Blocker check:** F3 `[x]`.

### F7 — Polish + E2E `[ ]`
**Agent:** All + TEAM LEAD review
**Wymagania:**
- [ ] E2E scenario test (zob. IMPLEMENTATION_PLAN F7)
- [ ] Performance ≥ 60 FPS na dev machine
- [ ] 5 min gameplay bez NullRef
- [ ] Build standalone (opcjonalny smoke)
**Blocker check:** wszystkie F1-F6 `[x]`.

---

## Blockers

> Jeśli któryś agent jest zablokowany, dopisz tutaj w formacie:
> `[YYYY-MM-DD HH:MM] Agent X / Faza Y — opis blockera. Akcja: ...`

_Brak blockerów._

---

## Changelog (dopisuj po każdej fazie)

> Format: `[YYYY-MM-DD] Agent X — Faza Y (status): krótki opis zmian.`

| Data | Agent | Faza | Status | Opis |
|------|-------|------|--------|------|
| 2026-05-28 | TEAM LEAD | ANALIZA + PLAN | completed | ASSET_AUDIT, TEAM_WORKFLOW, IMPLEMENTATION_PLAN, AGENT_TASKS, TEAM_STATUS — gotowe. Audyt: 1 / 17 paczek zaimportowana (Fantasy Forest Environment, 2 prefaby). 16 paczek PENDING. |
| 2026-05-28 | Agent C | F4 (druid + gobliny) | partial | Brak fizycznie zaimportowanych GanzSe / Stylized Goblin / Fantasy Goblin (re-skan Assets/). Dodany `WPG.Character.CharacterAnimDriver` — auto-detekcja Animatora z runtime controllerem + procedural fallback (Idle bob, Walk leg/arm swing, Attack swing, Cast hand raise, Death). Druid placeholder rozbudowany: torso + hips + head z hood/beard/uszami + 4 limby + StaffTip i CrystalLight. Goblin placeholder ulepszony: torso + hips + head z uszami goblińskimi + 4 limby + HandMount. Player/Goblin attack/cast eventy wpięte do drivera. Po imporcie GanzSe / Stylized Goblin / Fantasy Goblin — `WorldAssetPlacer.TryAttachCharacterModel` + `GameAssetRegistry` (rozszerzone fallback tokens + DruidModel/GoblinModel/GoblinElite slots) automatycznie podmienią placeholdery, a driver wykryje Animator. |
| 2026-05-28 | Agent B | F3 (trawa) | partial | Trawa Fantasy Forest `grass01.prefab` zintegrowana: nowy `Slot.Grass` w `GameAssetRegistry`, `WorldGrassPool`, `PickWorldGrass`, `WorldGrassPoolCount`. `WorldAssetPlacer.PlaceGrass` (URP MaterialUpgrader, randomowy scale/rotation, Collidery usuwane), `ScatterGrassNearPaths` (gęsto wzdłuż ścieżek), `ScatterGrassInClearings` (gęsto na polanach), `WorldGenerator.ScatterGrassSparseForest` (rzadko w gęstym lesie). Materiały trawy konwertowane na URP Lit + AlphaClip przez `MaterialUpgrader` (foliage detection już istniał). Bush slot wciąż używa Nature Starter Kit 2 / fallback do grass01. |
| 2026-05-28 | Agent B | F3 (trawa / ścieżki) | partial | Wykluczenie brązowych ścieżek przy spawnie trawy i krzaków: `WorldAssetPlacer.IsOnPath` (odległość punkt–segment XZ, promień **3.25 m** od osi = `pathHalfWidth` 2.75 m + margines 0.5 m). `PlaceGrass` odrzuca punkty na ścieżce; polany i łąki przy ścieżkach zachowane; krzaki przez `IsOnPath` w `FillForest`. `pathHalfWidth` zsynchronizowany z szerokością mesha ścieżki (5.5 m). |
| 2026-05-28 | Agent A | F2 (registry update) | partial | `Slot.Grass` dodany. `FallbackTokens` rozszerzone (ganzse, modular_character, starter_armature, playerarmature dla DruidModel; goblin_warrior/stylized_goblin/goblin_melee dla GoblinModel; fantasy_goblin/goblin_shaman/shaman dla GoblinElite). `GameAssetPaths.WorldGrass` candidate paths. `LogReport` rozszerzony o per-slot status FOUND/MISSING z hintem importu paczki + pool counts (trees/bushes/grass/rocks/ruins). |
| _czeka na Agenta A_ | — | F1 | pending | — |

---

## Quick start dla agenta

1. **Otwórz** ten plik. Sprawdź **Tabela zbiorcza** — jaki status ma Twoja faza.
2. **Sprawdź zależności** w kolumnie „Zależy od" — czy są `[x]`?
3. **Sprawdź Blockers** — czy ktoś czeka na coś co Ty zrobisz?
4. **Otwórz** `docs/AGENT_TASKS.md` → znajdź swój prompt (Agent A / B / C / D).
5. **Zmień** status swojej fazy z `[ ]` na `[~]`, dopisz datę start.
6. **Wykonaj** zadania zgodnie z promptem i `IMPLEMENTATION_PLAN.md`.
7. **NIE PSUJ** plików z listy NIE PSUĆ (`docs/TEAM_WORKFLOW.md`).
8. **Po zakończeniu:**
   - Zmień status na `[x]`, dopisz datę end.
   - Dopisz wpis w Changelog (data, agent, faza, status, opis).
   - Jeśli odblokowałeś kogoś — wskazj.
   - Jeśli wystąpił blocker — wpisz w sekcji Blockers, nie zamykaj fazy jako done.

---

## Powiązane dokumenty

- `docs/GAME_CONCEPT.md` — koncepcja gry
- `docs/WORLD_DESIGN.md` — mapa świata, pozycje obozów (nie zmieniać)
- `docs/MVP_ROADMAP.md` — wysokopoziomowe fazy MVP
- `docs/ASSET_AUDIT.md` — audyt paczek, prefab paths
- `docs/TEAM_WORKFLOW.md` — pipeline, zasady żelazne, role
- `docs/IMPLEMENTATION_PLAN.md` — szczegóły faz F1-F7
- `docs/AGENT_TASKS.md` — gotowe prompty agentów (kopiuj-wklej)

# TEAM WORKFLOW — Wpierdol of the Goblins

**Cel dokumentu:** zdefiniować pipeline pracy zespołu agentów AI nad projektem, role, kolejność, zależności i zasady żeby nikt sobie nie wszedł w drogę.

---

## Pipeline projektu

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌─────────────────────────┐
│  KONCEPCJA   │ →  │   ANALIZA    │ →  │     PLAN     │ →  │       WDROŻENIE         │
│   (done)     │    │ (TEAM LEAD)  │    │ (TEAM LEAD)  │    │  Agenci A / B / C / D   │
└──────────────┘    └──────────────┘    └──────────────┘    └───────────┬─────────────┘
                                                                         │
                                                                         ▼
                                                            ┌─────────────────────────┐
                                                            │   TEST + INTEGRACJA     │
                                                            │  (TEAM LEAD review)     │
                                                            └─────────────────────────┘
```

| Faza | Output | Kto |
|------|--------|-----|
| KONCEPCJA | `docs/GAME_CONCEPT.md`, `WORLD_DESIGN.md`, `MVP_ROADMAP.md` | **User** (gotowe) |
| ANALIZA | `docs/ASSET_AUDIT.md` | **TEAM LEAD** (gotowe) |
| PLAN | `docs/TEAM_WORKFLOW.md`, `IMPLEMENTATION_PLAN.md`, `AGENT_TASKS.md`, `TEAM_STATUS.md` | **TEAM LEAD** (gotowe) |
| WDROŻENIE | Kod, prefaby, sceny | **Agenci A / B / C / D** |
| TEST | Smoke test w Editorze, log błędów | **TEAM LEAD** / **User** |

---

## Role agentów

### Agent A — Core & Registry
**Specjalizacja:** infrastruktura, single source of truth, kompilacja.
**Pracuje na:** `Assets/_Game/Scripts/Core/`, build settings, `GameAssetPaths.cs` → `GameAssetRegistry`.
**Kompetencje:** C# clean code, ScriptableObject patterns, Unity 6 API, refactoring.
**Output:** kompilacja **bez błędów**, registry działa, agenci B/C/D mają jedno API do ładowania prefabów/sprite'ów/sfx.

### Agent B — World & Environment
**Specjalizacja:** świat 3D, environment art integracja, oświetlenie.
**Pracuje na:** `Assets/_Game/Scripts/World/`, scena `World.unity`, `WorldGenerator.cs`.
**Kompetencje:** terrain, foliage, lighting URP, atmosfera, NavMesh.
**Output:** las wygląda jak magiczny ciemny las (drzewa z Fantasy Forest, skały, ścieżki, golden hour z Celestial Cycles), baza druida + 5 obozów rozmieszczonych wg `WORLD_DESIGN.md`, NavMesh wypiekany.

### Agent C — Characters & Combat
**Specjalizacja:** postacie, animacje, walka, AI.
**Pracuje na:** `Assets/_Game/Scripts/Player/`, `Assets/_Game/Scripts/Enemies/`, `PlayerBuilder.cs`, goblin prefabs.
**Kompetencje:** Animator/Animation Rigging, NavMeshAgent, kombat tuning, VFX prefab integration.
**Output:** Druid GanzSe model zamiast capsule, 2 typy goblinów z prawdziwymi modelami i animacjami, fireball/heal VFX z Fantasy effects + Cartoon FX, audio kroków/uderzeń.

### Agent D — UI / Audio / Polish
**Specjalizacja:** UI, audio, feedback gracza, „feel".
**Pracuje na:** `Assets/_Game/Scripts/UI/`, MainMenu/CharacterCreation/HUD/Pause, audio integracja.
**Kompetencje:** uGUI / TMP, Fantasy Free GUI sprites, ikony, audio mixers, juice (camera shake, hit pause).
**Output:** wszystkie ekrany UI używają Fantasy Free GUI panel/buttons/bars, ikony Modern RPG icons w HUD, Basic RPG Sounds podpięte (UI click, hit, footstep), polish.

---

## Kolejność i zależności

```
                    ┌──────────────────────────────────┐
                    │  Agent A — Core & Registry       │
                    │  (F1 kompilacja + F2 registry)   │
                    └──────────────────┬───────────────┘
                                       │ KOMPLETNE
                  ┌────────────────────┼────────────────────┐
                  │                    │                    │
                  ▼                    ▼                    ▼
        ┌────────────────┐  ┌────────────────┐  ┌────────────────┐
        │  Agent B       │  │  Agent C       │  │  Agent D       │
        │  World         │  │  Characters    │  │  UI/Audio      │
        │  (F3, F6)      │  │  (F4, F5 VFX)  │  │  (F5 UI, F7)   │
        └───────┬────────┘  └───────┬────────┘  └───────┬────────┘
                │                    │                    │
                └────────────────────┼────────────────────┘
                                     ▼
                          ┌────────────────────┐
                          │  TEAM LEAD review  │
                          │  + integracja      │
                          └────────────────────┘
```

| Step | Agent | Blocker? |
|------|-------|----------|
| **0** | TEAM LEAD: dostarcza ASSET_AUDIT + plan | bez tego nikt nie startuje |
| **1** | Agent A: F1 (kompilacja) → F2 (Registry) | **BLOKER** dla B, C, D |
| **2** | Agent B: F3 (świat) + F6 (obozy/baza) | Może zacząć po F2 (potrzebuje registry) |
| **2** | Agent C: F4 (postacie) + F5 VFX | Może zacząć po F2 |
| **2** | Agent D: F5 (UI/audio) | Może zacząć po F2 |
| **3** | TEAM LEAD: review każdej fazy | po każdym PR od agenta |
| **4** | Polish + test (F7) | wszyscy razem |

Agenci B / C / D mogą pracować **równolegle** po skończeniu Agenta A (F1 + F2).

---

## Plik statusu — `docs/TEAM_STATUS.md`

Każdy agent **przed startem** czyta `TEAM_STATUS.md` i sprawdza:
1. Czy jego zależności są **completed**.
2. Czy nikt inny w danej chwili nie ma `in_progress` na tych samych plikach.

Każdy agent **po skończeniu** swojej fazy:
1. Zaznacza checkbox swojego taska w `TEAM_STATUS.md`.
2. Dopisuje skrót zmian (1-2 zdania) w sekcji "Changelog".
3. Wskazuje jeśli odblokował kogoś.

Format: zob. `docs/TEAM_STATUS.md` (template z checkboxami).

---

## Zasady żelazne (NIE PSUĆ)

> **Te elementy DZIAŁAJĄ i są stabilne. Każda zmiana wymaga zgody TEAM LEAD i osobnego review.**

| Plik / system | Dlaczego nie ruszać | Co wolno |
|---------------|--------------------|--------|
| `Assets/_Game/Scripts/Core/SaveSystem.cs` | Działająca persystencja JSON | Dodawać pola w `SaveData.cs`, nie zmieniać API `Save()` / `Load()` / `HasSave()` |
| `Assets/_Game/Scripts/Core/SettingsManager.cs` | Działający singleton z JSON + apply | Dodawać pola w `SettingsData.cs`, nie zmieniać API `ApplySettings()`, `EnsureExists()` |
| `GoblinCamp.cs` — stany Active/Cleared/Captured + eventy | Core game loop, integracja z `GameManager.campStates` | Dodać pola wizualne; nie rozbijać enum / nie zmieniać sygnatury `ApplyState` |
| `GameManager.cs` — singleton + DontDestroyOnLoad | Stan między scenami, save integration | Dodać nowe pola (np. `discoveredCamps`); nie usuwać istniejących |
| `WorldBootstrap.cs` flow | Bootstrap MVP działa end-to-end | Dodawać kroki; nie usuwać `_generator.Generate()` ani spawn gracza |

**Konwencje techniczne:**

| Reguła | Detal |
|--------|-------|
| **Unity 6 API** | `FindFirstObjectByType<T>()` / `FindAnyObjectByType<T>()` zamiast deprecated `FindObjectOfType<T>()`. `Object.FindObjectsByType<T>(FindObjectsSortMode.None)` zamiast `FindObjectsOfType<T>()`. |
| **Namespace** | `WPG.Core`, `WPG.Player`, `WPG.Enemies`, `WPG.World`, `WPG.UI`, `WPG.Character` |
| **Język** | C# nazwy / komentarze techniczne po angielsku; **UI gracza i debug logi: po polsku** |
| **Materiały** | Zawsze `MaterialFactory.Get(color, ...)` runtime — nie zostawiać "pink" missing-shader. Dla URP: `Universal Render Pipeline/Lit`. |
| **Sceny** | Tylko 3: `MainMenu`, `CharacterCreation`, `World`. **NIE tworzyć** `Level_01`, `Tutorial`, itp. |
| **Lava Tube** | **NIE używać** — wykluczone przez usera. Nie importować, nie linkować w registry. |
| **Resources fallback** | Jeśli paczka ma sprite/sfx i nie wiemy gdzie user ją wrzuci, dodać sprawdzenie `Resources/...` jako ostatni fallback (zob. `GameAssetPaths.Res*`). |
| **Refactor** | Małymi PR-ami. Jeden agent = jeden task = jeden zamknięty zakres plików. Nie refaktoryzować poza scope. |
| **Commity** | **NIE commituje agent**. Tylko user decyduje. |
| **AssetDatabase** | Dozwolone w editor scripts i `#if UNITY_EDITOR` — registry. Nie używać w runtime. |
| **Lokalizacja** | Wszystkie napisy widoczne dla gracza: PL. Klucze identyfikatorów (campId, siteId): EN snake_case. |

---

## Komunikacja agent ↔ TEAM LEAD ↔ User

```
User → TEAM LEAD: "Zrobione import paczki X" / nowy requirement
TEAM LEAD → User: podsumowanie po każdej fazie, ile prefabów, gotowość
TEAM LEAD → Agent (A/B/C/D): prompt z AGENT_TASKS.md + ewentualny update
Agent → TEAM LEAD: status, błędy kompilacji, brakujące assety
TEAM LEAD → integracja: review każdego PR przed merge
```

**Eskalacja:**
- Agent nie może zrobić taska bo brak paczki → wpis w `TEAM_STATUS.md` sekcja "Blockers" + powiadomienie TEAM LEAD.
- Agent musi zmienić plik z listy NIE PSUĆ → zgoda TEAM LEAD wymagana.
- Konflikt scope (dwóch agentów chce ten sam plik) → TEAM LEAD rozstrzyga.

---

## Definition of Done (per faza)

Każda faza w `IMPLEMENTATION_PLAN.md` musi spełniać:

- [ ] **Kompilacja:** 0 errors, 0 nowych warningów w Console.
- [ ] **Run:** scena `World.unity` w Play mode startuje bez NullRef.
- [ ] **Smoke test:** opisany w fazie (np. „gracz dochodzi do obozu i zabija totem").
- [ ] **NIE PSUĆ:** Save / Settings / GoblinCamp state machine nadal działają.
- [ ] **Status:** wpis w `TEAM_STATUS.md` z checkboxem, autorem, datą.

---

## Wersja dokumentu

| Wersja | Data | Zmiana |
|--------|------|--------|
| 1.0 | 2026-05-28 | Initial — TEAM LEAD setup |

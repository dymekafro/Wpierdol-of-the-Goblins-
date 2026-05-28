# IMPLEMENTATION PLAN — Wpierdol of the Goblins

**Wersja:** 1.0 (2026-05-28)
**Autor:** TEAM LEAD
**Cel:** Doprowadzić MVP do stanu **„grywalny demo build"** z prawdziwymi assetami (zamiast primitives).

Fazy są dobrane tak, żeby każda kończyła się **stabilnym Play mode** — kompilacja 0 errors, scena rusza, save działa.

Effort: **S** = ≤ 30 min, **M** = 30–90 min, **L** = > 90 min jednego agenta.

---

## F1 — Kompilacja & Baseline (Agent A) — **S**

**Cel:** Projekt kompiluje się **bez błędów i bez warningów** w Unity 6000.4.7f1, URP 17.4.0. Wszystkie sceny startują.

**Pliki:**
- `Assets/_Game/Scripts/**/*.cs` — sprawdzić wszystkie deprecated API
- `Assets/_Game/Scenes/World.unity` — upewnić się że ma `WorldBootstrap` na root GameObject
- `Assets/Settings/Build Profiles/*` — sceny w buildzie

**Checklist:**
- [ ] `dotnet build` lub Unity → Editor → Console: **0 errors, 0 nowych warnings**
- [ ] Usuń wszystkie `FindObjectOfType<T>()` → zastąp `FindFirstObjectByType<T>()` (Unity 6)
- [ ] Usuń wszystkie `FindObjectsOfType<T>()` → `FindObjectsByType<T>(FindObjectsSortMode.None)`
- [ ] Wszystkie `using UnityEngine;` na miejscu
- [ ] Build Settings: kolejność scen `MainMenu` → `CharacterCreation` → `World`
- [ ] Play mode z `MainMenu`: New Game → CharacterCreation → World **bez NullRef**
- [ ] Settings → ApplySettings działa (test: zmień głośność, sprawdź AudioListener.volume)
- [ ] Save / Load test: w World naciśnij F5 (lub menu) → save → quit → Continue → wczytuje pozycję

**Kryteria done:**
- ✅ Console clean.
- ✅ `MainMenu → CharacterCreation → World` przechodzi end-to-end.
- ✅ Save/Settings nie regresują.

**NIE:** nie dotykać logiki — tylko fixy API i build settings.

---

## F2 — GameAssetRegistry (Agent A) — **M**

**Cel:** Jedno API dla wszystkich agentów do ładowania prefabów, sprite'ów, audio clipów. Single source of truth. Fallback na primitives jeśli paczki brak.

**Pliki nowe:**
- `Assets/_Game/Scripts/Core/GameAssetRegistry.cs` — ScriptableObject lub static facade
- `Assets/_Game/Scripts/Core/GameAssetLoader.cs` (opcjonalnie) — runtime loader

**Pliki edytowane:**
- `Assets/_Game/Scripts/Core/GameAssetPaths.cs` — rozszerzenie o **prefab paths**
- `Assets/_Game/Scripts/UI/UIFactory.cs` — przejście z hardcode → Registry
- `Assets/_Game/Scripts/Player/PlayerBuilder.cs` — refactor żeby próbować Registry przed fallback

**Checklist:**
- [ ] `GameAssetRegistry` API:
  ```
  GameObject TryLoadPrefab(string[] candidatePaths)
  GameObject TryLoadPrefabResources(string resPath)
  Sprite     TryLoadSprite(string[] candidatePaths)
  AudioClip  TryLoadAudio(string[] candidatePaths)
  ```
- [ ] Dodaj **ścieżki kandydackie** dla:
  - Drzewa: `Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab`, `Assets/Nature Starter Kit 2/Prefabs/Tree_*.prefab`
  - Trawa: `Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab`
  - Skały: `Assets/Rock_pack/Prefabs/Rock_*.prefab`, `Assets/Rocks HD/Prefabs/*.prefab`
  - Druid: `Assets/GanzSe Character/Prefabs/*.prefab` (+ wszystkie warianty nazwy)
  - Goblin Stormtrooper: `Assets/3D Stylized Goblin/...`
  - Goblin Archer: `Assets/Fantasy Goblin/...`
  - VFX Fire: `Assets/Fantasy effects/Prefabs/VFX_Fire*.prefab`, `Assets/Cartoon FX Projectile/...`
  - VFX Heal: `Assets/Fantasy effects/Prefabs/VFX_Heal*.prefab`
  - Pociski: `Assets/Cartoon FX Projectile/Prefabs/Fireball*.prefab`
  - Ruiny: `Assets/RPG Dungeon Kit/Prefabs/Pillar_*.prefab`, `Altar*.prefab`
- [ ] Implementacja: `AssetDatabase.LoadAssetAtPath<GameObject>` w editorze, `Resources.Load` runtime fallback.
- [ ] **Lava Tube wykluczone** — żadne ścieżki do niego.
- [ ] Test: `Debug.Log` w bootstrap pokazuje ile prefabów `Registry` znalazło / ile nie.

**Kryteria done:**
- ✅ Registry kompiluje się.
- ✅ W edytorze `tree_1.prefab` i `grass01.prefab` są ładowane (2 / N).
- ✅ Pozostałe kandydaty wracają `null` bez błędu — fallback działa.
- ✅ `UIFactory.cs` używa Registry zamiast bezpośrednich ścieżek.

**NIE:** nie psuć `GameAssetPaths.cs` API (UI/audio kandydaci tam już są — rozszerz, nie usuwaj).

---

## F3 — Świat: drzewa, ścieżki, ruiny, oświetlenie (Agent B) — **L**

**Cel:** Las wygląda jak **magiczny ciemny las** z dokumentacji — mroczny ambient, golden hour akcent, drzewa, skały, ścieżki, ruiny, bioluminescencja.

**Pliki edytowane:**
- `Assets/_Game/Scripts/World/WorldGenerator.cs`
- `Assets/_Game/Scenes/World.unity` (przez WorldBootstrap)

**Pliki nowe (opcjonalnie):**
- `Assets/_Game/Scripts/World/CelestialCycleBinder.cs` — wrapper dla Celestial Cycles asset (jeśli paczka jest)
- `Assets/_Game/Scripts/World/RuinPlacer.cs` — rozmieszczenie ruin RPG Dungeon Kit

**Checklist:**
- [ ] Drzewa: zastąp `SpawnTree` primitives → instancjuj `tree_1.prefab` przez `GameAssetRegistry`. Fallback: primitives (już jest).
- [ ] Trawy: rozmieszczać `grass01.prefab` w klusterach (10-20 per kluster, około polan i ścieżek).
- [ ] Skały: jeśli Rocks HD/Rock_pack pojawi się → użyć; w PR1 zostaw fallback sphere.
- [ ] **Ścieżki**: zamiast Cube ze ścieżkowym kolorem → Terrain detail lub plane mesh z teksturą `dirt01` z Fantasy Forest. Alternatywa MVP: zostaw Cube ale z prawdziwą teksturą `dirt01.tga`.
- [ ] **Ruiny:** rozmieść 3–5 grup ruin (kolumny/ołtarze) z `RPG Dungeon Kit` przy `power_site_stone_circle`. W PR1: placeholdery (cube kolumny).
- [ ] **Bioluminescencja:** zachować świecące grzyby (już są), dodać Volume URP z lekkim bloom (jeśli URP Volume Profile dostępny).
- [ ] **Oświetlenie:** Jeśli **Celestial Cycles** dostępne → bind preset **„golden hour"** (mroczne niebo + ciepły akcent na horyzoncie). Fallback: bieżący `MoonLight` + cieplejszy fog color.
- [ ] **NavMesh:** dodać NavMeshSurface (com.unity.ai.navigation) na ground, bake w runtime (`Surface.BuildNavMesh()`).
- [ ] **Mgła:** zachować `FogMode.ExponentialSquared`, density 0.025 (bieżący). Sprawdzić że nie kasuje skali widoczności.

**Kryteria done:**
- ✅ Wchodząc w World jesteś w wyraźnym mrocznym lesie z drzewami modelami (nie tylko Cylinder+Sphere).
- ✅ Widoczne polany, ścieżki, świecące grzyby.
- ✅ NavMesh wypiekany — gobliny będą mogły patrzeć.
- ✅ Performance: > 30 FPS na lokalnym dev machine.
- ✅ Smoke test: gracz biegnie od bazy w stronę pierwszego obozu po ścieżce.

**NIE:** nie zmieniaj layout obozów (`CampDef` w `WorldGenerator.cs`) — tylko wizual. Pozycje 5 obozów są częścią `WORLD_DESIGN.md`.

---

## F4 — Postacie: druid (GanzSe) + gobliny (Agent C) — **L**

**Cel:** Capsule placeholder zastąpiony prawdziwymi modelami i animacjami. Gobliny chodzą, atakują.

**Pliki edytowane:**
- `Assets/_Game/Scripts/Player/PlayerBuilder.cs` — używać prefab GanzSe jeśli dostępny
- `Assets/_Game/Scripts/Enemies/GoblinStormtrooper.cs`, `GoblinArcher.cs` — model + Animator
- `Assets/_Game/Scripts/Player/PlayerController.cs` — podpiąć animacje (Idle/Walk/Run/Cast)

**Pliki nowe:**
- `Assets/_Game/Scripts/Player/PlayerAnimator.cs` — wrapper dla Animator (parametry `speed`, `attack`, `cast`)
- `Assets/_Game/Scripts/Enemies/GoblinAnimator.cs`

**Checklist:**
- [ ] **Druid:** załaduj `GanzSe Character` prefab przez Registry. Jeśli brak → capsule fallback (już jest).
- [ ] Druid Animator: parametry **`Speed` (float)**, **`Cast` (trigger)**, **`Hit` (trigger)**, **`Die` (trigger)**.
- [ ] **Control Rig** (jeśli dostępne): IK ręka prawa → `staffTip`, IK głowa → kierunek ruchu / cel.
- [ ] Goblin Stormtrooper: prefab z **3D Stylized Goblin**. Animator: Idle/Run/Attack/Die. NavMeshAgent dodać.
- [ ] Goblin Archer: prefab z **Fantasy Goblin**. Animator: Idle/Run/Aim/Shoot/Die.
- [ ] Skala goblinów ~0.85 (mniejsi od druida).
- [ ] Animacje **Mixamo** OK jako fallback jeśli paczki nie mają (user uploaduje).
- [ ] `WorldGenerator.BuildCamp` — wymień `var gob = go.AddComponent<GoblinStormtrooper>();` na `GoblinSpawner.Spawn(prefab, parent, position)`.

**Kryteria done:**
- ✅ Druid ma model z animacją idle/walk.
- ✅ Goblin szturmowiec biegnie do gracza i atakuje (chase + melee).
- ✅ Goblin łucznik trzyma dystans i strzela strzałami.
- ✅ Atak druida (combat) i czar (cast) odtwarzają animacje.
- ✅ Save/load: gracz nadal spawnuje się w bazie.

**NIE:** nie zmieniaj `PlayerController.cs` w zakresie inputu i fizyki — tylko podpinaj Animator. Nie usuwaj `PlayerBuilder.BuildVisual` (fallback).

---

## F5 — VFX + Audio + UI (Agent C & D razem) — **M**

**Cel:** Czary druida wybuchają kolorem, ekran ma prawdziwy fantasy HUD, dźwięki UI/walki działają.

### F5a — VFX (Agent C)
- [ ] `FireballProjectile.cs` — instancjuj `VFX_Fire` z Fantasy effects albo `Projectile_Fireball` z Cartoon FX zamiast prostego sphere.
- [ ] Heal czar (gdy jest) — efekt `VFX_Heal` na graczu.
- [ ] Totem zniszczony — efekt explosion (Cartoon FX).
- [ ] Capture ritual — ring of light VFX.

### F5b — UI (Agent D)
- [ ] `MainMenuBootstrap.cs` — tło: `GuiMenuBackground` sprite (jeśli dostępne) zamiast solid color.
- [ ] `UIFactory.cs` — buttony używają `GuiButton` sprite (przez Registry).
- [ ] `PlayerHUD.cs` — paski HP/Mana z `GuiBarBackground` + `GuiBarFillHp` / `GuiBarFillMana`.
- [ ] HUD ikony: `IconMelee`, `IconFireball`, `IconHeal` w slotach (Modern RPG icons).
- [ ] `PauseMenu.cs` — panel `GuiPanel` jako tło menu.
- [ ] `CharacterCreationBootstrap.cs` — opcjonalny refresh stylu.

### F5c — Audio (Agent D)
- [ ] `UIFactory.cs` — dodaj `AudioSource` dla UI click → `SfxUIClick`.
- [ ] `PlayerCombat.cs` — przy uderzeniu odtwórz `SfxHit`.
- [ ] `FireballProjectile.cs` — przy castowaniu `SfxFireballCast`.
- [ ] `PlayerController.cs` — kroki `SfxFootstep` co X jednostek odległości.
- [ ] `GoblinBase.cs` — przy śmierci `SfxDeath`.

**Kryteria done:**
- ✅ Klikanie buttonu w MainMenu robi „click" SFX.
- ✅ HUD ma prawdziwe paski i ikony.
- ✅ Fireball ma VFX trail / explosion.
- ✅ Audio sliders w Settings sterują głośnością.

**NIE:** nie usuwaj fallback w UIFactory (jeśli sprite brak — placeholder color, nie crash).

---

## F6 — Obozy + struktury (5 camps, baza) (Agent B) — **M**

**Cel:** 5 obozów rozmieszczonych zgodnie z `WORLD_DESIGN.md`, każdy z prawdziwą palisadą (drewno + bramka), totem 3D (zamiast cube), baza druida ma kamienne ruiny.

**Pliki edytowane:**
- `Assets/_Game/Scripts/World/WorldGenerator.cs` — `BuildCamp`, `BuildDruidBase`
- `Assets/_Game/Scripts/Enemies/Totem.cs` — wizual

**Checklist:**
- [ ] **5 obozów** wg `WORLD_DESIGN.md`:
  1. `goblin_camp_first_clearing` — pierścień 1 — 2 storm + 1 łucznik
  2. `goblin_camp_forest_den` — pierścień 1 — 3 storm
  3. `goblin_camp_ember_moss` — pierścień 2 — 3 storm + 2 łucznik
  4. `goblin_camp_lost_roots` — pierścień 2 — 2 storm + 3 łucznik
  5. `goblin_camp_shade_glade` — pierścień 3 — 4 storm + 3 łucznik (najtrudniejszy)
  → **Już są w `WorldGenerator.cs`** — sprawdź że wszystkie 5 spawn'ują (bug check).
- [ ] Palisada: zamiast `Cube` 0.25×2.2×0.25 → `Wall_Wooden` prefab z RPG Dungeon Kit (jeśli pack dostępny). Fallback: bieżący Cube.
- [ ] Bramka: każdy obóz ma 1 luk w palisadzie z modelem bramy (RPG Dungeon Kit `Gate.prefab`).
- [ ] Totem: zastąp prosty Sphere/Cube → wysoki cylindr z rzeźbionymi maskami (placeholder: 3 stacked rocks z Rock_pack + emisja runów).
- [ ] **Baza druida**: kamienny krąg z prawdziwymi rock prefabs, **kamienny ołtarz** (RPG Dungeon Kit `Altar.prefab`), drzewo życia (zostawić proceduralny + tree_1 jako otaczające).
- [ ] **Crafting station** — placeholder GameObject z `CraftingStation.cs` (stub) w bazie, interaktywne (przygotuj pod Agenta C druida).
- [ ] **Save point** — placeholder (świecący kryształ) z trigger interakcji „Save" (E).

**Kryteria done:**
- ✅ 5 obozów widocznych na mapie z różną trudnością.
- ✅ Każdy obóz: palisada, ognisko, totem, gobliny — działa Active→Cleared→Captured.
- ✅ Baza ma ołtarz i kamienny krąg.
- ✅ Save point w bazie działa (manual save przez E).

**NIE:** nie zmieniać `campId` ani `CampState` enum.

---

## F7 — Polish + Test (TEAM LEAD + wszyscy) — **M**

**Cel:** Build, smoke test, fix ostatnich rzeczy.

**Checklist:**
- [ ] Wszystkie scen play-mode OK
- [ ] Test scenariusz: MainMenu → New Game → Druid → World → biegnij do `goblin_camp_first_clearing` → zabij goblinów → zniszcz totem → wróć do bazy → save → quit → Continue → stan się wczytał
- [ ] Performance: ≥ 60 FPS na średnim laptopie (dev)
- [ ] Brak NullRef w konsoli przez 5 min gameplay
- [ ] Audio: wszystko gra, miksery działają, settings honorowane
- [ ] UI: skalowanie, anchoring na 1920x1080 i 2560x1440
- [ ] Settings reset → defaults działa
- [ ] Save / Continue → wszystkie obozy w stanie sprzed quit
- [ ] Build Windows + macOS standalone — kompiluje (opcjonalnie)
- [ ] **Tag wersji:** `v0.1-mvp` (user commituje)

**Kryteria done:**
- ✅ Demo można pokazać.

---

## Effort summary

| Faza | Agent | Effort | Blokuje |
|------|-------|--------|---------|
| F1 | A | S | F2 |
| F2 | A | M | F3, F4, F5, F6 |
| F3 | B | L | F6 (struktury) |
| F4 | C | L | F5a (VFX bound do animacji) |
| F5 | C+D | M | F7 |
| F6 | B | M | F7 |
| F7 | All | M | — |

**Całkowity szac. czas (1 agent na fazę, sekwencyjnie):** ~8–10 godzin pracy agenta. Z równoległością B+C+D po F2: **~5–6 godzin elapsed**.

---

## Mapowanie do MVP Roadmap

| Faza implementacji | Faza Roadmap (`MVP_ROADMAP.md`) |
|--------------------|---------------------------------|
| F1 + F2 | Faza 0 — fundament |
| F3 + F6 | Faza 1 — Baza + pierwszy obóz |
| F4 + F5a | Faza 1+2 — postacie + walka + VFX podboju |
| F5b + F5c | Faza 0+1 — UI/audio polish |
| F7 | Polish MVP |

Faza 2 (`Cleared → Captured`) jest **już zaimplementowana** w kodzie (`GoblinCamp.TryCapture`). Faza 3 (więcej obozów) — pokrywa F6 (już 5 obozów).

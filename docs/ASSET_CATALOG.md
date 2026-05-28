# ASSET CATALOG — Wpierdol of the Goblins

> Bieżący stan integracji modeli postaci, animacji, goblinów i trawy w grze. Jeśli rozbudowujesz / wymieniasz registry, **aktualizuj tę tabelę razem ze zmianą kodu**.

**Data:** 2026-05-28
**Owner:** Agent C (postacie + animacje), Agent B (trawa), Agent A (registry)
**Tryb integracji:** **dual-track** — placeholdery (proceduralna geometria + animacja) + auto-podmianka prefabów Asset Store przez `GameAssetRegistry` / `WorldAssetPlacer.TryAttachCharacterModel`.

---

## TL;DR

| Slot | Status | Aktywny prefab | Animator | Fallback |
|------|--------|----------------|----------|----------|
| **DruidModel** | MISSING (placeholder) | brak | brak (CharacterAnimDriver — proceduralna animacja) | rozbudowany humanoidalny placeholder (torso+hips+head+hood+beard+4 limby+staff+crystal light) |
| **GoblinModel** (melee + archer) | MISSING (placeholder) | brak | brak (CharacterAnimDriver — proceduralna animacja) | placeholder z 4 limbami i goblińskimi uszami |
| **GoblinElite** (Shaman) | MISSING (placeholder) | brak | brak (CharacterAnimDriver — proceduralna animacja) | placeholder GoblinShamanElite (większa skala 1.05, fioletowo-purpurowy) |
| **Grass** | **OK** | `Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab` | n/a | proceduralna polana + Bush placeholder |
| **Bush** | OK (fallback grass01) | `Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab` (przez WorldBushes path) | n/a | sphere placeholder |
| **TreeLarge** | OK | `Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab` | n/a | cylinder + sphere |
| **Rock**, **Ruin** | MISSING | brak | n/a | sphere/cube placeholder |

**Wynik:** gra jest grywalna 100% z aktualnie zaimportowaną pojedynczą paczką (Fantasy Forest Environment Free Sample). Reszta paczek z user listy (16) NIE jest fizycznie obecna w `Assets/` w czasie tej iteracji.

---

## ZADANIE 1 — Druid (model + animacje)

### Status: PLACEHOLDER

#### Prefab użyty
- **brak** — fallback proceduralny w `PlayerBuilder.BuildPlaceholderVisual`.

#### Co próbujemy wczytać (kolejność)
Kolejność z `WorldAssetPlacer.CharacterCandidates[CharacterModelKind.Druid]`:
```
Assets/GanzSe FREE Modular Character - Fantasy Low Poly Pack/Prefabs/Character.prefab
Assets/GanzSe Modular Character/Prefabs/Character.prefab
Assets/_Game/Prefabs/Characters/DruidModel.prefab
```
Plus skan tokenów: `ganzse`, `modular character`, `druidmodel`, `druid`, `starter_armature`, `playerarmature` (dla Starter Assets Third Person URP).

#### Animacje
- **Real Animator (gdy paczka się pojawi):** `CharacterAnimDriver` ustawia `Speed` (float), trigger `Attack`, trigger `Cast`, bool `Death` na detected Animatorze. Dopasuj nazwy parametrów na driverze jeżeli paczka używa innych nazw (publiczne pola `speedParam` / `attackTrigger` / `castTrigger` / `deathBool` / `isMovingParam`).
- **Placeholder (obecnie):**
  - **Idle:** subtelny bob (0.025 m amplitude) i wachlowanie głową
  - **Walk/Run:** bob 0.07 m, leg swing ±28°, arm swing ±22° (skala z aktualnego Speed / MaxSpeed)
  - **Attack (LMB):** swing handMount po łuku 120° (0.35s)
  - **Cast (E / Q):** hand raise w górę (0.6s, 70° tilt)
  - **Death:** reset poza, drivers stop bobbing

#### Pipeline
```
PlayerBuilder.BuildDruid()
  ├─ AddComponent<CharacterAnimDriver>()
  ├─ WorldAssetPlacer.TryAttachCharacterModel(root, Druid, 1.85f)
  │     └─ EnsureCharacterAssetsScanned() → ResolveCharacterPrefab()
  │             ├─ GameAssetRegistry.DruidModel (FOUND? → użyj)
  │             ├─ ResolvedCharacterPaths[Druid]
  │             └─ CharacterCandidates[Druid] (kolejne paths)
  ├─ jeśli OK → driver.bodyPivot=ModelRoot, driver.handMount=HandMount; ResolveAnimator() łapie Animator
  └─ jeśli FAIL → BuildPlaceholderVisual: torso+hips+head+hood+beard+earL/R+leftArm/rightArm/leftLeg/rightLeg+staff+crystalLight; driver podpięty do każdego pivota
```

#### Co zachowujemy
- `PlayerStats`, `PlayerCombat`, `PlayerController` — bez zmian funkcjonalnych poza wpięciem `animDriver` (pole publiczne, opcjonalne)
- Kostur (`staffTip`) — child handMount (placeholder) lub `StaffTip_Auto` doczepione do bone'a HandR (real model)

---

## ZADANIE 2 — Gobliny

### Status: PLACEHOLDER (3 typy)

| Klasa | Slot | Prefab użyty | Stan |
|-------|------|--------------|------|
| `GoblinStormtrooper` | GoblinModel | brak | placeholder (zielony, base scale 0.85) |
| `GoblinArcher` | GoblinModel (CharacterModelKind.GoblinArcher) | brak | placeholder + cube-bow |
| `GoblinShamanElite` | GoblinElite | brak | placeholder (purpurowy, scale 1.05) |

#### Co próbujemy wczytać
Z `WorldAssetPlacer.CharacterCandidates`:
```
GoblinMelee:
  Assets/3D Stylized Goblin/Prefabs/Goblin_Warrior.prefab
  Assets/Stylized Goblins Archer & Warrior/Prefabs/Goblin_Warrior.prefab
  Assets/_Game/Prefabs/Enemies/GoblinModel.prefab
GoblinArcher:
  Assets/3D Stylized Goblin/Prefabs/Goblin_Archer.prefab
  Assets/Stylized Goblins Archer & Warrior/Prefabs/Goblin_Archer.prefab
GoblinElite (Shaman):
  Assets/Fantasy Goblin/Prefabs/Fantasy Goblin.prefab
  Assets/Fantasy Goblin/Prefabs/Goblin_Shaman.prefab
  Assets/_Game/Prefabs/Enemies/GoblinElite.prefab
```
Plus skan tokenów: `goblin_warrior`, `stylized_goblin`, `goblin_melee`, `goblin`, `goblin_archer`, `fantasy_goblin`, `goblin_shaman`, `shaman`.

#### Animacje
- **Real Animator (gdy paczka się pojawi):** parametry takie jak druid (Speed/Attack/Cast/Death). 3D Stylized Goblin ma własny Animator Controller w paczce — `CharacterAnimDriver.ResolveAnimator()` go znajdzie automatycznie.
- **Placeholder:** Idle bob, walk leg/arm swing (proporcjonalny do `_currentMoveSpeed` z `MoveTowardsXZ`), attack swing handMount przy `DealDamage()` / `ShootArrow()`, death tilt 80° (rotacja istniała w `GoblinBase.Die`).

#### Pipeline
```
GoblinBase.BuildVisual()
  ├─ EnsureAnimDriver() (AddComponent<CharacterAnimDriver>())
  ├─ WorldAssetPlacer.TryAttachCharacterModel(transform, AssetModelKind, scale*1.8)
  │     └─ ten sam ResolveCharacterPrefab co druid
  ├─ jeśli OK → driver.bodyPivot=ModelRoot, driver.handMount=HandMount
  └─ jeśli FAIL → BuildPlaceholderVisual: torso+hips+head z uszami+oczy+4 limby+handMount; driver podpięty
```

---

## ZADANIE 3 — Trawa

### Status: OK

#### Prefab użyty
- **`Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab`** — pojedynczy mesh (cross-quad), materiał `grassmesh.mat` (URP Lit z `_AlphaClip = 1`, `_Cull = 0`, `_Cutoff ≈ 0.696`, RenderType=TransparentCutout).

#### Materiały
- `grassmesh.mat` — gotowy URP, dwustronny, alpha-clip aktywny. Oryginalna tekstura `grassmesh.png`.
- `grass01.mat` — alternatywny URP, ale **`_AlphaClip = 0`** w pliku. `MaterialUpgrader.IsFoliageMaterial` wykrywa po nazwie "grass" + "01" i włącza alpha-clip + double-sided na każdej instancji spawnowanej w runtime (`UpgradeHierarchy` w `PlaceGrass`).

#### Spawn
`WorldAssetPlacer` (nowe API):
- **`PlaceGrass(Transform, Vector3)`** — instancja prefab + losowa rotacja Y + scale 0.7..1.3 + (0.85..1.15 height) + usunięcie wszystkich Colliderów + `MaterialUpgrader.UpgradeHierarchy`. **Odrzuca punkty na ścieżce** (`IsOnPath`).
- **`IsOnPath(Vector3)`** — dystans punkt–segment ścieżki (XZ) &lt; `pathHalfWidth` (2.75 m) + `PathSurfaceExtraMargin` (0.5 m) → **3.25 m** od osi = brązowa nawierzchnia wykluczona.
- **`ScatterGrassNearPaths(parent, bandWidth=2.6m, density=1.4 tuft/m, mapRadius, isBlocked)`** — kępki **obok** ścieżek (łąki), min. odsadka ~3.6 m od osi; nigdy na brązowym pasie.
- **`ScatterGrassInClearings(parent, clearingCenters, clearingRadius=9, perClearing=22, mapRadius, isBlocked)`** — gęsta trawa na zielonych polanach; segmenty ścieżki w polanie pomijane.
- **`WorldGenerator.ScatterGrassSparseForest(parent, count=220)`** — rzadkie kępki w lesie; `IsOnPath` zamiast szerokiego bufora +4 m.

#### Pipeline
```
WorldGenerator.Generate()
  ├─ BuildGround() → tworzy 10 polan, dopisuje pozycje do ClearingCenters
  ├─ BuildVegetation()
  │     ├─ FillForest (drzewa, krzaki, mushroomy — bez zmian)
  │     └─ BuildGrassDecoration()
  │           ├─ ScatterGrassNearPaths (gęsto wzdłuż wszystkich ścieżek)
  │           ├─ ScatterGrassInClearings (gęsto w polanach)
  │           └─ ScatterGrassSparseForest (rzadko, omija ścieżki)
  └─ MaterialUpgrader.UpgradeHierarchy(parent)
```

#### Co się stanie po imporcie Nature Starter Kit 2
- `GameAssetPaths.WorldGrass` zawiera `Assets/Nature Starter Kit 2/Prefabs/Grass.prefab` jako kandydata
- `IsWorldGrassPath` rozpoznaje "grass" w `nature starter` ścieżce → dodaje do `WorldGrassPool`
- `PickWorldGrass(rng)` losuje między grass01 a Nature grass

---

## ZADANIE 4 — GameAssetRegistry update

### Slots i tokeny

| Slot | PrimaryPath (jeśli istnieje) | Tokens (FallbackTokens) | Pool? |
|------|------------------------------|--------------------------|-------|
| TreeLarge | _Game/Prefabs/World/TreeLarge.prefab | treelarge, tree_large, tree_1, tree1, pine, fir, oak | WorldTreePool |
| TreeSmall | _Game/Prefabs/World/TreeSmall.prefab | treesmall, tree_small, tree_2, tree2, sapling | WorldTreePool |
| Bush | _Game/Prefabs/World/Bush.prefab | bush, shrub, fern, plant | WorldBushPool |
| **Grass** (nowy) | **Fantasy Forest .../grass01.prefab** | **grass01, grass_01, grass_mesh, grassmesh, grass** | **WorldGrassPool** |
| Rock | _Game/Prefabs/World/Rock.prefab | rock, stone, boulder, cliff | WorldRockPool |
| Ruin | _Game/Prefabs/World/Ruin.prefab | ruin, ruins, altar, shrine, pillar, column, wall, arch | WorldRuinPool |
| DruidModel | _Game/Prefabs/Characters/DruidModel.prefab | druidmodel, druid_model, player_druid, ganzse, modular character, modular_character, starter_armature, playerarmature | — |
| GoblinModel | _Game/Prefabs/Enemies/GoblinModel.prefab | goblinmodel, goblin_model, goblin_storm, goblin_warrior, goblin warrior, stylized goblin, stylized_goblin, goblin_melee, goblin | — |
| GoblinElite | _Game/Prefabs/Enemies/GoblinElite.prefab | goblinelite, goblin_elite, goblin_shaman, shaman, fantasy goblin, fantasy_goblin | — |

### LogReport (przy starcie)
`GameAssetRegistry.Bootstrap` (RuntimeInitializeOnLoadMethod) → `Initialize()` + `LogReport()`. Format:
```
[GameAssetRegistry] === Asset Report ===
  TreeLarge      [OK     ] Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab
  TreeSmall      [OK     ] Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab
  Bush           [OK     ] Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab
  Grass          [OK     ] Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab
  Rock           [MISSING] (brak)
  Ruin           [MISSING] (brak)
  DruidModel     [MISSING] (brak)
  GoblinModel    [MISSING] (brak)
  GoblinElite    [MISSING] (brak)
  ...
[GameAssetRegistry] Slots: N/16 found
[GameAssetRegistry] World pools: trees=1, bushes=1, grass=1, rocks=0, ruins=0
[GameAssetRegistry] FOUND Grass → Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab
[GameAssetRegistry] MISSING DruidModel — placeholder fallback aktywny. Sugestia: zaimportuj GanzSe FREE Modular Character lub Starter Assets PlayerArmature.
[GameAssetRegistry] MISSING GoblinModel — placeholder fallback aktywny. Sugestia: zaimportuj 3D Stylized Goblin (Goblin_Warrior.prefab) lub Stylized Goblins Archer & Warrior.
[GameAssetRegistry] MISSING GoblinElite — placeholder fallback aktywny. Sugestia: zaimportuj Fantasy Goblin (Goblin_Shaman.prefab).
```

---

## Jak testować

### Smoke test (Editor)
1. Otwórz `Assets/_Game/Scenes/MainMenu.unity` i wciśnij Play.
2. **Sprawdź konsolę** — powinien pojawić się raport `[GameAssetRegistry] === Asset Report ===` z liczbą slotów FOUND/MISSING. Nie ma error / NullRef.
3. MainMenu → Nowa Gra → Character Creation → World.
4. **Druid:** placeholder powinien być wyraźnie humanoidalny (kaptur, broda, kostur, kryształ świecący). Idle = lekki bob; W/A/S/D = bob silniejszy + ręce/nogi się kołyszą; LMB = swing kostura; E = fireball + ręka uniesiona; Q = heal + ręka uniesiona.
5. **Trawa:** wzdłuż ścieżek (pomiędzy bazą a obozami) widać wyraźne kępy `grass01`, gęsto. Na polanach też. Brak różowych materiałów (URP OK).
6. **Gobliny:** w obozach widać humanoidalne placeholdery (torso + hips + głowa z uszami + 4 kończyny + czerwone oczy). Po zbliżeniu → goblin podchodzi (bob + nogi się kołyszą). Atak (Stormtrooper) → swing handMount. Strzelanie (Archer) → swing handMount + lecąca strzała.

### Sprawdź log szczegółowo
W konsoli przy generacji świata powinno być:
```
[WorldGenerator] prefaby świata + placeholdery=N
[WorldGenerator] Trawa rozłożona: ścieżki=X, polany=Y, las rzadko=Z
```
Jeśli `WorldGrassPoolCount==0`:
```
[WorldGenerator] Brak prefabu trawy — pomijam scatter (importuj Fantasy Forest grass01 lub Nature Starter Kit 2).
```

### Symulacja po-imporcie
Gdy user zaimportuje GanzSe / 3D Stylized Goblin / Fantasy Goblin / Nature Starter Kit 2:
- Restart Editora (lub `GameAssetRegistry.Initialize(force: true)` z menu) → re-skan
- Konsola: `FOUND DruidModel → Assets/GanzSe.../Character.prefab` (lub podobne)
- Druid w grze pojawia się jako prawdziwy model, jeśli paczka ma Animator Controller — `CharacterAnimDriver.HasRealAnimator == true`, log `Animator: OK`. Inaczej procedural fallback (na limbach pivota — działa też dla skinned mesh, jeśli pivot jest root modelu).

---

## Kompatybilność / wymagania

- **Unity 6** (`6000.4.7f1`)
- **URP 17.4.0** — wszystkie nowe materiały przez `MaterialFactory.Get` (URP Lit) lub `MaterialUpgrader.UpgradeHierarchy` po Instantiate
- **API:** `Object.FindAnyObjectByType` / `FindFirstObjectByType` (w PlayerController, GoblinBase nie używamy, ale projekt już używa nowych API)
- **MaterialUpgrader na spawnie** — wywoływany w `WorldAssetPlacer.TryInstantiatePrefab`, `PlaceGrass`, `PlaceRuin` oraz `WorldGenerator` po całym Generate()
- **Brak edycji:** `SaveSystem`, `GoblinCamp`, `MainMenu`, `WorldZone`, `GameManager`, `Totem`, `DruidBase`, `PowerSite`, `PlayerHUD`, `PauseMenu`, `UIFactory`, `SettingsManager` — dotknięte tylko nieinwazyjnie (żaden plik z tej listy nie był zmieniany)

---

## Zmienione pliki w tej iteracji

```
Assets/_Game/Scripts/Character/CharacterAnimDriver.cs        (NEW)
Assets/_Game/Scripts/Player/PlayerBuilder.cs                 (rebuilt placeholder + driver wiring)
Assets/_Game/Scripts/Player/PlayerController.cs              (animDriver ref + SetSpeed/SetDead)
Assets/_Game/Scripts/Player/PlayerCombat.cs                  (animDriver ref + TriggerAttack/Cast)
Assets/_Game/Scripts/Enemies/GoblinBase.cs                   (limb placeholder + driver wiring)
Assets/_Game/Scripts/Enemies/GoblinStormtrooper.cs           (NotifyAttackAnim w DealDamage)
Assets/_Game/Scripts/Enemies/GoblinArcher.cs                 (NotifyAttackAnim w ShootArrow)
Assets/_Game/Scripts/Core/GameAssetRegistry.cs               (Slot.Grass + WorldGrassPool + tokens + report)
Assets/_Game/Scripts/Core/GameAssetPaths.cs                  (WorldGrass paths + Nature/Models)
Assets/_Game/Scripts/Core/AssetCatalog.cs                    (Grass accessor)
Assets/_Game/Scripts/World/WorldAssetPlacer.cs               (PlaceGrass + scatter helpers)
Assets/_Game/Scripts/World/WorldGenerator.cs                 (BuildGrassDecoration + ClearingCenters)
docs/TEAM_STATUS.md                                          (changelog entries)
docs/ASSET_CATALOG.md                                        (NEW — ten plik)
```

NIE dotknięte:
```
SaveSystem, SaveData, GameManager, GoblinCamp, Totem, DruidBase, PowerSite,
WorldZone, MainMenuBootstrap, CharacterCreationBootstrap, PlayerHUD,
PauseMenu, SettingsMenuUI, UIFactory, ThirdPersonCamera, FireballProjectile,
GoblinArrow, MaterialFactory, MaterialUpgrader, GameAudioManager
```

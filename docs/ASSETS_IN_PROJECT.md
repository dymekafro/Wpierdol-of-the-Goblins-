# ASSETS_IN_PROJECT — skan `Assets/` (2026-05-28)

**Projekt:** Wpierdol of the Goblins  
**Unity:** 6000.4.7f1 (URP 17.4)  
**Skan dysku:** tylko foldery w `Assets/` (bez `Packages/`)

---

## TL;DR

User zadeklarował **~16 paczek Asset Store**. Na dysku w repozytorium są **2 paczki** (+ standard Unity: TMP, Scenes, Settings, TutorialInfo).

| Stan | Liczba | Prefaby `.prefab` w `Assets/` |
|------|--------|--------------------------------|
| **Na dysku (IMPORTED)** | 2 / ~15 | **221+** (GanzSe modular parts + 2 drzewa/trawa) |
| **Brak folderu (MISSING)** | ~13 | 0 |
| **Wykluczona** | 1 (Lava Tube) | — |

**Co jest już w grze?** Fantasy Forest (świat) + **GanzSe URP** (druid gracza). Pozostałe paczki wymagają reimportu — kod (`GameAssetRegistry`, `WorldAssetPlacer`, `PlayerBuilder`) podłączy je automatycznie (raport w Console przy Play: `[GameAssetRegistry]`).

**Log druida przy Play:** `[PlayerBuilder] Attached GanzSe model: Assets/URP GanzSe Free Modular Character Pack/.../GanzSe Free Modular Character Update 1_1.prefab`

---

## Top-level `Assets/` (bez standardu Unity)

| Folder | Typ |
|--------|-----|
| `Fantasy Forest Environment Free Sample` | **paczka usera** |
| `URP GanzSe Free Modular Character Pack` | **paczka usera** (druid) |
| `_Game` | kod gry, sceny, materiały |
| `_TerrainAutoUpgrade` | auto z importu Fantasy Forest |
| `Scenes`, `Settings`, `TextMesh Pro`, `TutorialInfo` | standard / template |

---

## Tabela paczek

| Pack | Folder exists? | Key prefabs / assets | Used in code? | Status |
|------|----------------|----------------------|---------------|--------|
| **Fantasy Forest Environment Free Sample** | ✅ `Assets/Fantasy Forest Environment Free Sample/` | `Meshes/Prefabs/tree_1.prefab`, `Meshes/Prefabs/grass01.prefab` | `GameAssetRegistry` TreeLarge/Grass, `WorldAssetPlacer` | **IMPORTED — WIRED** |
| **Nature Starter Kit 2** | ❌ | `Assets/Nature Starter Kit 2/Prefabs/Tree.prefab`, `Bush.prefab`, `Grass.prefab` | `GameAssetPaths.WorldTrees/Bushes/Grass`, world pools | **MISSING — re-import** |
| **Rocks HD / Rock_pack** | ❌ | `Assets/Rock_pack/Prefabs/Rock_01.prefab` | `PickWorldRock`, `PlaceRock` | **MISSING — re-import** |
| **RPG Dungeon Pack / Kit** | ❌ | `Assets/RPG Dungeon Kit/Prefabs/Wall.prefab`, `Pillar.prefab` | `PickWorldRuin`, `PlaceRuin`, obozy | **MISSING — re-import** |
| **GanzSe FREE Modular Character (URP)** | ✅ `Assets/URP GanzSe Free Modular Character Pack/` | `Prefabs/Modular Character/GanzSe Free Modular Character Update 1_1.prefab` | `PlayerBuilder` → `DruidModel` slot | **IMPORTED — WIRED** |
| **Starter Assets Third Person URP** | ❌ | `.../PlayerArmature.prefab` | `PlayerBuilder` fallback druid | **MISSING — re-import** |
| **3D Stylized Goblin** | ❌ | `.../Goblin_Warrior.prefab` | `GoblinStormtrooper` → `GoblinModel` | **MISSING — re-import** |
| **Stylized Goblins Archer & Warrior** | ❌ | `Goblin_Archer.prefab`, `Goblin_Warrior.prefab` | `GoblinArcher`, melee fallback | **MISSING — re-import** |
| **Fantasy Goblin** | ❌ | `Goblin_Shaman.prefab`, `Fantasy Goblin.prefab` | `GoblinShamanElite` → `GoblinElite` | **MISSING — re-import** |
| **Basic RPG Sounds** | ❌ | `hit.wav`, `death.wav`, `fireball.wav` | `GameAudioManager`, `GameAssetRegistry` Sfx* | **MISSING — re-import** |
| **Fantasy Free GUI** | ❌ | `Sprites/panel.png`, `bar_red.png`, … | `MainMenuBootstrap`, `PlayerHUD`, `UIFactory` | **MISSING — re-import** |
| **Modern RPG icons** | ❌ | `Icons/sword.png`, `fireball.png` | `PlayerHUD` skill icons | **MISSING — re-import** |
| **Cartoon FX Remaster (JMO)** | ❌ | `CFXR Fireball.prefab`, `CFXR2 Healing.prefab` | `WorldAssetPlacer.TrySpawnVfx` | **MISSING — re-import** |
| **Fantasy Effects Pack** | ❌ | `Prefabs/Fireball.prefab` | VFX fallback | **MISSING — re-import** |
| **Celestial Cycles** | ❌ | skybox / cycle asset | `GoldenHourLighting` (opcjonalnie) | **MISSING — re-import** |
| ~~Lava Tube~~ | — | — | **NIE UŻYWAĆ** | **EXCLUDED** |

---

## Co jest podłączone w kodzie (gotowe na import)

| Slot / rola | Plik | Źródło assetu |
|-------------|------|----------------|
| Druid | `PlayerBuilder` → `WorldAssetPlacer.Druid` | GanzSe **lub** Starter Assets `PlayerArmature` |
| Goblin melee | `GoblinStormtrooper` / `GoblinBase` | 3D Stylized Goblin `Goblin_Warrior` |
| Goblin archer | `GoblinArcher` | Stylized Goblins `Goblin_Archer` |
| Goblin elite | `GoblinShamanElite` | Fantasy Goblin `Goblin_Shaman` |
| Drzewa / trawa | `WorldAssetPlacer`, `WorldGenerator` | Fantasy Forest + Nature Starter Kit |
| Skały | `PlaceRock` | Rock_pack / Rocks HD |
| Ruiny | `PlaceRuin`, `DecorateCamp` | RPG Dungeon |
| SFX | `GameAudioManager` | Basic RPG Sounds (+ fuzzy scan) |
| UI | `PlayerHUD`, menu | Fantasy Free GUI + Modern RPG icons |
| VFX | `PlayerCombat`, `TrySpawnVfx` | CFXR / Fantasy Effects |

Centralny rejestr: `Assets/_Game/Scripts/Core/GameAssetRegistry.cs`  
Katalog paczek: `Assets/_Game/Scripts/Core/GameAssetPacks.cs`  
Ścieżki kandydackie: `Assets/_Game/Scripts/Core/GameAssetPaths.cs`

---

## Raport przy Play (Unity Console)

Po wejściu w Play Mode (lub menu **WPG → Refresh Asset Registry** w Editorze):

```
[GameAssetRegistry] === Imported Packs (Assets/ top-level) ===
  [FOUND  ] Fantasy Forest Environment Free Sample
  [FOUND  ] GanzSe FREE Modular Character (URP)
  [MISSING] Nature Starter Kit 2
  ...
[GameAssetRegistry] === Slot Resolution ===
  TreeLarge      [OK     ] Assets/Fantasy Forest .../tree_1.prefab
  DruidModel     [OK     ] Assets/URP GanzSe Free Modular Character Pack/.../GanzSe Free Modular Character Update 1_1.prefab
  ...
```

---

## Instrukcja: brakujące paczki — jak zaimportować

1. Otwórz projekt w **Unity 6**.
2. **Window → Package Manager** (lub **Asset Store** w przeglądarce).
3. Zakładka **My Assets** → znajdź paczkę → **Download** (jeśli trzeba) → **Import**.
4. W oknie Import zaznacz całą paczkę → **Import** — folder musi pojawić się w `Assets/` (np. `Assets/Nature Starter Kit 2/`).
5. **Play** — sprawdź log `[GameAssetRegistry]`; status paczki powinien zmienić się z `MISSING` na `FOUND`.
6. Dla URP: po imporcie modeli uruchom **Edit → Rendering → Materials → Convert Selected Built-in Materials to URP** (lub użyj istniejącego `MaterialUpgrader` w grze).

**Uwaga:** Import do **innego** folderu Unity na dysku nie aktualizuje tego repozytorium — assety muszą być w `Assets/` **tego** projektu (commit do gita jeśli chcesz je trzymać w repo).

### Opcjonalny fallback bez Asset Store

Skopiuj wybrane pliki do:

- `Assets/_Game/Resources/UI/` — sprites (patrz `ASSET_IMPORT.txt`)
- `Assets/_Game/Resources/Audio/` — klipy WAV

Nazwy zgodne z `GameAssetPaths.Res*` constants.

---

## GanzSe — kluczowe assety (IMPORTED)

| Typ | Ścieżka |
|-----|---------|
| **Druid (używany)** | `Assets/URP GanzSe Free Modular Character Pack/Prefabs/Modular Character/GanzSe Free Modular Character Update 1_1.prefab` |
| Fallback | `.../GanzSe Free Modular Character Original 1_0.prefab` |
| Części modularne | `Prefabs/Non-Skinned Mesh Parts/` + `Prefabs/Modular Character/` (setki partów) |
| FBX rig | `Models/Models Update 1.1/GanzSe Free Modular Character 1_1.fbx` |
| Demo | `Demo/Scenes/URP GanzSe Free Modular Character Demo.unity` |

Loadout druida w runtime: `ConfigureGanzSeDruidLoadout` — jedna część na kategorię (zielony Color 1 + brązowy pas), URP `MaterialUpgrader`, tint szat, `CharacterController` height 1.85 m. Animator bez controllera → proceduralny `CharacterAnimDriver` (Speed gdy controller się pojawi).

---

## `_Game/Prefabs` — brak na dysku

Folder `Assets/_Game/Prefabs/` ma tylko `.meta` — **nie ma** własnych prefabów gry.  
Registry próbuje je najpierw, potem przechodzi na paczki Asset Store. To oczekiwane w tym repo.

---

## Powiązane dokumenty

- `docs/ASSET_AUDIT.md` — audyt team lead (ten sam wniosek)
- `docs/TEXTURES_AND_ASSETS.md` — lista zamierzonych paczek
- `Assets/_Game/Scripts/Core/GameAssetPaths.cs` — pełna lista ścieżek

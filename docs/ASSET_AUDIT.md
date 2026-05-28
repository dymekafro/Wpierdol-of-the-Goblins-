# ASSET AUDIT — Wpierdol of the Goblins

**Data audytu:** 2026-05-28
**Audytor:** TEAM LEAD (Plan Manager)
**Unity:** 6000.4.7f1 (Unity 6)
**URP:** com.unity.render-pipelines.universal 17.4.0
**AI Navigation:** 2.0.12 (NavMesh)
**Input System:** 1.19.0 (new)

---

## TL;DR — Co realnie jest w `Assets/`

User zadeklarował **17 paczek**. Po skanie `Assets/` znaleziono **fizycznie zaimportowaną 1 paczkę**:

| Stan | Liczba paczek | Prefaby (.prefab) |
|------|---------------|---------------------|
| **IMPORTED** | 1 / 17 | **2** (`grass01.prefab`, `tree_1.prefab`) |
| **DECLARED, NOT FOUND** | 16 / 17 | 0 |
| **EXCLUDED (user)** | 1 (Lava Tube) | — nie używać |

> **WNIOSEK:** Większość paczek user dopiero zaimportuje (Asset Store / Package Manager) **albo** są już w koncie ale jeszcze nie ściągnięte do projektu. Agenci implementacyjni muszą działać **dwutorowo**:
> 1. Tryb placeholder (już działa: `WorldGenerator` z primitives) — **fallback obecny w kodzie**.
> 2. Tryb assetów (gdy paczka się pojawi) — `GameAssetRegistry` ma ścieżki kandydackie i przełącza się automatycznie.

---

## Tabela: Paczka | Folder | Kluczowe prefaby | URP? | Użycie | Status

> Kolumna „Status": **IMPORTED** = pliki w `Assets/`, **PENDING** = user zadeklarował ale brak na dysku, **EXCLUDED** = nie używać, **STD** = standardowy Unity.

| Paczka | Folder w `Assets/` | Kluczowe prefaby (pełne ścieżki) | URP? | Użycie w grze | Status |
|--------|--------------------|----------------------------------|------|---------------|--------|
| **Fantasy Forest Environment Free Sample** | `Assets/Fantasy Forest Environment Free Sample/` | `Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab`, `Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab` | Built-in (wymaga konwersji materiałów na URP Lit lub MaterialFactory) | Drzewa MVP (`tree_1`), trawy (`grass01`); tekstury kory, ziemi, trawy | **IMPORTED** |
| **Nature Starter Kit 2** | `Assets/Nature Starter Kit 2/` (oczekiwane) | Oczekiwane: `Assets/Nature Starter Kit 2/Prefabs/Tree_*.prefab`, `Bush_*.prefab`, `Grass_*.prefab` | Wymaga URP convert | Las — drzewa, krzaki, paprocie, kamienie | **PENDING** |
| **Rocks HD / Rock_pack** | `Assets/Rock_pack/` lub `Assets/Rocks HD/` (oczekiwane) | Oczekiwane: `Assets/Rock_pack/Prefabs/Rock_*.prefab` (Rock_01..Rock_08) | Wymaga URP convert | Skały w lesie, kamienne kręgi (PowerSite), ruiny | **PENDING** |
| **RPG Dungeon Kit** | `Assets/RPG Dungeon Kit/` (oczekiwane) | Oczekiwane: `Assets/RPG Dungeon Kit/Prefabs/Wall_*.prefab`, `Pillar_*.prefab`, `Floor_*.prefab`, `Altar.prefab`, `Brazier.prefab` | Wymaga URP convert | Ruiny druidów (kamienny ołtarz, kolumny), palisada obozów (alternatywa do Cube) | **PENDING** |
| **GanzSe Character** | `Assets/GanzSe Character/` (oczekiwane) | Oczekiwane: humanoid `.fbx` + materiały; prefab postaci ~`Assets/GanzSe Character/Prefabs/*.prefab` | Wymaga konwersji shader | **DRUID — bohater gracza**. Zastąpi capsule placeholder w `PlayerBuilder` | **PENDING** |
| **3D Stylized Goblin** | `Assets/3D Stylized Goblin/` (oczekiwane) | Oczekiwane: `Goblin.prefab`, `Goblin_Stormtrooper.prefab` | Wymaga URP convert | **Goblin szturmowiec** (`GoblinStormtrooper.cs`) | **PENDING** |
| **Fantasy Goblin** | `Assets/Fantasy Goblin/` (oczekiwane) | Oczekiwane: `Goblin_Archer.prefab`, `Goblin_Shaman.prefab` | Wymaga URP convert | **Goblin łucznik** / **szaman** (`GoblinArcher.cs`, future Shaman) | **PENDING** |
| **Fantasy effects** | `Assets/Fantasy effects/` (oczekiwane) | Oczekiwane: VFX prefaby Particle System: `VFX_Fire.prefab`, `VFX_Heal.prefab`, `VFX_Magic_Cast.prefab`, `VFX_Explosion.prefab` | Wymaga URP convert (Particle Lit) | VFX czarów druida (Fireball, Heal), totem destroyed, capture ritual | **PENDING** |
| **Legacy Particle** | `Assets/Legacy Particle/` lub `Assets/Standard Assets/Particles/` (oczekiwane) | Particles legacy `.prefab` | Built-in — refactor na nowy ParticleSystem albo użyć tylko stylowo | Dym, kurz, ogniska (alternatywa) | **PENDING** |
| **Cartoon FX Projectile** | `Assets/Cartoon FX Projectile/` (oczekiwane) | Oczekiwane: `Projectile_Fireball.prefab`, `Projectile_Arrow.prefab`, `Projectile_Spark.prefab` | Wymaga URP convert | Pociski: druid fireball (`FireballProjectile.cs`), strzała goblinów (`GoblinArrow.cs`) | **PENDING** |
| **Fantasy Free GUI** | `Assets/Fantasy Free GUI/` (oczekiwane) | Sprite'y UI: `panel.png`, `button.png`, `bar_background.png`, `bar_red.png`, `bar_blue.png`, `icon_frame.png`, `menu_background.png` (już zapisane w `GameAssetPaths.cs`) | UI — N/A | Main menu, character creation, HUD, pause menu | **PENDING** |
| **Free UI Pack** | `Assets/Free UI Pack/` (oczekiwane) | Alternatywne sprite'y UI, fonty | UI — N/A | Fallback dla Fantasy Free GUI (jeśli braki) | **PENDING** |
| **Modern RPG icons** | `Assets/Modern RPG icons/` (oczekiwane) | Ikony: `sword.png`, `fireball.png`, `heal.png` (zapisane w `GameAssetPaths.cs`) | UI — N/A | HUD: ikony skilli i czarów; inventory; drzewko Przyrody | **PENDING** |
| **Basic RPG Sounds** | `Assets/Basic RPG Sounds/` (oczekiwane) | `.wav/.ogg`: `hit.wav`, `death.wav`, `ui_click.wav`, `fireball.wav`, `footstep.wav` (zapisane w `GameAssetPaths.cs`) | Audio — N/A | SFX walki, UI, kroków, czarów | **PENDING** |
| **Celestial Cycles** | `Assets/Celestial Cycles/` (oczekiwane) | Skybox / cycle prefab, ScriptableObject z presetami | URP-friendly (zazwyczaj) | **Golden hour** ambient (mroczny las z ciepłym akcentem na horyzoncie); dzień/noc cycle (MVP: stała pora) | **PENDING** |
| **Starter Assets Third Person URP** | `Assets/Starter Assets/`, `Assets/StarterAssets/` lub `Packages/com.unity.starterassets.thirdperson` (oczekiwane) | `Nested Parent Armature.prefab`, `PlayerArmature.prefab`, animator controller `StarterAssetsThirdPerson.controller` | URP native | Animacje druida (idle/walk/run/jump). Alternatywa dla custom controller. **Konflikt z istniejącym `PlayerController.cs`** — patrz NIE PSUĆ. | **PENDING** |
| **Control Rig** | `Packages/com.unity.animation.rigging` lub `Assets/Control Rig/` (oczekiwane) | Constraints (IK), rig setup | N/A (system) | IK rąk druida do kostura, head look-at | **PENDING** |
| ~~Lava Tube~~ | — | — | — | **NIE UŻYWAĆ** (decyzja usera). Nie pasuje do magicznego lasu. | **EXCLUDED** |

---

## Już istniejące w projekcie (NIE z paczek user)

| Element | Ścieżka | Uwagi |
|---------|---------|-------|
| Materiały placeholder | `Assets/_Game/Materials/Goblin_Mat.mat`, `Ground_Mat.mat` | URP Lit |
| Sceny | `Assets/_Game/Scenes/MainMenu.unity`, `CharacterCreation.unity`, `World.unity` | World.unity jest pusta (cały świat budowany przez `WorldBootstrap.cs`) |
| Terrain layery | `Assets/_TerrainAutoUpgrade/layer_*.terrainlayer` | Z Fantasy Forest |
| TextMesh Pro | `Assets/TextMesh Pro/` | STD |
| InputActions | `Assets/InputSystem_Actions.inputactions` | STD |

---

## Co już jest w kodzie (registry, fallback)

Plik `Assets/_Game/Scripts/Core/GameAssetPaths.cs` zawiera **kandydackie ścieżki** dla:

- Fantasy Free GUI: `GuiPanel`, `GuiButton`, `GuiBarBackground`, `GuiBarFillHp`, `GuiBarFillMana`, `GuiIconFrame`, `GuiMenuBackground`
- Modern RPG icons: `IconMelee`, `IconFireball`, `IconHeal`
- Basic RPG Sounds: `SfxHit`, `SfxDeath`, `SfxUIClick`, `SfxFireballCast`, `SfxFootstep`
- Resources fallback: `Res*` (gdyby user przeniósł assety do `Resources/`)

**Brak** w obecnym registry:
- Prefaby drzew, skał, krzaków (Nature Starter Kit 2, Rocks HD, Fantasy Forest)
- Prefab Druida (GanzSe Character)
- Prefaby goblinów (3D Stylized Goblin, Fantasy Goblin)
- Prefaby VFX (Fantasy effects, Cartoon FX Projectile)
- Prefaby ruin (RPG Dungeon Kit)
- Celestial Cycles preset

➜ **Faza F2 (GameAssetRegistry)** musi to uzupełnić — szczegóły w `IMPLEMENTATION_PLAN.md`.

---

## Aktualne placeholdery (do zastąpienia)

`WorldGenerator.cs` (Assets/_Game/Scripts/World/WorldGenerator.cs) **proceduralnie buduje** świat z primitives:

| Placeholder | Element | Co go zastąpi (po pojawieniu się paczek) |
|-------------|---------|------------------------------------------|
| `Cylinder + Sphere` | Drzewo | `tree_1.prefab` (Fantasy Forest) lub Nature Starter Kit 2 |
| `Sphere` (rock) | Skała | Rocks HD / Rock_pack prefab |
| `Sphere` (bush) | Krzak | Nature Starter Kit 2 / Fantasy Forest |
| `Cylinder + Sphere` (mushroom) | Świecący grzyb | Custom prefab (zostawić proceduralnie + emisja) |
| `Cube` (palisade stick) | Pal palisady | RPG Dungeon Kit (Wall_Wooden) |
| `Capsule + Cylinder` (druid) | Druid | GanzSe Character |
| `MoonLight` directional | Oświetlenie | Celestial Cycles preset („golden hour mroczny") |
| `Cube + Cylinder` (logs/flame) | Ognisko obozu | Fantasy effects + Cartoon FX |

---

## Compat & Pitfalls

| Problem | Detal | Mitigacja |
|---------|-------|-----------|
| Built-in shader → URP | Fantasy Forest Environment ma własny `StandardNoCulling.shader` i materiały Built-in (z 2019). | `Window → Rendering → Render Pipeline Converter → Built-in to URP`. Albo zostawić jeśli renderuje się poprawnie w MVP. |
| Materiały pink (różowe) | Brak konwersji do URP po imporcie | Materiały: użyć URP/Lit lub MaterialFactory podmienić runtime. |
| Animator collision z PlayerController | Starter Assets Third Person URP **ma własny `ThirdPersonController.cs`** | **NIE zastępować** istniejącego `PlayerController.cs`. Użyć tylko armatury + animator controller — fizyka i input zostają WPG. |
| Goblin animacje | Brak na MVP | Mixamo upload lub animator z paczki goblina. Agent C użyje placeholder Animator. |
| Lava Tube w paczce | User wykluczył | Nie importować, nie linkować w registry. |

---

## Statystyki audytu

```
Paczki zadeklarowane przez usera: 17
Paczki zaimportowane (na dysku):   1 (Fantasy Forest Environment Free Sample)
Paczki PENDING (oczekiwane):       16
Paczki EXCLUDED:                   1 (Lava Tube — nie wymieniona w user pack, ale wykluczona explicitnie)

Prefaby found (.prefab):             2
  - Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab
  - Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab

Materiały Fantasy Forest:            6 (bark01_bottom, dirt01, grass01, grassmesh, skyMaterial, tree_branches)
Tekstury Fantasy Forest:             6 (bark01_bottom.tga, dirt01.tga, grass01.tga, grass01_b.tga, grassmesh.png, tree_branches.png)
Materiały WPG (już zrobione):        2 (Goblin_Mat, Ground_Mat)
Skrypty WPG:                         36 plików .cs
Sceny:                               3 (MainMenu, CharacterCreation, World) — wszystkie istnieją, World pusta (proc-gen)
```

---

## Kolejne kroki (skrót dla zespołu)

1. **TEAM LEAD (już zrobione)** — audyt, plan.
2. **Agent A (Core & Registry)** — rozbudowa `GameAssetPaths.cs` → `GameAssetRegistry` (load `.prefab` przez `AssetDatabase.LoadAssetAtPath` w editorze + `Resources` fallback).
3. **Agent B (World & Environment)** — podmiana primitives w `WorldGenerator` na prefaby z Fantasy Forest (już dostępne) + readiness dla Nature Starter Kit / Rocks HD.
4. **Agent C (Characters & Combat)** — gdy GanzSe + goblin packs są dostępne, podmienić `PlayerBuilder` i goblin spawn.
5. **Agent D (UI Audio Polish)** — gdy Fantasy Free GUI / Modern RPG icons / Basic RPG Sounds są dostępne, podpiąć przez `GameAssetRegistry`.

Pełne szczegóły: **`docs/AGENT_TASKS.md`**, **`docs/IMPLEMENTATION_PLAN.md`**, **`docs/TEAM_WORKFLOW.md`**.

# AGENT TASKS v2 — Wpierdol of the Goblins

**Wersja:** 2.0 (2026-05-28)
**Format:** gotowe prompty copy-paste dla agentów A / B / C / D.

> **Każdy agent:**
> 1. **NAJPIERW** czyta `docs/ASSET_AUDIT.md`, `TEAM_WORKFLOW.md`, `IMPLEMENTATION_PLAN.md`.
> 2. Aktualizuje `docs/TEAM_STATUS.md`: swój task `in_progress`, autor, data.
> 3. Robi swoją fazę.
> 4. Zaznacza checkboxy w `TEAM_STATUS.md`, dopisuje zmiany do Changelog.
> 5. **NIE commituje** — tylko user.

---

## 🟢 Agent A — Core & Registry

```
ROLA: Agent A — Core & Registry. Specjalizujesz się w infrastrukturze, kompilacji i single source of truth.

REPO: /Users/patrykskoczylas/Wpierdol-of-the-Goblins--2026-05-27-21-04-21
Unity 6000.4.7f1 (Unity 6), URP 17.4.0.

KONTEKST WEJŚCIOWY (przeczytaj w tej kolejności):
  1. docs/TEAM_WORKFLOW.md           — zasady żelazne, NIE PSUĆ, konwencje
  2. docs/ASSET_AUDIT.md             — co jest a czego brak w Assets/
  3. docs/IMPLEMENTATION_PLAN.md     — Twoje fazy F1 + F2
  4. docs/TEAM_STATUS.md             — sprawdź czy nikt nie ma in_progress na Core/

CEL (F1 + F2):
  F1 — Kompilacja 0 errors w Unity 6 (deprecated API fix).
  F2 — Stworzyć GameAssetRegistry: jedno API do ładowania prefab/sprite/audio
       z kandydackimi ścieżkami i fallbackiem na primitives / Resources / null.

OUTPUT (pliki):
  Edytuj:
    Assets/_Game/Scripts/Core/GameAssetPaths.cs        (rozszerz o prefab paths)
    Assets/_Game/Scripts/**/*.cs                       (jeśli są deprecated calls)
    Assets/_Game/Scripts/UI/UIFactory.cs               (przejście na Registry)
    Assets/_Game/Scripts/Player/PlayerBuilder.cs       (try Registry przed fallback)
  Nowe:
    Assets/_Game/Scripts/Core/GameAssetRegistry.cs
    (opcjonalnie) Assets/_Game/Scripts/Core/GameAssetLoader.cs

API REGISTRY (przykład):
  public static class GameAssetRegistry {
      public static GameObject TryLoadPrefab(string[] candidatePaths);
      public static GameObject TryLoadPrefabResources(string resPath);
      public static Sprite     TryLoadSprite(string[] candidatePaths);
      public static AudioClip  TryLoadAudio(string[] candidatePaths);
      public static T          TryLoadAny<T>(string[] candidatePaths) where T : UnityEngine.Object;
  }
  Implementacja:
    #if UNITY_EDITOR → AssetDatabase.LoadAssetAtPath<T>(path)
    #else            → Resources.Load<T>(resPath) (rolowane przez kandydatów Res*)
    Cache wyniku per ścieżka.

ŚCIEŻKI DO DODANIA (w GameAssetPaths.cs):
  // --- Trees / Foliage ---
  TreeMain[]:
    "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/tree_1.prefab"   ← FOUND
    "Assets/Nature Starter Kit 2/Prefabs/Tree_01.prefab"
    "Assets/Nature Starter Kit 2/Prefabs/Tree_02.prefab"
  GrassPatch[]:
    "Assets/Fantasy Forest Environment Free Sample/Meshes/Prefabs/grass01.prefab"  ← FOUND
    "Assets/Nature Starter Kit 2/Prefabs/Grass_01.prefab"
  // --- Rocks ---
  RockSmall[], RockLarge[]:
    "Assets/Rock_pack/Prefabs/Rock_01.prefab" ... Rock_08
    "Assets/Rocks HD/Prefabs/Rock_*.prefab"
  // --- Druid character ---
  DruidPrefab[]:
    "Assets/GanzSe Character/Prefabs/GanzSeMale.prefab"
    "Assets/GanzSe Character/Prefabs/GanzSe.prefab"
    "Assets/GanzSe Character/GanzSeMale.prefab"
    "Assets/GanzSe Character/Models/GanzSe.fbx"
  // --- Goblins ---
  GoblinStormtrooperPrefab[]:
    "Assets/3D Stylized Goblin/Prefabs/Goblin.prefab"
    "Assets/3D Stylized Goblin/Prefabs/Goblin_Warrior.prefab"
  GoblinArcherPrefab[]:
    "Assets/Fantasy Goblin/Prefabs/Goblin_Archer.prefab"
    "Assets/Fantasy Goblin/Prefabs/Goblin.prefab"
  // --- VFX ---
  VFX_FireballCast[]:
    "Assets/Fantasy effects/Prefabs/VFX_Fire_Cast.prefab"
    "Assets/Cartoon FX Projectile/Prefabs/CFX_Fireball_Cast.prefab"
  VFX_FireballHit[]:
    "Assets/Fantasy effects/Prefabs/VFX_Fire_Impact.prefab"
    "Assets/Cartoon FX Projectile/Prefabs/CFX_Fireball_Hit.prefab"
  VFX_Heal[]:
    "Assets/Fantasy effects/Prefabs/VFX_Heal.prefab"
  VFX_TotemDestroyed[]:
    "Assets/Cartoon FX Projectile/Prefabs/CFX_Explosion.prefab"
  // --- Ruins (RPG Dungeon Kit) ---
  RuinPillar[]:
    "Assets/RPG Dungeon Kit/Prefabs/Pillar_Stone.prefab"
  RuinAltar[]:
    "Assets/RPG Dungeon Kit/Prefabs/Altar.prefab"
  RuinWallWood[]:
    "Assets/RPG Dungeon Kit/Prefabs/Wall_Wooden.prefab"
  RuinGate[]:
    "Assets/RPG Dungeon Kit/Prefabs/Gate.prefab"
  // --- Celestial Cycles ---
  CelestialCyclePreset[]:
    "Assets/Celestial Cycles/Presets/GoldenHour.asset"
  // ⛔ NIE DODAWAJ: Assets/Lava Tube/...  (wykluczone przez usera)

TEST PLAN:
  1. Editor → Console: 0 errors, 0 nowych warnings.
  2. Otwórz World.unity, Play.
  3. Sprawdź log w Console:
     [GameAssetRegistry] tree_1.prefab    → FOUND
     [GameAssetRegistry] grass01.prefab   → FOUND
     [GameAssetRegistry] Rock_01.prefab   → NOT FOUND (oczekiwane — paczka PENDING)
     [GameAssetRegistry] GanzSe.prefab    → NOT FOUND (oczekiwane)
  4. UIFactory używa Registry. Main menu wciąż wyświetla buttony (fallback color).
  5. Save → quit → Continue → wczytuje pozycję.
  6. SettingsManager.ApplySettings() nie regresuje (głośność, FPS cap).

NIE RÓB (scope):
  - NIE zmieniaj SaveSystem.cs / SettingsData.cs publicznego API.
  - NIE zmieniaj WorldGenerator.cs (to scope Agenta B).
  - NIE zmieniaj GoblinCamp.cs state machine.
  - NIE refaktoryzuj UI / HUD layoutu (to scope Agenta D).
  - NIE importuj nowych paczek — Twoja praca jest „registry-ready" bez nich.
  - NIE commituj.

DEPENDS ON: nic (Ty jesteś bottleneck — agenci B/C/D czekają).
ODBLOKOWUJESZ: B, C, D.

ZAKOŃCZENIE:
  - Update docs/TEAM_STATUS.md: [x] F1, [x] F2, autor: Agent A, data.
  - Dopisz w Changelog: "F1+F2: Registry działa, ładuje 2 prefaby (tree_1, grass01), pozostałe PENDING (oczekiwane)."
```

---

## 🟢 Agent B — World & Environment

```
ROLA: Agent B — World & Environment. Specjalizujesz się w terrain, foliage, lighting URP, atmosferze.

REPO: /Users/patrykskoczylas/Wpierdol-of-the-Goblins--2026-05-27-21-04-21
Unity 6000.4.7f1, URP 17.4.0, AI Navigation 2.0.12.

KONTEKST WEJŚCIOWY:
  1. docs/TEAM_WORKFLOW.md
  2. docs/ASSET_AUDIT.md             (sprawdź sekcję "Już istniejące", "Aktualne placeholdery")
  3. docs/IMPLEMENTATION_PLAN.md     (Twoje fazy F3 + F6)
  4. docs/WORLD_DESIGN.md            (placement obozów — TRZYMAJ pozycje)
  5. docs/TEAM_STATUS.md

WARUNEK STARTU:
  Agent A → F1+F2 muszą być [x] completed. Jeśli nie — STOP, dopisz Blocker w TEAM_STATUS.

CEL (F3 + F6):
  F3 — Las wygląda jak magiczny ciemny las: drzewa tree_1 zamiast Cylinder+Sphere,
       trawy klustry, ścieżki teksturowane, ruiny placeholder, golden hour ambient,
       NavMesh.
  F6 — 5 obozów wg WORLD_DESIGN.md (już są w WorldGenerator.cs — zweryfikuj),
       palisada drewniana, totem 3D, kamienny ołtarz w bazie, save point trigger.

OUTPUT (pliki):
  Edytuj:
    Assets/_Game/Scripts/World/WorldGenerator.cs     (SpawnTree, SpawnBush, BuildCamp, BuildDruidBase)
    Assets/_Game/Scripts/World/WorldBootstrap.cs     (NavMesh bake post-Generate)
    Assets/_Game/Scripts/Enemies/Totem.cs            (wizual jeśli proceduralny)
  Nowe (opcjonalnie):
    Assets/_Game/Scripts/World/CelestialCycleBinder.cs   (jeśli paczka Celestial dostępna)
    Assets/_Game/Scripts/World/NavMeshBaker.cs           (helper runtime bake)
    Assets/_Game/Scripts/World/SavePoint.cs              (stub interaktywny w bazie)
    Assets/_Game/Scripts/World/CraftingStation.cs        (stub — Agent C wypełni)

F3 — Świat (CHECKLIST):
  [ ] SpawnTree: GameAssetRegistry.TryLoadPrefab(GameAssetPaths.TreeMain). Jeśli prefab → Instantiate.
      Jeśli null → bieżący Cylinder+Sphere fallback (zostawić).
  [ ] SpawnBush: tak samo z GrassPatch / fallback Sphere.
  [ ] BuildGround: ścieżki — zamiast solid color Cube, użyj materiału z teksturą "dirt01"
      z Fantasy Forest (Assets/Fantasy Forest Environment Free Sample/Materials/dirt01.mat).
      Jeśli nie ma — zostaw fallback.
  [ ] BuildAmbientLight: spróbuj załadować preset CelestialCyclePreset[]; jeśli null,
      przemodeluj MoonLight na "golden hour":
        color = (0.95, 0.75, 0.55)  // ciepły horyzont
        intensity = 0.6f
        ambientSky = (0.18, 0.15, 0.22)  // mroczny purpur
        fogColor = (0.10, 0.09, 0.12)
        fogDensity = 0.020f
  [ ] BuildVegetation: zmień treeCount = 320 → 250 jeśli FPS spada poniżej 45.
  [ ] Po Generate() wywołaj NavMeshBaker.Bake(worldRoot)
      (com.unity.ai.navigation: NavMeshSurface.AddData / BuildNavMesh).
      Markuj ground i palisady jako walkable.
  [ ] (Opcjonalnie) URP Volume Profile w bootstrap: Bloom intensity 0.6, Vignette 0.35.

F6 — Obozy + Baza (CHECKLIST):
  [ ] Zweryfikuj: WorldGenerator.BuildCamps() ma już 5 CampDef
      (goblin_camp_first_clearing, forest_den, ember_moss, lost_roots, shade_glade).
      Wszystkie spawn'ują i są widoczne w scenie.
  [ ] Palisada: zamień Cube 0.25x2.2x0.25 na RuinWallWood prefab (jeśli dostępne).
      Fallback: bieżący cube + materiał z teksturą bark01_bottom z Fantasy Forest.
  [ ] Bramka: w pętli sticks zaplanuj 1 lukę o szerokości 2m, w niej RuinGate prefab
      (jeśli dostępne); fallback: 2 wyższe stick'i obramowujące przejście.
  [ ] Totem: rozbuduj GameObject "Totem" o:
        - cylinder pień (1.5m wysoki, ciemny brąz)
        - rzeźbione "maski" — 3 sphere z emisją runów (czerwone)
        - light point red intensity 2, range 6
  [ ] BuildDruidBase:
        - kamienny krąg: spróbuj RockLarge prefab (Rocks HD); fallback bieżący Cube.
        - centralny ołtarz: RuinAltar prefab; fallback: większy Cube + napis runiczny.
        - drzewo życia: zostaw proceduralne, ale otacz 4-6 prefabami tree_1 (Fantasy Forest).
        - SavePoint stub: świecący kryształ + SphereCollider trigger, komponent SavePoint.cs.
        - CraftingStation stub: niski stół (Cube) + SphereCollider trigger, komponent CraftingStation.cs (stub).
  [ ] SavePoint.cs (stub):
        - implementuje IInteractable (już jest w Core)
        - GetPrompt = "Naciśnij E aby zapisać"
        - Interact: GameManager.Instance.BuildSaveData(...) → SaveSystem.Save(data)
  [ ] CraftingStation.cs (stub):
        - implementuje IInteractable
        - GetPrompt = "Naciśnij E — Crafting (wkrótce)"
        - Interact: Debug.Log("[Crafting] TODO Agent C")

TEST PLAN:
  1. World.unity → Play.
  2. Sprawdź log: "[WorldBootstrap] Wygenerowano: 5 obozów, 2 miejsc mocy".
  3. Wizualnie las jest mroczny z ciepłym akcentem (golden hour).
  4. Drzewa to tree_1, nie cylinder+sphere (przynajmniej część — 2 prefaby jest, fallback dla pozostałych OK).
  5. Idź do bazy → kryształ świeci → E → log "[SaveSystem] Zapisano grę".
  6. Idź do crafting → E → log "[Crafting] TODO Agent C".
  7. Idź do goblin_camp_first_clearing → palisada widoczna (wood lub fallback), totem stoi.
  8. NavMesh: gobliny chodzą (jeśli Agent C zrobił NavMeshAgent na nich; jeśli nie — debug NavMesh visible).
  9. Save → Quit → Continue → spawn w bazie, obozy w stanie sprzed quit.
  10. SaveSystem / SettingsManager nie zregresowane.

NIE RÓB (scope):
  - NIE zmieniaj campId, CampState, GoblinCamp logiki.
  - NIE zmieniaj 5 pozycji obozów (są z WORLD_DESIGN.md).
  - NIE używaj prefabów Lava Tube.
  - NIE zmieniaj UI / HUD (Agent D).
  - NIE zmieniaj PlayerBuilder / Player* (Agent C).
  - NIE refaktoryzuj GameAssetRegistry (Agent A) — tylko go używaj.
  - NIE commituj.

DEPENDS ON: Agent A (F1+F2 completed).
ODBLOKOWUJESZ: F7 (wymaga gotowych poziomów).

ZAKOŃCZENIE:
  - TEAM_STATUS.md: [x] F3, [x] F6, Changelog z liczbą drzew prefab vs fallback.
```

---

## 🟢 Agent C — Characters & Combat

```
ROLA: Agent C — Characters & Combat. Specjalizujesz się w postaciach, Animator, NavMesh AI, VFX integracji.

REPO: /Users/patrykskoczylas/Wpierdol-of-the-Goblins--2026-05-27-21-04-21
Unity 6000.4.7f1, URP 17.4.0, AI Navigation 2.0.12, Animation Rigging (Control Rig).

KONTEKST WEJŚCIOWY:
  1. docs/TEAM_WORKFLOW.md
  2. docs/ASSET_AUDIT.md             (GanzSe, 3D Stylized Goblin, Fantasy Goblin — wszystkie PENDING)
  3. docs/IMPLEMENTATION_PLAN.md     (Twoje fazy F4 + F5a)
  4. docs/GAME_CONCEPT.md            (atrybuty druida: MANA+3 INT+2 END+1 STR-1)
  5. docs/TEAM_STATUS.md

WARUNEK STARTU: Agent A → F1+F2 [x] completed.

CEL (F4 + F5a):
  F4 — Druid: model GanzSe (gdy paczka dostępna) + Animator (Idle/Walk/Run/Cast/Hit/Die)
       Gobliny: prawdziwe modele (Stylized Goblin / Fantasy Goblin) + Animator + NavMeshAgent
  F5a — VFX czarów (Fantasy effects + Cartoon FX) podpięte do FireballProjectile,
        explosion na totem destroyed, ring of light na capture ritual.

OUTPUT (pliki):
  Edytuj:
    Assets/_Game/Scripts/Player/PlayerBuilder.cs       (try Registry GanzSe; zachowaj fallback)
    Assets/_Game/Scripts/Player/PlayerCombat.cs        (Animator triggers)
    Assets/_Game/Scripts/Player/PlayerController.cs    (Speed parameter dla Animator)
    Assets/_Game/Scripts/Player/FireballProjectile.cs  (VFX instancja)
    Assets/_Game/Scripts/Enemies/GoblinStormtrooper.cs (model + Animator + NavMeshAgent)
    Assets/_Game/Scripts/Enemies/GoblinArcher.cs       (model + Animator + NavMeshAgent)
    Assets/_Game/Scripts/Enemies/GoblinArrow.cs        (VFX trail z Cartoon FX)
    Assets/_Game/Scripts/Enemies/Totem.cs              (VFX explosion on Destroy)
    Assets/_Game/Scripts/Enemies/GoblinCamp.cs         (VFX ring on TryCapture — minimalna edycja)
  Nowe:
    Assets/_Game/Scripts/Player/PlayerAnimator.cs      (wrapper)
    Assets/_Game/Scripts/Enemies/GoblinAnimator.cs     (wrapper)
    Assets/_Game/Scripts/Enemies/GoblinNav.cs          (wrapper na NavMeshAgent)
  Wykorzystaj (z Agenta B):
    Assets/_Game/Scripts/World/CraftingStation.cs      (możesz dopełnić logikę crafting)

F4 — Postacie (CHECKLIST):
  [ ] PlayerBuilder.BuildDruid:
        var prefab = GameAssetRegistry.TryLoadPrefab(GameAssetPaths.DruidPrefab);
        if (prefab != null) { Instantiate; podepnij PlayerStats/Controller/Combat na root; }
        else { fallback BuildVisual (capsule) — już jest, zostaw. }
  [ ] Druid Animator parametry: Speed (float, 0-1), Cast (trigger), Hit (trigger), Die (trigger).
  [ ] PlayerController: w Update przekaż Animator.SetFloat("Speed", _ccVelocity.magnitude / runSpeed).
  [ ] PlayerCombat.OnSwing: Animator.SetTrigger("Cast") albo "Attack" w zależności od typu.
  [ ] Control Rig (opcjonalne, jeśli paczka dostępna):
        - TwoBoneIKConstraint na prawej ręce → cel staffTip
        - MultiAimConstraint head → cel kamery / wroga
  [ ] GoblinStormtrooper:
        - var prefab = Registry.TryLoadPrefab(GameAssetPaths.GoblinStormtrooperPrefab);
        - Instantiate jako child + ukryj capsule placeholder
        - Add NavMeshAgent, speed 3.5, stoppingDistance 1.5
        - Animator: Speed, Attack, Die
        - Behavior: w Update jeśli player w range → SetDestination, gdy distance < 2 → SetTrigger("Attack")
  [ ] GoblinArcher:
        - prefab z Fantasy Goblin
        - NavMeshAgent, speed 2.5, stoppingDistance 8
        - Behavior: trzymaj dystans 6-10m, gdy LoS na gracza → wystrzel GoblinArrow
        - Animator: Speed, Aim, Shoot, Die
  [ ] Skala goblinów: 0.85f.

F5a — VFX (CHECKLIST):
  [ ] FireballProjectile:
        - Onstart spawn VFX_FireballCast (Registry).
        - OnHit spawn VFX_FireballHit przy kolizji.
        - Wyłącz built-in trail jeśli prefab ma własny.
  [ ] Totem.Die (lub jak nazywa się event śmierci):
        - Instantiate VFX_TotemDestroyed na pozycji totemu.
  [ ] GoblinCamp.TryCapture (już zwraca bool):
        - Po ApplyState(Captured), spawn ring of light VFX (VFX_Heal lub własny placeholder).
  [ ] Heal czar (PlayerSkillManager / Combat):
        - Gdy heal aktywny, spawn VFX_Heal na graczu.

TEST PLAN:
  1. New game → Druid w bazie. Animacja idle.
  2. Biegnij — animacja Run. Stań — Idle.
  3. Atak (klik) — animacja Cast/Attack.
  4. Idź do goblin_camp_first_clearing.
  5. Goblin szturmowiec biegnie do Ciebie i atakuje (NavMeshAgent path widoczny).
  6. Goblin łucznik trzyma dystans i strzela strzałą (NavMeshAgent + LoS).
  7. Castuj Fireball → VFX cast → projectile → VFX hit na celu.
  8. Zabij totem → VFX explosion.
  9. Wróć po Cleared → interakcja z totemem (przez CampInteractable) → Captured → VFX ring.
  10. Save / Continue nie regresuje.
  11. Performance: > 45 FPS z VFX.

NIE RÓB (scope):
  - NIE zmieniaj GoblinCamp.ApplyState / state machine.
  - NIE zmieniaj Save/Settings API.
  - NIE zmieniaj WorldGenerator placement (Agent B).
  - NIE zmieniaj UI / HUD (Agent D).
  - NIE refaktoryzuj GameAssetRegistry.
  - NIE psuj fallback PlayerBuilder.BuildVisual (capsule) — musi działać gdy GanzSe brak.
  - NIE commituj.

DEPENDS ON: Agent A (F1+F2). Może zacząć równolegle z Agentami B i D.
LUŹNO ZALEŻY OD: Agent B (NavMesh wypiekany dla goblin AI — w razie braku, F4 i tak działa,
                  ale AI się nie ruszy do czasu Bake).

ZAKOŃCZENIE:
  - TEAM_STATUS.md: [x] F4, [x] F5a, Changelog: "Druid GanzSe podpięty (fallback capsule), gobliny X/Y z modelami, VFX 4/5".
```

---

## 🟢 Agent D — UI / Audio / Polish

```
ROLA: Agent D — UI / Audio / Polish. Specjalizujesz się w uGUI, TMP, sprite sheet integration, audio mixers, juice.

REPO: /Users/patrykskoczylas/Wpierdol-of-the-Goblins--2026-05-27-21-04-21
Unity 6000.4.7f1, URP 17.4.0, TextMesh Pro.

KONTEKST WEJŚCIOWY:
  1. docs/TEAM_WORKFLOW.md           (NIE PSUĆ SettingsManager API)
  2. docs/ASSET_AUDIT.md             (Fantasy Free GUI / Modern RPG icons / Basic RPG Sounds — PENDING)
  3. docs/IMPLEMENTATION_PLAN.md     (Twoje fazy F5b + F5c + F7 polish)
  4. docs/TEAM_STATUS.md

WARUNEK STARTU: Agent A → F1+F2 [x] completed.

CEL (F5b + F5c + F7 polish):
  F5b — UI: MainMenu / CharacterCreation / HUD / Pause / Settings używają Fantasy Free GUI sprites + Modern RPG icons.
  F5c — Audio: SfxUIClick / SfxHit / SfxDeath / SfxFireballCast / SfxFootstep podpięte.
  F7 — Polish: skalowanie 1080/1440, FPS counter, brak NullRef.

OUTPUT (pliki):
  Edytuj:
    Assets/_Game/Scripts/UI/UIFactory.cs                (buttony → GuiButton sprite)
    Assets/_Game/Scripts/UI/MainMenuBootstrap.cs        (tło → GuiMenuBackground)
    Assets/_Game/Scripts/UI/CharacterCreationBootstrap.cs
    Assets/_Game/Scripts/UI/PlayerHUD.cs                (paski → GuiBarBackground + Fill)
    Assets/_Game/Scripts/UI/PauseMenu.cs                (panel → GuiPanel)
    Assets/_Game/Scripts/UI/SettingsMenuUI.cs           (panel + slidery)
    Assets/_Game/Scripts/UI/FPSCounter.cs               (polish)
    Assets/_Game/Scripts/Player/PlayerCombat.cs         (PlayAudio onHit)
    Assets/_Game/Scripts/Player/PlayerController.cs     (footstep audio co X dist)
    Assets/_Game/Scripts/Enemies/GoblinBase.cs          (PlayAudio onDeath)
    Assets/_Game/Scripts/Player/FireballProjectile.cs   (PlayAudio onCast)
  Nowe (opcjonalnie):
    Assets/_Game/Scripts/Core/AudioPlayer.cs            (static facade — Play(clip, position, volume))
    Assets/_Game/Scripts/UI/UIAudioBinder.cs            (auto-bind click sound do wszystkich buttonów)

F5b — UI (CHECKLIST):
  [ ] UIFactory: dodaj metodę CreateButton(parent, label, onClick):
        - Background = GameAssetRegistry.TryLoadSprite(GameAssetPaths.GuiButton)
        - Fallback: solid color jak teraz
        - TMP_Text label, kolor zielono-brązowy.
        - AudioSource onClick → SfxUIClick.
  [ ] MainMenuBootstrap:
        - Tło: Image fullscreen z GuiMenuBackground sprite (gdy null → gradient cienisty).
        - Tytuł: "Wpierdol of the Goblins" TMP, duży font, kolor #C9A76A.
        - Buttony: "Nowa Gra", "Kontynuuj" (greyed gdy SaveSystem.HasSave()==false), "Ustawienia", "Wyjdź".
  [ ] CharacterCreationBootstrap:
        - Karta druida: ramka GuiPanel, ikona druida (placeholder lub Modern RPG icons).
        - Atrybuty: paski progress (GuiBarBackground/Fill) dla STR/DEX/MANA/INT/END/CHA.
  [ ] PlayerHUD:
        - Pasek HP: GuiBarBackground + GuiBarFillHp (czerwony). Width proporcjonalnie do HP/MaxHP.
        - Pasek Mana: GuiBarBackground + GuiBarFillMana (niebieski).
        - Sloty skilli (dolny pasek): 3 slot frame'y (GuiIconFrame) z IconMelee, IconFireball, IconHeal.
        - Numerki HP "120/150" w TMP nad paskami.
        - DeathScreen overlay: czarne tło 50% + "Zginąłeś" + button "Respawn w bazie".
  [ ] PauseMenu: GuiPanel + buttony "Wznów", "Ustawienia", "Menu główne".
  [ ] SettingsMenuUI: slidery Volume Master/Music/SFX + dropdown Quality + FPS cap.

F5c — Audio (CHECKLIST):
  [ ] AudioPlayer.cs (helper static):
        public static void PlayOneShot(string[] paths, Vector3? worldPos = null, float volume = 1f);
        Implementacja: Registry.TryLoadAudio + AudioSource.PlayClipAtPoint lub global AudioSource.
  [ ] UI clicks: w UIFactory.CreateButton → onClick.AddListener(() => AudioPlayer.PlayOneShot(GameAssetPaths.SfxUIClick));
  [ ] PlayerCombat: po udanym uderzeniu → AudioPlayer.PlayOneShot(SfxHit, target.position);
  [ ] FireballProjectile.cs constructor / Start: PlayOneShot(SfxFireballCast, position);
  [ ] PlayerController: kroki — w Update jeśli isMoving && Time.time > _nextFootstep:
        AudioPlayer.PlayOneShot(SfxFootstep, transform.position, 0.4f);
        _nextFootstep = Time.time + (isRunning ? 0.32f : 0.5f);
  [ ] GoblinBase.Die: PlayOneShot(SfxDeath, transform.position);
  [ ] Honoruj SettingsManager.Settings.sfxVolume i masterVolume — używaj AudioSource.volume = clamp(setting * volume).

F7 — Polish (CHECKLIST):
  [ ] FPSCounter.cs: czytelny w prawym górnym rogu, toggle przez SettingsData.showFPS (już jest).
  [ ] Skalowanie: CanvasScaler.UIScaleMode = ScaleWithScreenSize, referenceResolution 1920x1080, matchWidthOrHeight = 0.5.
  [ ] Anchory: HUD bottom-left dla pasków, top-right dla FPS, top-center dla zone name.
  [ ] Test scenariusz E2E (zob. F7 IMPLEMENTATION_PLAN).

TEST PLAN:
  1. MainMenu: tło ładne (fantasy GUI lub gradient fallback), klik buttona → SFX.
  2. CharacterCreation: karta druida z atrybutami.
  3. World: HUD pokazuje HP/Mana paski + ikony skilli.
  4. Naciśnij Esc → pauza, panel ładny.
  5. Settings: zmień głośność master → click button → ciszej. (test honoring settings).
  6. Zabij goblina → SfxDeath gra. Castuj fireball → SfxFireballCast.
  7. Idź — co ~0.4s SfxFootstep.
  8. FPS counter widoczny gdy włączony w Settings.
  9. SaveSystem / SettingsManager nie regresują.
  10. Skaluj okno z 1080 → 1440 → 4K — UI poprawnie się skaluje.

NIE RÓB (scope):
  - NIE zmieniaj SettingsManager.cs public API (ApplySettings, UpdateAndApply).
  - NIE zmieniaj SettingsData.cs (chyba że dodaj pole — wtedy zostaw stare).
  - NIE zmieniaj logiki gry: WorldGenerator, GoblinCamp, PlayerBuilder.
  - NIE refaktoryzuj GameAssetRegistry (Agent A).
  - NIE psuj fallback (jeśli sprite brak — placeholder color, nie crash).
  - NIE commituj.

DEPENDS ON: Agent A (F1+F2). Może pracować równolegle z B i C.

ZAKOŃCZENIE:
  - TEAM_STATUS.md: [x] F5b, [x] F5c, [x] F7 polish, Changelog: "UI Fantasy GUI X/N, audio click/hit/cast/footstep/death podpięte".
```

---

## ⚠️ Wspólne dla wszystkich agentów

| Reguła | Detal |
|--------|-------|
| **Lava Tube** | Wykluczone. Żadnych ścieżek do `Assets/Lava Tube/...`. |
| **Save / Settings API** | Nie zmieniaj sygnatur publicznych w `SaveSystem`, `SaveData`, `SettingsManager`, `SettingsData`. Dodawać pola wolno. |
| **GoblinCamp state machine** | Enum `CampState { Active, Cleared, Captured }` i sygnatury `ApplyState`, `TryCapture`, `OnStateChanged` — nie tykać. |
| **Unity 6 API** | `FindFirstObjectByType`, `FindObjectsByType(FindObjectsSortMode.None)`. |
| **Console** | 0 errors, 0 nowych warnings. |
| **Lokalizacja** | UI gracza: PL. Identyfikatory kodowe (campId): EN snake_case. |
| **Commit** | NIE. User decyduje. |
| **Status** | Update `docs/TEAM_STATUS.md` przed i po. |

---

## Eskalacja

Jeśli:
- Twój task jest **zablokowany** (brak paczki, NavMesh fail, kompilacja błąd nie do naprawy w scope) →
- **STOP**. Dopisz w `TEAM_STATUS.md` sekcja **Blockers** z dokładnym opisem (plik, linia, błąd).
- Wróć z raportem do TEAM LEAD.

Nie improwizuj. Nie rób cudzego scope. Nie commituj.

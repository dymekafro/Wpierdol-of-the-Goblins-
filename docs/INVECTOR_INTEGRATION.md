# Invector Third Person Controller LITE — pełna integracja z WPG

Status: **DZIAŁA** w Edytorze (potwierdzone w `Editor.log`:
`[WorldBootstrap] Player driver: Invector | animator=Invector@BasicLocomotion`).

## Materiały URP (naprawa magenta / fioletu)

Paczka Invector (Asset Store **82048** — *Third Person Controller - Basic Locomotion FREE*)
domyślnie używa shaderów **Built-in Render Pipeline** (`Standard`, `Legacy`, ewentualnie custom Invector).
Projekt WPG działa na **URP (Unity 6)** — bez konwersji wszystko renderuje się na **magenta (#FF00FF)**.

### Szybka naprawa (zalecane)

1. W Unity: **WPG → Fix Invector Materials (URP)**
2. Poczekaj na dialog z liczbą naprawionych materiałów.
3. Uruchom grę — postać V-Bot i scena demo powinny mieć poprawne kolory.

Menu konwertuje wszystkie `.mat` w folderze Invector (`Assets/Invector-3rdPersonController_LITE/`)
na **Universal Render Pipeline/Lit**, kopiuje `_MainTex` → `_BaseMap`, `_Color` → `_BaseColor`,
zachowuje kolory skóry/materiałów postaci (Body, Joints, Rubber, Members_*), a dla cutout/transparent
włącza **Alpha Clipping**.

### Alternatywa: Unity Render Pipeline Converter

W Unity 6 **nie ma** pozycji *Edit → Rendering → Materials → Convert All Built-in to URP*.
Zamiast tego:

- **Window → Rendering → Render Pipeline Converter**
- Zakładka **Material Upgrade** (lub Built-in → URP)
- Zaznacz foldery Invector → **Initialize Converters** → **Convert Assets**

To oficjalne narzędzie Unity; nasze menu WPG robi to samo, ale z logiką specyficzną dla Invector
(zachowanie kolorów postaci, alpha clip na bannerach/logo, wykrywanie wielu nazw folderów po imporcie).

### Runtime (automatyczna naprawa przy spawnie)

`InvectorPlayerBuilder.TryBuild()` woła `MaterialUpgrader.UpgradeHierarchy(go)` zaraz po
`Instantiate` prefabu gracza. Dzięki temu nawet bez uruchomienia menu edytorowego instancja
V-Bota w grze dostaje kopie materiałów URP Lit na wszystkich `Renderer`.

Pliki:
- `Assets/_Game/Scripts/World/MaterialUpgrader.cs` — logika konwersji + wykrywanie ścieżek Invector
- `Assets/_Game/Scripts/Editor/MaterialUpgrader.cs` — menu **WPG/Fix Invector Materials (URP)**

### Brak oficjalnego pakietu URP od Invector

W paczce LITE **nie ma** osobnego readme/setup URP (`README_Basic.txt` wspomina tylko Unity 2018.4+).
Invector nie dostarcza gotowych materiałów URP — konwersja Built-in → URP to obowiązkowy krok
w projektach URP/HDRP.

### Input Handler (Old Input dla Invector)

Invector LITE czyta wejście przez **Old Input Manager** (`Input.GetAxis`, `Input.GetKey`).
W `ProjectSettings.asset` ustawione jest **`activeInputHandler: 2` (Both)** — Old + New Input System.
**Nie zmieniaj na „Input System Package only"** — Invector przestanie działać.

## Co zostało zrobione

Strategia: **Hybrid (Option B z briefu)**.
Spawnujemy oryginalny prefab Invectora (`ThirdPersonController_LITE.prefab`) jako root gracza
i **doklejamy** na nim warstwę WPG (PlayerStats, PlayerCombat, InteractionDetector,
CharacterAnimDriver, InvectorPlayerAdapter). Dzięki temu cała Invector locomotion
(ruch, animacja, sprint, jump, kamera) zostaje 1-do-1, a gameplay WPG (HP, mana,
fireball, heal, melee, save/load, pauza, obozy goblinów) działa nadal.

## Pliki — co dodano / zmodyfikowano

### Nowe pliki
- `Assets/_Game/Scripts/Player/InvectorPlayerBuilder.cs` — buduje gracza z prefabu Invector
  i dodaje warstwę WPG (PlayerStats, PlayerCombat, CharacterAnimDriver, InteractionDetector, adapter).
  Po spawnie woła `MaterialUpgrader.UpgradeHierarchy` (URP fix).
- `Assets/_Game/Scripts/Player/InvectorPlayerAdapter.cs` — most: blokuje `vThirdPersonInput`
  na czas śmierci gracza, synchronizuje czułość myszy z `SettingsManager`.

### Zmodyfikowane pliki
- `Assets/_Game/Scripts/World/WorldBootstrap.cs` — domyślnie `useInvectorLocomotion = true`,
  używa `InvectorPlayerBuilder.TryBuild()` + spawnuje `vThirdPersonCamera_LITE` prefab jako MainCamera.
  Stara ścieżka `PlayerBuilder.BuildDruid()` + `WPG.ThirdPersonCamera` pozostaje jako fallback,
  używana automatycznie gdy prefab Invectora nie zostanie znaleziony.
- `Assets/_Game/Scripts/Player/PlayerCombat.cs` — `ResolveAimDirection()` priorytetuje
  `PlayerController.AimDirection()` (legacy), potem `Camera.main.transform.forward` (Invector),
  potem `transform.forward`. Dzięki temu fireball celuje tam gdzie patrzy gracz w Invector mode.
- `Assets/_Game/Scripts/Player/PlayerStats.cs` — `ReviveAt()` obsługuje teraz również
  `Rigidbody`-based postać (Invector używa Rigidbody zamiast CharacterController).
  Resetuje `linearVelocity` / `angularVelocity` i ustawia `rb.position` przy respawnie.
- `Assets/_Game/Scripts/Core/GameAssetRegistry.cs` — dodano sloty `InvectorController`
  (prefab `ThirdPersonController_LITE`) i `InvectorCamera` (prefab `vThirdPersonCamera_LITE`).
  Stary `InvectorCharacter` (sam V-Bot) zachowany.

## Architektura runtime

GameObject `Player_Invector` (root):
- **Tag**: `Player`
- **Rigidbody** + **CapsuleCollider** (z prefabu Invector — height 1.9, radius 0.29, dynamic)
- **Animator** (controller `Invector@BasicLocomotion` z avatarem `vThirdPersonAvatar`)
- **vThirdPersonController** (Invector — ruch, sprint, jump, strafe)
- **vThirdPersonInput** (Invector — Old Input Manager: WASD, Mouse X/Y, Space, Tab, LeftShift)
- **PlayerStats** (WPG — HP/mana/IDamageReceiver)
- **PlayerCombat** (WPG — LPM melee, E fireball, Q heal)
- **InteractionDetector** (WPG — F/E na obozach)
- **CharacterAnimDriver** (WPG — w trybie `ConfigureForInvectorLocomotion()`,
  triggery Attack/Cast/Death proxy do animatora Invectora)
- **InvectorPlayerAdapter** (WPG↔Invector bridge: śmierć blokuje input, mysz z SettingsManager)
- Dziecko: V-Bot mesh + cały rig (`VBOT_:Reference / VBOT_:Hips / VBOT_:RightHand / ...`)

GameObject `MainCamera`:
- **Camera** + **AudioListener**
- **vThirdPersonCamera** z prefabu Invector (clip plane culling, smooth follow, mouse rotate)
- Target ustawiany przez `vThirdPersonInput` automatycznie przez `FindFirstObjectByType<vThirdPersonCamera>()`,
  dodatkowo `WorldBootstrap.BuildInvectorCamera()` woła `_invectorCamera.SetMainTarget(_player.transform)`.

## Sterowanie (Invector mode)

| Akcja | Klawisz | Komponent obsługujący |
|-------|---------|------------------------|
| Ruch | WASD | `vThirdPersonInput` (Old Input Manager: Horizontal/Vertical) |
| Rozglądanie | Mouse X / Y | `vThirdPersonInput` → `vThirdPersonCamera.RotateCamera` |
| Sprint | LeftShift (toggle) | `vThirdPersonInput.SprintInput` → `cc.Sprint(true)` |
| Skok | Spacja | `vThirdPersonInput.JumpInput` → `cc.Jump()` |
| Strafe toggle | Tab | `vThirdPersonInput.StrafeInput` → `cc.Strafe()` |
| Atak melee | LPM | `PlayerCombat.DoMelee()` (WPG) |
| Fireball | E (gdy nie celujesz w obóz) | `PlayerCombat.DoFireball()` (WPG) |
| Heal | Q | `PlayerCombat.DoHeal()` (WPG) |
| Interakcja (obóz, baza druida) | E lub F (gdy w zasięgu) | `InteractionDetector` (WPG) |
| Pauza | ESC | `PauseMenu` (WPG, New Input System) |

**Ważne:** gra używa OBU systemów inputu jednocześnie
(`ProjectSettings.asset → activeInputHandler: 2`). Invector czyta przez `Input.GetAxis`/`Input.GetKey`
(Old Input Manager), WPG przez `Keyboard.current` / `Mouse.current` (New Input System).
Oba systemy są aktywne i nie kolidują. **Nie zmieniaj `activeInputHandler`** — to złamie integrację.

### Kolizja `E` (Invector default vs WPG fireball)

`vThirdPersonInput` w prefabie Invector LITE **nie używa klawisza E**
(action button jest tylko w wersji Pro). Dlatego E w naszej grze pozostaje:
- **E + obiekt interaktywny w zasięgu** → interakcja (obóz, druidowa baza)
- **E poza zasięgiem interakcji** → fireball

Logika rozjazdu jest w `PlayerCombat.Update()` (`eConsumedByInteraction`).

## Tryby graczowe — przełączanie

`WorldBootstrap.useInvectorLocomotion` (domyślnie `true`):
- `true` (Invector): spawn z prefabu, kamera Invectora, V-Bot model.
- `false` (WPG legacy): stary CharacterController-based player + `WPG.ThirdPersonCamera` + GanzSe druid.

**Auto-fallback:** jeśli `useInvectorLocomotion = true` ale `GameAssetRegistry.InvectorController == null`
(np. ktoś usunie prefab), `WorldBootstrap` automatycznie spada do legacy WPG.

## Skala postaci 1.3x

`InvectorPlayerBuilder.InvectorPlayerScale = 1.0f` (domyślnie naturalne ~1.85m V-Bota).
Dla "większej postaci" zmień stałą na `1.3f` lub przekaż parametr `scale: 1.3f` do `TryBuild()`.
Skalowanie transform.localScale skaluje też kapsułę i ruch fizyczny.

> Uwaga: V-Bot przy 1.3x ma ~2.4m — może wyglądać olbrzymio przy goblinach.
> Aktualnie zostawiam 1.0 jako default i traktuję 1.3x jako opcjonalny override.
> WPG legacy ścieżka (GanzSe druid) nadal używa `WorldAssetPlacer.PlayerCharacterModelScale = 1.3f`.

## Zachowane funkcjonalności WPG

- ✅ MainMenu → CharacterCreation → World flow (`GameManager.attributes` przepisywane do `PlayerStats.Init`)
- ✅ Save/Load (`SaveSystem`, `pendingLoadData` — pozycja, HP, mana, lastZoneName)
- ✅ Pauza ESC (`PauseMenu`, niezależnie od trybu inputu)
- ✅ Obozy goblinów / druidowa baza (`CompareTag("Player")` działa, tag ustawiony w buildzie)
- ✅ Goblin AI atakuje gracza (`GameObject.FindGameObjectWithTag("Player")` znajduje root)
- ✅ HP / Mana / Death / Respawn (`PlayerStats.ReviveAt` reset Rigidbody.velocity)
- ✅ Fireball / Heal VFX (przez `WorldAssetPlacer.TrySpawnVfx`)
- ✅ HUD i death screen (`PlayerHUD.Bind(stats, combat)`)
- ✅ Kursor lock/unlock (Cursor.lockState w bootstrap + pause menu)
- ✅ SettingsManager.MouseSensitivity → `vThirdPersonCamera.xMouseSensitivity / y`
  (przez `InvectorPlayerAdapter`)

## Rozwiązane konflikty / niuanse

1. **Old vs New Input System** — `activeInputHandler: 2` (Both) już ustawione w
   `ProjectSettings.asset:923`. Bez tego Invector by nie działał (`Input.GetAxis` rzuca exception
   przy New only).

2. **CharacterController vs Rigidbody** — stara `PlayerStats.ReviveAt` zakładała wyłącznie
   CharacterController. Teraz obsługuje też Rigidbody (Invector). Bez tego respawn na
   Invector graczu zostawiał velocity z momentu śmierci i postać "leciała" po teleporcie.

3. **AimDirection bez PlayerController** — stary `PlayerCombat` bezpośrednio czytał
   `_ctrl.AimDirection()`. W Invector mode nie ma `PlayerController`. `ResolveAimDirection()`
   robi fallback na `Camera.main.transform.forward`, dzięki czemu fireball leci tam gdzie
   patrzy kamera.

4. **CharacterAnimDriver dla Invector animator-a** — driver miał już logikę
   `ConfigureForInvectorLocomotion()` (parametry InputMagnitude / IsGrounded zamiast Speed).
   `InvectorPlayerBuilder` woła ją explicite zaraz po `AddComponent<CharacterAnimDriver>()`,
   żeby pierwszy frame już używał Invector params.

5. **Triggery Attack/Cast** — controller `Invector@BasicLocomotion` LITE **nie ma
   trigerów Attack/Cast** (są w wersji Pro). `CharacterAnimDriver.TriggerAttack/Cast`
   sprawdza `HasParam(...)` — jeśli ich nie ma, fallback do proceduralnego swing/cast
   (StaffTip + handMount). Działa bez zmian w controllerze.

6. **Tag "Player"** — prefab `ThirdPersonController_LITE` ma tag "Untagged".
   `InvectorPlayerBuilder` ustawia `go.tag = "Player"` od razu po Instantiate.
   Bez tego goblin AI by nie znajdowała gracza, a triggery `WorldZone` / `DruidBase` by się nie odpalały.

7. **`useRootMotion`** — wyłączane w `InvectorPlayerBuilder` (`animator.applyRootMotion = false`),
   bo prefab z marketplace ma w controllerze Pose Speed bez root motion clipów,
   a my dodatkowo wyłączamy żeby nie kolidować z Rigidbody-driven movement.

## Test plan (manual)

W Edytorze, scena `Assets/_Game/Scenes/Bootstrap.unity` (lub jakkolwiek się zaczyna):

1. **Start gry → CharacterCreation → World**: spawn powinien dać log
   `[WorldBootstrap] Player driver: Invector | animator=Invector@BasicLocomotion`.
2. **Ruch WASD**: smooth, z animacją chodu/biegu (V-Bot mesh animowany przez Invector controller).
3. **Sprint LeftShift**: zauważalne przyspieszenie + animacja sprintu.
4. **Skok Spacja**: Jump animation + airborne control.
5. **Mysz**: kamera 3rd person rotuje, sensitivity respektuje SettingsManager (slider Settings → Mouse).
6. **Strafe Tab**: postać przełącza się w tryb strafe (boczne klatki).
7. **LPM**: melee w pobliżu goblina → goblin traci HP (`PlayerCombat.DoMelee` + `IDamageReceiver`).
8. **E w polu**: fireball leci w kierunku kamery, traci manę.
9. **E przy obozie**: zamiast fireballa odpala się interakcja (przejęcie obozu).
10. **Q**: heal — HP rośnie, kosztuje manę.
11. **Death**: gdy HP=0, ruch zatrzymuje się, kursor odblokowany, death screen.
12. **Respawn (przycisk)**: teleport do `DruidBase.spawnPoint`, HP/mana full, kontrole działają.
13. **ESC**: pauza, Time.timeScale=0, kursor visible. ESC ponownie wraca do gry.
14. **Save → Continue**: zamknij grę po zmianie pozycji, w MainMenu kliknij Kontynuuj — gracz spawnuje
    się w zapisanej pozycji z poprzednim HP/mana.

## Znane ograniczenia / TODO

- **Invector LITE nie ma melee/cast trigerów** — używamy proceduralnego swing/cast z
  `CharacterAnimDriver`. Dla pełnego efektu wizualnego trzeba albo Invector PRO,
  albo dodać własne klipsy ataku do controllera (Animator override).
- **V-Bot wygląda robotycznie** — nie pasuje stylistycznie do druida-bohatera.
  Można zostawić Invector locomotion + kamerę, ale w `InvectorPlayerBuilder` zamienić
  child mesh na GanzSe druida. To wymaga remap-a `Avatar` w Animatorze (musi być Humanoid).
  Aktualnie tego nie robimy — V-Bot jest reasonable placeholder, a integration goal został
  spełniony.
- **Skala 1.0 vs 1.3** — pozostawiona jako parametr. Dla GanzSe (legacy) użyłbym 1.3,
  dla V-Bota raczej 1.0–1.1.
- **Brak Camera.tag fallbacku w CharacterCreation/MainMenu** — niezmienione, działa jak wcześniej
  (osobne sceny tworzą własne kamery z tagiem MainCamera).
- **`UI/Skin/UISprite.psd could not be loaded`** w log — pre-existing issue
  (`UIFactory.GetDefaultSprite`), nie powiązany z integracją Invectora.

## Rollback (gdyby coś poszło źle)

```csharp
// W WorldBootstrap inspector lub przez kod:
worldBootstrap.useInvectorLocomotion = false;
```

Cała stara ścieżka (`PlayerBuilder.BuildDruid` + `WPG.ThirdPersonCamera`) jest nietknięta.

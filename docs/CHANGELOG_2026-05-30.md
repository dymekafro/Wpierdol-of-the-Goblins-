# Changelog — 30–31 maja 2026

Podsumowanie zmian wprowadzonych w tej sesji: warstwowy system muzyki, optymalizacje wydajności, naturalny teren (WorldGround), poprawki bugów, czyszczenie historii git oraz merge systemu tworzenia postaci od kolegi.

Dokument jest praktyczny — dla zespołu (Ty + kolega). Przy każdej zmianie: **co**, **dlaczego**, **jak przetestować / używać**.

> Commity tej sesji (od najnowszego): `043b3bc` merge origin/main · `66acb3d` TworzeniePostaci · `f536434` worldgenerator · `7bb49f6` chore(audio): ignore music WAVs · `9dd38cf` optiterrian · `51e774b` beforeopti · `41630cd` sounds · `ef3a407` NowySzajs.

---

## 1. Audio / Muzyka

### Warstwowy system muzyki (SOUNDTRACK / MENU / AMBIENT)
- **Co:** `GameAudioManager` gra teraz trzy **niezależne warstwy** muzyki jednocześnie, każda z osobnym `AudioSource` i głośnością:
  - **SOUNDTRACK** — gra od startu gry, cały czas (tło).
  - **MENU** — gra w menu głównym oraz w pauzie (ESC).
  - **AMBIENT** — gra w trakcie rozgrywki (scena World).
- **Stany:**
  - Menu główne → `EnterMainMenuMusic()` = SOUNDTRACK + MENU (bez AMBIENT).
  - Rozgrywka → `EnterGameplayMusic()` = SOUNDTRACK + AMBIENT (bez MENU).
  - Pauza (ESC) → `SetMenuLayerActive(true/false)` dokłada/zdejmuje warstwę MENU, a SOUNDTRACK i AMBIENT grają dalej bez przerwy.
- **Dlaczego:** płynne przejścia menu ↔ gra bez „twardego” cięcia muzyki; spójne tło przez całą sesję.
- **Jak przetestować:** Play z MainMenu → słychać SOUNDTRACK+MENU. Nowa gra → MENU wycisza się, wchodzi AMBIENT. W grze ESC → wraca warstwa MENU na pauzie.

### Osobny suwak głośności „Otoczenie” (Ambient)
- **Co:** warstwa AMBIENT ma **własny kanał głośności** (`ambientVolume` w `SettingsData`), niezależny od suwaka „Muzyka”. W menu ustawień doszedł suwak **„Otoczenie”** (`SettingsMenuUI`, kanał `VolumeChannel.Ambient`, domyślnie 70%).
- **Dlaczego:** można ściszyć ambient lasu, nie ruszając głównej muzyki (i odwrotnie).
- **Jak używać:** **Menu → Ustawienia → Audio → Otoczenie**. Master × Ambient steruje warstwą AMBIENT; Master × Music steruje SOUNDTRACK i MENU.

### Konwersja muzyki WAV → OGG
- **Co:** ścieżki muzyczne przeniesione na **OGG** (`SOUNDTRACK.ogg`, `MENU.ogg`, `AMBIENT.ogg` w `Assets/_Game/Audio/Music/`); ścieżki w `GameAssetPaths.cs` wskazują pliki OGG.
- **Dlaczego:** WAV-y były ciężkie i puchły repo — OGG jest dużo mniejszy przy dobrej jakości.

### Poprawka zapętlania muzyki (loop „klik”/przerwa)
- **Co:** import muzyki ustawiony na **Load Type = Compressed In Memory** (w `.ogg.meta`), źródła mają `loop = true`.
- **Dlaczego:** przy streamingu OGG pętla potrafiła „kliknąć” / zrobić mikroprzerwę na zawinięciu — Compressed In Memory eliminuje lukę.

### Nowe efekty SFX
- **Co:** dodane dźwięki: **kroki** (dwa warianty trawy, losowane), **skok** (`jump_grunt`), **uderzenie/punch** (`punch_grunt`), **upadek** (`fall_woosh`), **hop goblina** (`goblin_hop`). Metody: `PlayFootstep`, `PlayJump`, `PlayPunch`, `PlayFall`, `PlayGoblinHop`.
- **Dlaczego:** lepszy feedback ruchu i walki.
- **Jak przetestować:** bieganie postacią (kroki), skok, atak (punch), spadanie z wysokości (woosh), gobliny w ruchu (hop).

---

## 2. Wydajność

- **Pula głosów 3D dla SFX (`GameAudioManager`):** 10 pozycyjnych `AudioSource` w trybie round-robin zamiast `PlayClipAtPoint` (które tworzyło/niszczyło GameObjecty → skoki GC). Dotyczy kroków, hopów, walki.
- **Preload klipów audio:** wszystkie klipy ładowane raz w `Awake` (`LoadClips`), bez wczytywania w trakcie gry → brak przycięć przy pierwszym odtworzeniu.
- **Throttle `ForestAtmosphereSettings`:** w trybie gry atmosfera aplikuje się **~2× na sekundę** (`ApplyInterval = 0.5s`, `Time.unscaledTime`) zamiast co klatkę — koniec ciężkiej pracy w `Update`.
- **`CameraCache`:** wspólny, cache'owany dostęp do kamery głównej zamiast wielokrotnych `Camera.main` (drogie wyszukiwanie po tagu co klatkę).
- **`MaterialPropertyBlock`:** zmiana właściwości materiałów (np. tinty) przez property block zamiast tworzenia instancji materiału → mniej alokacji i draw-state changes.
- **Pula `DamageNumber`:** liczby obrażeń recyklowane z puli zamiast `Instantiate`/`Destroy` przy każdym trafieniu.

---

## 3. Teren (WorldGround)

Nowy, wspólny system kształtu terenu — łagodne pofalowanie (Perlin, 3 oktawy) + „plateau” (płaskie place) pod bazą, obozami i miejscami mocy.

### Komponenty
- **`WorldGround`** (`WPG.World`) — jedno źródło prawdy o wysokości terenu: `GetGroundHeight(x,z)`, `BuildMesh(size, resolution)`, strefy wypłaszczenia (`AddFlatZone`). Używane przez `WorldGenerator`, `WorldAssetPlacer`, profil runtime i gobliny (dociskane do terenu co klatkę). Gdy świat nieskonfigurowany → zwraca 0 (zachowanie jak płaska mapa).
- **`WorldGroundProfile`** — komponent zapisujący konfigurację terenu (offsety szumu, amplituda, skala, strefy) w prefabie, żeby **runtime** odtworzył dokładnie tę samą powierzchnię co edytor.
- **`WorldGroundShaper`** (Editor) — nadaje naturalny kształt **zapieczonemu** `WorldRoot.prefab` w miejscu, **bez** regeneracji świata: zamienia płaski `GroundPlane` na pofalowaną siatkę + `MeshCollider`, osadza obiekty na powierzchni i zapisuje profil. Siatka zapisywana jako asset (`WorldRoot_GroundMesh.asset`), bo runtime'owy `new Mesh()` nie przetrwałby zapisu prefabu.

### Naturalny grunt na WorldRoot
- **Co:** prefab `WorldRoot` dostał pofalowany grunt z kolizją; propsy, obozy, gobliny i ścieżki osadzone na powierzchni.
- **Osadzanie jest ABSOLUTNE** (`Y = wysokość terenu`, nie `Y += teren`), więc tool można odpalać wielokrotnie i ze zmienionymi parametrami — obiekty **się nie kumulują** (lek na „podwójne podniesienie” / floatery).

### Jak używać (menu WPG)
- **`WPG → World → Ukształtuj teren (ustawienia)…`** — okno z parametrami (Seed, Amplituda ±m, Skala szumu, Płaski plac bazy, Gęstość siatki). Przycisk **„Zastosuj / przebuduj teren”** przebudowuje `WorldRoot.prefab`. Można wielokrotnie.
- **`WPG → World → Napraw unoszące się obiekty`** — używa zapisanego `WorldGroundProfile`, nie zmienia kształtu mapy, tylko dociska propsy/struktury do powierzchni. Bezpieczne do wielokrotnego użycia.
- **Domyślne:** seed `13579` (jak `WorldBootstrap`), amplituda `3 m`, skala szumu `55`, płaski plac bazy `18 m`, gęstość `160`.

---

## 4. Naprawy bugów

- **`InteractableForwarder` w osobnym pliku:** wydzielony do `World/InteractableForwarder.cs` (mały pośrednik przekazujący trigger `SphereCollider` do właściwego `IInteractable`, np. `PowerSite`).
- **Trigger prefab fix:** poprawione przekazywanie interakcji przez trigger (forwarder zamiast bezpośredniego komponentu na colliderze).
- **UISprite fallback:** `UIFactory` ma bezpieczny fallback sprite'a, gdy zasób UI nie zostanie znaleziony (brak „różowych”/pustych elementów UI).
- **`WorldBootstrap` liczba obozów:** poprawiona liczba/spawn obozów w bootstrapie świata.
- **Strip missing scripts przy zapisie prefabu:** `WorldGroundShaper` usuwa komponenty z brakującym skryptem (`m_Script: {fileID: 0}`) przed `SaveAsPrefabAsset` — inaczej zapis prefabu by się wywalał.

---

## 5. Git / Repo

- **Usunięcie WAV-ów z historii:** ciężkie pliki muzyczne WAV wycięte z historii repo (force push). **Uwaga zespołowa poniżej.**
- **`.gitignore` na muzyczne WAV-y:** dodane reguły `Assets/_Game/Audio/Music/*.wav` i `*.wav.meta` — źródłowe WAV-y nie trafiają do gita, w repo trzymamy OGG.
- **OGG śledzone normalnie** (commit `7bb49f6` „ignore music WAVs, keep OGG tracked”).

---

## 6. Merge współpracy — System tworzenia postaci (TworzeniePostaci)

Wmergowany system tworzenia postaci od kolegi (`origin/main`, commit `66acb3d` + merge `043b3bc`).

- **Nowa scena:** `Assets/_Game/Scenes/CharacterCreation.unity` (zastąpiła `World_EmaceArt.unity`).
- **Nowe skrypty (`Assets/_Game/Scripts/Character/`):** `CharacterAppearanceData`, `CharacterClassDatabase`, `CharacterClassDefinition`, `CharacterCreationData`, `CharacterCreationEnums`, `CharacterCreationRules`, `CharacterStatsData`.
- **Core / UI:** `Core/CharacterCreationSession`, `UI/CharacterCreationUI`, `UI/CharacterPreviewController`, `UI/MainMenuNewGameRouter` (routing „Nowa gra” → ekran tworzenia postaci), aktualizacja `UI/CharacterCreationBootstrap`.
- **Build settings:** zaktualizowane `ProjectSettings/EditorBuildSettings.asset` (nowa scena na liście).

---

## Lista kluczowych plików

**Audio**
- `Assets/_Game/Scripts/Core/GameAudioManager.cs`
- `Assets/_Game/Scripts/Core/GameAssetPaths.cs`
- `Assets/_Game/Scripts/Core/SettingsData.cs`, `SettingsManager.cs`
- `Assets/_Game/Scripts/UI/SettingsMenuUI.cs`
- `Assets/_Game/Audio/Music/{SOUNDTRACK,MENU,AMBIENT}.ogg`
- `Assets/_Game/Audio/SFX/{footstep_grass_1,footstep_grass_2,jump_grunt,punch_grunt,fall_woosh}.wav`, `goblin_hop.ogg`

**Wydajność**
- `Assets/_Game/Scripts/Core/CameraCache.cs`
- `Assets/_Game/Scripts/World/ForestAtmosphereSettings.cs`
- `Assets/_Game/Scripts/UI/DamageNumber.cs`

**Teren**
- `Assets/_Game/Scripts/World/WorldGround.cs`
- `Assets/_Game/Scripts/World/WorldGroundProfile.cs`
- `Assets/_Game/Scripts/Editor/WorldGroundShaper.cs`
- `Assets/_Game/Prefabs/World/WorldRoot.prefab`, `WorldRoot_GroundMesh.asset`
- `Assets/_Game/Scripts/World/WorldGenerator.cs`, `WorldAssetPlacer.cs`, `WorldBootstrap.cs`

**Naprawy**
- `Assets/_Game/Scripts/World/InteractableForwarder.cs`
- `Assets/_Game/Scripts/UI/UIFactory.cs`

**Merge / Postacie**
- `Assets/_Game/Scenes/CharacterCreation.unity`
- `Assets/_Game/Scripts/Character/*`, `Core/CharacterCreationSession.cs`, `UI/CharacterCreation*`, `UI/MainMenuNewGameRouter.cs`

**Git**
- `.gitignore`

---

## Known issues / uwagi dla zespołu

- **Force push / re-clone:** historia repo została przepisana (usunięte WAV-y). Jeśli masz starszego clone'a, najprościej **zrobić świeży `git clone`**. Inaczej `git pull` może odbić się błędem o rozjechanej historii (rozwiązanie awaryjne: `git fetch` + `git reset --hard origin/main`, po wcześniejszym zabezpieczeniu lokalnych zmian).
- **Nie commitować `.vs/`:** katalog `.vs/` (cache Visual Studio) ciągle pojawia się w `git status` (`.dtbcache`, `.suo`, `FileContentIndex` itd.). Nie dodawać do commitów — to lokalne pliki IDE.
- **Muzyczne WAV-y są ignorowane:** wrzucaj nowe ścieżki jako **OGG** do `Assets/_Game/Audio/Music/`. Źródłowe WAV-y i tak nie wejdą do gita (`.gitignore`).
- **Teren w prefabie, nie w kodzie:** kształt terenu siedzi w `WorldRoot.prefab` + `WorldRoot_GroundMesh.asset` + `WorldGroundProfile`. Po zmianie parametrów w oknie `WPG/World/Ukształtuj teren` pamiętaj o commicie prefabu i meshu.
- **Po Stop Play (atmosfera):** zmiany na obiekcie `Atmosphere` zapisują się dopiero po wybraniu „Yes” i `Ctrl+S` na scenie (patrz `docs/LIVE_EDIT_ATMOSPHERE.md`).

---

## Informacje techniczne (dla AI / deweloperów)

Ta sekcja jest **machine-friendly** — konkretne ścieżki, klasy, metody, stałe i przepływy danych, żeby asystent AID kolegi mógł szybko zorientować się w kodzie. Wszystko zweryfikowane wprost w źródłach (stan na commit `043b3bc`).

### Commity z dzisiaj (`git log --since=2026-05-30`)

| Hash | Data | Tytuł | Czego dotyczy |
|------|------|-------|---------------|
| `043b3bc` | 2026-05-31 | Merge remote-tracking branch 'origin/main' | Wmergowanie systemu tworzenia postaci kolegi |
| `66acb3d` | 2026-05-31 | TworzeniePostaci | Skrypty `Character/*`, UI kreatora, routing |
| `f536434` | 2026-05-31 | worldgenerator | Teren (WorldGround / Shaper / Profile) |
| `7bb49f6` | 2026-05-31 | chore(audio): ignore music WAVs, keep OGG tracked | `.gitignore` na WAV, OGG śledzone |
| `9dd38cf` | 2026-05-31 | optiterrian | Optymalizacje (pula głosów, throttle, cache) |
| `51e774b` | 2026-05-30 | beforeopti | Stan przed optymalizacjami |
| `41630cd` | 2026-05-30 | sounds | SFX (kroki, skok, punch, fall, hop) |
| `ef3a407` | 2026-05-30 | NowySzajs | Warstwowa muzyka, suwak Ambient |

### 1. Audio — `GameAudioManager` (namespace `WPG.Core`)

Plik: `Assets/_Game/Scripts/Core/GameAudioManager.cs`. Singleton (`Instance`), tworzony leniwie przez `EnsureExists()` (z `DontDestroyOnLoad`).

**Public API (muzyka warstwowa):**
- `void EnterMainMenuMusic()` — SOUNDTRACK + MENU, zatrzymuje AMBIENT.
- `void EnterGameplayMusic()` — SOUNDTRACK + AMBIENT, zatrzymuje MENU.
- `void SetMenuLayerActive(bool active)` — dokłada/zdejmuje warstwę MENU (pauza ESC), nie rusza SOUNDTRACK/AMBIENT.
- `void PlayMusic(AudioClip clip, float volume = 0.5f)` / `void StopMusic()` — pojedynczy `_musicSource` (legacy, obok warstw).

**Public API (SFX):** `PlayHit`, `PlayDeath`, `PlayUIClick`, `PlayFireballCast`, `PlayFootstep`, `PlayJump`, `PlayFall`, `PlayGoblinHop`, `PlayPunch` — wszystkie przyjmują `Vector3? worldPos = null`. Gdy `worldPos != null` → odtwarzane na puli 3D (`spatialBlend = 1`), inaczej na `_sfxSource` (`spatialBlend = 0`). `PlayFootstep` losuje wariant (`_footstepClip` / `_footstepClip2`, próg `Random.value > 0.5f`).

**Wewnętrzne mechanizmy (wydajność):**
- `const int PositionalVoiceCount = 10` → `_positionalVoices` (round-robin `_positionalVoiceIndex`, `PlayOneShot`), tworzone w `CreatePositionalVoicePool()` jako dzieci `SfxVoice_{i}`.
- `Awake()` → dodaje `AudioSource`'y (`_sfxSource`, `_musicSource`, 3× warstwa przez `CreateMusicLayer()`), `CreatePositionalVoicePool()`, `LoadClips()` (preload przez `GameAssetLoader.LoadAudio`), `SettingsManager.EnsureExists()`, `StartLayer(_soundtrackSource, _soundtrackClip)`, `ApplyMusicVolumes()`.
- Skalowanie głośności: `SfxScale = master*sfx`, `MusicScale = master*music`, **`AmbientScale = master*ambient`** (osobny kanał). `ApplyMusicVolumes()`: soundtrack/menu × `MusicScale`, ambient × `AmbientScale`.
- `OnEnable/OnDisable` subskrybują `SettingsManager.OnSettingsChanged += ApplyMusicVolumes` → suwaki działają na żywo.

**Stałe ścieżek audio** — `Assets/_Game/Scripts/Core/GameAssetPaths.cs`:
- `MusicSoundtrack[] = { "Assets/_Game/Audio/Music/SOUNDTRACK.ogg" }`, `MusicMenu[] = { ".../MENU.ogg" }`, `MusicAmbient[] = { ".../AMBIENT.ogg" }`.
- Resources fallback: `ResMusicSoundtrack = "Audio/Music/SOUNDTRACK"`, `ResMusicMenu = "Audio/Music/MENU"`, `ResMusicAmbient = "Audio/Music/AMBIENT"`.
- SFX: `SfxFootstep`/`SfxFootstep2`/`SfxJump`/`SfxFallWoosh`/`SfxGoblinHop`/`SfxPunch` (tablice ścieżek) + analogiczne `ResSfx*` (`"Audio/SFX/..."`). `goblin_hop.ogg`, reszta `.wav`.
- `GameAudioManager.LoadClips()` woła `GameAssetLoader.LoadAudio(GameAssetPaths.X, GameAssetPaths.ResX)` dla każdego klipu.

### 2. Ustawienia / suwak Ambient

- `SettingsData` (`Core/SettingsData.cs`, `[Serializable]`): nowe pole `public float ambientVolume = 0.7f;` (kopiowane w `Clone()` przez `MemberwiseClone` i jawnie w `CopyFrom`). Inne pola audio: `masterVolume = 0.8f`, `musicVolume = 0.7f`, `sfxVolume = 1.0f`.
- Persystencja: `wpg_settings.json` w `Application.persistentDataPath` (`SettingsManager.SavePath`). `ClampAndNormalize()` robi `Mathf.Clamp01(ambientVolume)`.
- `SettingsManager` (`Core/SettingsManager.cs`): `static event Action OnSettingsChanged`, `void UpdateAndApply(Action<SettingsData> mutate, bool save = true)`, `ApplySettings(bool notify = true)`.
- `SettingsMenuUI` (`UI/SettingsMenuUI.cs`): `enum VolumeChannel { Master, Music, Ambient, Sfx }`. Suwak „Otoczenie” to `AmbientSlider`; `OnVolumeChanged(VolumeChannel.Ambient, v)` ustawia `s.ambientVolume = v` i odświeża `_ambientValue` (`%`).

### 3. Teren (WorldGround)

**`WorldGround`** — `Assets/_Game/Scripts/World/WorldGround.cs`, **klasa statyczna** (`WPG.World`). Jedno źródło prawdy o wysokości.
- `static float GetGroundHeight(float x, float z)` / `GetGroundHeight(Vector3 p)` — zwraca `0` gdy `!_configured`.
- `static Vector3 Snap(Vector3 p, float yOffset = 0f)` — osadza Y na terenie.
- `static Mesh BuildMesh(float size, int resolution)` — siatka wyśrodkowana w (0,0), `resolution` clamp 8–250, `IndexFormat.UInt32`, nazwa meshu `"WPG_GroundMesh"`.
- `static void Configure(int seed, float amplitude, float noiseScale)` (offsety deterministycznie z `System.Random(seed)`), `static void ConfigureRaw(float offsetX, float offsetZ, float amplitude, float noiseScale)`.
- `static void AddFlatZone(float x, float z, float radius, float falloff, float? pin = null)`, `AddFlatZoneRaw(...)`, `ClearZones()`.
- `static void PopulateProfile(WorldGroundProfile profile)` — serializuje offsety/amplitudę/skalę/strefy do profilu.
- Properties: `IsConfigured`, `Amplitude`, `NoiseScale`, `OffsetX`, `OffsetZ`, `IReadOnlyList<FlattenZone> Zones`. Szum: `RawHeight` = Perlin 3 oktawy (`freq *= 2.1f`, `amp *= 0.5f`), wynik `(n/total) * 2f * amplitude`.

**`WorldGroundProfile`** — `World/WorldGroundProfile.cs`, `MonoBehaviour`, atrybuty `[DisallowMultipleComponent]` + **`[DefaultExecutionOrder(-1000)]`** (odtwarza teren zanim ktokolwiek próbkuje). Pola: `offsetX`, `offsetZ`, `amplitude = 2f`, `noiseScale = 55f`, `SerializableZone[] flattenZones`. `Awake() → Apply()`; `Apply()` woła `WorldGround.ConfigureRaw(...)` + `AddFlatZoneRaw(...)` na strefę. Komponent siedzi na obiekcie `Ground` w `WorldRoot.prefab`.

**`WorldGroundShaper`** — `Assets/_Game/Scripts/Editor/WorldGroundShaper.cs`, **Editor**, namespace `WPG.WorldEditor`, klasa statyczna.
- Stałe: `PrefabPath = "Assets/_Game/Prefabs/World/WorldRoot.prefab"`, `GroundMeshPath = "Assets/_Game/Prefabs/World/WorldRoot_GroundMesh.asset"`, `PathYOffset = 0.06f`, `ClearingYOffset = 0.02f`.
- Menu: `[MenuItem("WPG/World/Ukształtuj teren (ustawienia)…")] OpenWindow()`, `[MenuItem("WPG/World/Napraw unoszące się obiekty")] FixFloatingMenu()`.
- `struct Settings { int seed; float amplitude; float noiseScale; float baseFlatRadius; int resolution; }`; `Settings.Default = { seed = 13579, amplitude = 3f, noiseScale = 55f, baseFlatRadius = 18f, resolution = 160 }`.
- `public static void Apply(Settings settings)` → `RunOnPrefab(snapOnly:false, ...)`. Pipeline: `LoadPrefabContents` → `ConfigureSampler` (plateau: baza `AddFlatZone(0,0,…)`, `Camp_*`, `PowerSite_*`) → `ReshapeGroundPlane` (`GroundPlane` → mesh asset + `MeshCollider`, `localScale = 1`) → `WriteProfile` → `SnapStructure` (osadzanie **absolutne** `Y = teren`) → `StripMissingScripts` (`GameObjectUtility.RemoveMonoBehavioursWithMissingScript`) → `PrefabUtility.SaveAsPrefabAsset`.
- `WorldGroundShaperWindow : EditorWindow` — **EditorPrefs** z prefiksem `"WPG.GroundShaper."`: klucze `seed`, `amplitude`, `noiseScale`, `baseFlatRadius`, `resolution`.
- `WorldBootstrap` (runtime) — domyślne pola: `seed = 13579`, `terrainHeightAmplitude = 2f`, `terrainNoiseScale = 55f`, `terrainBaseFlatRadius = 18f`, `terrainMeshResolution = 160` (`[Range(16,250)]`).

### 4. Naprawy bugów

- `InteractableForwarder` (`World/InteractableForwarder.cs`) — przekazuje trigger na `IInteractable`. **Meta GUID:** `c980b5d472c54119bd37e3216bddb0ea` (`InteractableForwarder.cs.meta`).
- `WorldGroundShaper.StripMissingScripts(...)` usuwa komponenty z `m_Script: {fileID: 0}` przed zapisem prefabu (inaczej `SaveAsPrefabAsset` pada).
- `UIFactory` — bezpieczny fallback sprite'a (brak „różowych”/pustych elementów UI).

### 5. Przepływ danych (data flow / call graph)

```
MainMenuBootstrap.Awake()
  └─ GameAudioManager.EnsureExists().EnterMainMenuMusic()   // SOUNDTRACK + MENU
przycisk „Nowa gra” (MainMenuBootstrap, l. ~92)
  └─ GameManager.Instance.StartNewGame()
       └─ currentState = CharacterCreation
       └─ SceneManager.LoadScene(SceneNames.CharacterCreation)   // "CharacterCreation"

CharacterCreationUI.StartGame()
  └─ BuildData() → data.IsValid(out msg)
  └─ CharacterCreationSession.SetCurrentCharacter(data)
  └─ SceneManager.LoadScene(gameplaySceneName)   // "World"  (UWAGA: nie przez GameManager)

WorldBootstrap.Awake()
  └─ GameManager.EnsureExists(); SettingsManager.EnsureExists()
  └─ GameAudioManager.EnsureExists().EnterGameplayMusic()   // SOUNDTRACK + AMBIENT

PauseMenu (ESC, l. 98/109)
  └─ GameAudioManager.Instance.SetMenuLayerActive(true/false)

SettingsMenuUI → OnVolumeChanged(Ambient) → SettingsManager.UpdateAndApply(s ⇒ s.ambientVolume = v)
  └─ OnSettingsChanged → GameAudioManager.ApplyMusicVolumes()   // AMBIENT na żywo
```

- Nazwy scen: `SceneNames` w `Core/GameState.cs` → `MainMenu = "MainMenu"`, `CharacterCreation = "CharacterCreation"`, `World = "World"`.
- Stany: `enum GameState` (`Core/GameState.cs`).

### 6. Punkty integracji dla merge kolegi (CharacterCreation)

- **`CharacterCreationSession`** (`Core/CharacterCreationSession.cs`, **global namespace**, klasa statyczna) — most: `static CharacterCreationData CurrentCharacter`, `bool HasCharacter`, `SetCurrentCharacter(CharacterCreationData)`, `Clear()`. Waliduje przez `data.IsValid(out msg)`.
- **Niespójność do wyrównania:** `GameManager.GoToWorldFromCreation()` istnieje (ustawia `GameState.Playing` + ładuje `SceneNames.World`), ale `CharacterCreationUI.StartGame()` ładuje scenę **bezpośrednio** (`SceneManager.LoadScene(gameplaySceneName)`), z pominięciem `GameManager`. Docelowo: podpiąć kreator pod `GameManager.GoToWorldFromCreation()`, żeby `currentState` i czyszczenie stanu były spójne.
- **`MainMenuNewGameRouter`** (`UI/MainMenuNewGameRouter.cs`, **global namespace**) — alternatywna ścieżka: `LoadCharacterCreation()`, pole `[SerializeField] string characterCreationSceneName = "CharacterCreation"`. Można podpiąć do `Button.onClick` zamiast `GameManager.StartNewGame()`.
- Build: `ProjectSettings/EditorBuildSettings.asset` musi zawierać `CharacterCreation.unity` i `World.unity`.
- Dane postaci: `Assets/_Game/Scripts/Character/` — `CharacterCreationData`, `CharacterClassDatabase`, `CharacterClassDefinition`, `CharacterAppearanceData`, `CharacterStatsData`, `CharacterCreationEnums`, `CharacterCreationRules`.

---

## Mapa plików (szybki indeks)

| Obszar | Pliki | Kluczowe klasy / typy |
|--------|-------|------------------------|
| Audio — manager | `Assets/_Game/Scripts/Core/GameAudioManager.cs` | `GameAudioManager` (singleton, warstwy + pula 3D) |
| Audio — ścieżki | `Assets/_Game/Scripts/Core/GameAssetPaths.cs` | `GameAssetPaths` (Music*/Sfx*/Res*) |
| Audio — pliki | `Assets/_Game/Audio/Music/{SOUNDTRACK,MENU,AMBIENT}.ogg`, `Assets/_Game/Audio/SFX/*` | — |
| Ustawienia — dane | `Assets/_Game/Scripts/Core/SettingsData.cs` | `SettingsData` (`ambientVolume`) |
| Ustawienia — manager | `Assets/_Game/Scripts/Core/SettingsManager.cs` | `SettingsManager` (`OnSettingsChanged`, `UpdateAndApply`) |
| Ustawienia — UI | `Assets/_Game/Scripts/UI/SettingsMenuUI.cs` | `SettingsMenuUI`, `VolumeChannel` |
| Teren — runtime | `Assets/_Game/Scripts/World/WorldGround.cs`, `WorldGroundProfile.cs` | `WorldGround` (static), `WorldGroundProfile` |
| Teren — editor | `Assets/_Game/Scripts/Editor/WorldGroundShaper.cs` | `WorldGroundShaper`, `WorldGroundShaperWindow` |
| Teren — assety | `Assets/_Game/Prefabs/World/WorldRoot.prefab`, `WorldRoot_GroundMesh.asset` | — |
| Teren — generacja | `Assets/_Game/Scripts/World/WorldGenerator.cs`, `WorldAssetPlacer.cs`, `WorldBootstrap.cs` | `WorldGenerator`, `WorldAssetPlacer`, `WorldBootstrap` |
| Wydajność | `Assets/_Game/Scripts/Core/CameraCache.cs`, `World/ForestAtmosphereSettings.cs`, `UI/DamageNumber.cs` | `CameraCache`, `ForestAtmosphereSettings`, `DamageNumber` |
| Naprawy | `Assets/_Game/Scripts/World/InteractableForwarder.cs`, `UI/UIFactory.cs` | `InteractableForwarder`, `UIFactory` |
| Stan gry / sceny | `Assets/_Game/Scripts/Core/GameManager.cs`, `GameState.cs` | `GameManager`, `GameState`, `SceneNames` |
| Menu / routing | `Assets/_Game/Scripts/UI/MainMenuBootstrap.cs`, `MainMenuNewGameRouter.cs`, `PauseMenu.cs` | `MainMenuBootstrap`, `MainMenuNewGameRouter`, `PauseMenu` |
| Tworzenie postaci | `Assets/_Game/Scenes/CharacterCreation.unity`, `Scripts/Character/*`, `Core/CharacterCreationSession.cs`, `UI/CharacterCreation*`, `UI/CharacterPreviewController.cs` | `CharacterCreationData`, `CharacterCreationSession`, `CharacterCreationUI`, `CharacterPreviewController` |
| Git | `.gitignore` | — |

---

## Jak szukać w repo

**Entry pointy (od czego zacząć czytanie):**
- `WorldBootstrap.Start()` / `WorldBootstrap.Awake()` — wejście do sceny `World` (`World.unity`), spawn gracza, świat, muzyka gameplay.
- `MainMenuBootstrap.Awake()` — wejście do menu głównego, muzyka menu, przycisk „Nowa gra”.
- `GameAudioManager.Awake()` + `EnsureExists()` — inicjalizacja całego audio (warstwy + pula 3D + preload).
- `SettingsManager.Awake()` → `Load()` → `ApplySettings()` — wczytanie `wpg_settings.json`.
- `WorldGroundProfile.Awake()` → `Apply()` — odtworzenie terenu w runtime.
- `WorldGroundShaper.Apply(Settings)` — przebudowa terenu z poziomu edytora.
- `CharacterCreationUI.StartGame()` — finalizacja kreatora i przejście do świata.

**Grep hints (ripgrep):**
- Warstwy muzyki: `rg "EnterMainMenuMusic|EnterGameplayMusic|SetMenuLayerActive"`
- Kanał Ambient: `rg "ambientVolume|AmbientScale|VolumeChannel.Ambient"`
- SFX API: `rg "PlayFootstep|PlayJump|PlayPunch|PlayFall|PlayGoblinHop"`
- Ścieżki audio: `rg "MusicSoundtrack|MusicMenu|MusicAmbient|ResMusic"`
- Wysokość terenu: `rg "GetGroundHeight|WorldGround\."`
- Profil terenu: `rg "WorldGroundProfile|ConfigureRaw|AddFlatZoneRaw"`
- Menu edytora: `rg "MenuItem\(\"WPG/World"`
- EditorPrefs terenu: `rg "WPG.GroundShaper\."`
- Most kreatora: `rg "CharacterCreationSession|GoToWorldFromCreation|StartNewGame"`
- Nazwy scen: `rg "SceneNames\.|LoadScene"`

**Konwencje:**
- Namespace gameplay/core: `WPG.Core`, `WPG.World`, `WPG.UI`, `WPG.Character`, `WPG.Player`, `WPG.Enemies`; editor: `WPG.WorldEditor`.
- Most kreatora (`CharacterCreationSession`) i `MainMenuNewGameRouter` są w **global namespace** (kod kolegi) — szukaj bez prefiksu `WPG.`.
- Singletony powstają przez `EnsureExists()` (+ `DontDestroyOnLoad`), nie przez ręczne `new`.

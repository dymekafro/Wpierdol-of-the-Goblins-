# Live edit — mgła i oświetlenie lasu

Komponent **`ForestAtmosphereSettings`** (`WPG.World`) pozwala regulować mgłę, ambient, słońce i kolor podłoża **w trakcie Play** bez przebudowy świata w kodzie.

## Skąd wziąć obiekt Atmosphere

### Automatycznie (zalecane przy grze z MainMenu)

`WorldBootstrap` na starcie sceny **World** woła `ForestAtmosphereSettings.EnsureExists()` — jeśli w hierarchii nie ma komponentu, powstaje pusty GameObject **`Atmosphere`** z `ForestAtmosphereSettings`.

### Ręcznie w scenie (zapis na stałe w World.unity)

1. Otwórz `Assets/_Game/Scenes/World.unity`.
2. Utwórz pusty GameObject: **Atmosphere**.
3. **Add Component** → `Forest Atmosphere Settings` (skrypt `ForestAtmosphereSettings`).
4. **Ctrl+S** — zapis sceny.

Przy następnym Play wartości startują z Inspector tego obiektu (zamiast tylko domyślnych z kodu).

## Workflow live edit

1. **Play** — z MainMenu (ładuje World) albo otwórz **World.unity** i wciśnij Play.
2. W **Hierarchy** wybierz **Atmosphere**.
3. W **Inspector** przesuwaj suwaki / zmieniaj kolory:
   - **Mgła** — włącz/wyłącz, kolor, gęstość
   - **Ambient** — sky / equator / ground (Trilight)
   - **Słońce** — kolor, intensywność, rotacja (Euler)
   - **Podłoże** — tint płaszczyzny `GroundPlane`
4. Zmiany widać od razu (`OnValidate` + `Update` w Play).
5. **Stop Play** — Unity może zapytać, czy **zachować zmiany na obiekcie sceny** → wybierz **Yes**.
6. **Ctrl+S** na `World.unity`, żeby zapisać mgłę/światło na dysku.

> Bez „Yes” po Stop Play wartości wracają do ostatniego zapisanego stanu sceny.

## Kontekst menu

Na komponencie **PPM** → **Apply Now** — wymusza `ApplyAtmosphere()` (to samo co przy zmianie w Inspector).

## Współpraca z GoldenHourLighting

`GoldenHourLighting` **nie nadpisuje** ustawień, gdy w scenie jest `ForestAtmosphereSettings`:

- mgła i ambient idą z komponentu,
- ręczne słońce (`GoldenHourSun`) bierze kolor / intensywność / rotację z Inspector,
- po wygenerowaniu terenu `WorldGenerator` ponownie stosuje atmosferę (m.in. podłoże).

Bez obiektu Atmosphere działają **domyślne stałe** w `GoldenHourLighting` (jak wcześniej).

## Pola w Inspector

| Sekcja | Pola |
|--------|------|
| Mgła / Fog | `fogEnabled`, `fogColor`, `fogDensity` |
| Ambient | `ambientSkyColor`, `ambientEquatorColor`, `ambientGroundColor` |
| Słońce / Sun | `sunColor`, `sunIntensity`, `sunRotation` |
| Podłoże / Ground | `groundColor` (tint `GroundPlane` + override w `MaterialFactory`) |

Niebo (skybox) nadal ustawia `GoldenHourLighting.ApplySkybox()` — osobna warstwa od mgły w Inspector.

## Wskazówki

- Jeśli nie widzisz **Atmosphere** w Hierarchy podczas Play, sprawdź root sceny — `WorldBootstrap` tworzy go na początku `Start`.
- Kamera główna dostosowuje `backgroundColor` do koloru mgły przy każdym `ApplyAtmosphere`.
- Dla trwałego „presetu” zapisz scenę po Stop Play z zachowanymi zmianami.

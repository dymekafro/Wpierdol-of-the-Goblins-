# Audyt i implementacja systemu postaci — Wpierdol of the Goblins

> Dokument wygenerowany automatycznie przez agenta. Zawiera pełne rozpoznanie systemu postaci,
> wprowadzone zmiany w kodzie oraz **uczciwą listę kroków, które trzeba wykonać ręcznie w Unity Editor**
> (konfiguracja avatarów Humanoid, podpięcie prefabów w Inspectorze). Tych kroków nie da się
> bezpiecznie zrobić z poziomu edycji plików tekstowych poza Unity.

Data: 2026-05-31 · Unity 6 (URP) · referencyjna postać: **Krasnolud (Player_Dwarf)**.

---

## 1. POSTACIE (prefaby graczy)

Lokalizacja: `Assets/_Game/Prefabs/Players/`

| Prefab | GUID | Model (mesh) | Avatar (Humanoid) | Animator Controller | Status |
|---|---|---|---|---|---|
| `Player_Dwarf.prefab` | `52282afe865c0cf47b9dd3880467be33` | **Barbarian Wyder** (Meshtint) | `Barbarian Wyder.FBX` (`006a8310…`) | `DruidBlink_OverrideController 1` (`c327278e…`) | ✅ Działa — referencja |
| `Player_Warrior.prefab` | `b19583326789ffe4ca85a4e3ccb25c0d` | **V-Bot** + części **Blink HumanMale** | root: `VBOT_LOD` (`340f99b8…`), dziecko: `HumanMale_Character` (`2255221d…`) | `DruidBlink_OverrideController` (`d123e389…`) | ⚠️ Zbudowany przez kolegę (klon Dwarfa), wymaga weryfikacji avatara/modelu w Unity |

Oba prefaby mają **identyczny zestaw 11 komponentów** (Warrior to klon Dwarfa) — patrz sekcja "RECEPTURA".

Brak postaci dla pozostałych klas (Łowca, Rycerz, Druid, Barbarzyńca jako osobna klasa, Mag, Łotrzyk, Wędrowiec) — patrz "BRAKUJĄCE".

---

## 2. MODELE (dostępne FBX postaci)

| Pack | Plik FBX | Proponowana klasa |
|---|---|---|
| Meshtint Free Barbarian | `Assets/Meshtint Free Barbarian/FBX/Barbarian Wyder.FBX` | Krasnolud (zajęty) / Barbarzyńca |
| Invector LITE | `Assets/Invector-3rdPersonController_LITE/3D Models/Characters/Invector@V-Bot 2.0/FBX/VBOT_LOD.fbx` | bazowy gracz (Player_Invector), Wojownik |
| Blink | `Assets/Blink/Art/Characters/Stylized/Humans/Meshes_Humans/HumanMale_Character.fbx` | Wojownik / Rycerz |
| Axe Warrior | `Assets/Axe Warrior/Model/Axe_Warrior.fbx` | Wojownik (alternatywa) |
| JC LP Medieval LITE | `Assets/JC_LP_MedievalCharacters_LITE/Models/SM_MedievalMaleLite_01.fbx` | Rycerz / Paladyn |
| JC Stylized Modular | `Assets/JC_StylizedModularCharacters/Models/SM_Ranger_Male.fbx`, `SM_Ranger_Female.fbx` | Łowca (Ranger) |
| URP GanzSe | `Assets/URP GanzSe Free Modular Character Pack/Models/Models Update 1.1/GanzSe Free Modular Character 1_1.fbx` | Druid / Mag |

---

## 3. ANIMACJE (dostępne zestawy)

**Blink** (`Assets/Blink/Art/Animations/`) — najbogatszy, humanoidalny:
- **Movement**: Idle, Walk/Run (Forward/Backward/Left/Right + diagonalne), Sprint, Strafe L/R, Jumps, Rolls (4 kierunki), FallingLoop.
- **Combat**: IdleCombat, MeeleeAttack_OneHanded / TwoHanded, Spin/SpinToWin, Punch L/R, BowShot, CastingLoop, SpellCast, Buff, Block, GetHit, Death, Stunned.
- **Gathering**: Gathering, MiningLoop.

**Meshtint Barbarian** (`…/Legacy (No longer supported)/Animations/`): idle, walk, run, attack01/02, defend, hit, jump, die (legacy, mniezalecane — preferuj Blink/Invector retarget).

**Invector LITE** (`…/3D Models/Animations/`): `Basic_FreeMovement.fbx`, `Basic_StrafeMoveset.fbx` — bazowa lokomocja używana przez kontrolery.

## 4. ANIMATORY (kontrolery)

| Plik | Typ | Użycie |
|---|---|---|
| `Assets/Invector-3rdPersonController_LITE/Animator/Invector@BasicLocomotion.controller` | AnimatorController | baza lokomocji Invector |
| `Assets/_Game/Animations/Player/DruidBlink_Combat.controller` | AnimatorController | baza walki WPG (Druid/Blink) |
| `Assets/_Game/Animations/Player/DruidBlink_OverrideController.overrideController` (`d123e389…`) | Override | **Player_Warrior** |
| `Assets/_Game/Animations/Player/DruidBlink_OverrideController 1.overrideController` (`c327278e…`) | Override | **Player_Dwarf** |
| `Assets/_Game/Resources/Anim/InvectorBasicLocomotion.controller` | AnimatorController | runtime (Resources) |

Wniosek: standard projektu = jeden bazowy controller + **AnimatorOverrideController per postać**. Nowe klasy powinny iść tym samym wzorcem (klon override, podmiana clipów jeśli trzeba).

---

## 5. KLASY ISTNIEJĄCE (kod)

`Assets/_Game/Scripts/Character/CharacterCreationEnums.cs` → `enum CharacterClassType`.
`Assets/_Game/Scripts/Character/CharacterClassDatabase.cs` → statystyki, ekwipunek, umiejętności.
UI kreatora (`CharacterCreationUI.cs`) **automatycznie** generuje przyciski dla każdej pozycji z `CharacterClassDatabase.All` — dodanie klasy do bazy = automatycznie pojawia się w kreatorze.

Statystyki: `CharacterStatsData(STR, DEX, INT, END, PER, CHA)`.

| Klasa (enum) | Nazwa PL | STR/DEX/INT/END/PER/CHA | Model docelowy |
|---|---|---|---|
| Warrior | Wojownik | 8/5/2/7/3/3 | V-Bot/HumanMale (Player_Warrior) |
| Mage | Mag | 2/3/9/4/5/5 | GanzSe (brak prefabu) |
| Archer | Łucznik | 4/8/3/4/8/3 | Ranger (brak prefabu) |
| Rogue | Łotrzyk | 4/9/4/4/6/4 | (brak prefabu) |
| Dwarf | Krasnolud | 7/3/3/9/4/3 | Barbarian Wyder (Player_Dwarf) ✅ |
| Wanderer | Wędrowiec | 5/6/4/6/6/4 | (brak prefabu) |
| **Barbarian** ✨ | Barbarzyńca | 9/5/1/8/3/2 | Barbarian Wyder (współdziel. z Dwarf) |
| **Ranger** ✨ | Łowca | 4/8/2/5/7/3 | SM_Ranger_Male/Female |
| **Knight** ✨ | Rycerz | 7/3/3/8/2/5 | SM_MedievalMaleLite_01 |
| **Druid** ✨ | Druid | 3/5/8/6/4/4 | GanzSe Modular |

✨ = dodane w tej iteracji (kod gotowy, prefab do zrobienia w Unity).

---

## 6. PRZEPŁYW DANYCH (spawn / bootstrap)

```
MainMenu → GameManager.StartNewGame() → CharacterCreation (scena)
   → CharacterCreationUI: wybór klasy + statystyki
   → CharacterCreationSession.SetCurrentCharacter(data)
   → scena World

World (scena):
   ├─ WorldBootstrap.Start(): generuje świat + buduje Player_Invector (V-Bot)
   │     z GameAssetRegistry.InvectorController, dokleja PlayerStats/PlayerCombat/
   │     CharacterAnimDriver/InvectorPlayerAdapter, tworzy kamerę vThirdPersonCamera, HUD.
   └─ CharacterClassPlayerSpawner.Start(): jeśli jest CharacterCreationSession.CurrentCharacter,
         niszczy Player_Invector i instancjonuje prefab klasy (Dwarf/Warrior/…).
```

### Receptura komponentów referencyjnej postaci (Player_Dwarf — root GameObject)
1. `Transform` (skala 1.3)
2. `vComment` (Invector, kosmetyczny)
3. `Animator` (Avatar Humanoid + AnimatorOverrideController)
4. `vThirdPersonController` (Invector — lokomocja)
5. `vThirdPersonInput` (Invector — wejście)
6. `Rigidbody`
7. `CapsuleCollider`
8. `AnimatorParameterMirror` (WPG)
9. `PlayerStats` (WPG)
10. `CharacterAnimDriver` (WPG)
11. `PlayerCombat` (WPG)
12. `InteractionDetector` (WPG)
13. `InvectorPlayerAdapter` (WPG)
14. `Inventory` (WPG)
15. `BaseUIManager` (WPG)

To jest wzorzec do skopiowania dla każdej nowej klasy.

---

## 7. ZMIANY WPROWADZONE W TEJ ITERACJI (kod, bezpieczne)

Wszystkie zmiany są **addytywne** i przeszły kontrolę lintera. Nie naruszają działającego Krasnoluda.

1. **`CharacterCreationEnums.cs`** — dopisane na końcu enuma (bezpieczne dla serializacji):
   `Barbarian`, `Ranger`, `Knight`, `Druid`.

2. **`CharacterClassDatabase.cs`** — pełne definicje 4 nowych klas (nazwa PL, opis, styl gry,
   statystyki, ekwipunek startowy, umiejętności). Automatycznie pojawiają się w kreatorze postaci.

3. **`PlayerAttributes.cs`** — nowa fabryka `FromCreatedCharacter(CharacterCreationData)`:
   mapuje statystyki z kreatora na atrybuty rozgrywki (STR/DEX/INT/END/CHA 1:1, `mana = intelligence`).
   ⚠️ `Percepcja` z kreatora nie ma jeszcze odpowiednika w `PlayerAttributes` (do rozważenia — luka).

4. **`WorldBootstrap.cs`** — przy **nowej grze** (nie przy „Kontynuuj") atrybuty gracza są
   teraz brane z postaci z kreatora (wcześniej zawsze `CreateDruidBase`). Zapisywane też do `GameManager.attributes`.

5. **`CharacterClassPlayerSpawner.cs`** — przepisany na **generyczne mapowanie klasa→prefab**:
   - nowe pole `classPrefabs` (lista `ClassPrefabEntry { classType, prefab }`) — preferowane,
   - zachowane pola `warriorPrefab` / `dwarfPrefab` (kompatybilność wsteczna ze sceną World),
   - po spawnie postać jest **poprawnie inicjalizowana**: `PlayerStats.Init` ze statystykami z kreatora,
     `Inventory.Bind` + przedmioty startowe, `BaseUIManager.Initialize`, przepięcie `PlayerHUD`
     oraz `vThirdPersonCamera.SetMainTarget` na nową postać (wcześniej HUD/kamera zostawały na
     zniszczonym Player_Invector — realny bug naprawiony).

---

## 8. BRAKUJĄCE (do zrobienia ręcznie w Unity)

> Tych rzeczy **nie da się** zrobić bezpiecznie poza edytorem Unity — wymagają importu rigów,
> konfiguracji Humanoid Avatar i wizualnego podpięcia referencji.

### A. Prefaby nowych klas (priorytet)
Dla każdej klasy: **Barbarzyńca, Łowca (Ranger), Rycerz (Knight), Druid**:
1. Zaimportuj model FBX → zakładka **Rig → Animation Type: Humanoid → Apply** (utworzy Avatar).
2. W `Assets/_Game/Prefabs/Players/` zduplikuj `Player_Dwarf.prefab` i zmień nazwę (np. `Player_Ranger`).
3. Podmień siatkę dziecka (model) na nowy FBX, zachowując komponenty roota (15 z receptury).
4. W komponencie `Animator` ustaw: **Avatar** = nowego modelu, **Controller** = klon
   `DruidBlink_OverrideController` (lub bazowy `DruidBlink_Combat`).
5. Sprawdź skalę (Dwarf = 1.3) i wysokość CapsuleCollider.
6. (Opcjonalnie) klon AnimatorOverrideController i podmiana clipów na Blink (Attack/Cast/Death).

### B. Podpięcie prefabów do spawnera (scena `World.unity`)
1. Zaznacz obiekt `CharacterClassPlayerSpawner`.
2. W liście **Class Prefabs** dodaj wpisy: `Barbarian→Player_Barbarian`, `Ranger→Player_Ranger`,
   `Knight→Player_Knight`, `Druid→Player_Druid`, (oraz Warrior/Dwarf jeśli chcesz przez listę).
3. Pola legacy `warriorPrefab`/`dwarfPrefab` mogą zostać — działają jako fallback.

### C. Weryfikacja Player_Warrior
Prefab kolegi miesza szkielet **V-Bot** (root) z częściami **Blink HumanMale** (dziecko, drugi Animator
z avatarem HumanMale). Trzeba w Unity sprawdzić, czy:
- model się renderuje (nie różowy/magenta — `MaterialUpgrader` to łata w runtime, ale prefab warto poprawić),
- avatar roota pasuje do faktycznej siatki (V-Bot vs HumanMale),
- drugi `Animator` na dziecku nie powoduje konfliktu (zwykle dziecko-mesh nie powinno mieć własnego Animatora).

### D. Luki integracyjne (opcjonalne usprawnienia)
- `Percepcja` nie wpływa jeszcze na `PlayerAttributes` (brak pola). Do dodania, jeśli ma mieć efekt w grze.
- Wygląd z kreatora (`CharacterAppearanceData`: fryzura, zarost, strój) **nie jest** aplikowany na model —
  modele nie są jeszcze modularne w runtime. Do zrobienia osobno (GanzSe/JC są modularne).
- `DwarfClassPlayerSpawner.cs` jest teraz nadmiarowy względem generycznego `CharacterClassPlayerSpawner`
  (zostawiony, nie usuwam — używany może być w innej scenie; do konsolidacji w przyszłości).

---

## 9. WALIDACJA

- ✅ Linty: brak błędów na wszystkich zmienionych plikach C#.
- ✅ Brakujące skrypty: w `Player_Dwarf.prefab`, `Player_Warrior.prefab` i `World.unity`
  **nie ma** komponentów z `m_Script: {fileID: 0}` ani pustych GUID-ów.
- ✅ Wszystkie 11 GUID-ów skryptów na obu prefabach rozwiązuje się do istniejących plików `.cs`.
- ✅ GUID-y avatarów/kontrolerów obu prefabów wskazują na istniejące assety.
- ✅ Referencja sceny World → spawner: `warriorPrefab`/`dwarfPrefab` poprawne; nowe pole
  `classPrefabs` dodane bezpiecznie (puste w istniejącej scenie, do uzupełnienia w Inspectorze).
- ⚠️ **Nie do przetestowania poza Unity**: faktyczny render modeli, retarget animacji Humanoid,
  poprawność avatarów, rozgrywka w trybie Play. Wymaga uruchomienia edytora.

---

## 9b. ITERACJA 2 — AUTOMATYZACJA PREFABÓW (2026-05-31)

Problem zgłoszony: w grze widać tylko **Krasnoluda** i **Wojownika**. Powód potwierdzony w plikach:
- istniały tylko `Player_Dwarf.prefab` i `Player_Warrior.prefab`,
- w `World.unity` spawner miał wpięte **wyłącznie** `warriorPrefab`/`dwarfPrefab` (brak listy `classPrefabs`),
  więc każda inna klasa lądowała na fallbacku → pierwszy dostępny prefab (Wojownik).

### Co dodano (kod, bezpieczne)
1. **`Assets/_Game/Scripts/Editor/PlayerPrefabBuilder.cs`** — narzędzie edytora (menu **WPG → Players**):
   - **„Build Class Prefabs + Wire Spawner"** — buduje prefaby i od razu wpina je do spawnera w `World.unity`,
   - **„Build Class Prefabs (only)"** — tylko buduje prefaby.

   Mechanizm budowy (bezpieczny dla rigów Humanoid): bierze sprawdzony `Player_Dwarf` jako szablon
   (root + 15 komponentów Invector/WPG + Animator + kontroler), instancjonuje i rozpakowuje, **usuwa
   tylko wizualne dziecko** (stary szkielet/mesh), wstawia model docelowego FBX jako nowe dziecko,
   ustawia `Animator.avatar` na avatar tego FBX (kontroler zostaje — Humanoid retarget działa dla
   dowolnego rigu), dopasowuje skalę roota i `CapsuleCollider` do gabarytów modelu, zapisuje prefab.
   Krasnolud i Wojownik **nie są modyfikowane** — tylko podpinane do mapy.

   Prefaby budowane (modele potwierdzone jako Humanoid `animationType: 3`):
   | Prefab | Model FBX | Skala roota |
   |---|---|---|
   | `Player_Ranger` | `JC_StylizedModularCharacters/Models/SM_Ranger_Male.fbx` | 1.0 |
   | `Player_Knight` | `JC_LP_MedievalCharacters_LITE/Models/SM_MedievalMaleLite_01.fbx` | 1.0 |
   | `Player_Druid` | `URP GanzSe …/GanzSe Free Modular Character 1_1.fbx` | 1.0 |
   | `Player_Barbarian` | `Meshtint Free Barbarian/FBX/Barbarian Wyder.FBX` | 1.3 |

2. **`CharacterClassPlayerSpawner.cs`** — dodano **runtime fallback aliasów klas** (bez ładowania assetów):
   gdy klasa nie ma własnego prefabu, używany jest model klasy pokrewnej. Aliasy:
   `Łucznik→Łowca`, `Wędrowiec→Łowca`, `Łotrzyk→Rycerz`, `Mag→Druid`.

### Mapowanie spawnera (wpinane automatycznie do `classPrefabs` w `World.unity`)
| Klasa | Prefab |
|---|---|
| Warrior | `Player_Warrior` |
| Dwarf | `Player_Dwarf` |
| Barbarian | `Player_Barbarian` |
| Ranger / Archer / Wanderer | `Player_Ranger` |
| Knight / Rogue | `Player_Knight` |
| Druid / Mage | `Player_Druid` |

### Co musi zrobić użytkownik w Unity (jednorazowo)
1. Otwórz Unity (przeładuje skrypty).
2. Menu **WPG → Players → Build Class Prefabs + Wire Spawner**.
3. Narzędzie utworzy 4 nowe prefaby w `Assets/_Game/Prefabs/Players/` i zapisze `World.unity` z pełną mapą.
4. (Opcjonalnie) skoryguj w Inspectorze skalę/`CapsuleCollider`, jeśli któryś model jest za duży/mały.

### Znane ograniczenia (bez zmian w tej iteracji)
- **Podgląd 3D w kreatorze**: `CharacterCreationUI` jest w pełni proceduralne i pokazuje podgląd **tekstowy**
  (klasa/statystyki/wygląd). `CharacterPreviewController` nie jest podpięty do flow kreatora — model 3D
  w kreatorze to osobne zadanie (poza zakresem tej naprawy spawnu w grze).
- Skala/kolider nowych modeli są dopasowane automatycznie z gabarytów mesha — wartości to sensowny start,
  warto je zweryfikować w Play Mode.
- Wygląd z kreatora (fryzura/zarost/strój) nadal nie jest aplikowany na model (modele nie są modularne w runtime).

---

## 10. PODSUMOWANIE DLA UŻYTKOWNIKA

**Zrobione w kodzie (gotowe, kompiluje się):** 4 nowe klasy w enumie + bazie, kreator pokazuje je
automatycznie, statystyki z kreatora trafiają wreszcie do gracza, spawner jest generyczny i
poprawnie podpina HUD/kamerę/ekwipunek po podmianie postaci.

**Do zrobienia przez Ciebie w Unity:** import + Humanoid dla nowych modeli, duplikacja prefabu Dwarfa
na 4 nowe klasy z podmianą modelu/avatara/controllera, wpięcie ich w listę `Class Prefabs`
spawnera w scenie World, oraz weryfikacja prefabu Warrior kolegi.

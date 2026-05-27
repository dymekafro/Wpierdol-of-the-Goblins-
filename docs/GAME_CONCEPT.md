# Wpierdol of the Goblins — Koncept gry

## Elevator pitch

Otwarty RPG akcji w **jednej ciągłej mapie** — **magicznym, ciemnym lesie**. Grasz **druidem**, ostatnim strażnikiem lasu, który ma **własną bazę** (sady / hub) w sercu puszczy. Wychodzisz z bazy, eksplorujesz las, znajdujesz **obozy goblinów**, walczysz, wracasz do bazy (craft, heal, save, planowanie), a po oczyszczeniu **podbijasz obóz** — staje się bezpieczny i daje bonusy. **To nie jest gra levelowa.** Brak ekranów „Level 1–2–3”; progres to podbój obozów, atrybuty, drzewko Przyrody i relikwie.

---

## Setting: Magiczny Ciemny Las

Jedna otwarta przestrzeń (`World.unity`) — nie seria osobnych poziomów.

**Atmosfera:**
- Gęsty, mroczny las o charakterze **magicznym**, nie realistycznym horrorze
- **Bioluminescencja** — świecące grzyby, mszyste runy, błękitne i fioletowe akcenty w mroku
- **Mgła** — warstwy gęstości, czytelność walki, tajemnica za drzewami
- **Ruiny** — kamienne kręgi, popękane ołtarze, resztki dawnej cywilizacji druidów
- Dźwięk: szelest, odległe krzyki goblinów, pulsująca magia natury

**Ton:** mroczny las, humor sytuacyjny, brutalna akcja (stąd tytuł). Gobliny są realnym zagrożeniem — nie parodia.

**MVP:** jeden duży las z bazą druida i kilkoma obozami goblinów.  
**Przyszłość:** wyjście z lasu (questy, nowe regiony, inne biomy) — poza zakresem MVP.

---

## Baza druida — serce rozgrywki

**Baza druida** (w docs: *Sady* / *Groves*) to centrum mapy i emocjonalny hub gracza:

| Funkcja | Opis |
|---------|------|
| **Punkt startowy** | Nowa gra i respawn po śmierci |
| **Save point** | Zapis postępu (pozycja, obozy, inventory, drzewko) |
| **Crafting** | Mikstury, amulety ze surowców z lasu |
| **Drzewko Przyrody** | UI skill tree — gałęzie Ogień / Życie / Kształt |
| **Odpoczynek** | Uzupełnienie HP i many |
| **Planowanie** | Bezpieczna strefa bez wrogów — decyzje przed kolejną wyprawą |

Baza **nie jest osobną sceną levelową** — to strefa w `World.unity` (trigger + wizualna enklawa). Gracz zawsze wraca „do domu” w tym samym świecie.

---

## Pętla rozgrywki (core loop)

```
┌─────────────────────────────────────────────────────────────┐
│  BAZA DRUIDA — save, craft, heal, drzewko, planowanie       │
└──────────────────────────┬──────────────────────────────────┘
                           │ wyjście w las
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  EKSPLORACJA — otwarty las, loot, miejsca mocy, zagrożenia  │
└──────────────────────────┬──────────────────────────────────┘
                           │ odkrycie obozu
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  OBIÓZ GOBLINÓW — walka, totem, oczyszczenie (Cleared)      │
└──────────────────────────┬──────────────────────────────────┘
                           │ powrót (opcjonalnie przed podbojem)
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  BAZA — przetworzenie łupów, upgrade, zapis                   │
└──────────────────────────┬──────────────────────────────────┘
                           │ powrót do obozu / kolejny obóz
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  PODBIJANIE — obóz Captured: bezpieczny, bonus, odblokowanie  │
└──────────────────────────┬──────────────────────────────────┘
                           │ następny obóz, głębiej w lesie
                           ▼
                    (powtórka pętli)
```

**Kluczowe zasady:**
- Eksploracja jest **swobodna** — brak liniowych „etapów”
- Trudność rośnie **odległością od bazy** (pierścienie), nie numerem levelu
- Powrót do bazy jest **częścią strategii**, nie karą

---

## Obozy goblinów

Główne cele na mapie. Każdy obóz to enklawa wrogów z **totemem** w centrum.

### Stany obozu

| Stan | PL | Zachowanie |
|------|-----|------------|
| **Active** | Aktywny | Pełny spawn goblinów, totem żywy, strefa niebezpieczna |
| **Cleared** | Oczyszczony | Wrogowie pokonani / totem zniszczony; nagroda (loot, punkty Przyrody); obóz jeszcze nie daje stałego bonusu |
| **Captured** | Podbity | Stały postęp: bezpieczna strefa, bonus (np. crafting, buff, skrót), czasem odblokowanie przejścia głębiej w las |

Przejścia: `Active` → `Cleared` (walka) → `Captured` (interakcja / ritual w oczyszczonym obozie — szczegóły w implementacji).

### Typy goblinów w obozach

| Typ | Rola | Zachowanie |
|-----|------|------------|
| **Szturmowiec** | Front, melee | Agresywny, goni gracza |
| **Łucznik** | Dystans | Utrzymuje dystans, ostrzał |
| **Szaman** | Support | Buffy sojuszników, debuffy (później) |
| **Saper** | Area denial | Pułapki, miny (później) |

MVP: szturmowiec + łucznik w pierwszym obozie.

---

## Bohater: Druid

- Ostatni strażnik lasu; magia: **Ogień**, **Życie**, **Kształt** (transformacje)
- Startowe bonusy: **MANA +3**, **INT +2**, **END +1**, **STR −1**
- Słabszy w melee, silniejszy w czarach i kontroli terenu

---

## Progres bez leveli

Brak tradycyjnego XP i leveli postaci.

### Atrybuty

| Skrót | Wpływ |
|-------|--------|
| STR | Melee, udźwig |
| DEX | Uniki, szybkość |
| MANA | Pula many, regeneracja |
| INT | Siła czarów |
| END | HP, odporność |
| CHA | NPC, ceny (future) |

Punkty z obozów, questów, miejsc mocy — **nie z leveli**.

### Drzewko Przyrody

Gałęzie: **Ogień**, **Życie**, **Kształt**. Odblokowanie w **bazie druida** za punkty Przyrody.

### Relikwie

Pasywne bonusy z obozów, skrzyń, miejsc mocy.

### Inne punkty na mapie

- **Miejsca mocy** — jednorazowe bonusy (atrybut, czar, relikwia)
- **Loot** — surowce do craftingu w bazie

---

## Przyszłe rozszerzenia (poza MVP)

- **Wyjście z lasu** — granica mapy, quest „Przejdź przez Przeszłą Bramę”
- **Questy i NPC** — w bazie i w podbitych obozach
- **Nowe regiony** — bagna, wąwozy, inne biomy jako **ciągłe obszary** tej samej mapy lub streamowane chunki — **nie** osobne leveli
- Pełne 4 typy goblinów, boss totem, dialogi, muzyka

---

## MVP scope

**W scope MVP:**

- [ ] Menu → tworzenie druida → **jedna scena** `World.unity`
- [ ] **Baza druida** w centrum mapy (trigger, save, heal, placeholder craft/drzewko)
- [ ] **1 obóz goblinów** (Active → Cleared → Captured)
- [ ] Ruch 3D, kamera, HUD HP/mana
- [ ] 1–2 typy goblinów, podstawowe czary
- [ ] Prosty loot + crafting w bazie
- [ ] Placeholder grafika URP

**Poza MVP:** wiele obozów, pełne podbijanie wizualne, streaming świata, regiony poza lasem, pełny save/load, wszystkie typy goblinów.

---

## Techniczne założenia (Unity)

- **URP**, C#, `Assets/_Game/Scripts/`
- Sceny: `MainMenu` → `CharacterSelection` → `AttributeAllocation` → **`World`** (jedna mapa)
- `GameData` (DontDestroyOnLoad) — stan między scenami i progres obozów
- Stuby: `DruidBase.cs`, `GoblinCamp.cs` — fundament pod agentów
- Zapis: JSON w `Application.persistentDataPath` (docelowo)

---

## Słownik

| PL | EN (kod) |
|----|----------|
| Baza druida / Sady | DruidBase / Groves |
| Obóz goblinów | GoblinCamp |
| Stan: aktywny / oczyszczony / podbity | Active / Cleared / Captured |
| Totem | Totem |
| Drzewko Przyrody | NatureSkillTree |
| Miejsce mocy | PowerSite |
| Magiczny Ciemny Las | (setting — World scene) |

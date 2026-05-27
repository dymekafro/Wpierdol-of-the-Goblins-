# Przewodnik assetów i tekstur

Projekt **Wpierdol of the Goblins** na start **nie wymaga** płatnych tekstur. Poniżej: co zrobić bez assetów zewnętrznych, skąd brać darmowe paczki i co musi zrobić user vs agent.

---

## Co można zrobić bez zewnętrznych tekstur

### Unity URP Lit + kolory
- Materiały `Universal Render Pipeline/Lit` z samym `_BaseColor` (bez albedo texture).
- Przykłady w projekcie: `Assets/_Game/Materials/Goblin_Mat.mat`.
- **Druid:** ciemna zieleń `#2D4A2D`, brąz `#5C4033`, akcent `#8BC34A`.
- **Gobliny:** oliwkowa zieleń, szary, czerwony akcent (szaman), ciemny (sapper).
- **Las:** ziemia brązowa, trawa zielona, kamienie szare, woda ciemnoniebieska.

### Primitive shapes (MVP)
| Element | Shape | Uwagi |
|---------|-------|--------|
| Druid (placeholder) | Capsule + Cylinder (laska) | Skala ~1.8 wysokości |
| Goblin szturmowiec | Capsule (niższy, szerszy) | |
| Łucznik | Capsule + Cube (łuk) | |
| Totem | Cylinder + Cube | Wysoki, kontrastowy kolor |
| Drzewo | Cylinder + Sphere | Proste „drzewko” |
| Obóz | Plane + Cube (płoty) | |
| Skrzynia | Cube | Już może być prefab |

### UI bez tekstur
- **TextMeshPro** — font wbudowany (Liberation Sans).
- **Unity UI Image** — domyślny sprite UI (`UISprite`) + **Color tint** (menu lasu: ciemne tło, zielone/brązowe przyciski).
- Gradient tła: dwa nakładające się `Image` z alpha.

### Proceduralne / agent
- Prosty **noise** na terenie: Unity Terrain lub mesh z losową wysokością (skrypt editor/runtime).
- **Particle System** bez tekstur: domyślne material particles (kropki) — ogień, uzdrowienie, eksplozja saperów.
- **LineRenderer** — pułapki, aura szamana.

---

## Darmowe źródła (user pobiera ręcznie)

### Kenney.nl
- [Nature Pack](https://kenney.nl/assets/nature-kit) — drzewa, skały, trawa (low-poly, CC0).
- [Platformer Kit](https://kenney.nl/assets) — proste kształty, czasem przydatne jako props.
- **Licencja:** CC0 — bez atrybucji, idealne na MVP.

### OpenGameArt.org
- Szukaj: `goblin`, `druid`, `forest`, `low poly`.
- Sprawdź licencję per asset (CC0, CC-BY, GPL).
- Rekomendacja: modele **low-poly** pod URP mobile/PC.

### Unity Asset Store (Free)
- **Polygon Nature Pack** (często darmowe promocje).
- **RPG Character Mecanim Animation Pack** — tylko jeśli potrzebne animacje humanoid.
- **TextMesh Pro** — już w projekcie.
- Filtruj: Price = Free, Compatible with URP.

### Inne
- **Mixamo** — animacje humanoid (Adobe account, free tier).
- **Freesound.org** — SFX lasu, goblinów (user importuje `.wav`/`.ogg`).

---

## User vs Agent — podział pracy

| Zadanie | Kto | Uwagi |
|---------|-----|--------|
| Pobranie paczek Kenney/OGA | **User** | Agent nie pobiera dużych ZIP bez prośby |
| Import modeli do `Assets/_Game/Art/` | **User** lub **Agent** (po wrzuceniu plików) | |
| Materiały URP Lit, kolory | **Agent** | |
| Primitive placeholders | **Agent** | |
| Prefaby goblinów/druid z placeholder mesh | **Agent** | |
| Menu UI (kolory, TMP) | **Agent** | Bez tekstur tła |
| Animacje Mixamo | **User** | Wymaga konta, upload humanoid |
| Muzyka / SFX wysokiej jakości | **User** | Agent może podpiąć placeholder AudioClip |
| Ikony skill tree | **User** (później) | MVP: tekst lub kolorowe kwadraty |
| Skybox lasu | **Agent** (Procedural Skybox / gradient) lub **User** (HDRI) | |

---

## Rekomendacje tematyczne

### Druid
- Sylwetka: płaszcz z `Cylinder` lub później low-poly cloak z Kenney.
- Laska: `Cylinder` + `Sphere` na górze (kryształ — jasnozielony emission niski).
- VFX czarów: zielone/pomarańczowe particles (Życie/Ogień).

### Gobliny
- Mniejsza skala niż gracz (~0.7–0.85).
- Różnicuj **tylko kolorem materiału** na MVP (4 materiały: Stormtrooper, Archer, Shaman, Sapper).
- Szaman: fioletowy akcent; Saper: pomarańczowe „bomby” (Sphere).

### Las / biomy
| Biom | Dominujący kolor terenu | Akcent |
|------|---------------------------|--------|
| Pierścień Wschodzącego Słońca | Jasna zieleń, ciepły ambient | Żółte światło |
| Bagna Cienia | Ciemna zieleń, szarość | Mgła (URP Volume) |
| Kamienne Wąwozy | Szary, brąz | Ostre światło |
| Serce Hordy | Brąz, czerwone totemy | Czerwone point light |

---

## Struktura folderów (docelowa)

```
Assets/_Game/
  Art/
    Models/      ← user imports
    Textures/    ← opcjonalnie
    Audio/       ← user imports
  Materials/     ← agent: kolory URP Lit
  Prefabs/
  Scenes/
```

---

## Status na dziś (MVP faza 0)

| Asset | Status |
|-------|--------|
| Menu UI (kolory lasu) | Agent — bez tekstur tła |
| Goblin_Mat | Istnieje — rozszerzyć warianty |
| Druid model | **Brak** — capsule placeholder |
| Tekstury terenu | **Brak** — Plane + kolor |
| Animacje | **Brak** — T-pose / prosty ruch |
| Audio | **Brak** |

**Wniosek:** Gra jest grywalna wizualnie na placeholderach. User powinien pobrać **Kenney Nature Kit** gdy będzie chciał podnieść jakość środowiska bez custom art.

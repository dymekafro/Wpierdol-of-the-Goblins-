# Rozpoznanie: animacje goblinów

**Data:** 2026-05-30  
**Agent:** explore (readonly)  
**Status:** Gotowe do planowania naprawy

---

## 1. Obecny stan

Pipeline: `GoblinBase.Update()` → `CharacterAnimDriver.SetSpeed()` + `TriggerAttack()`  
Setup runtime: `GoblinAnimSetup.EnsureAnimator()` + `WireDriver()`  
Spawning: proceduralny `GameObject` w `WorldGenerator.BuildCamps()` (nie prefab z gotowym Animatorem)

| Warstwa | Plik | Rola |
|---------|------|------|
| AI + ruch | `GoblinBase.cs` | `transform.position`, `SetSpeed()` co klatkę |
| Sterownik | `CharacterAnimDriver.cs` | Mecanim **lub** proceduralna animacja kości |
| Setup | `GoblinAnimSetup.cs` | Dodaje Animator + Invector controller do Fantasy Goblin |

Prefaby `Assets/Goblin/Prefab/skin1/2/3.prefab`: rig + SkinnedMeshRenderer, **brak Animatora**, **brak clipów** w FBX.

Controller przypisywany runtime: `Invector@BasicLocomotion` (parametry: `InputMagnitude`, brak `Attack`/`Death`).

---

## 2. Root causes (priorytet)

| ID | Priorytet | Problem | Skutek |
|----|-----------|---------|--------|
| RC-1 | P0 | `GoblinAnimSetup` ładuje controller/avatar tylko `#if UNITY_EDITOR` | W buildzie brak animacji |
| RC-2 | P0 | `_hasAnimator = controller != null` bez walidacji avatara | T-pose / bind pose, procedural wyłączony |
| RC-3 | P1 | Prefaby bez bake'owanego Animatora | Cały setup zależy od runtime patch |
| RC-4 | P1 | `WorldAssetPlacer.TryLoadPrefabAtPath` editor-only | W buildzie fallback na placeholder |
| RC-5 | P2 | Invector BasicLocomotion bez triggerów Attack/Death | Atak proceduralny zablokowany gdy `_hasAnimator` |
| RC-6 | P2 | `SetGrounded()` nigdy nie wywoływane u goblinów | Potencjalnie zła lokomocja Invector |
| RC-7 | P2 | `cullingMode = CullUpdateTransforms` vs gracz `AlwaysAnimate` | Animacja może się zatrzymać poza ekranem |

---

## 3. Kluczowe pliki

| Plik | Linie | Działanie |
|------|-------|-----------|
| `GoblinAnimSetup.cs` | 90–129 | Runtime loading (Resources / GameAssetLoader) |
| `CharacterAnimDriver.cs` | 90–96, 188–191 | `_hasAnimator` wymaga valid avatar; fallback proceduralny |
| `GoblinBase.cs` | 74, 163 | `SetGrounded(true)` w Update |
| `WorldAssetPlacer.cs` | ~930–938 | Runtime load prefabów goblinów |
| `Goblin/Prefab/skin*.prefab` | — | Bake Animator + Avatar + Controller (opcjonalnie w Editorze) |

---

## 4. Scenariusz awarii (Editor)

1. `EnsureAnimator()` przypisuje controller ✅  
2. `FindAvatar()` zwraca null ❌  
3. `_hasAnimator = true` → procedural wyłączony  
4. Mecanim bez valid avatara → **goblin stoi w T-pose**

---

## 5. Różnice gracz vs goblin

| Aspekt | Gracz | Goblin |
|--------|-------|--------|
| Prefab | Gotowy Animator+Avatar+Controller | Pusty GO + runtime setup |
| Sterowanie anim | `vThirdPersonController.UpdateAnimator()` | Tylko `SetSpeed()` |
| Combat anim | `DruidBlink_Combat.controller` | Brak |
| Broń | W prefabie | `#if UNITY_EDITOR` w Stormtrooper |

---

## 6. Diagram przepływu

```
WorldGenerator → GoblinStormtrooper.Start()
  → BuildVisual() → TryAttachCharacterModel(skin1.prefab)
    → GoblinAnimSetup.EnsureAnimator() [EDITOR: load controller+avatar]
    → WireDriver() → CharacterAnimDriver
  → Update(): SetSpeed() → _hasAnimator? → Mecanim LUB Procedural
```

**Bug:** controller przypisany + avatar invalid → **żadna animacja**.

---

## Następny krok

→ `docs/plans/GOBLIN_ANIM_FIX_PLAN.md` — plan implementacji dla Opus.

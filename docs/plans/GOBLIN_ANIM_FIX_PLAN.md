# Plan naprawy animacji goblinów

**Dla:** Opus (implementacja)  
**Na podstawie:** `docs/recon/GOBLIN_ANIM_RECON.md`  
**Priorytet:** P0 — gobliny muszą chodzić i atakować w Editor Play Mode  
**Effort:** M (~45–60 min)

---

## Cel

Gobliny z modelem Fantasy Goblin (`skin1/2/3.prefab`) mają:
- widoczną animację idle/chodu podczas chase
- swing ręki przy ataku (proceduralny minimum)
- brak T-pose / bind pose w Play Mode (Editor)

Build standalone — nice-to-have w tym PR (Krok 2), ale przygotuj runtime loading.

---

## Strategia (minimalny diff)

**Opcja wybrana:** Naprawić logikę fallback + procedural always-on dla attack, bez tworzenia nowego Animator Controller w tym PR.

Powód: paczka Fantasy Goblin nie ma clipów. Proceduralna animacja kości (`WireDriver` mapuje `upperarm_l`, `thigh_l` itd.) **działa dobrze** — problem polega na tym, że `_hasAnimator=true` ją blokuje nawet gdy Mecanim nie animuje.

---

## Krok 1 — `CharacterAnimDriver.cs` (P0)

### 1a. Popraw `ResolveAnimator()` / `_hasAnimator`

```csharp
bool MecanimReady =>
    animator != null
    && animator.runtimeAnimatorController != null
    && animator.avatar != null
    && animator.avatar.isValid;
```

`_hasAnimator` = `MecanimReady` (nie tylko controller != null).

### 1b. Update — hybrid mode

Gdy `MecanimReady`:
- Mecanim obsługuje lokomocję (SetSpeed → InputMagnitude)
- **ProceduralUpdate nadal działa dla `_swingT` / `_castT`** (atak ręką)

Zmiana w `Update()`:
```csharp
if (_dead) { ResetPlaceholderPose(); return; }

// Zawsze obsłuż procedural attack/cast swing na kościach
if (_swingT >= 0f || _castT >= 0f)
    ProceduralUpdate(Time.deltaTime);  // tylko swing/cast, nie pełny chód

if (MecanimReady) return;  // chód/idle z Mecanim

ProceduralUpdate(Time.deltaTime);  // pełna procedural gdy brak Mecanim
```

Alternatywa prostsza: w `ProceduralUpdate` na początku — jeśli `_hasAnimator && walkSpeed==0 && _swingT<0 && _castT<0` → return (idle z Mecanim). Inaczej animuj kości.

### 1c. `TriggerAttack()` — zawsze ustaw `_swingT`

Po bloku Mecanim trigger, **zawsze** ustaw `_swingT = 0f` (nie return early bez swing).

---

## Krok 2 — `GoblinAnimSetup.cs` (P0)

### 2a. Runtime loading controllera

Dodać fallback poza `#if UNITY_EDITOR`:

```csharp
static RuntimeAnimatorController LoadLocomotionController()
{
    if (_cachedController != null) return _cachedController;
#if UNITY_EDITOR
    _cachedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
        GameAssetPaths.InvectorLocomotionController);
#endif
    if (_cachedController == null)
        _cachedController = Resources.Load<RuntimeAnimatorController>("Anim/InvectorBasicLocomotion");
    return _cachedController;
}
```

**Akcja:** Skopiować `Invector@BasicLocomotion.controller` do `Assets/_Game/Resources/Anim/InvectorBasicLocomotion.controller` (lub użyć istniejącego `GameAssetLoader` jeśli ma runtime path).

### 2b. Runtime avatar

W `FindAvatar()` — fallback:
```csharp
var avatar = Resources.Load<Avatar>("Anim/GoblinHumanoidAvatar");
if (avatar != null) return avatar;
```

**Akcja:** Wyeksportować avatar z `skin1.fbx` do Resources (lub użyć `GameAssetLoader.LoadAllAssetsAtPath` runtime).

### 2c. `cullingMode = AlwaysAnimate`

Linia 40: zmienić z `CullUpdateTransforms` na `AlwaysAnimate`.

### 2d. Animator na węźle szkieletu

W `EnsureAnimator()` — jeśli `modelRoot` nie ma bone children, szukaj child `root` i dodaj Animator tam:
```csharp
var skeletonRoot = FindDeepChild(modelRoot, "root") ?? modelRoot;
// Add/Get Animator on skeletonRoot
```

---

## Krok 3 — `GoblinBase.cs` (P1)

W `Update()`, po `SetSpeed`:
```csharp
if (AnimDriver != null) AnimDriver.SetGrounded(true);
```

---

## Krok 4 — Diagnostyka (P1)

W `GoblinAnimSetup.EnsureAnimator()` — po setupie:
```csharp
if (animator.runtimeAnimatorController == null)
    Debug.LogWarning($"[GoblinAnimSetup] Brak controllera na {modelRoot.name} — procedural fallback");
if (animator.avatar == null || !animator.avatar.isValid)
    Debug.LogWarning($"[GoblinAnimSetup] Invalid avatar na {modelRoot.name} — procedural locomotion");
```

W `GoblinBase.BuildVisual()` — log która ścieżka: model 3D / placeholder.

---

## Krok 5 — Test regresji

| Scenariusz | Oczekiwany wynik |
|------------|------------------|
| Play Mode, goblin idle w obozie | Delikatny bob (procedural) LUB idle clip Invector |
| Play Mode, goblin chase | Chód (Mecanim InputMagnitude LUB procedural swing nóg) |
| Play Mode, goblin atakuje | Swing prawej ręki (procedural `_swingT`) |
| Console | Brak spam warnings; ewentualnie 1× info per goblin przy spawn |
| Gracz | Animacje gracza **bez regresji** |

---

## Czego NIE robić w tym PR

- Nie tworzyć nowego `GoblinLocomotion.controller` z clipami Mixamo (osobna faza)
- Nie refactorować `WorldGenerator` spawning na prefaby
- Nie dotykać `PlayerCombat` / gracza poza smoke test
- Nie bake'ować prefabów goblinów w Unity Editorze (wymaga ręcznej pracy w edytorze — opcjonalny follow-up)

---

## Pliki do edycji (checklist)

- [ ] `Assets/_Game/Scripts/Character/CharacterAnimDriver.cs`
- [ ] `Assets/_Game/Scripts/Enemies/GoblinAnimSetup.cs`
- [ ] `Assets/_Game/Scripts/Enemies/GoblinBase.cs`
- [ ] `Assets/_Game/Resources/Anim/` — nowy folder + skopiowany controller (jeśli brak runtime path)
- [ ] (opcjonalnie) `Assets/_Game/Scripts/Core/GameAssetPaths.cs` — ścieżka Resources

---

## Kryterium DONE

✅ W Play Mode gobliny w pierwszym obozie **ruszają nogami** podczas chase i **machają ręką** przy ataku.  
✅ Brak T-pose na modelu 3D Fantasy Goblin.  
✅ Kompilacja 0 errors.

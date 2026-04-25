# Sands of the Unseen - Codex Project Rules

These rules apply to every Codex task in this repository. Follow them exactly unless the user explicitly overrides one.

---

## Game Context

Top-down horde-survival roguelike built in Unity 6 with C#.
Design document: `docs/phase1/proposal.md`
One arena, one gun, wave-based enemies, coin-driven upgrade selection.
Read the proposal before making architectural or gameplay decisions.

---

## Unity Inspector Rules

Apply these to every script you create or modify.

### 1. Strict Encapsulation
- All fields that exist purely to be visible in the Inspector must be `[SerializeField] private`.
- Never use `public` on a field just to expose it in the Inspector.
- Expose a value to other scripts only via a public read-only property, and only if another script explicitly needs to read it.

### 2. Naming Convention
- Private fields use `camelCase`.
- Do not use `_camelCase` or `m_camelCase`.
- Public properties and methods use `PascalCase`.
- Do not mix naming styles within a file.

### 3. Designer Safety
- Add `[Range(min, max)]` to any float or int that has a known valid range.
- Add `[Min(value)]` to values that must not go below that value.
- Choose bounds that match gameplay design, not arbitrary broad limits.

### 4. Clarity
- Every `[SerializeField]` field must have a `[Tooltip("...")]` that states its purpose and unit when applicable.
- Group related fields with `[Header("...")]`.
- Use `[Space]` between logical groups.
- The Inspector should be understandable to a designer without opening the script.

### 5. State vs. Config
- Only configuration belongs in the Inspector.
- Runtime state and cached component references must be plain private fields with no `[SerializeField]`.
- Do not serialize cached `GetComponent` results, counters, timers, flags, pool state, or anything that resets on play.

### 6. Proactive Fixes
- Fix obvious syntax errors, unused variables, or misnamed fields while applying these rules.
- Preserve existing behavior unless the current task asks for a gameplay, architecture, or design-pattern change.

### 7. Clarification Protocol
- If a field's purpose, bounds, or serialization status is unclear, make the safest reasonable guess in code.
- Put a "Questions for User" section at the top of the final response listing those specific items.

---

## Code Quality Rules

### Events and Delegates
- UI and cross-system communication should use C# events (`event Action<...>`) where an event already fits the design.
- Always unsubscribe in `OnDestroy` or `OnDisable`, matching the subscribe location.
- Do not poll another component's state when an appropriate event already exists.

### Object Pooling
- Objects spawned repeatedly at runtime, such as bullets, currency orbs, and damage popups, should use pooling.
- Prefer Unity's built-in `UnityEngine.Pool.ObjectPool<T>` for new pooling work.
- Pre-warm pools in `Awake` or `Start` when initial runtime allocation would cause a spike.

### Coroutines
- Use coroutines for timed sequences, wave loops, and delayed actions.
- Never busy-wait with a loop that has no yield.
- Use `WaitForSeconds` for fixed scaled delays.
- Use `Time.unscaledTime` or `Time.unscaledDeltaTime` when `Time.timeScale` may be zero.

### Performance
- Do not call `Camera.main` inside `Update`, `FixedUpdate`, or `LateUpdate`; cache it when practical.
- Do not call `GameObject.Find` or `FindObjectOfType` inside per-frame methods.
- `FindGameObjectWithTag` is acceptable in `Start` or `Awake` as a fallback only when an Inspector reference is not set.

### Single Responsibility
- Each MonoBehaviour should do one clear job.
- If a class appears to handle multiple concerns, note it and ask before merging responsibilities.

### Separation of Concerns
- Game rules belong in manager or system classes.
- `PlayerHealth` must not call `SceneManager` directly; it should fire an event and let `GameManager` handle transitions.
- UI scripts read data through events or properties and should not write game state unless explicitly designed as an input action.

### Upgrade System
- New upgrade type means a new `UpgradeDefinition` ScriptableObject subclass.
- Do not modify `UpgradeManager`, `UpgradeMenuUI`, or `UpgradeCardUI` just to add a new upgrade type.

---

## What Not To Do

- Do not add features beyond what the task requires.
- Do not add error handling for scenarios that cannot happen.
- Do not write comments that merely restate what the code does; add comments only when the reason is non-obvious.
- Do not create new files unless the task requires a new class, asset, or project instruction file.
- Do not use public fields as a shortcut. Use `[SerializeField] private` plus a property when external read access is needed.

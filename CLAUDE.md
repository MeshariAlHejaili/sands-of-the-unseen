# Sands of the Unseen — Claude Code Project Rules

These rules apply to every task in this project. Follow them exactly unless the user explicitly overrides one.

---

## Game Context

Top-down horde-survival roguelike built in Unity 6 (C#).  
Design document: `docs/phase1/proposal.md`  
One arena, one gun, wave-based enemies, coin-driven upgrade selection.  
Refer to the proposal before making any architectural or gameplay decisions.

---

## Unity Inspector Rules

Apply these to every script you create or modify. No exceptions.

### 1. Strict Encapsulation
- All fields that exist purely to be visible in the Inspector must be `[SerializeField] private`.
- Never use `public` on a field just to expose it in the Inspector.
- Expose a value to other scripts only via a `public` read-only property (`public float Foo => foo;`), and only if another script explicitly needs to read it.

### 2. Naming Convention
- Private fields: `camelCase` — no underscores, no `m_` prefix.
- Public properties and methods: `PascalCase`.
- Do not mix styles within a file.

### 3. Designer Safety
- Add `[Range(min, max)]` to any float/int that has a known valid range (e.g. speeds, damage, rates, durations).
- Add `[Min(0)]` to any value that must never be negative.
- Choose bounds that match the gameplay design, not just "any positive number".

### 4. Clarity — Tooltips, Headers, Space
- Every `[SerializeField]` field must have a `[Tooltip("...")]` that states its purpose and unit (e.g. `"Movement speed in units per second"`).
- Group related fields with `[Header("...")]`.
- Use `[Space]` between groups for visual breathing room in the Inspector.
- The Inspector should read like documentation — a designer must understand every field without opening the script.

### 5. State vs. Config
- Only configuration belongs in the Inspector. Runtime state and cached component references must be plain `private` fields with no `[SerializeField]`.
- Never serialize: cached `GetComponent` results, counters, timers, flags, or anything that resets on play.

### 6. Proactive Fixes
- Fix obvious syntax errors, unused variables, or misnamed fields while applying the above rules.
- Preserve existing behavior unless the current task asks for a gameplay, architecture, or design-pattern change.

### 7. Clarification Protocol
- If you cannot determine a field's intended purpose, a safe `[Range]`, or whether it should be serialized at all: make the safest reasonable guess in the code and flag it clearly in your response so the user can confirm.

---

## Code Quality Rules

### Events & Delegates
- UI and cross-system communication must use C# events (`event Action<...>`).
- Always unsubscribe in `OnDestroy` or `OnDisable` (match the subscribe location).
- Never poll another component's state when an event already exists for it.

### Object Pooling
- Any object that spawns repeatedly at runtime (bullets, orbs, popups) must use a pool.
- Use Unity's built-in `UnityEngine.Pool.ObjectPool<T>` API. Do not write custom `Queue`/`List` based pools from scratch.
- Pre-warm pools in `Awake`/`Start` if the initial instantiation would cause a lag spike.

### Coroutines
- Use coroutines for: timed sequences, wave loops, delayed actions.
- Never busy-wait with `while(true) { }` and no yield.
- Use `WaitForSeconds` for fixed delays; `yield return null` only for per-frame checks.
- When `Time.timeScale` may be zero, use `Time.unscaledTime` / `Time.unscaledDeltaTime`.

### Performance
- Never call `Camera.main` inside `Update`, `FixedUpdate`, or `LateUpdate` — cache it in `Awake`.
- Never call `GameObject.Find` or `FindObjectOfType` inside any per-frame method.
- `FindGameObjectWithTag` is acceptable in `Start`/`Awake` as a fallback only when an Inspector reference is not set.

### Single Responsibility
- Each MonoBehaviour does one job. If a class handles more than one concern, note it and ask before merging responsibilities.

### Separation of Concerns
- Game rules belong in manager/system classes.
- `PlayerHealth` must not call `SceneManager` directly — fire an event, let a GameManager handle transitions.
- UI scripts only read data via events or properties; they never write back to game state.

### SOLID — Upgrade System
- New upgrade type = new `UpgradeDefinition` ScriptableObject subclass only. No changes to `UpgradeManager`, `UpgradeMenuUI`, or `UpgradeCardUI`.

---

## Odin Inspector Usage

This project has Odin Inspector installed in **Editor Only Mode**. Odin serialization is stripped from builds — only Inspector attributes and the Validator window are available at runtime/build.

### Required Workflow
- Run **Tools → Odin Inspector → Validator → Open Validator → Scan Project** before opening any PR. Fix or document every error before requesting review.

### Permitted Attributes (opportunistic, not as a sweep refactor)
- `[Required]` on `[SerializeField]` reference fields that must not be null at runtime.
- `[SceneObjectsOnly]` / `[AssetsOnly]` to constrain what can be dragged into reference fields.
- `[ShowInInspector, ReadOnly]` on a private property to expose runtime state for debugging without serializing it or breaking encapsulation.
- `[Button("Label")]` on a private method for editor-only debug actions (e.g. "Spawn Test Boss").

### Forbidden
- Do not inherit from `SerializedMonoBehaviour` or `SerializedScriptableObject`. Editor Only Mode removes this serialization from builds.
- Do not import packages from `Assets/Plugins/Sirenix/Demos/`. They rely on Odin serialization and will break the build.
- Do not replace existing `[Header]` / `[Range]` / `[Tooltip]` / `[Min]` / `[Space]` with Odin equivalents like `[BoxGroup]`, `[FoldoutGroup]`, `[TabGroup]`. Keep existing Inspector hygiene unchanged for this phase.

---

## What NOT to Do
- Do not add features beyond what the task requires.
- Do not add error handling for scenarios that cannot happen.
- Do not write comments that explain what the code does — only write comments when the *why* is non-obvious.
- Do not create new files unless the task explicitly requires a new class.
- Do not use `public` fields as a shortcut. Always use `[SerializeField] private` + property.

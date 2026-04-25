# Phase 2 — Systems Vertical Slice (Advanced Mechanics Checkpoint)

## Project: Survival Arena Vertical Slice

---

## 1. Playable Slice Summary

### Start Boundary (Current Implementation)

- Scene loads into a contained arena.
- Player spawns with full health and stamina.
- Game begins in a **Main Menu** state.
- Player movement, aiming, shooting, and enemy spawning are disabled until the player presses **Start Run**.
- Main menu music plays during the menu state and stops when gameplay begins.
- The survival timer is initialized at **05:00**.

### Core Challenge Loop (Playable)

**Start Run → Move → Aim → Shoot → Damage → Kill → Collect Coins → Upgrade → Resume → Survive scaling pressure → Repeat**

- Player moves with WASD and aims using the mouse.
- Player shoots projectile bullets with fire-rate control.
- Enemies spawn from fixed spawn points after gameplay begins.
- Enemies chase the player and deal contact/range-based damage on cooldown.
- Enemies drop currency when killed.
- Currency collection feeds the upgrade loop.
- Upgrade selection pauses gameplay, presents three upgrade cards, then resumes play after selection.
- The timer counts down during gameplay only.

### End Boundary / Win–Fail Logic

- **Win:** Survive until the 5-minute timer reaches 0 → gameplay pauses → Victory screen appears.
- **Lose:** Player HP reaches 0 → death event fires → gameplay pauses → Defeat screen appears.
- Victory and Defeat screens include restart buttons that reload the scene.

### Known Limitations

- Victory and Defeat screens are implemented but still need final visual polish.
- Main menu exists, but button styling and transitions can be improved.
- Enemy AI is functional but still simple; explicit FSM polish is planned.
- Audio is partially integrated: menu music, shooting SFX, and dash SFX are present, but more combat/UI sounds are needed.
- Dash movement still needs additional collision validation near arena edges.
- Scene visuals are still graybox and require the Phase 3 environment/art pass.

---

## 2. Systems Implementation Status

### Controls & Interaction Model

- WASD movement.
- Mouse-based aiming.
- Projectile shooting with fire-rate control.
- Stamina-based dash.
- Stamina drain and regeneration.
- Player HP system with event-driven death.
- Gameplay input is gated by session state, so the player cannot move or shoot in menu/end states.

**Status:** Implemented and connected.

---

### Physics & Collision Systems

- Projectile bullets use collision/range-based damage handling.
- Currency pickup uses trigger/radius interaction.
- Enemy attacks use range checks and cooldown timing.
- Arena containment uses boundary logic.
- Gameplay systems are paused/disabled during non-gameplay states.

**Defensibility:** Interactions are deterministic enough for the current graybox slice and are testable through repeatable start, combat, upgrade, victory, and defeat flows.

**Next:** Validate dash collision more deeply using sweep checks or a CharacterController-style movement approach.

---

### AI / Combat / Challenge Logic

**Current Behavior:**

Spawn → Chase → Attack → Dead → Drop Currency

- Enemies spawn from fixed points.
- Enemies chase the player.
- Enemies attack when close enough using a cooldown.
- Enemies die when health reaches zero.
- Currency drops support the upgrade economy.

**Status:** Functional for Phase 2. Phase 3 will formalize this into a clearer explicit FSM structure.

---

### Game-State Flow

Current flow:

**Main Menu → Playing → Upgrade Pause → Playing → Victory / Defeat → Restart**

Implemented states and behavior:

- **Main Menu:** UI visible, menu music plays, player/enemy gameplay systems disabled.
- **Playing:** HUD visible, timer runs, player and enemy systems active.
- **Upgrade Pause:** time scale pauses while the player selects an upgrade.
- **Victory:** timer reaches zero, gameplay pauses, Victory screen appears.
- **Defeat:** player dies, gameplay pauses, Defeat screen appears.
- **Restart:** restart buttons reload the current scene.

**Status:** Implemented and testable.

---

## 3. UI + Audio First Pass

### UI (Implemented)

- Main Menu screen.
- Start Run button.
- HUD grouping for gameplay UI.
- Survival timer display.
- Player health bar.
- Player stamina bar.
- Currency counter.
- Enemy health bars.
- Damage popups.
- Upgrade selection UI with three cards.
- Victory screen.
- Defeat screen.
- Restart buttons for end screens.

### UI Writing / Tone

Current menu and defeat text use a short, action-focused tone:

- Main Menu: **“Survive. Upgrade. Don’t hesitate.”**
- Defeat: **“You hesitated. You lost.”**

This keeps the text punchy and game-like while avoiding directly copying an external quote.

### UI Still Planned

- Better title layout and typography.
- Button hover/click polish.
- Screen fade transitions.
- Stronger timer readability.
- Low-health warning feedback.
- Victory/Defeat screen visual polish.

---

### Audio (First Pass)

Implemented:

- Menu music controller.
- Menu music plays only during Main Menu state.
- Menu music stops when gameplay starts.
- Shooting SFX.
- Dash SFX.

Planned:

- Gameplay background music loop.
- Enemy hit SFX.
- Enemy death SFX.
- Currency pickup SFX.
- Upgrade selection SFX.
- UI button hover/click SFX.
- AudioMixer with Music, SFX, and UI groups.

---

## 4. Playtest Report

### Test Setup

Testing was performed inside the Unity Editor using the current graybox scene.

Main scenarios tested:

- Main menu starts before gameplay.
- Player cannot move/shoot before pressing Start Run.
- Enemy spawner does not spawn during Main Menu.
- Start Run begins gameplay.
- Timer starts counting down only during gameplay.
- Enemies spawn and chase after gameplay begins.
- Player can shoot and kill enemies.
- Currency drops and can be collected.
- Upgrade UI pauses gameplay and resumes after selection.
- Player death opens Defeat screen.
- Timer expiration opens Victory screen.
- Restart buttons reload the scene.

### Findings

| Priority | Issue | Observation | Planned Fix |
|---|---|---|---|
| High | Enemy/gameplay systems must stay disabled in menu | Player was initially locked but enemy spawner still ran | Added gameplay gating for enemy spawner |
| High | End screens need polish | Victory/Defeat screens work but are visually simple | Improve layout, colors, typography, transitions |
| Medium | Timer visibility | Timer originally appeared on menu because it was outside HUD group | Move timer under HUD screen and route through screen state |
| Medium | Audio gaps | Menu music exists, but gameplay music and UI sounds are missing | Add event-driven audio controller and mixer |
| Medium | AI simplicity | Enemy behavior works but is not yet clearly formalized | Convert to explicit FSM states |
| Low | UI wording | Some wording felt awkward or copied | Use original short game-style text |

### Fixes Applied

- Added session-based gameplay gating.
- Disabled player control before gameplay starts.
- Disabled enemy spawning before gameplay starts.
- Added Main Menu UI.
- Added Start Run flow.
- Added survival timer display.
- Added Victory and Defeat screen routing.
- Added restart flow from end screens.
- Added menu music behavior tied to game state.

### Next Test Actions

- Run multiple complete 5-minute survival tests.
- Validate Victory screen after full 300-second timer.
- Validate Defeat screen after different enemy pressure levels.
- Test restart stability after both win and loss.
- Tune menu/audio/UI transitions.
- Test dash near arena boundaries.

---

## 5. Production Update

### Completed (Phase 2)

- Player movement, dash, stamina.
- Mouse aiming and shooting.
- Bullet/projectile damage loop.
- Enemy health and chase/contact behavior.
- Enemy spawning from fixed points.
- Currency drops and pickup.
- Upgrade selection with three choices.
- Health/stamina/currency HUD.
- Damage popups and enemy health bars.
- Main Menu screen.
- Start Run button.
- Survival timer.
- Victory condition.
- Defeat condition.
- Victory screen.
- Defeat screen.
- Restart buttons.
- Menu music first pass.
- Shooting and dash SFX first pass.

### In Progress

- Final menu styling.
- Victory/Defeat screen polish.
- Gameplay music and additional SFX.
- Explicit enemy FSM cleanup.
- Desert arena art pass.
- Lighting and readability pass.

### Carry-Over to Phase 3

- Desert arena environment and map dressing.
- Materials, lighting, post-processing, and VFX.
- AudioMixer and full gameplay audio pass.
- More polished victory/defeat/menu transitions.
- Dash collision robustness.
- Performance profiling evidence.
- Final balancing for the 5-minute survival curve.

### GitHub Workflow

- Continue using feature branches for UI, audio, art, AI, and gameplay polish.
- Track tasks using GitHub Issues and Project board columns.
- Open PRs for completed features before merging.
- Document known bugs and Phase 3 carry-over tasks.

### Phase 3 Readiness

- The project now has a complete playable structure: menu, start, gameplay loop, upgrade pause, victory, defeat, and restart.
- Core systems are integrated and testable.
- Phase 3 can focus on polish, art, audio, balancing, and stability.

---

## 6. Competency Mapping

### Core Gameplay Systems Engineering

Player controls, shooting, stamina/dash, upgrade flow, session state, win state, fail state, and restart flow are implemented.

### Physics & Collision Systems

Projectile damage, pickup interaction, enemy attack range, and arena containment are implemented. Dash collision robustness remains a Phase 3 validation item.

### AI Behavior Design

Enemy chase and attack behavior is functional. Phase 3 will formalize the current behavior into an explicit FSM for clearer state ownership and debugging.

### Real-Time Graphics Pipeline

The current scene is readable as a graybox top-down arena. Phase 3 will add desert materials, lighting, shadows, color grading, and improved scene readability.

### Technical Art & Polish

Current feedback includes damage popups, health bars, stamina/currency UI, and upgrade cards. Planned polish includes improved menus, VFX, screen transitions, camera polish, and environment dressing.

### Audio Systems Design

Menu music and initial SFX are integrated. Phase 3 will expand this into gameplay music, pickup sounds, combat sounds, UI sounds, and mixer balancing.

### UI/UX Feedback

The project now includes menu, HUD, timer, upgrade UI, victory, and defeat screens. Next steps are readability, animation, and usability polish.

### Performance Optimization and Debugging

Pooling and simple event-driven systems reduce runtime overhead. Phase 3 will include profiling evidence, spawn stress tests, and optimization notes.

### Production and Collaboration Practices

Work is tracked through GitHub workflow, issues, project board updates, and milestone planning. Phase 3 tasks are clearly identified.

---

## 7. Conclusion

The Phase 2 vertical slice now supports a clear playable flow from menu to gameplay to win or loss. The current implementation includes the core survival loop, upgrade loop, timer-based victory, death-based defeat, UI routing, restart flow, and first-pass audio. The remaining work is primarily polish: visual identity, desert arena art, richer audio, AI FSM cleanup, collision robustness, performance evidence, and final balancing.

# Phase 3 Sprint Plan — Sands of the Unseen
**Deadline:** May 5, 2026 | **Team:** 2AM Games (4 people) | **Branch model:** trunk = `Development`, feature branches per commit

---

## What We Are Shipping

A polished vertical slice of a top-down horde-survival roguelike. Player survives a 5-minute desert arena holdout against melee + ranged enemies, then a stationary boss spawns at the center, throws projectiles, summons melee children, and on death opens a portal the player walks into to win.

Final loop:
```
Main Menu
  -> Narrative intro fades in: "In 5 minutes, their Warlord enters the arena."
  -> 3-2-1-GO countdown
  -> Wave survival: melee + ranged enemies, 5-minute timer
  -> Timer hits 0 -> regular spawning stops, Warlord spawns at arena center
  -> Boss alternates: projectile volleys / summon melee children
  -> Boss HP -> 0 -> portal opens at boss location
  -> Player walks into portal -> Victory screen
```

---

## What Changed from Phase 2

| Area | Phase 2 | Phase 3 |
|------|--------|---------|
| Scene | `SampleScene.unity` graybox arena, in build | `Arena.unity` (production) + `SampleScene.unity` (test sandbox, NOT in build) |
| Win condition | Timer hits 0 -> Victory | Timer hits 0 -> Boss phase -> Kill boss -> Portal -> Victory |
| Enemy types | One melee chaser | Melee chaser (rebuilt with model) + ranged thrower (new) + boss (new) |
| State authority | Split: `GameManager` reloads on death AND `GameSessionController` shows Defeat | Single: `GameSessionController` only. `GameManager` retired. |
| State machine | 4 states (MainMenu / Playing / Victory / Defeat) | 7 states (+ Countdown / UpgradeSelection / BossPhase) |
| Bullet damage | Hardcoded to `EnemyBoxAgent` | Targets `IDamageable` (works for any enemy + boss) |
| Pause control | `UpgradeManager` writes `Time.timeScale` directly | Pause routed through session state |
| Player events | `HealthChanged`, `Died` only | + `Damaged(amount, current, max)` event for shake/SFX/vignette |
| Shoot VFX | Polls `Input.GetMouseButton(0)` | Subscribes to `PlayerShooting.ShotFired` |
| Audio | Menu music + few SFX | Full AudioMixer (Music/SFX/UI), 3D spatial enemy SFX, state-aware music |
| Upgrades | 3 types | 7 types with icons |
| UI | Functional | Narrative intro, countdown, boss HP bar, screen fades, polished end screens |
| Layers | Only `Enemy` | `Player`, `Enemy`, `EnemyProjectile`, `PlayerProjectile`, `World`, `Pickup`, `Hitbox` + locked physics matrix |
| Performance | Not profiled | Unity Profiler evidence, documented bottlenecks, stable 60 FPS |
| GitHub | Issues + branches | Project board with milestones, final release tag, public Issues |

---

## Two-Scene Policy

There are two scenes from now on. This is the most important workflow rule in the project.

| Scene | Role | Who edits | In build? |
|---|---|---|---|
| `Assets/_Project/Scenes/SampleScene.unity` | **Test sandbox** — colleagues drop their prefabs into it for fast personal iteration | Anyone, locally | No |
| `Assets/_Project/Scenes/Arena.unity` | **Production scene** — what ships, what the build loads | Only P2 commits changes | Yes (only entry) |

Rules:
- **Anyone may edit `SampleScene.unity` locally for testing**, but DO NOT commit those changes unless the lead approves.
- **Only P2 commits to `Arena.unity`.** If you need something added there, hand P2 a prefab on Discord.
- If you accidentally stage SampleScene: `git restore Assets/_Project/Scenes/SampleScene.unity`.

---

## Issue #0 — Foundation Contracts (Lead, Solo, May 2 Morning)

These commits MUST land on `Development` before any other M1 commit starts. Each is a separate small PR (or batch within one branch). All 20 are sequential — code only, no team conflict.

### Code commits (pure script work)

| # | Commit message | Files |
|---|---|---|
| F-01 | `chore: retire legacy GameManager script` | Delete `GameManager.cs` |
| F-02 | `fix(ui): rename MenuButtonGameHover class to match filename` | `MenuButtonHover.cs` |
| F-04 | `feat(state): add BossPhase + Countdown + UpgradeSelection states` | `GameSessionState.cs` |
| F-05 | `feat(state): session-owned transitions and timeScale control` | `GameSessionController.cs` (add `EnterCountdown`, `EnterPlaying`, `EnterBossPhase`, `EnterUpgradeSelection`, `ExitUpgradeSelection`, `TriggerVictory`, `TriggerDefeat` — each owns its own timeScale) |
| F-06 | `fix(state): GameplayBehaviourGate enables player during BossPhase` | `GameplayBehaviourGate.cs` (one-line change in `HandleStateChanged`) |
| F-07 | `fix(state): GameScreenRouter routes Countdown + BossPhase + UpgradeSelection` | `GameScreenRouter.cs` (add SerializeFields for new screens) |
| F-08 | `feat(damage): IDamageable interface + EnemyHealth implements it` | New `IDamageable.cs`, edit `EnemyHealth.cs` |
| F-09 | `feat(damage): DamageableHitbox child component` | New `DamageableHitbox.cs` |
| F-10 | `refactor(combat): bullets target IDamageable, not EnemyBoxAgent` | `BulletDamageDealer.cs` |
| F-11 | `refactor(upgrade): UpgradeManager routes pause through session` | `UpgradeManager.cs` (replace timeScale writes with `session.EnterUpgradeSelection() / ExitUpgradeSelection()`) |
| F-12 | `feat(player): PlayerHealth.Damaged event + IncreaseMaxHealth + ActivateShield` | `PlayerHealth.cs` |
| F-13 | `feat(player): PlayerShooting.ShotFired + bulletsPerShot + spread` | `PlayerStats.cs`, `PlayerShooting.cs` |
| F-14 | `refactor(player-vfx): MuzzleFlash subscribes to ShotFired event` | `PlayerShootingVFXObserver.cs` |
| F-15 | `feat(enemy): IEnemyBehaviour interface + EnemyStatsContext struct` | New `IEnemyBehaviour.cs`, `EnemyStatsContext.cs`, edit `EnemyAI.cs` |
| F-16 | `refactor(enemy): EnemyBoxAgent uses IEnemyBehaviour polymorphically` | `EnemyBoxAgent.cs` |
| F-17 | `feat(spawner): dual-prefab support + StopSpawning + SpawnChildWave` | `EnemyWaveSpawner.cs` (add `rangedEnemyPrefab` + `rangedSpawnChance`, two pools, `StopSpawning()`, `ClearActiveEnemies()`, `SpawnChildWave(int count, Vector3 center, float radius)`, locked-file header) |
| F-18 | `chore: stub empty classes for downstream feature PRs` | New: `BossAI.cs`, `BossPhaseController.cs`, `PortalController.cs`, `BossHealthBarUI.cs`, `RangedEnemyAI.cs`, `EnemyProjectile.cs`, `NarrativeTextUI.cs`, `CountdownUI.cs`, `CameraShaker.cs`, `LowHPVignette.cs`, `ScreenFadeController.cs`, `MusicController.cs` |

All modified scripts must follow the CLAUDE.md Inspector rules: every `[SerializeField]` gets a `[Tooltip]` stating purpose and unit, numeric fields get `[Range]` or `[Min]`, related fields grouped with `[Header]`.

---

### Unity Editor work (cannot be done in IDE — needs Unity open)

These cannot be done from VS Code. They require opening the Unity Editor and clicking through Inspectors, scene hierarchy, or Project Settings windows. They commit changes to `.unity` and `.asset` files.

| # | Editor commit | What you actually do in Unity |
|---|---|---|
| F-03 | `chore(layers): define physics layer convention` | **Edit -> Project Settings -> Tags and Layers**. Add user layers: `Player`, `EnemyProjectile`, `PlayerProjectile`, `World`, `Pickup`, `Hitbox`. Add tags: `Boss`, `Portal` (also add to `GameTags.cs` constants). Then **Edit -> Project Settings -> Physics**: untick all collisions in the matrix, then re-tick only the pairs we need (Player x Enemy, Player x EnemyProjectile, Player x Pickup, Player x World, Enemy x World, Enemy x PlayerProjectile, Enemy x Hitbox, EnemyProjectile x World, PlayerProjectile x World). Commits `ProjectSettings/TagManager.asset` and `ProjectSettings/DynamicsManager.asset`. |
| F-01b | `chore(scene): retire GameManager component, rename Managers GameObject` | Open `SampleScene.unity`. Find the `GameManager` GameObject (also holds SurvivalTimer + GameSessionController + GameplayBehaviourGate). **Remove the `GameManager` MonoBehaviour component only** — keep the GameObject. Rename it from "GameManager" to "Managers". Save scene. (Pairs with F-01 script delete.) |
| F-07b | `chore(scene): wire new screen references on GameScreenRouter` | After F-07 code lands, open `SampleScene.unity`, select the GameScreenRouter object, drag the new SerializeField references (countdown screen, boss overlay) — leave them empty for now if the prefabs don't exist yet, the script handles null. |
| F-11b | `chore(scene): wire session reference on UpgradeManager` | After F-11 code lands, open `SampleScene.unity`, select UpgradeManager, drag GameSessionController into its new `session` field. |
| F-17b | `chore(scene): leave ranged enemy prefab field empty on spawner for now` | Verify the new `rangedEnemyPrefab` field exists in Inspector. Leave it null — P4 fills it later. Confirm `rangedSpawnChance` defaults to 0. |
| F-19 | `chore(scene): SampleScene perimeter + spawn anchors + collider layers` | Open `SampleScene.unity`. Create empty parent `_World` for organization. Add 4 invisible BoxColliders at arena perimeter (no Renderer, layer = `World`, very tall to catch jumps). Verify enemy spawn point empty GameObjects exist on the EnemyWaveSpawner reference list (6 around the perimeter). Add 1 boss spawn anchor at center named `Boss_SpawnPoint`. Add 1 portal anchor named `Portal_SpawnPoint` slightly offset from boss. Set existing player + enemy GameObject layers to the new convention. Save scene. |
| F-20 | `chore: smoke-test foundation` | Press Play in Editor with SampleScene loaded. Verify: timer counts down, expires -> console logs "BossPhase entered", player still moves and shoots after timer expiry, no scene auto-reload on player death, Defeat screen actually shows. Document any failures as new issues. |

---

### After Issue #0 lands, these files become LOCKED

Only the lead may edit them. Anyone needing a change opens an Issue tagged `needs-lead`.

```
GameSessionState.cs
GameSessionController.cs
GameplayBehaviourGate.cs
GameScreenRouter.cs
EnemyBoxAgent.cs
EnemyWaveSpawner.cs
BulletDamageDealer.cs
IDamageable.cs
IEnemyBehaviour.cs
EnemyStatsContext.cs
PlayerHealth.cs
PlayerShooting.cs
PlayerStats.cs
UpgradeManager.cs
ProjectSettings/TagManager.asset
ProjectSettings/DynamicsManager.asset
ProjectSettings/EditorBuildSettings.asset
```

---

## Track Commits — Run in Parallel After Foundation

### P1 — Boss System (May 2 afternoon -> May 3)

| # | Commit message | Files |
|---|---|---|
| B-01 | `chore(assets): import boss model + animations` | `Assets/_Project/Models/Boss/...` |
| B-02 | `feat(boss): BossAI FSM (Idle / ProjectileVolley / SummonChildren / Dead)` | `BossAI.cs` |
| B-03 | `feat(boss): boss projectile prefab variant of EnemyProjectile` | New prefab `Boss_Projectile.prefab` |
| B-04 | `feat(boss): Boss_Warlord prefab (model + EnemyHealth + BossAI + DamageableHitbox + collider on Enemy layer)` | New prefab `Boss_Warlord.prefab` |
| B-05 | `feat(boss): BossPhaseController stops waves and spawns boss on phase enter` | `BossPhaseController.cs` |
| B-06 | `feat(portal): PortalController activates on boss death, fires victory on player trigger` | `PortalController.cs`, new `Portal.prefab` |
| B-07 | `feat(ui): BossHealthBarUI prefab on its own Canvas (Sort Order 30)` | `BossHealthBarUI.cs`, new `UI_BossHealthBar.prefab` |
| B-08 | `chore(handoff): hand boss + portal + HP bar prefabs to P2 for Arena wiring` | Discord message + GitHub Issue update |

### P2 — Map, Lighting, UI Routing (May 2 afternoon -> May 4)

| # | Commit message | Files |
|---|---|---|
| **M-00** | `chore(scenes): duplicate SampleScene as Arena, set Arena as build target` | New `Assets/_Project/Scenes/Arena.unity`, edit `ProjectSettings/EditorBuildSettings.asset` (Arena = first scene in build) |
| M-01 | `chore(arena): import desert environment Fab asset` | `Assets/Imported/DesertEnv/...` |
| M-02 | `feat(arena): build arena layout — terrain + perimeter dressing + visual-only props` | `Arena.unity` |
| M-03 | `feat(ui): NarrativeTextUI prefab fades story intro on game start` | `NarrativeTextUI.cs`, new `UI_NarrativeText.prefab` (Canvas Sort 10) |
| M-04 | `feat(ui): CountdownUI 3-2-1-GO prefab` | `CountdownUI.cs`, new `UI_Countdown.prefab` (Canvas Sort 20) |
| M-05 | `feat(ui): screen fade controller for state transitions` | `ScreenFadeController.cs`, new `UI_ScreenFade.prefab` (Canvas Sort 1000) |
| M-06 | `feat(arena): wire boss spawn + portal anchor + spawn points` | `Arena.unity` |
| M-07 | `feat(arena): drop boss + portal + boss-HP prefabs into Arena, wire references` | `Arena.unity` |
| M-08 | `feat(visuals): lighting bake + post-processing volume (bloom, color grading, vignette)` | `Arena.unity`, lighting data |
| M-09 | `feat(camera): CameraFollow gets arena bounds clamp` | `CameraFollow.cs` |
| M-10 | `feat(ui): victory + defeat screen polish` | `Arena.unity` UI panels |

### P3 — Upgrades, Audio, Player Feel (May 2 afternoon -> May 4)

| # | Commit message | Files |
|---|---|---|
| U-01 | `feat(upgrade): MaxHPUpgrade ScriptableObject` | New `MaxHPUpgrade.cs` |
| U-02 | `feat(upgrade): ShieldUpgrade ScriptableObject` | New `ShieldUpgrade.cs` |
| U-03 | `feat(upgrade): MultiShotUpgrade ScriptableObject` | New `MultiShotUpgrade.cs` |
| U-04 | `feat(upgrade): ReloadSpeedUpgrade ScriptableObject` | New `ReloadSpeedUpgrade.cs` |
| U-05 | `chore(assets): import upgrade icons + assign to SOs` | `Assets/_Project/UI/UpgradeIcons/`, edit 7 SO assets |
| U-06 | `feat(audio): AudioMixer asset with Music / SFX / UI groups` | New `GameAudioMixer.mixer` |
| U-07 | `chore(assets): import all SFX clips` | `Assets/_Project/Audio/SFX/...` |
| U-08 | `feat(audio): MusicController subscribes to StateChanged, crossfades music` | `MusicController.cs` |
| U-09 | `feat(audio): wire SFX prefabs to game events (hit, death, coin, shoot, dash, hurt)` | New `SFXEventBindings.cs`, prefab wiring |
| U-10 | `feat(audio): enemy AudioSources set to 3D spatial blend` | Enemy prefab edits |
| U-11 | `feat(feel): CameraShaker subscribes to PlayerHealth.Damaged` | `CameraShaker.cs` |
| U-12 | `feat(feel): LowHPVignette lerps post-processing intensity below 25% HP` | `LowHPVignette.cs` |

### P4 — Enemies (May 2 afternoon -> May 3)

| # | Commit message | Files |
|---|---|---|
| E-01 | `chore(assets): import melee + ranged enemy models from Mixamo` | `Assets/_Project/Models/Enemies/...` |
| E-02 | `feat(enemy): EnemyAI explicit FSM enum (Idle/Chase/Attack/Dead)` | `EnemyAI.cs` (this is the AI scripts only — locked-file changes go through lead) |
| E-03 | `feat(enemy): EnemyProjectile pooled component` | `EnemyProjectile.cs`, new `Enemy_Projectile.prefab` |
| E-04 | `feat(enemy): RangedEnemyAI FSM (ApproachMidRange/Throw/Retreat)` | `RangedEnemyAI.cs` |
| E-05 | `feat(enemy): MeleeEnemy prefab with Mixamo model + animator` | New `Enemy_Melee.prefab` |
| E-06 | `feat(enemy): RangedEnemy prefab with model + animator` | New `Enemy_Ranged.prefab` |
| E-07 | `feat(vfx): EnemyHitParticle prefab tied to EnemyHealth.Damaged` | New prefab + new `EnemyHitVFX.cs` |
| E-08 | `feat(enemy): death animation event triggers pool release` | Animation events on prefabs |
| E-09 | `fix(player): dash collision sweep at perimeter walls` | `PlayerCollisionMotor.cs` |

---

## Milestones

### Milestone 1 — Foundation + Skeleton (May 2)
Deliverables: F-01 to F-20 merged. P1/P3/P4 ship their first 2-3 commits in parallel. P2 ships M-00.

### Milestone 2 — Playable Boss Fight (May 3)
Deliverables: All P1/P3/P4 prefab work complete. P2 ships M-06, M-07. Integration playtest at end of day.

### Milestone 3 — Polish & Feel (May 4)
Deliverables: Lighting, post-processing, VFX, balance, all SFX wired, screen fades.

### Milestone 4 — Ship (May 5)
Deliverables: Profiler evidence, bug log, final report, README, build, GitHub Release `v1.0.0-phase3`, demo video.

---

## Integration + Ship Commits

### M3 Integration (May 3 evening - May 4)

| # | Commit | Owner |
|---|---|---|
| I-01 | `chore(arena): drop all prefabs into Arena, wire references` | P2 |
| I-02 | `chore(qa): integration playthrough, log breaks as Issues` | All |
| I-03 | `balance: tune boss HP, throw cadence, summon cadence` | P1 |
| I-04 | `balance: tune wave growth + enemy stats` | P4 |
| I-05 | `balance: tune upgrade values` | P3 |

### M4 Ship (May 4 - May 5)

| # | Commit | Owner |
|---|---|---|
| S-01 | `perf: profiler session, capture CPU/GPU/memory screenshots` | P4 |
| S-02 | `docs(phase3): bug triage log in final_report` | All |
| S-03 | `docs(phase3): final_report visual production section` | P2 |
| S-04 | `docs(phase3): final_report audio production section` | P3 |
| S-05 | `docs(phase3): final_report performance section` | P4 |
| S-06 | `docs(phase3): final_report postmortem` | All |
| S-07 | `docs: README update — controls, build, team roles, screenshot` | P1 |
| S-08 | `build: Windows build + GitHub Release v1.0.0-phase3` | P1 |
| S-09 | `docs: demo video link in README` | P3 |
| S-10 | `chore: close all issues, clean Project board` | P2 |

---

## Team Workflow Rules — Copy to Discord

```
=== Sands of the Unseen — Phase 3 Workflow Rules ===

TWO-SCENE POLICY
- Assets/_Project/Scenes/SampleScene.unity = TEST SANDBOX
  - Edit it freely on your machine for testing.
  - DO NOT commit changes unless the lead asks.
  - If accidentally staged: git restore Assets/_Project/Scenes/SampleScene.unity
- Assets/_Project/Scenes/Arena.unity = PRODUCTION SCENE
  - Only Mesh (P2) commits to it.
  - Need something added? Hand Mesh a prefab.

CANVAS OWNERSHIP
- Main HUD Canvas in Arena belongs to P2.
- New UI = new Canvas prefab on its own GameObject with unique Sort Order:
    HUD            = 0    (P2)
    Narrative      = 10   (P2)
    Countdown      = 20   (P2)
    Boss HP        = 30   (P1 builds, P2 wires)
    Upgrade Menu   = 100  (P3)
    Screen Fade    = 1000 (P2)
- Never add UI directly under another person's Canvas.

PREFAB WORKFLOW
- Every gameplay object lives as a prefab in Assets/_Project/Prefabs/.
- Edit in Prefab Mode (double-click), never as scene instance.
- "Apply All Overrides" before committing.
- Naming: <System>_<Object>.prefab  e.g. Boss_Warlord.prefab,
  UI_BossHealthBar.prefab, VFX_EnemyHit.prefab.

LOCKED FILES (lead-only)
GameSessionState.cs, GameSessionController.cs, GameplayBehaviourGate.cs,
GameScreenRouter.cs, EnemyBoxAgent.cs, EnemyWaveSpawner.cs,
BulletDamageDealer.cs, IDamageable.cs, IEnemyBehaviour.cs,
EnemyStatsContext.cs, PlayerHealth.cs, PlayerShooting.cs, PlayerStats.cs,
UpgradeManager.cs, ProjectSettings/TagManager.asset,
ProjectSettings/DynamicsManager.asset, ProjectSettings/EditorBuildSettings.asset

If you need a change here, open an Issue tagged "needs-lead".

BRANCH + PR RULES
- Branch from Development: feature/<issue-number>-<short-name>
- Commit early, push often.
- Open PR when ready, body must contain: Closes #N
- Move issue to "Review" on the project board.
- Lead reviews and merges.
- Delete branch after merge.

ENEMY PREFAB SPEC
- Root: Collider on Enemy layer
- Required: EnemyHealth (implements IDamageable), EnemyBoxAgent,
  one IEnemyBehaviour implementation (EnemyAI for melee,
  RangedEnemyAI for ranged)
- Big enemies (boss): use DamageableHitbox child colliders on Hitbox layer

LAYER USAGE
- Player: only player root.
- Enemy: enemy roots + main visual collider.
- EnemyProjectile: enemy bullets/throwables.
- PlayerProjectile: player bullets.
- Hitbox: child damage colliders on big enemies (boss).
- World: invisible perimeter walls + ground.
- Pickup: orbs, ammo boxes.
- Physics matrix is locked — don't touch.

DAILY STANDUP (10 min, voice or text)
- Morning: today's commits, blockers.
- End of day: PR open, what carries to tomorrow.
- Blocked > 1 hour, ping the lead.

DESIGN LOCKED
- Boss is stationary at arena center.
- Boss alternates projectile volleys / summon child waves.
- Portal opens at boss death position. Player walks in to win.
- Interior arena props are visual-only (no colliders).
- Perimeter is caged by invisible walls (BoxCollider, no Renderer).
```

---

## GitHub Setup

The rubric explicitly requires Issues, PRs, and Milestones visible in the GitHub Projects tab.

1. **Authenticate gh CLI** — already done.
2. **Run setup script** (to be regenerated):
   ```
   .\setup-github-project.ps1
   ```
   Creates 4 milestones (M1-M4), 5 labels, 11 high-level issues mapped to milestones.
3. **Create the Project board** at:
   `https://github.com/MeshariAlHejaili/sands-of-the-unseen/projects`
   - New project -> Board template
   - Name: "Phase 3 Sprint"
   - Columns: Backlog / In Progress / Review / Done
   - Add all open issues
   - Pin to repository
4. **Workflow during sprint:**
   - Each commit in this plan corresponds to a sub-task on the high-level issue.
   - Move Issue to "In Progress" when work starts.
   - Open PR with `Closes #N` when ready -> Issue auto-closes on merge -> moves to "Done".

---

## Asset Sources

| Asset | Source | Owner |
|---|---|---|
| Desert environment | https://www.fab.com/listings/725e2f8e-18dc-432d-b427-86431d1fa9f2 | P2 (M-01) |
| Ammo boxes | https://www.fab.com/listings/76f33491-c282-487d-9b70-407ea18eb425 | P2 (M3 polish) |
| Upgrade icons | https://www.fab.com/listings/2e9ec43b-24b0-4f72-868a-84e3e588d9ec | P3 (U-05) |
| Melee enemy model | https://www.mixamo.com (search: Zombie / Mutant) | P4 (E-01) |
| Ranged enemy model | https://www.mixamo.com (visually distinct from melee) | P4 (E-01) |
| Boss model | https://www.mixamo.com (large/imposing) | P1 (B-01) |
| SFX clips | https://freesound.org / https://itch.io/game-assets | P3 (U-07) |
| Fallback models | Meshy AI (already in project) | Anyone, last resort |

---

## Rubric Coverage

| Rubric Criteria (Points) | Where Covered |
|---|---|
| **Final Build Quality & Playability (25)** | M4: S-08 build + S-09 demo video; All milestones for stability |
| **Visual Production Pass** (in 25) | P2 M-08 lighting/post; P1 B-06 portal VFX; P4 E-07 hit particles |
| **Audio Production Pass** (in 25) | P3 U-06 mixer, U-08 music, U-09 SFX, U-10 3D spatial |
| **Performance & Optimization (20)** | M4 S-01 profiler; existing pooling architecture |
| **Production Discipline (15)** | GitHub Project board + Issues + PRs throughout; S-08 release tag; S-07 README |
| **Presentation & Postmortem (15)** | S-02 to S-06 final_report sections; S-09 demo video |
| Core gameplay engineering | Phase 2 base + F-04/F-05 boss state + B-05 portal flow |
| Physics & collision | F-19 perimeter; F-10 IDamageable bullets; E-09 dash sweep |
| AI behavior (FSM) | E-02 melee FSM; E-04 ranged FSM; B-02 boss FSM |
| Real-time graphics | M-01 environment; M-08 lighting + post-processing |
| Technical art & polish | E-07/E-08 enemy VFX/anim; B-03 boss VFX; M-05 fades; U-11 shake |
| Audio systems | U-06 to U-10 full audio pass |
| UI/UX feedback | M-03 narrative; M-04 countdown; B-07 boss HP; U-12 vignette |
| Performance optimization | S-01 profiler evidence; existing pooling |
| Production & collaboration | Issue/PR/Milestone workflow; daily standups; locked-file discipline |

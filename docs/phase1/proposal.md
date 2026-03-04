# Phase 1 Proposal

---

## 1. Scene Summary

### Scene Goal
Build one complete playable survival scene (vertical slice) of a horde shooter roguelite inspired by *The Horde Wants You Dead*.
The player must survive against waves of enemies for a fixed duration (5 minutes).
This is not a full game. It is one production-quality survival encounter demonstrating complete system integration.

### Player Experience (Start → End)

**Start Boundary**
* Player spawns in a small arena.
* Countdown (3…2…1… Start).
* Wave 1 begins.
* Background music fades in.

**Gameplay Flow**
* Enemies spawn from multiple fixed spawn points around the map.
* Enemies path toward the player and attack at close range.
* Player shoots using a single weapon (manual aim).
* Enemies drop money when killed.
* When enough money is collected:
    * Game pauses briefly.
    * Upgrade selection UI appears (3 upgrade choices).
    * Player selects one.
    * Combat resumes.
* Waves increase in spawn rate/density.

**End Boundary**
* **Win Condition:** Survive 5 minutes → Victory screen.
* **Fail Condition:** Player health reaches 0 → Game Over screen.
* The scene fully resets after end screen.

### Core Gameplay Loop
> Move → Aim → Shoot → Kill Enemy → Collect Money → Choose Upgrade → Survive Stronger Waves → Repeat

This loop continues until:
* Time survived = 5 minutes (win)
* Player HP = 0 (lose)

---

## 2. Benchmark Reference

**Target Reference:** *The Horde Wants You Dead* – https://youtu.be/t2MoeUD624E?si=GfAXBovjDSJpeEqU&t=72 until 1:31

**We are replicating:**
* Top-down arena survival
* Continuous enemy pressure
* Simple enemy AI (move toward player and attack)
* Upgrade selection popup during gameplay
* Clean readable combat

**We are intentionally simplifying:**

| System | Reference | Our Version |
| :--- | :--- | :--- |
| **Weapons** | Multiple weapons | 1 weapon only |
| **Enemy Types** | Multiple behaviors | 1 basic melee enemy |
| **Map** | Large map | Small contained arena |
| **Meta progression** | Yes | None |
| **Boss** | Yes | None |

*This keeps scope realistic for one semester vertical slice.*

---

## 3. Scope Definition

### Must-Have (Vertical Slice Core)

**Gameplay Systems**
* Player movement (WASD)
* Mouse aiming
* Shooting system (projectile or raycast)
* Dodge mechanic (short burst movement with cooldown)
* Player HP system
* Enemy HP system
* Wave spawner
* Survival timer
* Win/Lose states
* Upgrade selection system (3 random upgrades)
* Money drop & pickup system

**Enemy**
* One melee enemy type
* State-driven AI (Finite State Machine):
    * Spawn
    * Chase
    * Attack
    * Dead

**UI**
* Health bar
* Timer
* Money counter
* Upgrade selection panel
* Game Over / Victory screen

**Visual & Audio**
* Ready materials and lighting from assists store
* Muzzle flash VFX
* Enemy hit flash
* Death VFX
* Shooting SFX
* Hit SFX
* Background music

### Nice-to-Have (Only if Time Allows)
* Second enemy variant (faster but lower HP)
* Screen shake on damage
* Blood impact effect
* Slight camera zoom-out as waves increase
* Simple minimap
* Difficulty scaling curve tuning

### Explicitly NOT Doing
* No multiplayer
* No open world
* No inventory system
* No procedural map generation
* No advanced skill trees
* No story mode
* No saving/loading system
* No weapon switching
* No advanced enemy pathfinding (NavMesh only)

*Scope discipline ensures feasibility.*

---

## 4. Technical Competency Coverage

### Core Gameplay Systems Engineering
* Player controller script (movement + dodge)
* Weapon firing logic (rate-limited)
* GameManager controlling:
    * Wave system
    * Timer
    * State transitions (Playing / Upgrade / GameOver / Victory)
* **Game State Flow:**
    `Start → Playing → (Upgrade Pause) → Playing → Victory/GameOver → Restart`

### Physics & Collision Systems
* Rigidbody-based player movement
* Collider-based enemy hit detection
* Deterministic raycast or projectile collision
* Trigger collider for money pickup
* Damage application with invulnerability frames during dodge
* *Reliable collision logic ensures no double hits or missed detection.*

### AI Behavior Design
* **Enemy Finite State Machine:**
    `Spawn → Chase → Attack → Dead`
* **Chase:** Move toward player position using NavMesh.
* **Attack:** If within attack range:
    * Stop movement
    * Play attack animation
    * Apply damage with cooldown timer
* *State transitions clearly defined and deterministic.*

### Real-Time Graphics Pipeline
* Simple stylized materials
* Clear color separation:
    * Player: Bright
    * Enemy: Dark red
    * Money: Gold glow
* Bloom on muzzle flash
* Baked lighting for performance
* Shadow casting for depth
* *Scene readability prioritized over realism.*

### Technical Art & Polish
* Muzzle flash particle system
* Hit spark particle
* Death dissolve effect
* Camera:
    * Slight follow smoothing
    * Minor shake on damage
* Post-processing:
    * Bloom
    * Slight vignette when low HP

### Audio Systems Design
* Event-driven audio triggers:
    * Shoot
    * Enemy hit
    * Enemy death
    * Player damaged
    * Upgrade select
* Background loop music
* Volume mixing balance
* Optional 3D spatial sound for enemies

### UI/UX Feedback
* Health bar flashes when hit
* Damage number popups
* Upgrade screen pauses gameplay
* Clear countdown at start
* High contrast UI
* **UX goal:** Always know:
    * How much HP?
    * How much time left?
    * When next upgrade?

### Performance Optimization & Debugging
* **Target:** Stable 60 FPS.
* **Optimization Plan:**
    * Object pooling for enemies
    * Object pooling for bullets
    * Profile spawn spikes
    * Avoid unnecessary `Update()` calls
    * Use Unity Profiler screenshots for Phase 3

### Production & Collaboration Practices
* **Repository Setup**
* Feature branching workflow
* Pull Requests required
* Issue tracking per feature
* PR review required before merge
* Weekly milestone tags
* **Branch Naming:**
    * `feature/player-controller`
    * `feature/enemy-ai`
    * `feature/ui-system`

---

## 5. Production Plan

### Team Roles
* **Gameplay Programmer:** Player controller, Shooting system, Dodge mechanic, Upgrade logic
* **AI & Systems Programmer:** Enemy FSM, Wave spawner, GameManager, Difficulty scaling
* **Technical Artist / UI:** UI implementation, VFX, Lighting setup, Audio integration
* *(All members contribute to testing and debugging.)*

### GitHub Project Board Plan
* **Milestone 1 – Core Mechanics (Week 10)**
    * Player movement
    * Shooting
    * Basic enemy chase
    * Basic damage system
    * Arena blockout
    * *Acceptance:* Player can shoot and kill enemies.
* **Milestone 2 – Complete Loop (Week 11 – Phase 2 Checkpoint)**
    * Wave system
    * Timer
    * Money drop
    * Upgrade selection
    * Win/Lose states
    * *Acceptance:* Full start → end playable graybox.
* **Milestone 3 – Visual & Audio Pass (Week 14)**
    * VFX
    * UI polish
    * Lighting
    * Audio integration
    * Performance profiling
    * *Acceptance:* Polished production-quality slice.

---

## 6. Risks & Mitigation

| Risk | Mitigation |
| :--- | :--- |
| **Enemy count causing lag** | Object pooling |
| **Upgrade system complexity** | Keep 4 simple stat upgrades only |
| **Scope creep** | Strict NOT DOING list |
| **AI bugs** | Early FSM implementation |

---

## 7. Phase 2 Checkpoint Deliverables

By Week 11 we will have:
* Fully playable graybox
* Complete gameplay loop
* Functional enemy AI
* Working upgrade system
* Basic UI
* Playtest report
* Documented known bugs

---

## Final Statement

This vertical slice focuses on:
* Clean combat loop
* Deterministic AI
* Clear system integration
* Production-quality polish within tight scope

We are building one complete, polished survival encounter, not a full game.

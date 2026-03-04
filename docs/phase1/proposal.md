# Phase 1 – Proposal & Pre-Production

# Survival Arena Vertical Slice

---

# 1. Scene Summary

## Scene Goal
Build one complete playable survival scene (vertical slice) of a top-down horde shooter inspired by *The Horde Wants You Dead*.

The player must survive against enemy waves for **5 minutes** inside a small arena.

This is not a full game — it is one polished, fully integrated survival encounter.

---

## Player Experience (Start → End)

### Start Boundary
- Player spawns in arena
- 3-second countdown (3…2…1… Start)
- Wave 1 begins
- Background music starts

### Gameplay Flow
- Enemies spawn from fixed spawn points
- Enemies chase the player and attack in melee
- Player aims using mouse cursor
- Player shoots using one gun
- Enemies flash and show **damage numbers** when hit
- Enemies play **death animation** when killed
- Enemies drop coins
- Player collects coins
- Upgrade bar fills
- When full:
  - Game pauses
  - 3 random upgrades appear
  - Player selects one
  - Combat resumes
- Waves increase in difficulty over time

### End Boundary
- **Win:** Survive 5 minutes → Victory screen
- **Lose:** Player HP reaches 0 → Game Over screen
- Scene resets after end

---

## Core Gameplay Loop

Move → Aim (Cursor-Based) → Shoot → Damage (Flash + Numbers) →  
Kill → Collect Coins → Upgrade → Survive Stronger Waves → Repeat

---

# 2. Benchmark Reference

---

# Benchmark 1 – The Horde Wants You Dead  
**Scene Reference: (1:12-1:31)**  
https://youtu.be/t2MoeUD624E?si=GfAXBovjDSJpeEqU&t=72  until 1:31 

This benchmark defines our **core survival structure and wave system i.e. Our game mechanics**.

## What We Are Taking

### Perspective
- Fixed **top-down camera**
- Smooth follow
- High combat readability

### Wave System
- Enemies spawn from **specific fixed spawn points**
- Spawn rate increases over time
- Enemy count increases per wave
- Continuous pressure scaling during 5-minute survival

We will implement a `WaveManager` that controls:
- Spawn intervals
- Enemy scaling
- Survival timer

### Enemy AI
One melee enemy using FSM:

`Spawn → Chase → Attack → Dead`

- Chase player using NavMesh
- Attack in close range
- Drop coins on death
- Play **death animation** before removal

### Combat Feedback
- Enemy hit flash when damaged
- **Damage numbers above characters**
- Coin drop on kill

### Upgrade System
- Enemies drop coins
- Upgrade bar fills
- Game pauses
- **3 random upgrades displayed**
- Player selects one
- Combat resumes

### Scope Control

In the benchmark:
- Multiple guns
- Multiple enemy types
- Boss fights
- Large map

In our version:
- **One gun only**
- One enemy type
- No boss
- No meta progression
- Small contained arena

---

# Benchmark 2 – Aim & Gun Feel Reference  
**Scene Reference (2:10–2:20):**  
https://www.youtube.com/watch?v=gIlfWonzubM&t=128s  

This benchmark defines our **aiming system, gun feel, lighting effect, and audio feedback**.

## What We Are Taking

### Aim System
- Precise **mouse-based aiming**
- Cursor-style targeting
- Responsive shooting feel

### Gun Effects
- Muzzle flash VFX
- **Light flash when firing** that briefly lights a small area around the player
- Clear hit feedback

### Audio Design
- Gun firing sound inspired by benchmark
- **Collectable pickup sound**
- Damage feedback sounds

### Combat Implementation
- Single gun only
- Raycast-based hit detection
- Manual mouse aim
- Integrated visual + audio feedback

---

# Benchmark 3 - Map & Visuals
**Target Reference:** *Soulstone Survivors* – https://youtu.be/VNCzWloz2C8?si=P84rhtZ5mUs9U0-h

**We are replicating:**
* **Environment:** Top-down desert arena.
* **Palette:** Warm, saturated earth tones (red/orange sand).
* **Layout:** Open flat terrain bordered by rocky cliffs.
* **Atmosphere:** Bright daylight with sharp shadows.

**We are intentionally simplifying:**

| System | Reference | Our Version |
| :--- | :--- | :--- |
| **Map Size** | Infinite/Large Maps | Small contained Desert Arena |
| **Biome Variety** | Multiple Biomes | 1 Desert biome |
| **Detail Level** | High fidelity assets | Stylized low-poly assets |
| **Lighting** | Dynamic day/night | Baked bright daylight |


# 3. Scope Definition

## Must-Have (Core Vertical Slice)

### Gameplay Systems
- Player movement (WASD)
- Mouse aiming
- Shooting system (raycast)
- Dodge roll mechanic
- Stamina system
- Player HP system
- Enemy HP system
- Wave spawner
- Survival timer
- Win/Lose states
- Upgrade selection system (3 random upgrades)
- Money drop & pickup system

### Enemy
- One melee enemy
- FSM behavior

### UI
- Health bar
- Stamina bar
- Timer
- Money counter bar
- Upgrade selection panel
- Game Over / Victory screen
- Menu Screen

### Visual & Audio
- Muzzle flash
- Damage numbers
- Hit flash
- Death animation
- Gun sound
- Pickup sound
- Background music

---

## Nice-to-Have (If Time Allows)
- Second enemy variant
- Screen shake on damage
- Minor camera zoom scaling
- Blood impact effect
- Balancing pass for difficulty curve

---

## Explicitly NOT Doing
- No multiplayer
- No open world
- No inventory system
- No procedural map
- No boss fights
- No saving/loading
- No weapon switching
- No advanced enemy types

---

# 4. Technical Competency Coverage

## Core Gameplay Systems Engineering
- Player controller (movement + dodge)
- Weapon firing logic
- GameManager controlling:
  - Wave system
  - Timer
  - Upgrade state
  - Win/Lose states

Game State Flow:

Start → Playing → UpgradePause → Playing → Victory/GameOver → Restart

---

## Physics & Collision Systems
- Rigidbody-based movement
- Raycast hit detection
- Collider-based damage logic
- Trigger collider for coin pickup
- Invulnerability frames during dodge
- Object pooling for enemies

---

## AI Behavior Design
Finite State Machine:

Spawn → Chase → Attack → Dead

- Deterministic transitions
- NavMesh-based movement
- Attack cooldown system

---

## Real-Time Graphics Pipeline
- Top-down rendering setup
- Clear color contrast (player vs enemies)
- Muzzle flash with emissive material
- **Temporary light flash when firing**
- Basic post-processing (bloom)
- Optimized lighting setup

---

## Technical Art & Polish
- Damage number popups
- Enemy hit flash
- Death animation
- Camera smoothing
- Particle effects for shooting

---

## Audio Systems Design
- Gun firing sound
- Pickup sound for coins
- Damage sound feedback
- Background music loop
- Balanced audio mixing

---

## UI/UX Feedback
- Health bar feedback
- Stamina bar feedback
- Timer display feedback
- Upgrade Selection UI display when money bar is filled
- Flash feedback when taking damage
- Money bar feedback
- Money bar reset after choosing a passive
- Low HP feedback

---

## Performance Optimization & Debugging
Target: Stable 60 FPS

- Object pooling (enemies & effects)
- Profiling enemy spawn spikes
- Avoid unnecessary Update calls
- Unity Profiler evidence in later phases

---

## Production & Collaboration Practices
- GitHub repository
- Feature branching workflow
- Pull Requests required before merge
- GitHub Project board for task tracking
- Weekly milestones
- Issue tracking per feature

---

# 5. Production Plan

## Team Roles

**Gameplay Programmer**
- Player controller
- Shooting system
- Dodge & stamina
- Upgrade logic

**AI & Systems Programmer**
- Enemy FSM
- WaveManager
- GameManager
- Difficulty scaling

**Technical Artist / UI**
- UI implementation
- VFX
- Lighting setup
- Audio integration

All members contribute to testing and debugging.

---

## Milestones

### Milestone 1 – Core Mechanics
- Player movement
- Shooting
- Basic enemy chase
- Basic damage system
- Arena blockout

Acceptance:
Player can shoot and kill enemies.

---

### Milestone 2 – Complete Gameplay Loop (Phase 2 Checkpoint)
- Wave system
- Timer
- Coin drops
- Upgrade selection
- Win/Lose states

Acceptance:
Full start → end playable graybox.

---

### Milestone 3 – Polish & Optimization
- VFX
- UI polish
- Lighting pass
- Audio integration
- Performance profiling

Acceptance:
Stable, polished vertical slice.

---

# 6. Risks & Mitigation

| Risk | Mitigation |
|------|------------|
| Too many enemies cause lag | Object pooling |
| Upgrade system complexity | Limit to simple stat upgrades |
| Scope creep | Strict NOT DOING list |
| AI bugs | Implement FSM early |

---

# 7. Phase 2 Checkpoint Deliverables

By Phase 2:
- Fully playable graybox
- Complete gameplay loop
- Functional enemy AI
- Working upgrade system
- Basic UI
- Playtest feedback
- Known bugs documented

---

# Final Statement

This vertical slice demonstrates:

- Core gameplay systems integration  
- AI behavior design  
- Physics and collision reliability  
- Real-time graphics decisions  
- Technical art polish  
- Audio feedback systems  
- UI/UX clarity  
- Performance optimization  
- Structured production workflow  

We are building one complete, polished survival encounter — not a full game.

# Cross Discipline Feature Development
**Unity Version:** 2022.3.62f2  
**Project Type:** University cross-discipline feature development project  
**Feature:** Knife-based teleportation system with enemy AI

---

## Setup

### Requirements
- Unity **2022.3.62f2** (other versions not tested and not recommended)
- Git / GitHub for version control

### Required Packages
Install both of these via **Window > Package Manager** before opening any scenes:

| Package | Where to find it |
|---|---|
| AI Navigation | Unity Registry |
| Input System | Unity Registry |

> If you open the project without these installed you will get missing script errors and the NavMesh components will not resolve.

### First Time Opening
1. Clone the repo
2. Open in Unity 2022.3.62f2
3. Install the two packages above if not already present
4. Open the `PlayerTesting` scene -- this is the active development and testing scene
5. Do **not** use or modify the main project scene unless you have been explicitly assigned to it

---

## Project Structure

### Active Scene
`PlayerTesting` -- all feature development and testing happens here. It is intentionally kept separate from the main project scene to avoid conflicts during development.

### Key Scripts

| Script | Responsibility |
|---|---|
| `EnemyAI.cs` | FSM managing Patrol, Chase, Investigate, and Confused states |
| `EnemyPerception.cs` | FOV detection with inner/outer cone angles and range falloff |
| `EnemyFOVRenderer.cs` | Procedural ground-projected fan mesh, changes colour per state |
| `PlayerMovement.cs` | Movement, jumping, sprint, knife selection via line-of-sight, teleport hook |
| `PlayerHealth.cs` | Health, invincibility frames, respawn logic -- implements `IDamageable` |
| `ITeleportable.cs` | Interface that notifies enemies when the player teleports |
| `IDamageable.cs` | Shared interface for anything that can take damage |

---

## Scene Reset
Press **R** to reset the scene. This works in both the editor and in a build.

---

## Known Architecture Notes
- `NavMeshAgent` components on enemies must have **Is Kinematic** enabled on their Rigidbody to prevent jitter on ramps
- Enemy teleportation uses `NavMeshAgent.Warp()` -- do not use `transform.position` directly or the agent will snap back
- FOV renderer material is created entirely in code, no manual material assignment is needed
- `rb.velocity` is used throughout -- `rb.linearVelocity` is deprecated in this Unity version

---


## Contact
Ryan -- enemy AI, player systems, teleportation implementation  
Jack Smith -- feature concept and design (see design document in repo)

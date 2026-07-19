# I LOVE NATURE

**A 2D platformer about a small robot who makes the forest worse, one upgrade at a time.**

Made in 48 hours for UNOG Jam 2026 · Unity 6 (URP 2D)

---

## The premise

You are a machine sent into the woods with a directive and a gun.

Every tree you fell and every animal you shoot makes you *stronger* — a little more health, a little more speed, a little more reach. The upgrade arrives automatically at the start of each level, whether you wanted it or not.

The catch is that the forest is keeping score. Across four levels the world drains from green to brown, the birdsong gives way to factory noise, and your chassis grows sleeker and colder to match. By the end there is very little left to harvest.

Including you.

> Walk up to the bodies you leave behind and press **E**. The robot has something to say about them.

---

## Features

- **Four hand-built levels** that degrade as you progress — the forest doesn't fade with a colour filter, each level is authored as its own dying environment.
- **A robot that changes with the world.** Four distinct player sprites, one per level. You get shinier as the woods get worse.
- **Automatic progression.** No shop, no menu, no choice: clear a level and the next one starts with you already upgraded. The *game* decides you should be stronger.
- **Destructible forest.** Trees are obstacles with health bars. The path forward is through them.
- **Enemies that fight back** — a fast charging boar, a heavy bear that rears up and swipes, and birds that patrol out of reach.
- **Loot from everything you kill.** Animals and trees both drop resources. The economy of the game is destruction.
- **Carcass dialogue.** Approach a body, press E, and read what the machine logs about it.
- **Game feel:** muzzle flash lighting, hit-stop on impact, screen shake, damage flashes, 2D lights and bloom throughout.

---

## Controls

| Action | Keyboard / Mouse | Gamepad |
|---|---|---|
| Move | `WASD` or Arrow keys | Left stick |
| Jump | `Space` | A / Cross |
| Shoot | `Left Mouse` or `J` | Right trigger |
| Interact | `E` | Y / Triangle |

Variable jump height — hold to go higher, release early to cut the jump short.

---

## Credits

**Programming** — Aziz Önder · Mert Kaya
**Art** — Sude · Dilay

Built with [Unity 6](https://unity.com) and [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween).
Ambient audio and UI art from third-party packs (see in-game credits).

---

## For developers

### Architecture

The project is deliberately data-driven so that content scales without new code. Adding an enemy, a player look, or an upgrade is a ScriptableObject plus a prefab — never a new class or a new `switch` case.

```
Game.Core            pure C#, no Unity gameplay types, unit-tested
  └─ Health, RunState, GameFlow FSM, EventChannel<T>
Game.Runtime         MonoBehaviours: combat, enemies, player, level flow, UI
Game.Editor          tooling and data validation
Game.Tests.EditMode  EditMode tests over Game.Core
```

Assembly definitions enforce the dependency direction, so a boundary violation is a compile error rather than a code review.

### Notable systems

| System | Notes |
|---|---|
| `LevelSequence` | One asset defines the whole run: scene order, upgrade granted on entry, robot appearance, ambience. |
| `PlayerAppearance` | A full sprite set (idle/walk/jump × left/right) as a single asset. Levels without one inherit the previous look, so art can land level by level. |
| `EnemyDefinition` | Stats, animation frames and attack tuning per archetype. Enemy #4 is an asset, not a subclass. |
| `DropOnDeath` | Generic loot spawner — used by both enemies and trees. Raycasts to place drops on whatever surface is actually below. |
| `Interactable` / `DialogueBubble` | Proximity prompt plus a world-space bubble with a typewriter reveal. |
| Object pooling | Bullets and impact flashes are pooled; combat allocates nothing per shot. |
| Event channels | ScriptableObject channels decouple level, HUD, audio and run progression. |

### Running from source

Requires **Unity 6000.0.79f1**. Open the project and load `Assets/Scenes/mainmenu.unity`.

> **Note:** the build scene order currently starts at `Level_01`, so a compiled build skips the menu and intro. Move `mainmenu` to index 0 in *File ▸ Build Settings* before shipping.

Git LFS is used for large binary assets — run `git lfs install` before cloning.

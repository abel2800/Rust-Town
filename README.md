# 🏚️ Rust Town

A post-apocalyptic zombie survival shooter game built in Unity. Experience intense wave-based combat in a desolate, sun-scorched abandoned town.

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![License](https://img.shields.io/badge/License-Proprietary-red)

---

## 🎮 Game Overview

**Rust Town** is a first-person wave-based zombie shooter set in a hauntingly beautiful post-apocalyptic environment. Fight endless waves of zombies in an abandoned small town frozen in perpetual sundown.

### Core Features

- 🧟 **Wave-Based Zombie Combat** - Survive increasingly difficult waves of zombie enemies
- 🌅 **"Sundown Desolation" Environment** - Atmospheric post-apocalyptic town with perpetual sunset
- 🔫 **Realistic Shooting Mechanics** - Raycast-based combat with recoil and muzzle flash
- 🏘️ **Procedurally Generated Town** - Detailed buildings, roads, gas station, residential areas
- 🔥 **Dynamic Atmosphere** - Burning barrels, fog, dust particles, flickering lights

---

## 🌆 Environment: "Sundown Desolation"

The game features a meticulously crafted post-apocalyptic environment:

### Main Street
- Cracked asphalt roads with potholes and oil stains
- Faded road markings and tire marks
- Damaged curbs and sidewalks

### Buildings
- **Abandoned Shops**: Rusty's Diner, General Store, Hardware, Pharmacy, Bar, Laundromat
- **Residential Houses**: Detailed homes with porches, damaged roofs, broken fences
- **Gas Station**: Complete with canopy, rusted pumps, oil puddles

### Atmosphere
- Perpetual sunset lighting (orange/red sky)
- Low-lying fog patches
- Floating dust particles and embers
- Burning barrels with flickering firelight
- Utility poles with dangling wires

---

## 🎯 Gameplay

### Controls
| Key | Action |
|-----|--------|
| WASD | Move |
| Mouse | Look around |
| Left Click | Shoot |
| Shift | Sprint |
| ESC | Pause |

### Combat System
- Instant hit-scan raycast shooting
- Headshot bonus damage (2x)
- Realistic gun recoil animation
- Shell casing ejection
- Accuracy tracking and bonus points

### Wave System
- Progressive difficulty scaling
- Enemy count increases per wave
- Zombie stats scale with difficulty
- Wave completion bonuses

---

## 🧟 Enemies

### Zombie AI
- **Detection**: Seeks and chases player within range
- **Attack**: Melee damage when close
- **Evasion**: Random dodge movements
- **Death Animation**: Ragdoll-style fall and fade

---

## 🛠️ Technical Details

### Project Structure
```
Rust Town/
├── Assets/
│   ├── Editor/           # Unity editor tools
│   ├── Models/           # 3D models (weapons, zombies)
│   ├── Resources/        # Runtime-loaded assets
│   ├── Scenes/           # Game scenes
│   └── Scripts/
│       ├── AI/           # Enemy AI system
│       ├── Core/         # Game manager, UI, input
│       ├── Player/       # Player controller, weapons
│       ├── Rendering/    # Material factory
│       ├── System/       # Object pooling
│       └── World/        # Map generation, atmosphere
├── Packages/
├── ProjectSettings/
└── UserSettings/
```

### Key Scripts
| Script | Description |
|--------|-------------|
| `GameManager.cs` | Central game state, waves, scoring |
| `PlayerController.cs` | FPS movement and camera |
| `WeaponSystem.cs` | Shooting mechanics and effects |
| `EnemyAI.cs` | Zombie behavior and animations |
| `PostApocalypticMapGenerator.cs` | Town environment generation |
| `AtmosphereEffects.cs` | Dynamic particles and lighting |
| `DetailedBuildingGenerator.cs` | Procedural building creation |
| `DetailedRoadGenerator.cs` | Road surface details |

### Requirements
- Unity 2022.3 or later
- Built-in Render Pipeline

---

## 🚧 Development Status

**This is an unfinished project** that needs additional work in the following areas:

### TODO
- [ ] Improved visual effects (particles, post-processing)
- [ ] Better zombie animations and models
- [ ] Sound effects and music
- [ ] More weapon variety
- [ ] Enhanced environment details
- [ ] UI polish and menus
- [ ] Performance optimization
- [ ] Game balancing

---

## 🚀 Getting Started

1. Clone the repository:
```bash
git clone https://github.com/abel2800/Rust-Town.git
```

2. Open in Unity Hub (Unity 2022.3+)

3. Open the scene: `Assets/Scenes/MainGame.unity`

4. Press Play to test!

---

## 📸 Screenshots

*Screenshots coming soon*

---

## 👤 Author

**abel2800**

- GitHub: [@abel2800](https://github.com/abel2800)

---

## 📄 License

This project is **proprietary software**. All rights reserved.

See [LICENSE](LICENSE) for details.

---

## 🙏 Acknowledgments

- Unity Technologies
- Mixamo (animations)
- Sketchfab (3D models)

---

*Rust Town - Survive the Sundown* 🌅🧟


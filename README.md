# 🏚️ Rust Town

A post-apocalyptic zombie survival shooter game built in Unity. Experience intense wave-based combat in a desolate, sun-scorched abandoned town.

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Status](https://img.shields.io/badge/Status-In%20Development-orange)
![License](https://img.shields.io/badge/License-Proprietary-red)

---

## 🎮 Game Overview

**Rust Town** is a first-person wave-based zombie shooter set in a hauntingly beautiful post-apocalyptic environment called "Sundown Desolation". Fight endless waves of zombies in an abandoned small town frozen in perpetual sunset.

### ✨ Current Features

- 🧟 **Wave-Based Zombie Combat** - Survive increasingly difficult waves of zombie enemies
- 🌅 **"Sundown Desolation" Environment** - Atmospheric post-apocalyptic town with perpetual sunset lighting
- 🔫 **FPS Shooting Mechanics** - Raycast-based combat with recoil, shell ejection, and muzzle flash
- 🏘️ **Procedurally Generated Town** - Detailed buildings, roads, gas station, residential areas
- 🔥 **Dynamic Atmosphere** - Burning barrels, fog, dust particles, flickering lights
- 🎯 **Headshot System** - Bonus damage for precision shots

---

## 🌆 Environment: "Sundown Desolation"

The game features a procedurally generated post-apocalyptic small town:

### 🛣️ Main Street
- Cracked asphalt roads with potholes and cracks
- Faded yellow center lines and tire marks
- Oil stains and road debris
- Damaged curbs and concrete sidewalks

### 🏢 Buildings
| Location | Description |
|----------|-------------|
| **Rusty's Diner** | Abandoned restaurant with boarded windows |
| **General Store** | Looted shop with broken glass |
| **Hardware Store** | Tools scattered, shelves empty |
| **Pharmacy** | Medical supplies long gone |
| **Gas Station** | Rusted pumps, oil puddles, canopy structure |
| **Residential Houses** | Detailed homes with porches, damaged roofs, overgrown yards |

### 🌫️ Atmosphere Effects
- **Perpetual Sunset** - Orange/red sky casting long shadows
- **Low-lying Fog** - Patches drifting across the streets
- **Dust Particles** - Floating in the sunlight
- **Burning Barrels** - Flickering firelight throughout town
- **Embers** - Rising from fires
- **Utility Poles** - With dangling, broken wires

---

## 🎯 Controls

| Key | Action |
|-----|--------|
| `W A S D` | Move |
| `Mouse` | Look Around |
| `Left Click` | Shoot |
| `Shift` | Sprint |
| `Space` | Jump |
| `ESC` | Pause |

---

## 🧟 Enemy System

### Zombie AI Behaviors
- **Seek** - Wanders when player not detected
- **Chase** - Pursues player when in range
- **Attack** - Melee damage when close
- **Evade** - Random dodge movements
- **Death** - Ragdoll fall animation with fade out

### Wave Progression
- Enemy count increases each wave
- Zombie health and damage scale with difficulty
- Wave completion bonus points

---

## 🔫 Combat System

- **Raycast Shooting** - Instant hit detection
- **Headshot Bonus** - 2x damage for head hits
- **Gun Recoil** - Visual kickback animation
- **Shell Ejection** - Brass casings fly out
- **Muzzle Flash** - Realistic light-based flash
- **Accuracy Tracking** - Stats and bonus rewards

---

## 🛠️ Project Structure

```
Rust Town/
├── Assets/
│   ├── Editor/              # Unity editor tools
│   │   ├── ZombieAnimatorAutoSetup.cs
│   │   ├── ZombieAnimatorSetup.cs
│   │   ├── ZombieModelFixer.cs
│   │   └── ZombieSetupTool.cs
│   ├── Models/
│   │   ├── Enemies/         # Zombie models & animations
│   │   └── Weapons/         # Gun models
│   ├── Resources/           # Runtime-loaded prefabs
│   ├── Scenes/
│   │   └── MainGame.unity   # Main game scene
│   └── Scripts/
│       ├── AI/
│       │   └── EnemyAI.cs
│       ├── Core/
│       │   ├── GameManager.cs
│       │   ├── InputHandler.cs
│       │   └── UISystem.cs
│       ├── Player/
│       │   ├── PlayerController.cs
│       │   └── WeaponSystem.cs
│       ├── Rendering/
│       │   └── NeonMaterialFactory.cs
│       ├── System/
│       │   └── ObjectPool.cs
│       └── World/
│           ├── PostApocalypticMapGenerator.cs
│           ├── DetailedBuildingGenerator.cs
│           ├── DetailedRoadGenerator.cs
│           └── AtmosphereEffects.cs
├── Packages/
├── ProjectSettings/
└── UserSettings/
```

---

## 🚧 Development Status

> ⚠️ **This is an unfinished project** currently in active development.

### ✅ Completed
- [x] Basic FPS controls and movement
- [x] Wave-based enemy spawning
- [x] Zombie AI (chase, attack, death)
- [x] Shooting mechanics with raycast
- [x] Procedural town generation
- [x] Atmospheric effects (fog, dust, fire)
- [x] HUD (health, score, wave counter)

### 🔨 Needs Work
- [ ] **Visual Effects** - Better particles, post-processing
- [ ] **Zombie Animations** - Smoother transitions, more variety
- [ ] **Sound Design** - Gunshots, zombie sounds, ambient audio
- [ ] **More Weapons** - Shotgun, rifle, melee options
- [ ] **Environment Polish** - More detailed textures and models
- [ ] **UI/UX** - Main menu, settings, game over screen
- [ ] **Performance** - Optimization for lower-end hardware
- [ ] **Game Balance** - Difficulty tuning, progression

---

## 🚀 Getting Started

### Requirements
- Unity 2022.3 or later
- Built-in Render Pipeline

### Installation
1. Clone the repository:
```bash
git clone https://github.com/abel2800/Rust-Town.git
```

2. Open in Unity Hub

3. Open scene: `Assets/Scenes/MainGame.unity`

4. Press Play to test!

### Setting Up Zombies
1. In Unity: `Tools → Auto Setup Zombie Animations`
2. Click "AUTO-SETUP EVERYTHING"
3. Press Play

---

## 📸 Screenshots

*Coming soon*

---

## 👤 Author

**abel2800**

- GitHub: [@abel2800](https://github.com/abel2800)

---

## 📄 License

**© 2024 abel2800. All Rights Reserved.**

This is proprietary software. Unauthorized copying, modification, distribution, or use of this software is strictly prohibited without explicit written permission from the author.

See [LICENSE](LICENSE) for full details.

---

## 🙏 Assets Used

- [Mixamo](https://www.mixamo.com/) - Character animations
- [Sketchfab](https://sketchfab.com/) - 3D models

---

*Rust Town - Survive the Sundown* 🌅🧟


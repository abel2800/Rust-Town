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
| `Left Click` | Shoot (Full Auto) |
| `Right Click` | Aim Down Sights (ADS) |
| `Shift` | Sprint |
| `Space` | Jump |
| `F11` | Toggle Fullscreen |
| `ESC` | Pause / Exit Fullscreen |

---

## 🧟 Enemy System

### Zombie Model & Animations
The game features a detailed zombie character with the **Scary Zombie Pack** animations:

| Animation | Description |
|-----------|-------------|
| 🏃 **Run** | Default movement toward player |
| ⚔️ **Attack** | Melee strike when in range |
| 🦷 **Bite** | Alternate attack animation |
| 💀 **Death** | Death animation when killed |
| 😱 **Scream** | Zombie scream effect |
| 🚶 **Walk** | Slow movement animation |
| 🧎 **Crawl** | Crawling zombie variant |

### Zombie AI Behaviors
- **Seek** - Wanders when player not detected
- **Chase** - Pursues player when in range (uses Run animation)
- **Attack** - Melee damage when close (Attack/Bite animations)
- **Evade** - Random dodge movements
- **Death** - Animated death with configurable despawn time

### Wave Progression
- Enemy count increases each wave
- Zombie health and damage scale with difficulty
- Wave completion bonus points

---

## 🔫 Combat System

### AK-47 Assault Rifle
| Stat | Value |
|------|-------|
| **Damage** | 35 per hit |
| **Fire Mode** | Full Auto |
| **Range** | 150m |
| **Hip Fire Accuracy** | 95% |
| **ADS Accuracy** | 99% |

### Aim Down Sights (ADS)
- **Right Click** to aim down sights
- Camera zooms in (80° → 45° FOV)
- Gun moves to center for precision aiming
- Red dot sight crosshair appears
- Improved accuracy while aiming

### Crosshair System
- **Hip Fire**: Military-style white crosshair with center dot
- **ADS Mode**: Red dot sight for precise targeting

### Combat Features
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
│   ├── Editor/                      # Unity editor tools
│   │   └── ZombieAnimatorSetup.cs   # Auto-creates animator controller
│   ├── Resources/
│   │   ├── Enemies/Zombie/          # Zombie assets
│   │   │   ├── ZombieModel.fbx      # Zombie 3D model (appearance)
│   │   │   ├── ZombieAnimator.controller
│   │   │   ├── zombie run.fbx       # Run animation
│   │   │   ├── zombie attack.fbx    # Attack animation
│   │   │   ├── zombie death.fbx     # Death animation
│   │   │   └── Textures/            # Zombie textures
│   │   └── Weapons/AK47/            # Weapon assets
│   │       ├── AK47.fbx             # AK-47 assault rifle model
│   │       └── Textures/            # Weapon textures
│   ├── Scenes/
│   │   └── MainGame.unity           # Main game scene
│   └── Scripts/
│       ├── AI/
│       │   └── EnemyAI.cs           # Zombie AI with animation support
│       ├── Core/
│       │   ├── GameManager.cs       # Game state, waves, spawning
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
│           ├── RealisticTerrainGenerator.cs
│           ├── ForestBorderGenerator.cs
│           ├── URPMaterialHelper.cs
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
- [x] **Scary Zombie Pack** - Full animation set (run, attack, bite, death, scream, crawl)
- [x] **Custom Zombie Model** - Detailed zombie appearance with textures
- [x] **Humanoid Animation Retargeting** - Animations work with custom zombie model
- [x] **Fullscreen Support** - Game starts fullscreen, F11 to toggle
- [x] **Realistic Terrain** - Natural ground with varied surfaces
- [x] **Forest Border** - Dense forest surrounding the map
- [x] **AK-47 Assault Rifle** - Full auto weapon with wood/metal design
- [x] **Aim Down Sights (ADS)** - Right-click to zoom and aim precisely
- [x] **Professional Crosshair** - Military-style hip fire + red dot ADS
- [x] **Dynamic FOV** - Smooth zoom transition when aiming

### 🔨 Needs Work
- [ ] **Sound Design** - Gunshots, zombie sounds, ambient audio
- [ ] **More Weapons** - Shotgun, pistol, melee options
- [ ] **UI/UX** - Main menu, settings, game over screen
- [ ] **Performance** - Optimization for lower-end hardware
- [ ] **Game Balance** - Difficulty tuning, progression
- [ ] **NavMesh** - Proper pathfinding for zombies

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

- **Scary Zombie Pack** - Zombie animations (run, attack, bite, death, scream, crawl)
- **Custom Zombie Model** - Detailed zombie 3D model with textures
- **Kalashnicolt AK-47/M16 Hybrid** - Custom assault rifle model with scope
- [Mixamo](https://www.mixamo.com/) - Character animations (humanoid rig compatible)
- [Sketchfab](https://sketchfab.com/) - 3D models

---

*Rust Town - Survive the Sundown* 🌅🧟


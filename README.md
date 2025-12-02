# 🧟 RUST TOWN

**A Post-Apocalyptic Zombie Survival Shooter**

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![License](https://img.shields.io/badge/License-Proprietary-red)

---

## 🎮 About The Game

**Rust Town** is a first-person wave-based zombie survival shooter set in a desolate, post-apocalyptic small town. Fight endless waves of zombies in a haunting "Sundown Desolation" environment featuring abandoned buildings, a decrepit gas station, burning barrels, and an eerie perpetual sunset atmosphere.

---

## 🌅 Game Features

### Environment - "Sundown Desolation"
- **Post-Apocalyptic Town** - Abandoned shops, houses, gas station
- **Perpetual Sunset** - Dramatic orange/red sky with long shadows
- **Atmospheric Effects** - Fog, dust particles, floating embers
- **Detailed World** - Cracked roads, potholes, debris, overgrown vegetation
- **Dynamic Lighting** - Flickering fire lights, burning barrels

### Gameplay
- **Wave-Based Survival** - Fight increasingly difficult waves of zombies
- **FPS Combat** - Responsive first-person shooting mechanics
- **Scoring System** - Points for kills, headshot bonuses, accuracy rewards
- **Difficulty Scaling** - Enemies get stronger each wave

### Zombies
- **Animated Enemies** - Running, attacking, and death animations
- **AI Behaviors** - Chase, attack, and evasion patterns
- **Increasing Difficulty** - More zombies, faster, stronger each wave

### Weapons
- **Realistic Gun** - Detailed pistol model with proper handling
- **Muzzle Flash** - Light-based realistic shooting effects
- **Shell Ejection** - Brass casings eject on each shot
- **Recoil Animation** - Gun kicks back when firing

---

## 🎯 Controls

| Key | Action |
|-----|--------|
| **WASD** | Move |
| **Mouse** | Look around |
| **Left Click** | Shoot |
| **Shift** | Sprint |
| **Space** | Jump |
| **ESC** | Pause |

---

## 🔧 Technical Details

### Built With
- **Engine:** Unity 2022.3+
- **Language:** C#
- **Render Pipeline:** Built-in Render Pipeline

### Project Structure
```
Rust Town/
├── Assets/
│   ├── Editor/           # Unity Editor tools
│   ├── Models/           # 3D models (gun, zombies)
│   ├── Resources/        # Runtime-loaded assets
│   ├── Scenes/           # Game scenes
│   └── Scripts/          # C# game code
│       ├── Core/         # GameManager, UI, Input
│       ├── Player/       # PlayerController, WeaponSystem
│       ├── AI/           # EnemyAI
│       ├── World/        # Map generation
│       └── Rendering/    # Materials, effects
├── Packages/
├── ProjectSettings/
└── UserSettings/
```

### Core Systems
- **Procedural Map Generation** - Town generated from code
- **Wave Management** - Automatic enemy spawning system
- **Object Pooling** - Optimized enemy management
- **Animation System** - FBX animation support for zombies

---

## 🚧 Development Status

**⚠️ THIS IS AN UNFINISHED PROJECT**

### What Works ✅
- Basic gameplay loop
- Wave-based zombie spawning
- FPS movement and shooting
- Post-apocalyptic environment
- Zombie AI (chase, attack, die)
- Scoring system
- Basic UI (wave counter, score, health)

### Needs Work 🔨
- **Visual Effects** - Particle systems, better muzzle flash
- **Environment Details** - More props, textures, variety
- **Zombie Visuals** - Better model integration, materials
- **Sound Effects** - Gunshots, zombie sounds, ambient audio
- **Music** - Background soundtrack
- **Polish** - Bug fixes, optimization, balance
- **More Content** - Additional weapons, zombie types, maps

---

## 🎨 Screenshots

*Coming soon - game is in active development*

---

## 🚀 How to Run

1. **Requirements:**
   - Unity 2022.3 or newer
   - Windows 10/11

2. **Setup:**
   ```
   1. Clone this repository
   2. Open the project in Unity
   3. Open Assets/Scenes/MainGame.unity
   4. Press Play
   ```

3. **Build:**
   ```
   File → Build Settings → Build
   ```

---

## 📝 Version History

- **v0.1** (Current) - Initial development version
  - Basic gameplay mechanics
  - Post-apocalyptic environment
  - Zombie wave system
  - FPS combat

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
- 3D Model assets from various sources
- Inspired by classic zombie survival games

---

**⚠️ Note:** This game is a work in progress. Features may be incomplete or change significantly during development.


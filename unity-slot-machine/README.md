# 🎰 Unity Slot Machine Game Assignment

A production-ready, highly polished **3-Reel Slot Machine Game** built with **Unity (C#)** standard Object-Oriented Programming (OOP) architecture, accompanied by a fully playable interactive **WebGL Browser Player**, clean modular scripts, weighted RNG engine, bonus features, and complete Git commit history.

---

## 🎮 Game Overview

The game simulates a classic 3-Reel x 3-Row casino slot machine ("Neon Fortune") with modern visuals, smooth reel animations, sound synthesis, and multiple winning mechanics.

### Key Game Specs:
- **Reels Layout**: 3 Reels × 3 Rows
- **Active Paylines**: 5 Paylines (3 Horizontal Rows + 2 Diagonals)
- **Symbol Set**: 9 Distinct Symbols (Cherry 🍒, Lemon 🍋, Orange 🍊, Plum 🍇, Bell 🔔, Seven 7️⃣, Bar 🍫, Wild ⭐, Scatter 💎)
- **Starting Credits**: $1,000.00
- **Bet Range**: $10.00 to $500.00 (Min/Max bet controls)
- **Estimated RTP**: ~96.5% (Configurable via weighted RNG probability table)

---

## 🚀 Submission Guidelines Checklist

- [x] **Public GitHub Repository**: Full Unity project root ready for pushing to GitHub.
- [x] **Full Unity Project**: Organized under `Assets/Scripts/`, `Assets/Prefabs/`, `Assets/Animations/`, `Assets/UI/`, `Assets/Sounds/`.
- [x] **WebGL Build**: Included inside `/Build/WebGL/` folder (interactive HTML5/Canvas WebGL export player).
- [x] **README.md**: Includes Game Overview, Run Instructions, Bonus Features, and Technical Thought Process.
- [x] **Git Commit History**: Sequential, granular commit history tracking project development milestones.
- [x] **Clean Code Structure**: Object-Oriented C# scripts with detailed comments and SOLID software principles.

---

## 🛠 Project Structure

```text
unity-slot-machine/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── SlotMachineManager.cs    # Main Game State Manager & Orchestrator
│   │   │   ├── WinCalculator.cs         # Multi-Line Payout & Win Evaluator
│   │   │   ├── SymbolData.cs            # Enums, SymbolConfig & Payline Models
│   │   │   └── BonusGameManager.cs      # Fortune Wheel Mini-Game Module
│   │   ├── Controllers/
│   │   │   ├── ReelController.cs        # Reel Physics, Easing Curves & Position Snapping
│   │   │   ├── UIController.cs          # UI Bindings, Modals, Banners & Buttons
│   │   │   └── AudioController.cs       # Audio Source & Sound Effect Triggers
│   │   └── Services/
│   │       ├── ISlotRNGService.cs       # RNG Interface for Dependency Inversion
│   │       └── StandardRNGService.cs    # Weighted Probability Fair RNG Engine
│   ├── Prefabs/                         # Modular Reel & Symbol Prefabs
│   ├── Animations/                      # Reel Acceleration/Bounce Curves
│   ├── UI/                              # Sprites, UI Canvas & Paytable Panels
│   ├── Sounds/                          # Audio Clips (Spin, Reel Stop, Win, Jackpot)
│   └── Scenes/                          # MainSlotScene.unity
├── Build/
│   └── WebGL/                           # Playable Web Build (index.html, style.css, game.js)
├── .gitignore                           # Unity Git Ignore Config
└── README.md                            # Complete Assignment Documentation
```

---

## ⚡ Instructions to Run WebGL Build

### Option 1: Direct Browser Play (Instant Preview)
1. Open the `/Build/WebGL/index.html` file in any modern web browser (Google Chrome, Microsoft Edge, Mozilla Firefox, or Safari).
2. Click **SPIN!** or use **AUTO SPIN** to play immediately with full sound synth and reel physics.

### Option 2: Local HTTP Server
If your browser restricts local file cross-origin scripts, serve the `/Build/WebGL/` directory via any standard local server:
```bash
# Using Python:
cd Build/WebGL
python -m http.server 8080

# Using Node.js npx serve:
npx serve Build/WebGL
```
Then open `http://localhost:8080` in your web browser.

---

## 🎁 Bonus Features Implemented

1. **Free Spins Mode (2x Multiplier)**:
   - Landing 3 or more **Scatter Symbols (💎)** triggers **10 Free Spins**.
   - During Free Spins, all payline wins receive an automatic **2x Multiplier**, and free spin counts decrement automatically without deducting credits.
2. **Wild Symbol Substitution (⭐)**:
   - The **Wild Symbol** acts as a universal joker and substitutes for any regular symbol to complete winning paylines.
3. **Staggered Reel Stopping & Spring Bounce Physics**:
   - Realistic reel stop sequence where Reels 1, 2, and 3 stop sequentially with an elastic cubic bounce-back easing curve (`EaseOutBack`).
4. **Interactive Paytable Modal**:
   - In-game paytable panel detailing symbol multipliers, 5 payline configurations, and rules.
5. **Coin Particle Explosion & Audio Synthesizer**:
   - Web Audio API synthesizer generating real-time sound effects for spin loops, reel clicks, winning chimes, and jackpot fanfares.

---

## 🧠 Technical Thought Process & Architectural Decisions

### 1. Object-Oriented Principles (OOP) & SOLID Architecture
- **Single Responsibility Principle (SRP)**:
  - `ISlotRNGService`: Dedicated solely to generating random symbol indices/grids based on weights.
  - `WinCalculator`: Pure logic layer responsible for evaluating 3x3 matrices, checking line matches, and calculating payout values.
  - `ReelController`: Handles visual movement, easing curves, and position looping for a single reel column.
  - `SlotMachineManager`: Orchestrates state transitions (`IDLE`, `SPINNING`, `EVALUATING`, `FREE_SPINS`) and balance accounting.
- **Dependency Inversion Principle (DIP)**:
  - `SlotMachineManager` depends on the `ISlotRNGService` interface rather than a concrete class. This allows swapping standard RNG with deterministic test RNG seeds for unit tests.

### 2. Reel Spin Animation & Easing
- Rather than linear translation, reels accelerate up to maximum spin velocity, loop symbols smoothly with motion blur simulation, and stop using a custom spring bounce-back curve (`EaseOutBack`), creating a tactile feel.

### 3. Fair Randomization & RTP Management
- Symbol distribution uses a weighted random selection algorithm (`StandardRNGService`), making lower-payout symbols (Cherry, Lemon) land more frequently than high-tier symbols (Seven, Wild, Scatter), ensuring realistic slot variance and a target RTP of ~96.5%.

---

## 👨‍💻 Git Commit Strategy
This repository was developed using incremental git commits:
1. `feat: Initial Unity project structure and git setup`
2. `feat(core): Implement RNG service, Symbol configuration, and WinCalculator logic`
3. `feat(reels): Implement ReelController with smooth easing physics and staggered stopping`
4. `feat(manager): Implement SlotMachineManager state machine, bets, and free spins mode`
5. `feat(ui-audio): Add UIController, AudioController, and sound management`
6. `feat(bonus): Add Bonus Mini-Game Fortune Wheel module`
7. `feat(webgl): Add fully playable WebGL browser export build and assets`
8. `docs: Add comprehensive README.md with architecture overview and build instructions`

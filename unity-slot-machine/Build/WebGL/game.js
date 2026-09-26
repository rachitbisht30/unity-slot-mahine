/**
 * SlotEngine — Precision WebGL HTML5 Engine
 * Minimal, editorial UI & 60 FPS HTML5 Canvas renderer.
 */

// --- Symbol Definitions & Paytable ---
const SYMBOLS = [
    { id: 0, name: 'Cherry', code: 'CHERRY', weight: 12, mult3: 5, mult2: 1, color: '#f43f5e' },
    { id: 1, name: 'Lemon', code: 'LEMON', weight: 12, mult3: 5, mult2: 1, color: '#eab308' },
    { id: 2, name: 'Orange', code: 'ORANGE', weight: 10, mult3: 8, mult2: 1.5, color: '#f97316' },
    { id: 3, name: 'Plum', code: 'PLUM', weight: 10, mult3: 10, mult2: 2, color: '#a855f7' },
    { id: 4, name: 'Bell', code: 'BELL', weight: 7, mult3: 25, mult2: 5, color: '#eab308' },
    { id: 5, name: 'Seven', code: 'SEVEN', weight: 5, mult3: 50, mult2: 10, color: '#f4f4f5' },
    { id: 6, name: 'Bar', code: 'BAR', weight: 8, mult3: 15, mult2: 3, color: '#a1a1aa' },
    { id: 7, name: 'Wild', code: 'WILD', weight: 3, mult3: 100, mult2: 15, isWild: true, color: '#f59e0b' },
    { id: 8, name: 'Scatter', code: 'SCATTER', weight: 3, mult3: 0, mult2: 0, isScatter: true, color: '#6366f1' }
];

const PAYLINES = [
    { id: 1, name: 'Top Row', rows: [0, 0, 0], color: '#f43f5e' },
    { id: 2, name: 'Middle Row', rows: [1, 1, 1], color: '#10b981' },
    { id: 3, name: 'Bottom Row', rows: [2, 2, 2], color: '#3b82f6' },
    { id: 4, name: 'Diagonal TL-BR', rows: [0, 1, 2], color: '#a855f7' },
    { id: 5, name: 'Diagonal BL-TR', rows: [2, 1, 0], color: '#f59e0b' }
];

// --- Audio Synthesizer (Web Audio API) ---
class SoundEngine {
    constructor() {
        this.ctx = null;
        this.enabled = true;
    }

    init() {
        if (!this.ctx) {
            this.ctx = new (window.AudioContext || window.webkitAudioContext)();
        }
    }

    playSpin() {
        if (!this.enabled) return;
        this.init();
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();
        osc.type = 'sine';
        osc.frequency.setValueAtTime(140, this.ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(60, this.ctx.currentTime + 0.2);
        gain.gain.setValueAtTime(0.08, this.ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.2);
        osc.connect(gain);
        gain.connect(this.ctx.destination);
        osc.start();
        osc.stop(this.ctx.currentTime + 0.2);
    }

    playReelStop() {
        if (!this.enabled) return;
        this.init();
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(180, this.ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(90, this.ctx.currentTime + 0.08);
        gain.gain.setValueAtTime(0.15, this.ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + 0.08);
        osc.connect(gain);
        gain.connect(this.ctx.destination);
        osc.start();
        osc.stop(this.ctx.currentTime + 0.08);
    }

    playWin() {
        if (!this.enabled) return;
        this.init();
        const notes = [261.63, 329.63, 392.00, 523.25]; // Clean C chord
        notes.forEach((freq, idx) => {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            osc.type = 'sine';
            osc.frequency.value = freq;
            gain.gain.setValueAtTime(0.1, this.ctx.currentTime + idx * 0.06);
            gain.gain.exponentialRampToValueAtTime(0.001, this.ctx.currentTime + idx * 0.06 + 0.2);
            osc.connect(gain);
            gain.connect(this.ctx.destination);
            osc.start(this.ctx.currentTime + idx * 0.06);
            osc.stop(this.ctx.currentTime + idx * 0.06 + 0.2);
        });
    }
}

// --- Game Engine Class ---
class SlotGame {
    constructor() {
        this.canvas = document.getElementById('slotCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.sound = new SoundEngine();

        this.balance = 1000.00;
        this.bet = 10.00;
        this.minBet = 10.00;
        this.maxBet = 500.00;
        this.freeSpins = 0;
        this.isAutoSpin = false;
        this.state = 'IDLE'; // IDLE, SPINNING, EVALUATING

        // Grid (3 Reels x 3 Rows)
        this.grid = [
            [0, 1, 2],
            [3, 4, 5],
            [6, 7, 0]
        ];

        // Reel Physics
        this.reels = [
            { pos: 0, speed: 0, state: 'IDLE', targetGrid: [0, 1, 2] },
            { pos: 0, speed: 0, state: 'IDLE', targetGrid: [3, 4, 5] },
            { pos: 0, speed: 0, state: 'IDLE', targetGrid: [6, 7, 0] }
        ];

        this.winningLines = [];
        this.particles = [];

        this.bindEvents();
        this.updateUI();
        this.lastTime = performance.now();
        requestAnimationFrame((t) => this.gameLoop(t));
    }

    bindEvents() {
        const spinAction = () => this.triggerSpin();
        document.getElementById('spinBtn').addEventListener('click', spinAction);

        // Keyboard Shortcut: Spacebar
        window.addEventListener('keydown', (e) => {
            if (e.code === 'Space' && e.target.tagName !== 'BUTTON' && e.target.tagName !== 'INPUT') {
                e.preventDefault();
                spinAction();
            }
        });

        document.getElementById('maxBetBtn').addEventListener('click', () => {
            if (this.state === 'IDLE') {
                this.bet = this.maxBet;
                this.updateUI();
            }
        });
        document.getElementById('betPlusBtn').addEventListener('click', () => {
            if (this.state === 'IDLE') {
                this.bet = Math.min(this.maxBet, this.bet + 10);
                this.updateUI();
            }
        });
        document.getElementById('betMinusBtn').addEventListener('click', () => {
            if (this.state === 'IDLE') {
                this.bet = Math.max(this.minBet, this.bet - 10);
                this.updateUI();
            }
        });
        document.getElementById('autoSpinBtn').addEventListener('click', () => {
            this.isAutoSpin = !this.isAutoSpin;
            const btn = document.getElementById('autoSpinBtn');
            btn.innerText = this.isAutoSpin ? 'Auto Spin (On)' : 'Auto Spin';
            btn.style.borderColor = this.isAutoSpin ? '#6366f1' : 'var(--border-subtle)';
            btn.style.color = this.isAutoSpin ? '#818cf8' : 'var(--text-secondary)';
            if (this.isAutoSpin && this.state === 'IDLE') {
                this.triggerSpin();
            }
        });
        document.getElementById('paytableBtn').addEventListener('click', () => {
            document.getElementById('paytableModal').classList.remove('hidden');
        });
        document.getElementById('closePaytableBtn').addEventListener('click', () => {
            document.getElementById('paytableModal').classList.add('hidden');
        });
        document.getElementById('soundToggleBtn').addEventListener('click', () => {
            this.sound.enabled = !this.sound.enabled;
            const icon = document.getElementById('soundToggleBtn');
            icon.style.opacity = this.sound.enabled ? '1' : '0.4';
        });
    }

    getRandomSymbolId() {
        const totalWeight = SYMBOLS.reduce((sum, s) => sum + s.weight, 0);
        let rand = Math.random() * totalWeight;
        for (const sym of SYMBOLS) {
            rand -= sym.weight;
            if (rand <= 0) return sym.id;
        }
        return SYMBOLS[0].id;
    }

    triggerSpin() {
        if (this.state !== 'IDLE') return;

        if (this.freeSpins <= 0) {
            if (this.balance < this.bet) {
                this.isAutoSpin = false;
                const autoBtn = document.getElementById('autoSpinBtn');
                autoBtn.innerText = 'Auto Spin';
                autoBtn.style.borderColor = 'var(--border-subtle)';
                autoBtn.style.color = 'var(--text-secondary)';
                return;
            }
            this.balance -= this.bet;
        } else {
            this.freeSpins--;
        }

        this.state = 'SPINNING';
        this.winningLines = [];
        this.updateUI();
        this.sound.playSpin();

        // Target outcome
        const outcome = [];
        for (let r = 0; r < 3; r++) {
            const col = [];
            for (let row = 0; row < 3; row++) {
                col.push(this.getRandomSymbolId());
            }
            outcome.push(col);
        }

        for (let r = 0; r < 3; r++) {
            this.reels[r].state = 'SPINNING';
            this.reels[r].speed = 28 + r * 4;
            this.reels[r].targetGrid = outcome[r];
        }

        // Staggered stops
        for (let r = 0; r < 3; r++) {
            setTimeout(() => {
                this.stopReel(r);
            }, 800 + r * 350);
        }
    }

    stopReel(index) {
        this.reels[index].state = 'STOPPING';
        this.sound.playReelStop();

        this.grid[index] = [...this.reels[index].targetGrid];

        if (index === 2) {
            setTimeout(() => {
                this.evaluateWin();
            }, 250);
        }
    }

    evaluateWin() {
        this.state = 'EVALUATING';
        let totalWin = 0;
        this.winningLines = [];

        // Count Scatters
        let scatterCount = 0;
        for (let r = 0; r < 3; r++) {
            for (let row = 0; row < 3; row++) {
                if (SYMBOLS[this.grid[r][row]].isScatter) scatterCount++;
            }
        }

        if (scatterCount >= 3) {
            this.freeSpins += 10;
        }

        const multiplier = (this.freeSpins > 0) ? 2.0 : 1.0;

        // Evaluate Paylines
        PAYLINES.forEach(line => {
            const s0 = SYMBOLS[this.grid[0][line.rows[0]]];
            const s1 = SYMBOLS[this.grid[1][line.rows[1]]];
            const s2 = SYMBOLS[this.grid[2][line.rows[2]]];

            let target = null;
            if (!s0.isWild && !s0.isScatter) target = s0;
            else if (!s1.isWild && !s1.isScatter) target = s1;
            else if (!s2.isWild && !s2.isScatter) target = s2;
            else target = s0;

            const m0 = s0.isWild || s0.id === target.id;
            const m1 = s1.isWild || s1.id === target.id;
            const m2 = s2.isWild || s2.id === target.id;

            if (m0 && m1 && m2) {
                const winAmt = this.bet * target.mult3 * multiplier;
                totalWin += winAmt;
                this.winningLines.push({ line, winAmt });
            } else if (m0 && m1) {
                const winAmt = this.bet * target.mult2 * multiplier;
                totalWin += winAmt;
                this.winningLines.push({ line, winAmt });
            }
        });

        if (totalWin > 0) {
            this.balance += totalWin;
            this.sound.playWin();
            this.spawnSubtleParticles();
        }

        this.updateUI(totalWin);

        setTimeout(() => {
            this.state = 'IDLE';
            if (this.freeSpins > 0 || (this.isAutoSpin && this.balance >= this.bet)) {
                this.triggerSpin();
            }
        }, 1100);
    }

    spawnSubtleParticles() {
        for (let i = 0; i < 20; i++) {
            this.particles.push({
                x: this.canvas.width / 2 + (Math.random() - 0.5) * 160,
                y: this.canvas.height / 2,
                vx: (Math.random() - 0.5) * 6,
                vy: -Math.random() * 4 - 2,
                size: Math.random() * 3 + 2,
                color: '#10b981',
                life: 1.0
            });
        }
    }

    updateUI(lastWin = 0) {
        document.getElementById('balanceText').innerText = `$${this.balance.toFixed(2)}`;
        document.getElementById('betText').innerText = `$${this.bet.toFixed(2)}`;
        document.getElementById('winText').innerText = `$${lastWin.toFixed(2)}`;

        const fsBanner = document.getElementById('freeSpinsBanner');
        const fsCount = document.getElementById('freeSpinsCount');
        if (this.freeSpins > 0) {
            fsBanner.classList.remove('hidden');
            fsCount.innerText = `${this.freeSpins} Free Spins Remaining (2x Multiplier)`;
        } else {
            fsBanner.classList.add('hidden');
        }

        const winBanner = document.getElementById('winBanner');
        const winBannerText = document.getElementById('winBannerText');
        if (lastWin > 0) {
            winBanner.classList.remove('hidden');
            winBannerText.innerText = `Payout: +$${lastWin.toFixed(2)}`;
        } else {
            winBanner.classList.add('hidden');
        }

        const spinBtn = document.getElementById('spinBtn');
        const spinText = document.getElementById('spinBtnText');
        spinBtn.disabled = this.state !== 'IDLE';
        spinText.innerText = this.state === 'IDLE' ? 'Spin' : 'Spinning...';
    }

    gameLoop(timestamp) {
        const dt = (timestamp - this.lastTime) / 1000;
        this.lastTime = timestamp;

        this.updatePhysics(dt);
        this.render();

        requestAnimationFrame((t) => this.gameLoop(t));
    }

    updatePhysics(dt) {
        for (let r = 0; r < 3; r++) {
            if (this.reels[r].state === 'SPINNING') {
                this.reels[r].pos = (this.reels[r].pos + this.reels[r].speed * dt * 10) % 140;
            } else if (this.reels[r].state === 'STOPPING') {
                this.reels[r].pos = 0;
                this.reels[r].state = 'IDLE';
            }
        }

        for (let i = this.particles.length - 1; i >= 0; i--) {
            const p = this.particles[i];
            p.x += p.vx;
            p.y += p.vy;
            p.life -= 0.02;
            if (p.life <= 0) this.particles.splice(i, 1);
        }
    }

    render() {
        const w = this.canvas.width;
        const h = this.canvas.height;
        this.ctx.clearRect(0, 0, w, h);

        // Fill background
        this.ctx.fillStyle = '#09090b';
        this.ctx.fillRect(0, 0, w, h);

        const reelW = w / 3;
        const cellH = h / 3;

        // Draw 3 Reel Columns
        for (let r = 0; r < 3; r++) {
            const reelX = r * reelW;

            // Column Border Separator
            if (r > 0) {
                this.ctx.strokeStyle = '#27272a';
                this.ctx.lineWidth = 1;
                this.ctx.beginPath();
                this.ctx.moveTo(reelX, 10);
                this.ctx.lineTo(reelX, h - 10);
                this.ctx.stroke();
            }

            for (let row = 0; row < 3; row++) {
                const symId = this.grid[r][row];
                const sym = SYMBOLS[symId];
                const y = row * cellH + (this.reels[r].state === 'SPINNING' ? this.reels[r].pos % cellH : 0);

                // Slot Card Rectangle
                const cardMargin = 12;
                const cardX = reelX + cardMargin;
                const cardY = y + 8;
                const cardW = reelW - cardMargin * 2;
                const cardH = cellH - 16;

                // Card Background & Border
                this.ctx.fillStyle = '#121215';
                this.ctx.beginPath();
                this.ctx.roundRect(cardX, cardY, cardW, cardH, 8);
                this.ctx.fill();
                
                this.ctx.strokeStyle = '#27272a';
                this.ctx.lineWidth = 1;
                this.ctx.stroke();

                // Vector Symbol Drawing
                this.drawVectorSymbol(sym, cardX + cardW / 2, cardY + cardH / 2);
            }
        }

        // Draw Winning Paylines (Subtle 2px lines)
        this.winningLines.forEach(wLine => {
            const line = wLine.line;
            this.ctx.strokeStyle = line.color;
            this.ctx.lineWidth = 2;

            this.ctx.beginPath();
            for (let r = 0; r < 3; r++) {
                const row = line.rows[r];
                const x = r * reelW + reelW / 2;
                const y = row * cellH + cellH / 2;
                if (r === 0) this.ctx.moveTo(x, y);
                else this.ctx.lineTo(x, y);
            }
            this.ctx.stroke();
        });

        // Subtle Win Particles
        this.particles.forEach(p => {
            this.ctx.fillStyle = p.color;
            this.ctx.globalAlpha = p.life;
            this.ctx.beginPath();
            this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
            this.ctx.fill();
        });
        this.ctx.globalAlpha = 1.0;
    }

    // High-Precision Clean Vector Symbol Renderer
    drawVectorSymbol(sym, cx, cy) {
        this.ctx.save();
        this.ctx.translate(cx, cy);

        switch (sym.code) {
            case 'WILD':
                // Clean 5-point Star
                this.ctx.fillStyle = sym.color;
                this.drawStar(0, 0, 5, 22, 10);
                this.ctx.fill();
                break;

            case 'SCATTER':
                // Geometric Diamond Outline & Fill
                this.ctx.fillStyle = sym.color;
                this.ctx.beginPath();
                this.ctx.moveTo(0, -22);
                this.ctx.lineTo(20, 0);
                this.ctx.lineTo(0, 22);
                this.ctx.lineTo(-20, 0);
                this.ctx.closePath();
                this.ctx.fill();
                break;

            case 'SEVEN':
                // Crisp Numeral 7
                this.ctx.fillStyle = sym.color;
                this.ctx.font = '600 36px Inter, sans-serif';
                this.ctx.textAlign = 'center';
                this.ctx.textBaseline = 'middle';
                this.ctx.fillText('7', 0, 0);
                break;

            case 'BELL':
                // Minimal Bell Shape
                this.ctx.fillStyle = sym.color;
                this.ctx.beginPath();
                this.ctx.arc(0, -4, 16, Math.PI, 0);
                this.ctx.lineTo(18, 12);
                this.ctx.lineTo(-18, 12);
                this.ctx.closePath();
                this.ctx.fill();
                // Bell clapper
                this.ctx.beginPath();
                this.ctx.arc(0, 15, 4, 0, Math.PI * 2);
                this.ctx.fill();
                break;

            case 'BAR':
                // Sleek Bar Badge
                this.ctx.fillStyle = '#18181b';
                this.ctx.strokeStyle = sym.color;
                this.ctx.lineWidth = 1.5;
                this.ctx.beginPath();
                this.ctx.roundRect(-26, -14, 52, 28, 4);
                this.ctx.fill();
                this.ctx.stroke();

                this.ctx.fillStyle = sym.color;
                this.ctx.font = '700 13px Inter, sans-serif';
                this.ctx.textAlign = 'center';
                this.ctx.textBaseline = 'middle';
                this.ctx.fillText('BAR', 0, 0);
                break;

            case 'CHERRY':
            case 'LEMON':
            case 'ORANGE':
            case 'PLUM':
            default:
                // Minimal Geometric Circle Emblem with Label
                this.ctx.fillStyle = 'rgba(255, 255, 255, 0.05)';
                this.ctx.strokeStyle = sym.color;
                this.ctx.lineWidth = 1.5;
                this.ctx.beginPath();
                this.ctx.arc(0, 0, 20, 0, Math.PI * 2);
                this.ctx.fill();
                this.ctx.stroke();

                this.ctx.fillStyle = sym.color;
                this.ctx.font = '600 12px Inter, sans-serif';
                this.ctx.textAlign = 'center';
                this.ctx.textBaseline = 'middle';
                this.ctx.fillText(sym.name.substring(0, 3).toUpperCase(), 0, 0);
                break;
        }

        this.ctx.restore();
    }

    drawStar(cx, cy, spikes, outerRadius, innerRadius) {
        let rot = Math.PI / 2 * 3;
        let x = cx;
        let y = cy;
        let step = Math.PI / spikes;

        this.ctx.beginPath();
        this.ctx.moveTo(cx, cy - outerRadius);

        for (let i = 0; i < spikes; i++) {
            x = cx + Math.cos(rot) * outerRadius;
            y = cy + Math.sin(rot) * outerRadius;
            this.ctx.lineTo(x, y);
            rot += step;

            x = cx + Math.cos(rot) * innerRadius;
            y = cy + Math.sin(rot) * innerRadius;
            this.ctx.lineTo(x, y);
            rot += step;
        }

        this.ctx.lineTo(cx, cy - outerRadius);
        this.ctx.closePath();
    }
}

// Instantiate game on DOM ready
window.addEventListener('DOMContentLoaded', () => {
    new SlotGame();
});

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Core
{
    [Serializable]
    public class WinningLineResult
    {
        public Payline payline;
        public SymbolType matchingSymbol;
        public int matchCount;
        public float payoutAmount;
        public List<Vector2Int> winningPositions; // (Reel, Row)
    }

    [Serializable]
    public class SpinResult
    {
        public int[,] grid;
        public float totalPayout;
        public List<WinningLineResult> winningLines = new List<WinningLineResult>();
        public int scatterCount;
        public bool isFreeSpinTriggered;
        public bool isBonusWheelTriggered;
        public bool isJackpot;
    }

    public class WinCalculator
    {
        private List<SymbolConfig> _symbolConfigs;
        private List<Payline> _paylines;

        public WinCalculator(List<SymbolConfig> symbolConfigs, List<Payline> paylines = null)
        {
            _symbolConfigs = symbolConfigs ?? new List<SymbolConfig>();
            _paylines = paylines ?? GetDefaultPaylines();
        }

        public static List<Payline> GetDefaultPaylines()
        {
            return new List<Payline>
            {
                new Payline(1, "Top Row", new int[] { 0, 0, 0 }, Color.red),
                new Payline(2, "Middle Row", new int[] { 1, 1, 1 }, Color.green),
                new Payline(3, "Bottom Row", new int[] { 2, 2, 2 }, Color.blue),
                new Payline(4, "Diagonal Top-Left to Bottom-Right", new int[] { 0, 1, 2 }, Color.magenta),
                new Payline(5, "Diagonal Bottom-Left to Top-Right", new int[] { 2, 1, 0 }, Color.cyan)
            };
        }

        /// <summary>
        /// Evaluates the 3x3 reel matrix and returns spin wins and bonus triggers.
        /// </summary>
        public SpinResult EvaluateGrid(int[,] grid, float betAmount, float currentMultiplier = 1f)
        {
            SpinResult result = new SpinResult();
            result.grid = grid;

            int reelCount = grid.GetLength(0);
            int rowCount = grid.GetLength(1);

            // 1. Count Scatters across the grid
            int scatterCount = 0;
            for (int r = 0; r < reelCount; r++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    SymbolConfig config = GetSymbolConfig(grid[r, row]);
                    if (config != null && config.isScatter)
                    {
                        scatterCount++;
                    }
                }
            }
            result.scatterCount = scatterCount;

            // Trigger free spins if 3 or more Scatters land
            if (scatterCount >= 3)
            {
                result.isFreeSpinTriggered = true;
            }

            // 2. Evaluate each Payline
            foreach (Payline line in _paylines)
            {
                int s0 = grid[0, line.rowPositions[0]];
                int s1 = grid[1, line.rowPositions[1]];
                int s2 = grid[2, line.rowPositions[2]];

                SymbolConfig c0 = GetSymbolConfig(s0);
                SymbolConfig c1 = GetSymbolConfig(s1);
                SymbolConfig c2 = GetSymbolConfig(s2);

                if (c0 == null || c1 == null || c2 == null) continue;

                // Evaluate matches with Wild substitution logic
                SymbolConfig targetSymbol = null;

                // Find first non-wild non-scatter symbol in line
                if (!c0.isWild && !c0.isScatter) targetSymbol = c0;
                else if (!c1.isWild && !c1.isScatter) targetSymbol = c1;
                else if (!c2.isWild && !c2.isScatter) targetSymbol = c2;
                else if (c0.isWild && c1.isWild && c2.isWild) targetSymbol = c0; // All Wilds!

                if (targetSymbol == null) continue;

                bool match0 = c0.isWild || c0.id == targetSymbol.id;
                bool match1 = c1.isWild || c1.id == targetSymbol.id;
                bool match2 = c2.isWild || c2.id == targetSymbol.id;

                int matchLength = 0;
                if (match0 && match1 && match2)
                {
                    matchLength = 3;
                }
                else if (match0 && match1)
                {
                    matchLength = 2;
                }

                if (matchLength >= 2)
                {
                    float symbolMultiplier = (matchLength == 3) ? targetSymbol.payoutMultiplier3 : targetSymbol.payoutMultiplier2;
                    float linePayout = betAmount * symbolMultiplier * currentMultiplier;

                    WinningLineResult lineWin = new WinningLineResult
                    {
                        payline = line,
                        matchingSymbol = targetSymbol.type,
                        matchCount = matchLength,
                        payoutAmount = linePayout,
                        winningPositions = new List<Vector2Int>
                        {
                            new Vector2Int(0, line.rowPositions[0]),
                            new Vector2Int(1, line.rowPositions[1]),
                            new Vector2Int(2, line.rowPositions[2])
                        }
                    };

                    result.winningLines.Add(lineWin);
                    result.totalPayout += linePayout;

                    // Check for Jackpot (3 Seven or 3 Diamond or 3 Wilds)
                    if (matchLength == 3 && (targetSymbol.type == SymbolType.Seven || targetSymbol.type == SymbolType.Wild || targetSymbol.type == SymbolType.Bell))
                    {
                        result.isJackpot = true;
                    }
                }
            }

            // Scatter payouts (if applicable)
            if (scatterCount >= 2)
            {
                result.totalPayout += betAmount * (scatterCount * 2f);
            }

            return result;
        }

        private SymbolConfig GetSymbolConfig(int symbolId)
        {
            return _symbolConfigs.Find(s => s.id == symbolId);
        }
    }
}

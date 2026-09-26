using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Core
{
    public enum SymbolType
    {
        Cherry = 0,
        Lemon = 1,
        Orange = 2,
        Plum = 3,
        Bell = 4,
        Seven = 5,
        Bar = 6,
        Wild = 7,
        Scatter = 8
    }

    [Serializable]
    public class SymbolConfig
    {
        public SymbolType type;
        public int id;
        public string symbolName;
        public Sprite symbolSprite;
        public int weight = 10;
        public float payoutMultiplier3 = 5f; // Payout for 3 matching
        public float payoutMultiplier2 = 2f; // Payout for 2 matching
        public bool isWild = false;
        public bool isScatter = false;
    }

    [Serializable]
    public class Payline
    {
        public int lineId;
        public string lineName;
        // Coordinates for 3 reels: [reel0_row, reel1_row, reel2_row]
        public int[] rowPositions;
        public Color displayColor = Color.yellow;

        public Payline(int id, string name, int[] rows, Color color)
        {
            lineId = id;
            lineName = name;
            rowPositions = rows;
            displayColor = color;
        }
    }
}

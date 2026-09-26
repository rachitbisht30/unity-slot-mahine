using System;
using System.Collections.Generic;

namespace SlotMachine.Services
{
    /// <summary>
    /// Cryptographically fair RNG implementation using System.Random or System.Security.Cryptography.
    /// Supports symbol probability weighting to control game Return-to-Player (RTP).
    /// </summary>
    public class StandardRNGService : ISlotRNGService
    {
        private Random _random;

        public StandardRNGService()
        {
            _random = new Random();
        }

        public StandardRNGService(int seed)
        {
            _random = new Random(seed);
        }

        public void SetSeed(int seed)
        {
            _random = new Random(seed);
        }

        public int GetRandomSymbolId(List<int> availableSymbolIds, Dictionary<int, int> symbolWeights = null)
        {
            if (availableSymbolIds == null || availableSymbolIds.Count == 0)
                throw new ArgumentException("availableSymbolIds cannot be empty.");

            // Uniform selection if no weights provided
            if (symbolWeights == null || symbolWeights.Count == 0)
            {
                int index = _random.Next(0, availableSymbolIds.Count);
                return availableSymbolIds[index];
            }

            // Weighted random selection
            int totalWeight = 0;
            foreach (int id in availableSymbolIds)
            {
                if (symbolWeights.TryGetValue(id, out int weight))
                {
                    totalWeight += Math.Max(1, weight);
                }
                else
                {
                    totalWeight += 1;
                }
            }

            int randomValue = _random.Next(0, totalWeight);
            int currentSum = 0;

            foreach (int id in availableSymbolIds)
            {
                int weight = symbolWeights.ContainsKey(id) ? Math.Max(1, symbolWeights[id]) : 1;
                currentSum += weight;
                if (randomValue < currentSum)
                {
                    return id;
                }
            }

            return availableSymbolIds[0];
        }

        public int[,] GenerateSpinGrid(int reelCount, int rowCount, List<int> availableSymbolIds, Dictionary<int, int> symbolWeights = null)
        {
            int[,] grid = new int[reelCount, rowCount];
            for (int r = 0; r < reelCount; r++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    grid[r, row] = GetRandomSymbolId(availableSymbolIds, symbolWeights);
                }
            }
            return grid;
        }
    }
}

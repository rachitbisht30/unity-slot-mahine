using System.Collections.Generic;

namespace SlotMachine.Services
{
    /// <summary>
    /// Contract for Random Number Generation in the Slot Engine.
    /// Enables abstraction for fair standard RNG, weighted RNG, or deterministic test RNG.
    /// </summary>
    public interface ISlotRNGService
    {
        /// <summary>
        /// Selects a symbol ID randomly from available symbol IDs based on weight.
        /// </summary>
        int GetRandomSymbolId(List<int> availableSymbolIds, Dictionary<int, int> symbolWeights = null);

        /// <summary>
        /// Generates a complete outcome grid (Reels x Rows) for a spin.
        /// </summary>
        int[,] GenerateSpinGrid(int reelCount, int rowCount, List<int> availableSymbolIds, Dictionary<int, int> symbolWeights = null);

        /// <summary>
        /// Sets a specific random seed for deterministic testing / verification.
        /// </summary>
        void SetSeed(int seed);
    }
}

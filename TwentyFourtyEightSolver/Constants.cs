using System;

namespace TwentyFourtyEightSolver
{
    public static class Constants
    {
        public const int DefaultRowSize = 4;
        public const int DefaultColumnSize = 4;
        public const int MovesPerUpdate = 10;
        public const int UpdateRate = 5;
        public const int StartingTileCount = 2;
        public const int TileSpawnAmount = 1;
        public static readonly string DefaultPath = Environment.CurrentDirectory + "\\board.txt";
    }
}
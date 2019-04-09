using System;

namespace TwentyFourtyEightSolver
{
    public class GameOverEventArgs : EventArgs
    {
        public GameOverEventArgs(int score)
        {
            Score = score;
        }

        public int Score { get; set; }
    }
}
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace TwentyFourtyEightSolver
{
    internal class Program
    {
        private static bool _exit;
        private static bool _gameOver;
        private static Board _board;
        private static Board _savedBoard;
        private static Thread _moveThread;
        private static bool _playThread;

        private static void Main()
        {
            CreateBoard(Constants.DefaultRowSize, Constants.DefaultColumnSize);
            while (!_exit)
            {
                Console.Clear();
                DisplayBoard();
                //Console.WriteLine($"Highest: {_board.GetHighestValue()}");
                //Console.WriteLine(!_gameOver ? _board.ToString() : "Game over!");

                var key = Console.ReadKey();
                // Up, Down, Left and Right - Move tiles
                // R - Restart board
                // O - Save board
                // P - Open to saved board
                // Escape - Close game
                // U - Save to File
                // I - Load from File
                // L - Start/stop random move thread
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        _board.Move(Directions.Up);
                        break;
                    case ConsoleKey.DownArrow:
                        _board.Move(Directions.Down);
                        break;
                    case ConsoleKey.LeftArrow:
                        _board.Move(Directions.Left);
                        break;
                    case ConsoleKey.RightArrow:
                        _board.Move(Directions.Right);
                        break;
                    case ConsoleKey.R:
                        CreateBoard(Constants.DefaultRowSize, Constants.DefaultColumnSize);
                        _gameOver = false;
                        break;
                    case ConsoleKey.O:
                        _savedBoard = _board.Clone();
                        break;
                    case ConsoleKey.P:
                        _board = _savedBoard;
                        _board.GameOver += Board_GameOver;
                        break;
                    case ConsoleKey.Escape:
                        _exit = true;
                        break;
                    case ConsoleKey.U:
                        _board.SaveToFile(Constants.DefaultPath);
                        break;
                    case ConsoleKey.I:
                        _board = Board.LoadFromFile(Constants.DefaultPath);
                        _board.GameOver += Board_GameOver;
                        break;
                    case ConsoleKey.L:
                        _playThread = !_playThread;
                        if (_moveThread == null)
                        {
                            _moveThread = new Thread(RandomMoves);
                            _moveThread?.Start();
                        }

                        break;
                    case ConsoleKey.K:
                        _board.AutoChain();
                        break;
                }
            }

            Console.WriteLine("Repeat?");
            Console.ReadLine();
        }

        private static void RandomMoves()
        {
            var counter = 0;
            var random = new Random();
            while (!_gameOver)
            {
                while (_playThread)
                {
                    var toggle = random.Next(1, 5);
                    switch (toggle)
                    {
                        case 1:
                            _board.Move(Directions.Up);
                            break;
                        case 2:
                            _board.Move(Directions.Down);
                            break;
                        case 3:
                            _board.Move(Directions.Right);
                            break;
                        case 4:
                            _board.Move(Directions.Left);
                            break;
                    }

                    _board.AutoChain();
                    //Console.Clear();

                    if (counter == Constants.MovesPerUpdate)
                    {
                        DisplayBoard();
                        //Console.WriteLine($"Highest: {_board.GetHighestValue()}");
                        //Console.WriteLine(!_gameOver
                        //    ? _board.ToString()
                        //    : "Game over! " + Environment.NewLine + _board);
                        counter = 0;
                        Thread.Sleep(Constants.UpdateRate);
                        Console.Clear();
                    }

                    counter++;
                }

                //Checks for a change in state pretty dodgey
                Thread.Sleep(1000);
            }
        }

        private static void Board_GameOver(object sender, GameOverEventArgs e) => _gameOver = true;

        private static void CreateBoard(int row, int column)
        {
            _board = new Board(row, column);
            _board.GameOver += Board_GameOver;
        }

        private static void DisplayBoard()
        {
            Console.WriteLine($"Highest Tile: {_board.GetHighestValue()}");
            if (_gameOver)
            {
                Console.WriteLine("Game Over!");
            }
            var boardString = _board.ToString();
            var boardCharArray = Regex.Split(boardString, @"(?<=[ ])");
            foreach (var section in boardCharArray)
            {
                if (section.Trim() == "0")
                    Console.ForegroundColor = ConsoleColor.White;
                else if (section.Trim().Length > 4)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (section.Contains("4096"))
                    Console.ForegroundColor = ConsoleColor.Magenta;
                else if (section.Contains("2048"))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (section.Contains("1024"))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (section.Contains("512"))
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (section.Contains("256"))
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (section.Contains("128"))
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (section.Contains("64"))
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (section.Contains("32"))
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (section.Contains("16"))
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (section.Contains("8"))
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (section.Contains("4"))
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (section.Contains("2"))
                    Console.ForegroundColor = ConsoleColor.White;
                else
                    Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(section);
            }
        }
    }
}
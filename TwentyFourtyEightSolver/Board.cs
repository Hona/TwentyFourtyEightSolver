using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace TwentyFourtyEightSolver
{
    public class Board
    {
        public delegate void GameOverEventHandler(object sender, GameOverEventArgs e);

        private readonly int _columns;
        private readonly Random _random = new Random();
        private readonly int _rows;

        public Board(int rows, int columns)
        {
            _rows = rows;
            _columns = columns;
            Tiles = GenerateBoard(rows, columns);
            AddTiles(Constants.StartingTileCount);
        }

        public int[][] Tiles { get; set; }

        private static int[][] GenerateBoard(int rows, int columns)
        {
            var tiles = new int[rows][];
            for (var i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new int[columns];
            }
            return tiles;
        }

        private List<Position> GetEmptyTiles()
        {
            var emptyPositions = new List<Position>();
            for (var row = 0; row < Tiles.Length; row++)
            {
                for (var column = 0; column < Tiles[row].Length; column++)
                    if (Tiles[row][column] == 0)
                        emptyPositions.Add(new Position(row, column));
            }
            return emptyPositions;
        }

        private void AddTiles(int tilesCount)
        {
            var emptyTiles = GetEmptyTiles();
            if (emptyTiles.Count == 0)
            {
                // Create a temporary board for testing
                var testBoard = new Board(_rows, _columns)
                {
                    Tiles = (int[][]) Tiles.Clone()
                };

                // Try to move in all directions, dont spawn new tiles (this stops recursive game overs)
                testBoard.Move(Directions.Up, false);
                testBoard.Move(Directions.Down, false);
                testBoard.Move(Directions.Left, false);
                testBoard.Move(Directions.Right, false);

                // Loops through the boards, checking if there is a difference
                var difference = false;
                for (var row = 0; row < _rows; row++)
                {
                    for (var column = 0; column < _columns; column++)
                        if (testBoard.Tiles[row][column] != Tiles[row][column])
                        {
                            difference = true;
                            break;
                        }
                    if (difference)
                        break;
                }
                // If there is no difference, then the game is over
                if (!difference)
                    OnGameOver(new GameOverEventArgs(0));
                return;
            }

            for (var i = 0; i < tilesCount; i++)
            {
                var newPosition = emptyTiles[_random.Next(0, emptyTiles.Count)];
                AddTile(newPosition);
            }
        }

        private void AddTile(Position position)
        {
            var randomValue = _random.Next(1, 10);

            var value = randomValue < 9 ? 2 : 4;
            Tiles[position.Row][position.Column] = value;
        }

        public int GetHighestValue()
        {
            return Tiles.Aggregate(0, (current, tile) => tile.Concat(new[] {current}).Max());
        }

        public override string ToString()
        {
            var highestLength = GetHighestValue().ToString().Length + 1;
            var toReturn = "";
            foreach (var row in Tiles)
            {
                foreach (var column in row)
                {
                    var stringToAdd = column.ToString();
                    if (stringToAdd.Length < highestLength)
                    {
                        var paddingAmount = highestLength - stringToAdd.Length;
                        var front = true;
                        for (var i = 0; i < paddingAmount; i++)
                        {
                            // Switches between padding front and back to cause numbers to be centered
                            if (front)
                                stringToAdd = " " + stringToAdd;
                            else
                                stringToAdd = stringToAdd + " ";
                            front = !front;
                        }
                    }

                    toReturn += stringToAdd;
                }

                toReturn += Environment.NewLine;
            }

            return toReturn;
        }

        public void Move(Directions direction, bool addTiles = true)
        {
            var newTiles = GenerateBoard(_rows, _columns);
            switch (direction)
            {
                case Directions.Up:
                    MoveUp(ref newTiles);
                    MergeUp(ref newTiles);
                    Tiles = newTiles;
                    newTiles = GenerateBoard(_rows, _columns);
                    MoveUp(ref newTiles);
                    break;
                case Directions.Down:
                    MoveDown(ref newTiles);
                    MergeDown(ref newTiles);
                    Tiles = newTiles;
                    newTiles = GenerateBoard(_rows, _columns);
                    MoveDown(ref newTiles);
                    break;
                case Directions.Left:
                    MoveLeft(ref newTiles);
                    MergeLeft(ref newTiles);
                    Tiles = newTiles;
                    newTiles = GenerateBoard(_rows, _columns);
                    MoveLeft(ref newTiles);
                    break;
                case Directions.Right:
                    MoveRight(ref newTiles);
                    MergeRight(ref newTiles);
                    Tiles = newTiles;
                    newTiles = GenerateBoard(_rows, _columns);
                    MoveRight(ref newTiles);
                    break;
            }

            Tiles = newTiles;
            if (addTiles)
            {
                AddTiles(Constants.TileSpawnAmount);
            }
            
        }

        private void MoveUp(ref int[][] newTiles)
        {
            // Loops through each column going downwards (every vertical slice)
            for (var column = 0; column < _columns; column++)
                // Loops through each row
            for (var row = 0; row < _rows; row++)
                // Checks if the tile has a value
                if (Tiles[row][column] != 0)
                    for (var newRow = 0; newRow < _rows; newRow++)
                        // Looks for the closest empty spot
                        if (newTiles[newRow][column] == 0)
                        {
                            // Sets the value of the old board to the new position in the new board
                            newTiles[newRow][column] = Tiles[row][column];
                            break;
                        }
        }

        private void MergeUp(ref int[][] newTiles)
        {
            //Loops through the newboard, merging tiles
            for (var column = 0; column < _columns; column++)
            for (var row = 0; row < _rows; row++)
                // Ensures it doesnt look outside the grid
                if (row != _rows - 1)
                    if (newTiles[row][column] == newTiles[row + 1][column])
                    {
                        // Creates a new tile, and deletes the old ones
                        newTiles[row][column] *= 2;
                        newTiles[row + 1][column] = 0;
                    }
        }

        private void MoveDown(ref int[][] newTiles)
        {
            // Loops through each column going downwards (every vertical slice)
            for (var column = _columns - 1; column >= 0; column--)
                // Loops through each row
            for (var row = _rows - 1; row >= 0; row--)
                // Checks if the tile has a value
                if (Tiles[row][column] != 0)
                    for (var newRow = _rows - 1; newRow >= 0; newRow--)
                        // Looks for the closest empty spot
                        if (newTiles[newRow][column] == 0)
                        {
                            // Sets the value of the old board to the new position in the new board
                            newTiles[newRow][column] = Tiles[row][column];
                            break;
                        }
        }

        private void MergeDown(ref int[][] newTiles)
        {
            //Loops through the newboard, merging tiles
            for (var column = _columns - 1; column >= 0; column--)
            for (var row = _rows - 1; row <= 0; row--)
                // Ensures it doesnt look outside the grid
                if (row != 0)
                    if (newTiles[row][column] == newTiles[row - 1][column])
                    {
                        // Creates a new tile, and deletes the old ones
                        newTiles[row][column] *= 2;
                        newTiles[row - 1][column] = 0;
                    }
        }

        private void MoveLeft(ref int[][] newTiles)
        {
            // Loops through each column going downwards (every vertical slice)
            for (var row = 0; row < _rows; row++)
                // Loops through each row
            for (var column = 0; column < _columns; column++)
                // Checks if the tile has a value
                if (Tiles[row][column] != 0)
                    for (var newColumn = 0; newColumn < _columns; newColumn++)
                        // Looks for the closest empty spot
                        if (newTiles[row][newColumn] == 0)
                        {
                            // Sets the value of the old board to the new position in the new board
                            newTiles[row][newColumn] = Tiles[row][column];
                            break;
                        }
        }

        private void MergeLeft(ref int[][] newTiles)
        {
            //Loops through the newboard, merging tiles
            for (var row = 0; row < _rows; row++)
            for (var column = 0; column < _columns; column++)
                // Ensures it doesnt look outside the grid
                if (column != _columns - 1)
                    if (newTiles[row][column] == newTiles[row][column + 1])
                    {
                        // Creates a new tile, and deletes the old ones
                        newTiles[row][column] *= 2;
                        newTiles[row][column + 1] = 0;
                    }
        }

        private void MoveRight(ref int[][] newTiles)
        {
            // Loops through each column going downwards (every vertical slice)
            for (var row = _rows - 1; row >= 0; row--)
                // Loops through each row
            for (var column = _columns - 1; column >= 0; column--)
                // Checks if the tile has a value
                if (Tiles[row][column] != 0)
                    for (var newColumn = _columns - 1; newColumn >= 0; newColumn--)
                        // Looks for the closest empty spot
                        if (newTiles[row][newColumn] == 0)
                        {
                            // Sets the value of the old board to the new position in the new board
                            newTiles[row][newColumn] = Tiles[row][column];
                            break;
                        }
        }

        private void MergeRight(ref int[][] newTiles)
        {
            //Loops through the newboard, merging tiles
            for (var row = _rows - 1; row >= 0; row--)
            for (var column = _columns - 1; column >= 0; column--)
                // Ensures it doesnt look outside the grid
                if (column != 0)
                    if (newTiles[row][column] == newTiles[row][column - 1])
                    {
                        // Creates a new tile, and deletes the old ones
                        newTiles[row][column] *= 2;
                        newTiles[row][column - 1] = 0;
                    }
        }

        public event GameOverEventHandler GameOver;

        private void OnGameOver(GameOverEventArgs e)
        {
            GameOver?.Invoke(this, e);
        }

        public void AutoChain()
        {
            var list = Tiles.SelectMany(row => row).ToList();
            list.Sort();
            list.Reverse();

            var leftToRight = true;
            for (var row = 0; row < _rows; row++)
            {
                if (leftToRight)
                    for (var column = 0; column < _columns; column++)
                    {
                        Tiles[row][column] = list.First();
                        list.RemoveAt(0);
                    }
                else
                    for (var column = _columns - 1; column >= 0; column--)
                    {
                        Tiles[row][column] = list.First();
                        list.RemoveAt(0);
                    }

                leftToRight = !leftToRight;
            }
        }

        public Board Clone()
        {
            return MemberwiseClone() as Board;
        }

        public void SaveToFile(string path)
        {
            var toWrite = "";
            foreach (var row in Tiles)
            {
                toWrite = row.Aggregate(toWrite, (current, column) => current + (column + ","));
                toWrite = toWrite.TrimEnd(',');
                toWrite += Environment.NewLine;
            }

            File.WriteAllText(path, toWrite);
        }

        public static Board LoadFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            var rows = lines.Length;
            var columns = lines[0].Split(',').Length;
            var toReturn = new Board(rows, columns);

            for (var row = 0; row < rows; row++)
            {
                var columnArray = lines[row].Split(',');
                for (var column = 0; column < columns; column++)
                    toReturn.Tiles[row][column] = Convert.ToInt32(columnArray[column]);
            }

            return toReturn;
        }
    }
}
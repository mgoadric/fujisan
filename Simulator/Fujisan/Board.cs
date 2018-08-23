using System;
using System.Collections.Generic;

namespace Fujisan
{
    public enum Setup {
        DOMINO, COIN, ANYCOIN, RANDOM, HARDCODE
    }

    /********
     * A Board class to represent a configuration of the game Fujisan, main
     * elements include the values for the spaces on the board and the 
     * location of the pawns
     */
    public class Board
    {

        public int[,] values; // 0-5 representing the distance needed to travel
        public int[,] pawns;  // 0 for no pawn, 1 for pawn, there will be 4
        public Board parent;  // reference to the board before the move
        public string move;   // string to denote how the board was found
        public int length;    // number of steps to get to this board

        public Random random;

        /******
         * Empty constructor for cloning
         */
        public Board() {
            values = new int[2, 14];
            pawns = new int[2, 14];
        }

        /******
         * Creates a random starting board state
         */
        public Board(Random random, Setup s)
        {
            this.random = random;
            move = "START";

            values = new int[2, 14];
            if (s == Setup.DOMINO)
            {

                // Make the 25 tiles matching domino distribution
                List<Tile> list = new List<Tile>();
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 1; j < 6; j++)
                    {
                        list.Add(new Tile(i, j));
                    }
                }

                Shuffle<Tile>(list, random);

                // Fill in numbers on the left half
                int t = -1;
                for (int i = 0; i < 5; i++)
                {
                    t++;
                    if (random.NextDouble() > 0.5)
                    {
                        list[t].Rotate();
                    }
                    values[0, t + 1] = list[t].values[0, 0];
                    values[1, t + 1] = list[t].values[1, 0];
                }
                values[0, t + 2] = list[t].values[0, 1];
                values[1, t + 2] = list[t].values[1, 1];

                // Fill in numbers on the right half
                for (int i = 0; i < 5; i++)
                {
                    t++;
                    if (random.NextDouble() > 0.5)
                    {
                        list[t].Rotate();
                    }
                    values[0, 12 - i] = list[t].values[0, 1];
                    values[1, 12 - i] = list[t].values[1, 1];
                }
                values[0, 7] = list[t].values[0, 0];
                values[1, 7] = list[t].values[1, 0];

            }
            else if (s == Setup.COIN)
            {

                // Make the coins for the piecepack
                List<Byte>[] coins = new List<Byte>[4];
                for (int i = 0; i < 4; i++)
                {
                    coins[i] = new List<Byte>();
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        coins[j].Add((byte)i);
                    }
                }

                // Shuffle the coins within each type (sun, moon, etc)
                for (int i = 0; i < 4; i++)
                {
                    Shuffle<Byte>(coins[i], random);
                }

                // Place the coins on the board, starting at the 
                // right, and filling two from each type at a time
                int where = 12;
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            values[j, where] = coins[i][j + k * 2];
                        }
                        where--;
                    }
                }
            }
            else if (s == Setup.ANYCOIN)
            {

                // Make the coins for the piecepack
                List<Byte> coins = new List<Byte>();
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        coins.Add((byte)i);
                    }
                }

                // Shuffle the coins
                Shuffle<Byte>(coins, random);

                // Place the coins on the board, starting at the 
                // right, and filling two from each type at a time
                int where = 12;
                for (int k = 0; k < 12; k++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        values[j, where] = coins[j + k * 2];
                    }
                    where--;
                }

            }
            else if (s == Setup.HARDCODE)
            {
                // Initial board from Puzzle Four
                // http://www.ludism.org/ppwiki/Fuji-san#Heading9

                values[0, 12] = 5;
                values[1, 12] = 4;
                values[0, 11] = 3;
                values[1, 11] = 5;
                values[0, 10] = 3;
                values[1, 10] = 0;
                values[0, 9] = 4;
                values[1, 9] = 1;
                values[0, 8] = 2;
                values[1, 8] = 0;
                values[0, 7] = 2;
                values[1, 7] = 0;
                values[0, 6] = 2;
                values[1, 6] = 1;
                values[0, 5] = 0;
                values[1, 5] = 3;
                values[0, 4] = 1;
                values[1, 4] = 3;
                values[0, 3] = 1;
                values[1, 3] = 4;
                values[0, 2] = 5;
                values[1, 2] = 4;
                values[0, 1] = 5;
                values[1, 1] = 2;
            } else if (s == Setup.RANDOM) {
                    
                    for (int i = 0; i < 2; i++) {
                        for (int j = 1; j < 13; j++) {
                            values[i, j] = random.Next(0, 6);
                        }
                    }
            }

            // Place the pawns on the edges of the board
            pawns = new int[2, 14];
            pawns[0, 0] = 1;
            pawns[1, 0] = 1;
            pawns[0, 13] = 1;
            pawns[1, 13] = 1;

        }

        /*******
         * Returns an approximate value for the distance to a solution.
         * Counts the number of empty spots in the center of the board.
         * Fudges this with a random double to make them differentiable
         * in a map data structure.
         */
        public double Heuristic() {
            int h = 0;
            h += 1 - pawns[0, 6];
            h += 1 - pawns[1, 6];
            h += 1 - pawns[0, 7];
            h += 1 - pawns[1, 7];
            return Math.Max(0, h - (0.1 * random.NextDouble()));
        }

        /********
         * Determines if the board is solves, such that all pawns
         * are in the four center locations
         */
        public bool Solved()
        {
            return pawns[0, 6] == 1 && pawns[0, 7] == 1 &&
                pawns[1, 6] == 1 && pawns[1, 7] == 1;
        }

        /********
         * Determines if any pawn is in the center location. Useful 
         * for debugging
         */
        public bool AnyTop()
        {
            return pawns[0, 6] == 1 || pawns[0, 7] == 1 ||
                pawns[1, 6] == 1 || pawns[1, 7] == 1;
        }

        /********
         * Add a new child based on moving the pawn from (x1, y1) to (x2, y2)
         */
        public void AddChild(int x1, int y1, int x2, int y2, List<Board> children) {
            Board board = Clone(x1, y1, x2, y2, length);
            board.pawns[x1, y1] = 0;
            board.pawns[x2, y2] = 1;
            children.Add(board);           
        }

        /********
         * Create a list of all possible moves from this board state
         */
        public List<Board> GetChildren() {
            List<Board> children = new List<Board>();

            // Check each row and spot for a pawn
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    // When a pawn is found
                    if (pawns[i, j] == 1) {
                        
                        // Check for lateral movement between rows
                        if (j > 0 && j < 13)
                        {
                            // Find index for other row
                            int other = 1 - i;

                            // When no pawn is there, you can move sideways
                            if (pawns[other, j] != 1)
                            {
                                AddChild(i, j, other, j, children);
                            }
                        }

                        // Pawns cannot move when on top of the mountain
                        if (j != 6 && j != 7) {

                            // Check for spot to move up/down the mountain
                            for (int k = 1; k < 13; k++)
                            {
                                // If the spot is different than current and open
                                if (k != j && pawns[i, k] != 1) {

                                    // Find the distance to the spot
                                    int dist = Math.Abs(j - k);

                                    // Look for intervening pawns, recalibrate
                                    // needed distance
                                    int low = k + 1;
                                    int high = j;
                                    if (j < k) {
                                        low = j + 1;
                                        high = k;
                                    }
                                    for (int m = low; m < high; m++) {
                                        if (pawns[i, m] == 1) {
                                            dist--;
                                        }
                                    }

                                    // If the number at the spot matches
                                    // the distance to travel, it is a valid move
                                    if (values[i, k] == dist)
                                    {
                                        AddChild(i, j, i, k, children);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return children;
        }

        /*********
         * Returns a new board with a pawn moved from (x1, y1) to
         * (x2, y2), and sets the parent relationship.
         */
        public Board Clone(int x1, int y1, int x2, int y2, int len)
        {
            int[,] vs = new int[2, 14];
            int[,] ps = new int[2, 14];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    vs[i, j] = values[i, j];
                    ps[i, j] = pawns[i, j];
                }
            }
            string m = "(" + x1 + "," + y1 + ") -> (" + x2 + "," + y2 + ")";
            return new Board()
            {
                values = vs,
                pawns = ps,
                parent = this,
                move = m,
                length = 1 + len,
                random = random
            };
        }

        /********
         * Recursively return the path of moves that led you to this
         * particular board state. If you were the initial, then
         * stop the recursion.
         */
        public string Path() {
            if (parent == null) {
                return move + "\n" + ToString();
            } else {
                return parent.Path() + "\n" + move + "\n" + ToString();
            }
        }

        /********
         * Uses the string of the board to make a hashcode
         */
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /*********
         * Boards are equal when they have the same coins with the
         * pawns in the same locations.
         */
        public override bool Equals(object obj)
        {
            Board other = (Board)obj;
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 14; j++) {
                    if (pawns[i, j] != other.pawns[i, j])
                    {
                        return false;
                    }
                    if (values[i, j] != other.values[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /**********
         * Returns a string representation of the board. Pawns are 
         * represented by "."
         */
        public override string ToString() {
            string s = "";
            string s2 = "";
            for (int i = 0; i < 14; i++) {
                if (pawns[0, i] == 1)
                {
                    s += ".";
                } else if (i == 0 || i == 13) 
                {
                    s += " ";
                } else {
                    s += values[0, i];
                }

                if (pawns[1, i] == 1)
                {
                    s2 += ".";
                }
                else if (i == 0 || i == 13)
                {
                    s2 += " ";
                }
                else
                {
                    s2 += values[1, i];
                }
            }
            return s + "\n" + s2;
        }

        /************
         * Shuffle the elements of a generic list, using the provided
         * Random number generator
         */
        public static void Shuffle<T>(List<T> list, Random random) {
            // Shuffle the tiles
            var count = list.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = random.Next(i, count);
                var tmp = list[i];
                list[i] = list[r];
                list[r] = tmp;
            }
        }
    }
}

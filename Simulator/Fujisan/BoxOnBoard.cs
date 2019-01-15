using System;
using System.Collections.Generic;

namespace Fujisan
{
    public enum BoxOnSetup
    {
        TILES, RANDOM, HARDCODE
    }

    public class BoxOnBoard
    {

        public int height;
        public int width;
        public int totalMoves;
        public int colorCount;
        public byte[,] values; // 0 is empty 1-colorCount representing a color
            // Should this be a simple array?
        public BoxOnBoard parent;  // reference to the board before the move
        public string move;   // string to denote how the board was found
        public int length;    // number of steps to get to this board
        public Random random;

        /******
         * Empty constructor for cloning
         */
        public BoxOnBoard(int height, int width, int colorCount)
        {
            values = new byte[height, width];
            this.height = height;
            this.width = width;
            totalMoves = height * width / 2;
            this.colorCount = colorCount;
        }

        /******
         * Creates a random starting board state
         */
        public BoxOnBoard(Random random, int height, int width, int colorCount, BoxOnSetup s)
        {
            this.random = random;
            move = "START";

            values = new byte[height, width];
            this.height = height;
            this.width = width;
            totalMoves = height * width / 2;

            if (s == BoxOnSetup.RANDOM)
            {

                // Make the coins for the piecepack
                List<Byte> colors = new List<Byte>();
                byte nextColor = 1;
                while (colors.Count < height * width)
                {
                    colors.Add(nextColor);
                    nextColor += 1;
                    if (nextColor > colorCount)
                    {
                        nextColor = 1;
                    }
                }

                // Shuffle the colors
                Shuffle<Byte>(colors, random);

                // Place the colors on the board
                for (int k = 0; k < width; k++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        values[j, k] = colors[j + k * height];
                    }
                }
            } else if (s == BoxOnSetup.TILES)
            {
                List<BoxOnTile> tiles = new List<BoxOnTile>();
                tiles.Add(new BoxOnTile(1, 2));
                tiles.Add(new BoxOnTile(1, 5));
                tiles.Add(new BoxOnTile(2, 3));
                tiles.Add(new BoxOnTile(2, 6));
                tiles.Add(new BoxOnTile(3, 4));
                tiles.Add(new BoxOnTile(3, 1));
                tiles.Add(new BoxOnTile(4, 5));
                tiles.Add(new BoxOnTile(4, 2));
                tiles.Add(new BoxOnTile(5, 6));
                tiles.Add(new BoxOnTile(5, 3));
                tiles.Add(new BoxOnTile(6, 1));
                tiles.Add(new BoxOnTile(6, 4));

                Shuffle(tiles, random);

                int t = 0;
                for (int i = 0; i < 6; i += 2)
                {
                    for (int j = 0; j < 6; j += 3)
                    {
                        BoxOnTile tile = tiles[t];
                        values[i, j] = tile.dup;

                        if (random.Next(0, 2) == 1)
                        {
                            values[i, j + 1] = tile.single;
                            values[i + 1, j] = tile.dup;
                        }
                        else
                        {
                            values[i, j + 1] = tile.dup;
                            values[i + 1, j] = tile.single;
                        }
                        t++;
                    }
                }
                for (int i = 0; i < 6; i += 2)
                {
                    for (int j = 1; j < 6; j += 3)
                    {
                        BoxOnTile tile = tiles[t];
                        values[i + 1, j + 1] = tile.dup;

                        if (random.Next(0, 2) == 1)
                        {
                            values[i, j + 1] = tile.single;
                            values[i + 1, j] = tile.dup;
                        }
                        else
                        {
                            values[i, j + 1] = tile.dup;
                            values[i + 1, j] = tile.single;
                        }
                        t++;
                    }
                }
            }
        }

        /*******
         * Returns an exact value for the distance to a solution.
         * A* is not as helpful in this game.        
         */
        public double Heuristic()
        {
            return totalMoves - length;
        }

        /********
         * Determines if the board is solves, such that all pawns
         * are in the four center locations
         */
        public bool Solved()
        {
            return length == totalMoves;
        }

        /********
         * Add a new child based on removing the pieces from (x1, y1) and (x2, y2)
         */
        public void AddChild(int x1, int y1, int x2, int y2, List<BoxOnBoard> children)
        {
            BoxOnBoard board = Clone(x1, y1, x2, y2, length);
            board.values[x1, y1] = 0;
            board.values[x2, y2] = 0;
            children.Add(board);
        }

        public bool Clear(int x1, int y1, int x2, int y2)
        {
            int top = y2;
            int bottom = y1;
            if (y1 < y2)
            {
                top = y1;
                bottom = y2;
            }

            //Console.WriteLine("is this clear? " + x1 + "," + y1 + ";" + x2 + "," + y2);

            //Console.WriteLine(this);

            for (int i = x1; i <= x2; i++)
            {
                for (int j = top; j <= bottom; j++)
                {
                    //Console.WriteLine("checking " + i + "," + j);
                    if (!(i == x1 && j == y1) && 
                    !(i == x2 && j == y2) && values[i, j] != 0)
                    {
                        //Console.WriteLine("fail");
                        return false;
                    }
                }
            }
            return true;
        }

        /********
         * Create a list of all possible moves from this board state
         */
        public List<BoxOnBoard> GetChildren()
        {
            List<BoxOnBoard> children = new List<BoxOnBoard>();

            // Check each pair of row and spot for a color match
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int k = i;
                    for (int m = j + 1; m < width; m++)
                    {
                        // When a pawn is found
                        if ((i != k || j != m) && values[i, j] != 0 && 
                            values[i, j] == values[k, m] &&
                            Clear(i, j, k, m))
                        {
                            AddChild(i, j, k, m, children);
                        }
                    }

                    for (k = i + 1; k < height; k++)
                    {
                        for (int m = 0; m < width; m++)
                        {
                            // When a pawn is found
                            if ((i != k || j != m) && values[i,j] != 0 &&
                                values[i, j] == values[k, m] &&
                                Clear(i, j, k, m))
                            {
                                AddChild(i, j, k, m, children);
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
        public BoxOnBoard Clone(int x1, int y1, int x2, int y2, int len)
        {
            byte[,] vs = values.Clone() as byte[,];

            string m = "(" + x1 + "," + y1 + ") , (" + x2 + "," + y2 + ")";
            return new BoxOnBoard(height, width, colorCount)
            {
                values = vs,
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
        public string Path()
        {
            if (parent == null)
            {
                return move + "\n" + ToString();
            }
            else
            {
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
         * Boards are equal when they have the same colors in the same locations.
         */
        public override bool Equals(object obj)
        {
            BoxOnBoard other = (BoxOnBoard)obj;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
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
        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    s += values[i, j];
                }
                s += "\n";
            }
            return s;
        }

        /************
         * Shuffle the elements of a generic list, using the provided
         * Random number generator
         */
        public static void Shuffle<T>(List<T> list, Random random)
        {
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

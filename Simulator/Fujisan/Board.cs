using System;
using System.Collections.Generic;

namespace Fujisan
{
    public class Board
    {

        public int[,] values;
        public int[,] pawns;
        public Board parent;
        public string move;
        public int length;
        public Random random;

        public Board() {
            values = new int[2, 14];
            pawns = new int[2, 14];
        }

        public Board(Random random)
        {
            this.random = random;
            move = "START";
            values = new int[2, 14];

            // Make the tiles
            List<Tile> list = new List<Tile>();
            for (int i = 0; i < 6; i++) 
            {
                for (int j = 1; j < 6; j++) 
                {
                    list.Add(new Tile(i, j));
                }
            }

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

            // TRYING TO OPTIMIZE FOR SPEED, NOT OOP VISUALIZATION
            // Left half
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

            // Right half
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

            //            pawns = new Pawn[4];
            //            pawns[0] = new Pawn(0, 0);
            //            pawns[1] = new Pawn(1, 0);
            //            pawns[2] = new Pawn(0, 13);
            //            pawns[3] = new Pawn(1, 13);

            pawns = new int[2, 14];
            pawns[0, 0] = 1;
            pawns[1, 0] = 1;
            pawns[0, 13] = 1;
            pawns[1, 13] = 1;

        }

        public double Heuristic() {
            int h = 0;
            h += 1 - pawns[0, 6];
            h += 1 - pawns[1, 6];
            h += 1 - pawns[0, 7];
            h += 1 - pawns[1, 7];
            return Math.Max(0, h - (0.1 * random.NextDouble()));
        }

        public bool Solved()
        {
            return pawns[0, 6] == 1 && pawns[0, 7] == 1 &&
                pawns[1, 6] == 1 && pawns[1, 7] == 1;
        }

        public bool AnyTop()
        {
            return pawns[0, 6] == 1 || pawns[0, 7] == 1 ||
                pawns[1, 6] == 1 || pawns[1, 7] == 1;
        }

        public List<Board> GetChildren() {
            List<Board> c = new List<Board>();

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    if (pawns[i, j] == 1) {
                        
                        // Sideways movement
                        if (j > 0 && j < 13)
                        {
                            int other = 1 - i;
                            if (pawns[other, j] != 1)
                            {
                                Board board = Clone(i, j, other, j, length);
                                board.pawns[i, j] = 0;
                                board.pawns[other, j] = 1;
                                c.Add(board);
                            }
                        }

                        // Not on top
                        if (j != 6 || j != 7) {
                        if (j != 6 && j != 7) {
                            for (int k = 1; k < 13; k++)
                            {
                                if (k != j && pawns[i, k] != 1) {
                                    
                                    int dist = Math.Abs(j - k);

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

                                    if (values[i, k] == dist)
                                    {
                                        Board board = Clone(i, j, i, k, length);
                                        board.pawns[i, j] = 0;
                                        board.pawns[i, k] = 1;
                                        c.Add(board);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return c;
        }

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

        public string Path() {
            if (parent == null) {
                return move + "\n" + ToString();
            } else {
                return parent.Path() + "\n" + move + "\n" + ToString();
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

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
    }
}

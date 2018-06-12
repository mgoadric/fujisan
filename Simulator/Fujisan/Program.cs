using System;
using System.Collections.Generic;

namespace Fujisan
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // Tile tile = new Tile(2, 3);
            // Console.WriteLine(tile);

            int count = 0;
            int lensum = 0;
            int dead = 0;
            Random random = new Random();
            for (int i = 0; i < 1000; i++)
            {
                HashSet<Board> found = new HashSet<Board>();
                SortedList<double, Board> frontier = new SortedList<double, Board>();

                Board start = new Board(random);
                Console.WriteLine("Starting:");
                Console.WriteLine(start + "\n");
                frontier.Add(start.length + start.Heuristic() + (0.1 * random.NextDouble()), start);

                while (frontier.Count > 0)
                {
                    //Console.WriteLine("FRONTIER SIZE = " + frontier.Count);
                    //Console.WriteLine("FOUND SIZE = " + found.Count);
                    //Console.ReadLine();
                    var keys = frontier.Keys;
                    // or with LINQ
                    var first = keys[0];
                    Board board = frontier[first];
                    frontier.Remove(first);
                    found.Add(board);

                    List<Board> stuff = board.GetChildren();
                    foreach (Board b in stuff)
                    {
                        if (b.Solved())
                        {
                            Console.WriteLine("SOLUTION!!!!");
                            Console.WriteLine(b.Path());
                            lensum += b.length;
                            frontier = new SortedList<double, Board>();
                            count++;
                            break;
                        }
                        //Console.WriteLine(board.Equals(b));
                        if (!found.Contains(b) && !frontier.ContainsValue(b))
                        {
                            //Console.WriteLine(b);
                            frontier.Add(b.length + b.Heuristic() + (0.1 * random.NextDouble()), b);
                        }
                        else
                        {
                            //Console.WriteLine("WOAH!");
                        }
                    }
                }
                //Console.WriteLine("FOUND SIZE = " + found.Count);
                if (found.Count == 1)
                {
                    dead++;
                }
                Console.WriteLine("" + i + ": solved:" + ((float)count / (i + 1)) + 
                                  ", dead: " + ((float)dead / (i + 1)) + 
                                  ", avelen: " + ((float)lensum / count));
            }
        }
    }
}

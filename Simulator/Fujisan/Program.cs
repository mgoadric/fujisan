using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fujisan
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            int TRIALS = 10;
            int EXP = 50;

            // Easy output for copying into a spreadsheet
            Console.WriteLine("solved\tdead\tavelen");

            // Run the specified number of experiments within the number
            // of specified trials
            for (int t = 0; t < TRIALS; t++)
            {
                int count = 0;
                int lensum = 0;
                int dead = 0;
                Random random = new Random();

                for (int i = 0; i < EXP; i++)
                {
                    // Store all boards seen in found
                    HashSet<Board> found = new HashSet<Board>();

                    // Keep track of board states to explore in frontier
                    // Sort them by heuristic plus current path length for A*
                    SortedList<double, Board> frontier = new SortedList<double, Board>();

                    // Create a new board and place it in the frontier
                    Board start = new Board(random);
                    Debug.WriteLine("Starting:");
                    Debug.WriteLine(start + "\n");
                    frontier.Add(start.length + start.Heuristic() + (0.1 * random.NextDouble()), start);

                    // Keep searching the frontier until it is empty or
                    // a solution is found
                    while (frontier.Count > 0)
                    {

                        // Take the most promising board state, remove from
                        // frontier and add it to the found set
                        var keys = frontier.Keys;
                        var first = keys[0];
                        Board board = frontier[first];
                        frontier.Remove(first);
                        found.Add(board);

                        // Find the children of the current board
                        List<Board> stuff = board.GetChildren();
                        foreach (Board b in stuff)
                        {
                            // Did you find a solution?
                            if (b.Solved())
                            {
                                // Yay! Record statistics
                                Debug.WriteLine("SOLUTION!!!!");
                                Debug.WriteLine(b.Path());
                                lensum += b.length;
                                frontier = new SortedList<double, Board>();
                                count++;
                                break;
                            }

                            // If you have never seen this board before
                            // Add it to the frontier
                            if (!found.Contains(b) && !frontier.ContainsValue(b))
                            {
                                frontier.Add(b.length + b.Heuristic() + (0.1 * random.NextDouble()), b);
                            }
                            else
                            {
                                //Console.WriteLine("WOAH!");
                            }
                        }
                    }

                    // Record when no children of initial state could be found
                    if (found.Count == 1)
                    {
                        dead++;
                    }
                }

                Console.WriteLine(((float)count / EXP) +
                                  "\t" + ((float)dead / EXP) +
                                  "\t" + ((float)lensum / count));
            }
        }
    }
}

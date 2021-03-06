﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Fujisan
{
    public enum Search {
        ASTAR, BFS, RANDOM, FULL
    }

    class MainClass
    {
        public static void Main(string[] args)
        {

            BoxOnMain();

        }

        public static void FujisanMain()
        {
            int TRIALS = 1;
            int EXP = 100;

            // Choose here the setup algorithm you wish to use
            FujisanSetup setup = FujisanSetup.PIECEPACK;

            // Choose here the search algorithm for the solver
            Search search = Search.ASTAR;

            List<int> hist = new List<int>();
            List<int> countermoves = new List<int>();
            Random random = new Random();

            // Easy output for copying into a spreadsheet
            Console.WriteLine("solved\tdead\tavelen\tsconn\tfconn");

            // Run the specified number of experiments within the number
            // of specified trials
            for (int t = 0; t < TRIALS; t++)
            {
                int count = 0;
                int lensum = 0;
                int dead = 0;
                double sconn = 0;
                double fconn = 0;
                int max = 0;

                Parallel.For(0, EXP, i =>
                //for (int i = 0; i < EXP; i++)
                {
                    // Total count of boards, for HashSet later
                    int bcount = 0;

                    // Store all boards seen in found
                    HashSet<Board> found = new HashSet<Board>();

                    // Keep track of board states to explore in frontier
                    // Sort them by heuristic plus current path length for A*
                    SortedList<double, Board> frontier = new SortedList<double, Board>();

                    // Create a new board and place it in the frontier
                    Board start = new Board(random, setup);
                    Console.WriteLine(start);
                    Console.WriteLine("Starting:");
                    Console.WriteLine(start + "\n");
                    frontier.Add(start.length + start.Heuristic() + (1e-12 * bcount), start);

                    // Keep searching the frontier until it is empty or
                    // a solution is found
                    bool solved = false;
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
                        List<Board> children = board.GetChildren();
                        List<Board> stuff = new List<Board>();
                        if (search == Search.ASTAR)
                        {
                            stuff = children;
                        }
                        else  // Pick a child randomly
                        {
                            if (children.Count > 0)
                            {
                                stuff.Add(children[random.Next(0, children.Count)]);
                            }
                        }
                        Console.WriteLine(frontier.Count);
                        foreach (Board b in stuff)
                        {
                            // Did you find a solution?
                            if (b.Solved())
                            {
                                // Yay! Record statistics
                                solved = true;
                                Debug.WriteLine("SOLUTION!!!!");
                                Debug.WriteLine(b.Path());

                                frontier.Clear();
                                lock (random)
                                {
                                    sconn += start.ConnectionStrength();
                                    lensum += b.length;
                                    count++;
                                    Console.WriteLine(b.length + "," +
                                    b.countermoves + "," + b.MovePath());

                                    if (b.length > max)
                                    {
                                        Debug.WriteLine("SOLUTION!!!!");
                                        Debug.WriteLine(b.Path());
                                        max = b.length;
                                    }
                                }
                                break;
                            }

                            // If you have never seen this board before
                            // Add it to the frontier
                            if (!found.Contains(b) && !frontier.ContainsValue(b))
                            {
                                bcount++;
                                frontier.Add(b.length + b.Heuristic() + (1e-12 * bcount), b);
                            }
                            else
                            {
                                //Console.WriteLine("WOAH!");
                            }
                        }
                    }

                    // Record when no children of initial state could be found
                    if (!solved)
                    {
                        fconn += start.ConnectionStrength();
                        if (found.Count == 1)
                        {
                            lock (random)
                            {
                                dead++;
                            }
                        }
                    }
                });

                Console.WriteLine(((float)count / EXP) +
                                  "\t" + ((float)dead / EXP) +
                                  "\t" + ((float)lensum / count) +
                                  "\t" + sconn / count +
                                  "\t" + fconn / (EXP - count));
            }
            Console.WriteLine("Steps");
            foreach (int i in hist)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine("CounterMoves");
            foreach (int i in countermoves)
            {
                Console.WriteLine(i);
            }
        }

        public static void BoxOnMain()
        {
            int TRIALS = 1;
            int EXP = 10;

            // Choose here the setup algorithm you wish to use
            BoxOnSetup setup = BoxOnSetup.TILES;

            // Choose here the search algorithm for the solver
            Search search = Search.BFS;

            Random random = new Random();

            // Easy output for copying into a spreadsheet
            Console.WriteLine("solved\tdead\tavelen\tsconn\tfconn");

            // Run the specified number of experiments within the number
            // of specified trials
            for (int t = 0; t < TRIALS; t++)
            {
                int count = 0;
                int lensum = 0;
                int dead = 0;
                double sconn = 0;
                double fconn = 0;
                int max = 0;

                Parallel.For(0, EXP, i =>
                //for (int i = 0; i < EXP; i++)
                {
                    // Total count of boards, for HashSet later
                    int bcount = 0;

                    // Store all boards seen in found
                    HashSet<BoxOnBoard> found = new HashSet<BoxOnBoard>();

                    // Keep track of board states to explore in frontier
                    // Sort them by heuristic plus current path length for A*
                    Queue<BoxOnBoard> frontier = new Queue<BoxOnBoard>();

                    // Create a new board and place it in the frontier
                    BoxOnBoard start = new BoxOnBoard(random, 6, 6, 6, setup);
                    //Console.WriteLine(start);
                    //Console.WriteLine("Starting:");
                    //Console.WriteLine(start + "\n");
                    frontier.Enqueue(start);

                    // Keep searching the frontier until it is empty or
                    // a solution is found
                    bool solved = false;
                    int maxLenLocal = 0;
                    while (frontier.Count > 0)
                    {

                        // Take the next promising board state
                        BoxOnBoard board = frontier.Dequeue();
                        found.Add(board);

                        // Find the children of the current board
                        List<BoxOnBoard> children = board.GetChildren();
                        //Console.WriteLine("numChildren:" + children.Count);
                        List<BoxOnBoard> stuff = new List<BoxOnBoard>();
                        if (search == Search.BFS)
                        {
                            stuff = children;
                        }
                        else  // Pick a child randomly
                        {
                            if (children.Count > 0)
                            {
                                int which = random.Next(0, children.Count);
                                //Console.WriteLine("adding child" + which);

                                stuff.Add(children[which]);
                            }
                        }
                        //Console.WriteLine(board.Path());
                        if (board.length > maxLenLocal)
                        {
                            Console.WriteLine(board.length + "," + frontier.Count);
                            maxLenLocal = board.length;
                        }
                        foreach (BoxOnBoard b in stuff)
                        {
                            // Did you find a solution?
                            if (b.Solved())
                            {
                                // Yay! Record statistics
                                solved = true;
                                Console.WriteLine("SOLUTION!!!!");
                                Console.WriteLine(b.Path());

                                frontier.Clear();
                                lock (random)
                                {
                                    lensum += b.length;
                                    count++;

                                    if (b.length > max)
                                    {
                                        //Console.WriteLine("SOLUTION!!!!");
                                        //Console.WriteLine(b.Path());
                                        max = b.length;
                                    }
                                }
                                break;
                            }
                            else
                            {

                            }

                            // If you have never seen this board before
                            // Add it to the frontier
                            if (!found.Contains(b) && !frontier.Contains(b))
                            {
                                bcount++;
                                frontier.Enqueue(b);
                            }
                            //else if (found.Contains(b))
                            //{
                            //    Console.WriteLine("found before!");
                            //}
                            //else if (frontier.Contains(b))
                            //{
                            //    Console.WriteLine("in frontier!");
                            //}
                        }
                    }

                    // Record when no children of initial state could be found
                    if (!solved)
                    {
                        if (found.Count == 1)
                        {
                            lock (random)
                            {
                                dead++;
                            }
                        }
                    }
                });

                Console.WriteLine(((float)count / EXP) +
                                  "\t" + ((float)dead / EXP) +
                                  "\t" + ((float)lensum / count) +
                                  "\t" + sconn / count +
                                  "\t" + fconn / (EXP - count));
            }
        }
    }
}

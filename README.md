# Analysis of Procedural Puzzle Challenge Generation for Fujisan

Challenges for solitaire puzzle games are typically limited in number and designed in advance by humans. Alternately,
some games incorporate stochastic setup rules, in which the solver randomly sets up the game board before solving the challenge,
which can greatly increase the number of possible challenges. However, these setup rules can often generate unsolvable or
uninteresting challenges. 

For the game [Fujisan](https://boardgamegeek.com/boardgame/35893/fujisan), we examine how
different stochastic challenge generation algorithms affect ease of physical setup, challenge solvability, and challenge difficulty. 
We find that algorithms can be simple for the solver yet generate solvable and difficult challenges, by constraining randomness through
embedding sub-elements of the puzzle mechanics into the physical pieces of the game.

## Requirements

* Visual Studio 

## Setup

1. Open "Fujisan.sln" project in Simulator directory.
2. Alter the Program.cs class to choose the PCG algorithm and solver of your choice.
3. Run the program.
4. Output to the console will be reported in a tab-delimited form
    1. The number of solvable challenges
    2. The number of "dead" challenges that have no moves possible
    3. The average minimum solution length of solvable challenges
    4. The average connectedness for solvable challenges
    5. The average connectedness for unsolvable challenges
5. The final output will be a list of the minimum solution lengths for all solvable challenges

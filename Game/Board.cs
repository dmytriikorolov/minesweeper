using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Game;

public class Board
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public Cell[,] Grid {get; set;}
    private Random rng_ = new Random();
    private bool firstMove_ = true;
    private int totalMines_;
    private int safeSize_;

    public Board(int rows, int columns, int mines)
    {
        totalMines_ = mines;
        Rows = rows;
        Columns = columns;
        safeSize_ = rows * columns / 10;      // about ten percent of board
        Grid = new Cell[rows, columns];

        for (int r = 0; r < rows; ++r)
        {
            for (int c = 0; c < columns; ++c)
            {
                Grid[r, c] = new Cell();
            }
        }
    }

    private void PlaceMines(int mines)
    {
        var positions = Enumerable.Range(0, Rows * Columns).OrderBy(x => rng_.Next()).Take(mines);

        foreach (int p in positions)
        {
            Grid[p / Columns, p % Columns].isMine = true;
        }
    }
    
    private void PlaceMinesWithSafeCluster(int centerR, int centerC)
    {
        var safe = new HashSet<int>();
        
        for (int dr = -1; dr <= 1; ++dr)
        {
            for (int dc = -1; dc <= 1; ++dc)
            {
                int rr = centerR + dr, cc = centerC + dc;
                if (rr >= 0 && rr < Rows && cc >= 0 && cc < Columns)
                {
                    safe.Add(rr * Columns + cc);
                }
            }
        }
        
        var frontier = safe.Select(i => (i / Columns, i % Columns)).ToList();
        while (safe.Count < safeSize_ && frontier.Count > 0)
        {
            var (cr, cc) = frontier[rng_.Next(frontier.Count)];
            var nbs = NeighbourCoords(cr, cc);
            var (nr, nc) = nbs[rng_.Next(nbs.Count)];
            int idx = nr * Columns + nc;

            if (safe.Add(idx))
            {
                frontier.Add((nr, nc));
            }
        }


        var positions = Enumerable.Range(0, Rows * Columns).Where(i => !safe.Contains(i)).OrderBy(_ => rng_.Next()).Take(totalMines_);

        foreach (int p in positions)
        {
            Grid[p / Columns, p % Columns].isMine = true;
        }
    }

    public bool Reveal(int r, int c)
    {
        if (firstMove_)
        {
            PlaceMinesWithSafeCluster(r, c);
            ComputeAdjacency();
            firstMove_ = false;
        }

        var cell = Grid[r, c];
        if (cell.isRevealed || cell.isFlagged) return true;

        cell.isRevealed = true;

        if (cell.isMine) return false;
        if (cell.Adjacent == 0)
        {
            foreach (var (nr, nc) in NeighbourCoords(r, c))
            {
                Reveal(nr, nc);
            }
        }

        return true;
    }

    private List<(int,int)> NeighbourCoords(int r, int c)
    {
        var list = new List<(int,int)>();

        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < Rows && nc >= 0 && nc < Columns)
                {
                    list.Add((nr, nc));
                }
            }
        }

        return list;
    }

    private void ComputeAdjacency()
    {
        for (int r = 0; r < Rows; ++r)
        {
            for (int c = 0; c < Columns; ++c)
            {
                var cell = Grid[r, c];

                if (cell.isMine) continue;

                cell.Adjacent = NeighbourCoords(r, c).Count(x => Grid[x.Item1, x.Item2].isMine);
            }
        }
    }

    public bool HelperReveal(int r, int c)
    {
        var cell = Grid[r, c];
        
        if (!cell.isRevealed || cell.Adjacent == 0) return true;

        // count how many neighbouring flags we have
        int flagged = NeighbourCoords(r, c)
            .Count(t => Grid[t.Item1, t.Item2].isFlagged);
        
        if (flagged != cell.Adjacent) return true;
        
        foreach (var (nr, nc) in NeighbourCoords(r, c))
        {
            var nb = Grid[nr, nc];
            if (nb.isRevealed || nb.isFlagged) continue;
            
            if (!Reveal(nr, nc))
                return false;
        }

        return true;   // no mine
    }
    // auto-flag: if the number is revealed and the only logical neighbours are mines. Cool thing i stole from Dota 2 Minesweeper haha
    public void FlagHelper(int r, int c)
    {
        var cell = Grid[r, c];
        if (!cell.isRevealed || cell.Adjacent == 0) return;

        int flagged = 0;
        var hiddenUnflagged = new List<(int,int)>();

        foreach (var (nr, nc) in NeighbourCoords(r, c))
        {
            var nb = Grid[nr, nc];
            if (nb.isFlagged) flagged++;
            else if (!nb.isRevealed) hiddenUnflagged.Add((nr, nc));
        }

        int missing = cell.Adjacent - flagged;

        /* only if the count of hidden neighbours = the number of missing flags we place  automatically */
        if (missing == hiddenUnflagged.Count && missing > 0)
        {
            foreach (var (nr, nc) in hiddenUnflagged)
                Grid[nr, nc].isFlagged = true;
        }
    }

    public void ToggleFlag(int r, int c)
    {
        var cell = Grid[r, c];
        if(!cell.isRevealed) cell.isFlagged = !cell.isFlagged;
    }

    public bool HasWon()
    {
        for (int r = 0; r < Rows; ++r)
        {
            for (int c = 0; c < Columns; ++c)
            {
                var cell = Grid[r, c];
                if (!cell.isMine && !cell.isRevealed) return false;
            }
        }

        return true;
    }
}

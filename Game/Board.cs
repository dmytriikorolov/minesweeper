using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Game;

public class Board
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public Cell[,] Grid {get; set;}
    
    private Random _rng = new Random();

    public Board(int rows, int columns, int mines)
    {
        Rows = rows;
        Columns = columns;
        Grid = new Cell[rows, columns];

        for (int r = 0; r < rows; ++r)
        {
            for (int c = 0; c < columns; ++c)
            {
                Grid[r, c] = new Cell();
            }
        }
        PlaceMines(mines);
        ComputeAdjacency();
        
    }

    private void PlaceMines(int mines)
    {
        var positions = Enumerable.Range(0, Rows * Columns).OrderBy(x => _rng.Next()).Take(mines);

        foreach (int p in positions)
        {
            Grid[p / Columns, p % Columns].isMine = true;
        }
    }

    public bool Reveal(int r, int c)
    {
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
        // return all neighbours coords
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
                // very simple idea: if some safe cell is still not revelead -- we still haven't won.
            }
        }

        return true;
    }
}

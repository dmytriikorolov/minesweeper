namespace Minesweeper.Game;
// self explanatory, dont think i need comments here.

public class Cell
{
    public bool isMine { get; set; }
    public bool isRevealed { get; set; }
    public bool isFlagged { get; set; }
    public int Adjacent { get; set; }
}
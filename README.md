# Minesweeper — Code Geass Edition

Tiny Avalonia-based desktop Minesweeper with a Geass-flavoured sprite set.  
Rules are 100 % classic; artwork is just for fun.


---



## Default board

* **Size:** 12 × 12  
* **Mines:** 25  
* **Safe blob:** ≈10 % of the board is guaranteed clear after the first click.

## Controls

* **Left-click**  
  • Reveals the clicked square.  
  • Your very first click always opens a safe blob, so you never lose on move 1.  

* **Left-click on a number that’s already revealed**  
  • If you have flagged the correct number of neighbouring mines, all other neighbouring squares open at once (classic “chord reveal”).  
  • If a flag is wrong, the mistaken square explodes.

* **Right-click on a hidden square**  
  • Toggles a flag to mark / unmark a suspected mine.

* **Right-click on a revealed number**  
  • If the only logical neighbours can *only* be mines, the game auto-drops the missing flags for you.

---
## How to build & run

```bash
git clone https://github.com/dmytriikorolov/minesweeper.git
cd minesweeper
dotnet build      
dotnet run

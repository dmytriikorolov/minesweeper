# minesweeper – developer notes 

---

## 1. Cell.cs

* `isMine` – true when this square hides a bomb  
* `isRevealed` – player already opened it  
* `isFlagged` – user marked it as a suspected mine  
* `Adjacent` – how many bombs sit in the 8 surrounding squares  

No behaviour here, just a tiny data holder.

## 2. Board.cs

### fields  
* `Rows`, `Columns` – obvious grid size  
* `Grid` – 2-D array of `Cell` objects  
* `rng_` – single `Random` instance for the whole game  
* `firstMove_` – starts true; flips false after first left-click  
* `totalMines_` – mine count we got from the constructor  
* `safeSize_` – 10 % of the board; how big the opening blob should be

### constructor  
* takes `rows, columns, mines`  
* fills `Grid` with blank `Cell`s  

### methods  

* `PlaceMinesWithSafeCluster(centerR, centerC)`  
  * no params besides the click coords  
  * builds a hash-set of safe indices: clicked cell + neighbours, then grows it randomly until it hits `safeSize_`  
  * picks `totalMines_` random positions outside that set and tags them as mines  

* `Reveal(r, c)`  
  * on very first call -> calls `PlaceMinesWithSafeCluster`, computes adjacency numbers, flips `firstMove_`  
  * then: if the cell was flagged or already open, bail; mark it revealed; if it’s a mine return false; if it shows 0 flood-fill neighbours recursively  

* `NeighbourCoords(r, c)` – returns a `List<(int,int)>` with all legal neighbours; keeps border math in one place  

* `ComputeAdjacency()` – double loop, counts surrounding mines for every safe cell, stores result in `Adjacent`  

* `HelperReveal(r, c)` 
  * expects `(r,c)` to be a revealed number  
  * if flags around it equal that number, opens every hidden neighbour (uses `Reveal` so it can trigger a loss)  
  * returns false only when a wrong flag blows up a mine  

* `FlagHelper(r, c)` – auto-flag  
  * also needs a revealed number  
  * if “hidden-neighbour count” exactly matches “number still missing”, sets flags on those hidden cells  

* `ToggleFlag(r, c)` – flips flag on a hidden cell, ignores revealed cells  

* `HasWon()` – quick scan: if any safe cell is still hidden, game isn’t over

---


## 3. MainWindow.axaml.cs

### important fields  
* `_board` – instance of `Board`  
* `_buttons` – `Button` controls 
* `_gameOver` – blocks further clicks after win or loss  
* `_numbers[]`, `_imgBomb`, `_imgFlag`, `_imgSafe` – sprite sheet loaded once with the `Load` helper

### Build sequence  
1. `InitializeComponent()` – loads XAML  
2. `LoadNumberSprites()` – fills the digit image array  
3. `BuildButtons()` – 144 buttons, `(row,col)` stored in `Tag`, click events wired  
4. `RefreshUI()` – paints grey blanks

### click handlers  

* `Cell_LeftClick`  
  * if `_gameOver` ignore  
  * if tile is already revealed and shows a number -> call `_board.HelperReveal`  
  * else call `_board.Reveal`  
  * always repaint and call `EndGame` when needed  

* `Cell_RightClick`  
  * if click is on a revealed number -> `_board.FlagHelper` (auto-flags)  
  * else toggle a single flag on that hidden square  
  * repaint  

### RefreshUI() (core repaint)  
* walks the whole grid every time  
* revealed squares: transparent background, disabled **only if** they’re bombs (numbers stay clickable)  
* picks correct sprite or fallback text  
* hidden squares: grey background; flag sprite if flagged

### EndGame(msg)  
* sets `_gameOver`  
* flips every mine to revealed  
* repaints once more  
* pops a tiny modal window with the win/lose message

---

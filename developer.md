# minesweeper – developer notes (plain-speak)

---

## 1. Cell.cs

* `isMine` – true when this square hides a bomb  
* `isRevealed` – player already opened it  
* `isFlagged` – user marked it as a suspected mine  
* `Adjacent` – how many bombs sit in the 8 surrounding squares  

No behaviour here, just a tiny data holder.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Minesweeper.Game;

namespace Minesweeper;

// window code behind
public partial class MainWindow : Window
{
    // board size and mine count
    private const int Rows  = 12;
    private const int Cols  = 12;
    private const int Mines = 25;   // todo later add a menu to switch these

    // game model
    private Board _board = new(Rows, Cols, Mines);

    // button refs so we can repaint quick
    private readonly Button[,] _buttons = new Button[Rows, Cols];

    // flag once game is finished
    private bool _gameOver;


    // helper loads embedded image once
    private static IImage Load(string uri) =>
        new Bitmap(AssetLoader.Open(new Uri(uri)));

    private IImage?[] _numbers = new IImage?[9];  
    private IImage _imgBomb = Load("avares://Minesweeper/Assets/bomb.png");
    private IImage _imgFlag = Load("avares://Minesweeper/Assets/flag.png");
    private IImage _imgSafe = Load("avares://Minesweeper/Assets/safe.png");

    public MainWindow()
    {
        InitializeComponent();   // xaml
        LoadNumberSprites();     // fill _numbers[]
        BuildButtons();          // make the ui cells
        RefreshUI();             // first paint
    }

    // load 1.jpg .. 8.jpg
    private void LoadNumberSprites()
    {
        for (int n = 1; n <= 8; n++)
        {
            var uri = $"avares://Minesweeper/Assets/{n}.jpg";
            if (AssetLoader.Exists(new Uri(uri)))
                _numbers[n] = Load(uri);
        }
    }

    // wrap bitmap so it stretches
    private static Image StretchImg(IImage src) =>
        new() { Source = src, Stretch = Stretch.Fill };

    private void BuildButtons()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                var btn = new Button { Tag = (r, c) };
                btn.Click          += Cell_LeftClick;   // left reveal
                btn.PointerPressed += Cell_RightClick;  // right flag
                BoardGrid.Children.Add(btn);
                _buttons[r, c] = btn;
            }
        }
    }


    // left click reveal
    private void Cell_LeftClick(object? sender, RoutedEventArgs e)
    {
        if (_gameOver || sender is not Button btn) return;
        var (r, c) = ((int, int))btn.Tag!;
        bool safe = _board.Reveal(r, c);
        RefreshUI();
        if (!safe)            EndGame("💣 boom you hit a mine");
        else if (_board.HasWon()) EndGame("🎉 you win");
    }

    // right click toggle flag
    private void Cell_RightClick(object? sender, PointerPressedEventArgs e)
    {
        if (_gameOver) return;
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed) return;
        if (sender is not Button btn) return;
        var (r, c) = ((int, int))btn.Tag!;
        _board.ToggleFlag(r, c);
        RefreshUI();
        e.Handled = true;   // stop left click from firing too
    }

    private void RefreshUI()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                var cell = _board.Grid[r, c];
                var btn  = _buttons[r, c];

                if (cell.isRevealed)
                {
                    btn.IsEnabled  = false;
                    btn.Background = Brushes.Transparent;

                    if (cell.isMine) btn.Content = StretchImg(_imgBomb);
                    else if (cell.Adjacent==0)btn.Content = StretchImg(_imgSafe);
                    else
                    {
                        var img = _numbers[cell.Adjacent];
                        btn.Content = img == null ? cell.Adjacent.ToString()  : StretchImg(img);   // fallback text
                    }
                }
                else
                {
                    btn.IsEnabled  = true;
                    btn.Content = cell.isFlagged ? StretchImg(_imgFlag) : null;
                    // keep default grey background so hover still shows
                }
            }
        }
    }

    private void EndGame(string msg)
    {
        _gameOver = true;

        // flip every mine so player can see the layout
        foreach (var cell in _board.Grid)
        {
            if (cell.isMine) cell.isRevealed = true;
        }

        RefreshUI();

        // small popup
        new Window
        {
            Width  = 260,
            Height = 120,
            Topmost = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new TextBlock
            {
                Text = msg,
                FontSize = 18,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center
            }
        }.ShowDialog(this);
    }
}

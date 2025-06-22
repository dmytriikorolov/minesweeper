using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Minesweeper.Game;

namespace Minesweeper;


public partial class MainWindow : Window
{

    private const int Rows  = 12;
    private const int Cols  = 12;
    private const int Mines = 25;   // todo later add a menu to switch these
    
    private Board _board = new(Rows, Cols, Mines);
    private  Button[,] _buttons = new Button[Rows, Cols];


    private bool _gameOver;
    

    private static IImage Load(string uri) => new Bitmap(AssetLoader.Open(new Uri(uri)));

    private IImage?[] _numbers = new IImage?[9];  
    private IImage _imgBomb = Load("avares://Minesweeper/Assets/bomb.png");
    private IImage _imgFlag = Load("avares://Minesweeper/Assets/flag.png");
    private IImage _imgSafe = Load("avares://Minesweeper/Assets/safe.png");

    public MainWindow()
    {
        InitializeComponent();   
        LoadNumberSprites();     
        BuildButtons();         
        RefreshUI();            
    }

    // load 1.jpg .. 8.jpg
    private void LoadNumberSprites()
    {
        for (int n = 1; n <= 8; n++)
        {
            var uri = $"avares://Minesweeper/Assets/{n}.jpg";
            if (AssetLoader.Exists(new Uri(uri)))
            {
                _numbers[n] = Load(uri);
            }
        }
    }
    
    private Image StretchImg(IImage src) => new() { Source = src, Stretch = Stretch.Fill };

    private void BuildButtons()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                var btn = new Button { Tag = (r, c) };
                btn.Click += Cell_LeftClick;   
                btn.PointerPressed += Cell_RightClick; 
                BoardGrid.Children.Add(btn);
                _buttons[r, c] = btn;
            }
        }
    }


    private void Cell_LeftClick(object? sender, RoutedEventArgs e)
    {
        if (_gameOver || sender is not Button btn) return;

        var (r, c) = ((int,int))btn.Tag!;
        var cell = _board.Grid[r, c];

        // new: helper-reveal on cell
        if (cell.isRevealed && cell.Adjacent > 0)
        {
            bool safe = _board.HelperReveal(r, c);
            RefreshUI();
            if (!safe) EndGame("💣 boom you hit a mine");
            else if (_board.HasWon()) EndGame("🎉 you win");
            return;
        }


        bool ok = _board.Reveal(r, c);
        RefreshUI();
        if (!ok) EndGame("💣 boom you hit a mine");
        else if (_board.HasWon()) EndGame("🎉 you win");
    }


    private void Cell_RightClick(object? sender, PointerPressedEventArgs e)
    {
        if (_gameOver) return;
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed) return;
        if (sender is not Button btn) return;

        var (r, c) = ((int,int))btn.Tag!;
        var cell = _board.Grid[r, c];
        
        if (cell.isRevealed && cell.Adjacent > 0)
        {
            _board.FlagHelper(r, c);
            RefreshUI();
            return;        
        }
        _board.ToggleFlag(r, c);
        RefreshUI();
        e.Handled = true;
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
                    btn.Background = Brushes.Transparent;
                    btn.IsEnabled = !cell.isMine;

                    if (cell.isMine) btn.Content = StretchImg(_imgBomb);
                    else if (cell.Adjacent==0)btn.Content = StretchImg(_imgSafe);
                    else
                    {
                        var img = _numbers[cell.Adjacent];
                        btn.Content = img == null ? cell.Adjacent.ToString() : StretchImg(img);
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
        
        foreach (var cell in _board.Grid)
        {
            if (cell.isMine) cell.isRevealed = true;
        }

        RefreshUI();
        
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

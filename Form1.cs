using System.Drawing;
using System.Windows.Forms;
using WormholeGame.Core;
using WormholeGame.Input;
using WormholeGame.GameObjects;

namespace WormholeGame;

public partial class Form1 : Form
{
    private Game game = null!;
    private MenuManager menuManager = null!;
    private InputManager inputManager = null!;
    private System.Windows.Forms.Timer gameTimer = null!;
    
    // Silent restart timer for menu deaths
    private int silentRestartTimer = 0;
    private const int SILENT_RESTART_DELAY = 600; // 10 seconds at 60 FPS (16ms intervals)

    public Form1()
    {
        InitializeComponent();
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // Set up the form
        this.Text = "Vortex Evader - Level 1";
        this.Size = new Size(Settings.Instance.Resolution.Width + 16, Settings.Instance.Resolution.Height + 39);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.KeyPreview = true;
        
        // Enable double buffering to reduce flicker
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer, true);
        
        // Initialize game components
        game = new Game(3); // Start at level 3 for interesting menu background
        menuManager = new MenuManager(this, game);
        menuManager.ShowMainMenu(); // Start with main menu
        inputManager = new InputManager();
        
        // Set up game timer
        gameTimer = new System.Windows.Forms.Timer();
        gameTimer.Interval = 16; // ~60 FPS
        gameTimer.Tick += GameLoop;
        gameTimer.Start();
        
        // Set up event handlers
        this.Paint += OnPaint;
        this.KeyDown += OnKeyDown;
        this.KeyUp += OnKeyUp;
        this.MouseMove += OnMouseMove;
        this.MouseClick += OnMouseClick;
        this.MouseDown += OnMouseDown;
        this.MouseUp += OnMouseUp;
    }
    
    private void GameLoop(object? sender, EventArgs e)
    {
        if (menuManager.HasActiveMenu())
        {
            game.ShowHUD = false; // Hide HUD during menus
            game.Update(); // Update game in background
            menuManager.Update(); // Update current menu
            
            // Handle player death during menu - silent restart after delay
            if (game.Player.IsDead())
            {
                silentRestartTimer++;
                if (silentRestartTimer >= SILENT_RESTART_DELAY)
                {
                    game.InitializeGame(1); // Silent restart
                    silentRestartTimer = 0;
                }
            }
            else
            {
                silentRestartTimer = 0;
            }
        }
        else
        {
            game.ShowHUD = true; // Show HUD during gameplay
            
            // Handle input for gameplay
            var (deltaX, deltaY) = inputManager.GetMovementInput(Player.DEFAULT_SPEED);
            game.MovePlayer(deltaX, deltaY);
            game.Update();
            
            // Update window title
            this.Text = $"Vortex Evader - Level {game.CurrentLevel.Number}";
            
            // Handle player death during gameplay - show game over immediately
            if (game.Player.IsDead())
            {
                menuManager.ShowGameOverMenu(game.CurrentLevel.Number, game.Score);
            }
        }

        this.Invalidate(); // Refresh display
    }
    
    private void OnPaint(object? sender, PaintEventArgs e)
    {
        // Game renders itself with scaling
        game.Render(e.Graphics, this);
        
        // Render current menu if visible
        menuManager.Render(e.Graphics, this);
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (menuManager.HasActiveMenu()) return; // Ignore input when menu is visible
        
        inputManager.OnKeyDown(e.KeyCode);
        
        if (e.KeyCode == Keys.Escape)
        {
            menuManager.ShowMainMenu();
        }
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (menuManager.HasActiveMenu()) return; // Ignore input when menu is visible
        
        inputManager.OnKeyUp(e.KeyCode);
    }
    
    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        menuManager.HandleMouseMove(e.X, e.Y, this);
    }
    
    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            menuManager.HandleMouseClick(e.X, e.Y, this);
        }
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            menuManager.HandleMouseDown(e.X, e.Y, this);
        }
        else if (e.Button == MouseButtons.Right)
        {
            // Handle right-click if needed
            // For now, just ignore it
        }
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            menuManager.HandleMouseUp(e.X, e.Y, this);
        }
    }
}

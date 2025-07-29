using System.Drawing;
using System.Windows.Forms;
using WormholeGame.Core;
using WormholeGame.Input;
using WormholeGame.GameObjects;

namespace WormholeGame;

public partial class Form1 : Form
{
    private Game game = null!;
    private Menu menu = null!;
    private GameOver gameOver = null!;
    private InputManager inputManager = null!;
    private System.Windows.Forms.Timer gameTimer = null!;

    public Form1()
    {
        InitializeComponent();
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // Set up the form
        this.Text = "Wormhole Game - Level 1";
        this.Size = new Size(Game.GAME_WIDTH + 16, Game.GAME_HEIGHT + 39);
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
        menu = new Menu();
        gameOver = new GameOver();
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
    }
    
    private void GameLoop(object? sender, EventArgs e)
    {
        if (menu.IsVisible)
        {
            game.ShowHUD = false; // Hide HUD during menu
            // Update game (but no player input due to menu)
            game.Update();
            
            // Update menu
            menu.Update();
        }
        else if (gameOver.IsVisible)
        {
            game.ShowHUD = false; // Hide HUD during game over
            // Game over screen - still update missiles for explosion effect
            game.Update();
            
            // Update game over screen
            gameOver.Update();
        }
        else
        {
            game.ShowHUD = true; // Show HUD during active gameplay
            // Active gameplay
            var (deltaX, deltaY) = inputManager.GetMovementInput(Player.DEFAULT_SPEED);
            game.MovePlayer(deltaX, deltaY);
            
            // Update game
            game.Update();

            // Check if player died - show game over screen
            if (game.Player.IsDead())
            {
                gameOver.Show(game.CurrentLevel.Number, game.Score);
            }
            
            // Update UI title
            this.Text = $"Wormhole Game - Level {game.CurrentLevel.Number}";
        }
        
        // Redraw
        this.Invalidate();
    }
    
    private void RestartGame()
    {
        game.RestartGame();
        gameOver.Hide();
        inputManager.Clear();
        this.Text = "Wormhole Game - Level 1";
    }
    
    private void OnPaint(object? sender, PaintEventArgs e)
    {
        // Game renders itself
        game.Render(e.Graphics);
        
        // Render menu on top if visible
        if (menu.IsVisible)
        {
            menu.Render(e.Graphics);
        }
        
        // Render game over screen on top if visible
        if (gameOver.IsVisible)
        {
            gameOver.Render(e.Graphics);
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (menu.IsVisible) return; // Ignore input when menu is visible
        
        inputManager.OnKeyDown(e.KeyCode);
        
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }
        if (e.KeyCode == Keys.R && !game.CanContinuePlaying())
        {
            RestartGame();
        }
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (menu.IsVisible) return; // Ignore input when menu is visible
        
        inputManager.OnKeyUp(e.KeyCode);
    }
    
    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (menu.IsVisible)
        {
            menu.HandleMouseMove(e.X, e.Y);
        }
        else if (gameOver.IsVisible)
        {
            gameOver.HandleMouseMove(e.X, e.Y);
        }
    }
    
    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (menu.IsVisible && e.Button == MouseButtons.Left)
        {
            if (menu.HandleMouseClick(e.X, e.Y))
            {
                // Play button was clicked - start the game!
                game.InitializeGame(1);
                this.Text = "Wormhole Game - Level 1";
            }
        }
        else if (gameOver.IsVisible && e.Button == MouseButtons.Left)
        {
            if (gameOver.HandleMouseClick(e.X, e.Y))
            {
                // Restart button was clicked
                RestartGame();
            }
        }
    }
}


using System.Drawing;
using System.Windows.Forms;
using WormholeGame.Core;
using WormholeGame.Input;
using WormholeGame.Rendering;
using WormholeGame.GameObjects;

namespace WormholeGame;

public partial class Form1 : Form
{
    private Game game = null!;
    private InputManager inputManager = null!;
    private GameRenderer renderer = null!;
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
        game = new Game();
        inputManager = new InputManager();
        renderer = new GameRenderer();
        
        // Set up game timer
        gameTimer = new System.Windows.Forms.Timer();
        gameTimer.Interval = 16; // ~60 FPS
        gameTimer.Tick += GameLoop;
        gameTimer.Start();
        
        // Set up event handlers
        this.Paint += OnPaint;
        this.KeyDown += OnKeyDown;
        this.KeyUp += OnKeyUp;
    }
    
    private void GameLoop(object? sender, EventArgs e)
    {
        // Handle input
        var (deltaX, deltaY) = inputManager.GetMovementInput(Player.DEFAULT_SPEED);
        game.MovePlayer(deltaX, deltaY);
        
        // Update game (includes collision detection)
        game.Update();
        
        // React to game state changes
        if (game.GameJustEnded)
        {
            HandleGameOver();
            return;
        }
        
        if (!game.CanContinuePlaying()) return;
        
        // Update UI title
        this.Text = $"Wormhole Game - Level {game.CurrentLevel.Number}";
        
        // Redraw
        this.Invalidate();
    }
    
    private void HandleGameOver()
    {
        gameTimer.Stop();
        game.AcknowledgeGameOver();
        MessageBox.Show($"Game Over! You reached level {game.CurrentLevel.Number}\nPress OK to restart.", 
                       "Game Over", MessageBoxButtons.OK);
        RestartGame();
    }
    
    private void RestartGame()
    {
        game.RestartGame();
        inputManager.Clear();
        this.Text = "Wormhole Game - Level 1";
        gameTimer.Start();
    }
    
    private void OnPaint(object? sender, PaintEventArgs e)
    {
        renderer.Render(e.Graphics, game);
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
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
        inputManager.OnKeyUp(e.KeyCode);
    }
}


using System.Drawing;
using System.Windows.Forms;
using WormholeGame.Core;
using WormholeGame.Input;
using WormholeGame.Rendering;
using WormholeGame.GameObjects;

namespace WormholeGame;

public partial class Form1 : Form
{
    private GameState gameState = null!;
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
        this.Size = new Size(GameState.GAME_WIDTH + 16, GameState.GAME_HEIGHT + 39);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.KeyPreview = true;
        
        // Enable double buffering to reduce flicker
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer, true);
        
        // Initialize game components
        gameState = new GameState();
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
        if (!gameState.IsRunning) return;
        
        // Handle input
        var (deltaX, deltaY) = inputManager.GetMovementInput(Player.DEFAULT_SPEED);
        gameState.MovePlayer(deltaX, deltaY);
        
        // Update game state
        gameState.UpdateGame();
        
        // Check collisions
        if (gameState.CheckCollisions())
        {
            GameOver();
            return;
        }
        
        // Update UI title
        this.Text = $"Wormhole Game - Level {gameState.Level}";
        
        // Redraw
        this.Invalidate();
    }
    
    private void GameOver()
    {
        gameState.GameOver();
        gameTimer.Stop();
        MessageBox.Show($"Game Over! You reached level {gameState.Level}\nPress OK to restart.", 
                       "Game Over", MessageBoxButtons.OK);
        RestartGame();
    }
    
    private void RestartGame()
    {
        gameState.RestartGame();
        inputManager.Clear();
        this.Text = "Wormhole Game - Level 1";
        gameTimer.Start();
    }
    
    private void OnPaint(object? sender, PaintEventArgs e)
    {
        renderer.Render(e.Graphics, gameState);
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        inputManager.OnKeyDown(e.KeyCode);
        
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }
        if (e.KeyCode == Keys.R && !gameState.IsRunning)
        {
            RestartGame();
        }
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        inputManager.OnKeyUp(e.KeyCode);
    }
}


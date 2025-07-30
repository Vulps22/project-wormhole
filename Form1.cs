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
    private SettingsMenu settingsMenu = null!;
    private CreditsMenu creditsMenu = null!;
    private GameOver gameOver = null!;
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
        this.Text = "Wormhole Game - Level 1";
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
        menu = new Menu();
        settingsMenu = new SettingsMenu(this);
        creditsMenu = new CreditsMenu();
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
        // Check if settings have changed and need reinitialization
        if (settingsMenu.IsDirty)
        {
            Console.WriteLine("🔄 Settings dirty flag detected - reinitializing game...");
            settingsMenu.ClearDirtyFlag();
            ReinitializeGame();
        }

        if (menu.IsVisible)
        {
            game.ShowHUD = false; // Hide HUD during menu
            // Update game (but no player input due to menu)
            game.Update();
            
            // Update menu
            menu.Update();
        }
        else if (settingsMenu.IsVisible)
        {
            game.ShowHUD = false; // Hide HUD during settings
            // Settings menu - still update game in background
            game.Update();
            
            // Update settings menu
            settingsMenu.Update();
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
            
            // Update UI title
            this.Text = $"Wormhole Game - Level {game.CurrentLevel.Number}";
        }
        
        // Check if player died - handle differently based on context
        if (game.Player.IsDead())
        {
            if (game.ShowHUD) // Player died during active gameplay
            {
                gameOver.Show(game.CurrentLevel.Number, game.Score);
                silentRestartTimer = 0; // Reset timer
            }
            else // Player died during menu - start silent restart timer
            {
                silentRestartTimer++;
                if (silentRestartTimer >= SILENT_RESTART_DELAY)
                {
                    RestartGame();
                    silentRestartTimer = 0; // Reset timer
                }
            }
        }
        else
        {
            silentRestartTimer = 0; // Reset timer when player is alive
        }
        
        // Redraw
        this.Invalidate();
    }
    
    private void RestartGame()
    {
        game.RestartGame();
        gameOver.Hide();
        inputManager.Clear();
        silentRestartTimer = 0; // Reset silent restart timer
        this.Text = "Wormhole Game - Level 1";
    }
    
    private void ReinitializeGame()
    {
        // Preserve current game state
        int currentLevel = game.CurrentLevel.Number;
        int currentScore = game.Score;
        bool wasPlayerDead = game.Player.IsDead();
        bool wasSettingsVisible = settingsMenu.IsVisible;
        bool wasCreditsVisible = creditsMenu.IsVisible;
        
        // Reinitialize the game with new settings
        game = new Game(currentLevel);
        game.SetScore(currentScore);
        
        // Reinitialize menus with updated form reference and recalculate layouts
        menu = new Menu();
        menu.RecalculateLayout();
        settingsMenu = new SettingsMenu(this);
        settingsMenu.RecalculateLayout();
        creditsMenu = new CreditsMenu();
        creditsMenu.RecalculateLayout();
        gameOver = new GameOver();
        gameOver.RecalculateLayout();
        
        // Restore menu visibility states
        if (wasSettingsVisible)
        {
            settingsMenu.Show();
            menu.Hide();
        }
        else if (wasCreditsVisible)
        {
            creditsMenu.Show();
            menu.Hide();
        }
        
        // If player was dead, show game over again
        if (wasPlayerDead)
        {
            gameOver.Show(currentLevel, currentScore);
        }
        
        Console.WriteLine($"Game reinitialized with new settings! Level: {currentLevel}, Score: {currentScore}");
    }
    
    private void OnPaint(object? sender, PaintEventArgs e)
    {
        // Game renders itself with scaling
        game.Render(e.Graphics, this);
        
        // Render menu on top if visible (with scaling)
        if (menu.IsVisible)
        {
            menu.Render(e.Graphics, this);
        }
        
        // Render settings menu on top if visible (with scaling)
        if (settingsMenu.IsVisible)
        {
            settingsMenu.Render(e.Graphics, this);
        }
        
        // Render credits menu on top if visible (with scaling)
        if (creditsMenu.IsVisible)
        {
            creditsMenu.Render(e.Graphics, this);
        }
        
        // Render game over screen on top if visible (with scaling)
        if (gameOver.IsVisible)
        {
            gameOver.Render(e.Graphics, this);
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
            menu.HandleMouseMove(e.X, e.Y, this);
        }
        else if (settingsMenu.IsVisible)
        {
            settingsMenu.HandleMouseMove(e.X, e.Y, this);
        }
        else if (creditsMenu.IsVisible)
        {
            creditsMenu.HandleMouseMove(e.X, e.Y, this);
        }
        else if (gameOver.IsVisible)
        {
            gameOver.HandleMouseMove(e.X, e.Y, this);
        }
    }
    
    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (menu.IsVisible && e.Button == MouseButtons.Left)
        {
            string result = menu.HandleMouseClick(e.X, e.Y, this);
            if (result == "play")
            {
                // Play button was clicked - start the game!
                game.InitializeGame(1);
                silentRestartTimer = 0; // Reset timer when starting new game
                this.Text = "Wormhole Game - Level 1";
            }
            else if (result == "settings")
            {
                // Settings button was clicked - show settings menu
                settingsMenu.Show();
                menu.Hide();
            }
            else if (result == "credits")
            {
                // Credits button was clicked - show credits menu
                creditsMenu.Show();
                menu.Hide();
            }
        }
        else if (settingsMenu.IsVisible && e.Button == MouseButtons.Left)
        {
            if (settingsMenu.HandleMouseClick(e.X, e.Y, this))
            {
                // Back button was clicked - return to main menu
                // Settings are applied automatically when changed
                settingsMenu.Hide();
                menu.Show();
            }
        }
        else if (creditsMenu.IsVisible && e.Button == MouseButtons.Left)
        {
            if (creditsMenu.HandleMouseClick(e.X, e.Y, this))
            {
                // Back button was clicked - return to main menu
                creditsMenu.Hide();
                menu.Show();
            }
        }
        else if (gameOver.IsVisible && e.Button == MouseButtons.Left)
        {
            if (gameOver.HandleMouseClick(e.X, e.Y, this))
            {
                // Restart button was clicked
                RestartGame();
            }
        }
    }
}


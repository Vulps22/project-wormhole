using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjectWormhole.Core
{
    public class MenuManager
    {
        private Menu? currentMenu;
        private Form form;
        private Game game;

        public Menu? CurrentMenu => currentMenu;

        public MenuManager(Form form, Game game)
        {
            this.form = form;
            this.game = game;
            
            // Start background music when MenuManager is created
            AudioManager.Instance.PlayBackgroundMusic("sleepless-city.mp3", true);
        }

        public void ShowMainMenu()
        {
            currentMenu?.Hide();
            currentMenu = new MainMenu(this);
            currentMenu.Show();
        }

        public void ShowSettingsMenu()
        {
            currentMenu?.Hide();
            currentMenu = new SettingsMenu(this, form);
            currentMenu.Show();
        }

        public void ShowCreditsMenu()
        {
            currentMenu?.Hide();
            currentMenu = new CreditsMenu(this);
            currentMenu.Show();
        }

        public void ShowGameOverMenu(int level, int score)
        {
            currentMenu?.Hide();
            currentMenu = new GameOverMenu(this, level, score);
            currentMenu.Show();
        }

        public void StartGame()
        {
            currentMenu?.Hide();
            currentMenu = null;
            game.InitializeGame(1);
        }

        public void QuitGame()
        {
            Application.Exit();
        }

        public void Update()
        {
            currentMenu?.Update();
        }

        public void HandleMouseMove(int mouseX, int mouseY, Form form)
        {
            currentMenu?.HandleMouseMove(mouseX, mouseY, form);
        }

        public void HandleMouseClick(int mouseX, int mouseY, Form form)
        {
            currentMenu?.HandleMouseClick(mouseX, mouseY, form);
        }

        public void HandleMouseDown(int mouseX, int mouseY, Form form)
        {
            currentMenu?.HandleMouseDown(mouseX, mouseY, form);
        }

        public void HandleMouseUp(int mouseX, int mouseY, Form form)
        {
            currentMenu?.HandleMouseUp(mouseX, mouseY, form);
        }

        public void Render(Graphics graphics, Form form)
        {
            currentMenu?.Render(graphics, form);
        }

        public void RecalculateLayout()
        {
            currentMenu?.RecalculateLayout();
        }

        public bool HasActiveMenu()
        {
            return currentMenu != null && currentMenu.IsVisible;
        }
    }
}

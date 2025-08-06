using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjectWormhole.Core
{
    public abstract class Menu
    {
        public bool IsVisible { get; protected set; }

        public virtual void Show()
        {
            IsVisible = true;
        }

        public virtual void Hide()
        {
            IsVisible = false;
        }

        public abstract void Update();
        public abstract void HandleMouseMove(int mouseX, int mouseY);
        public abstract void HandleMouseMove(int mouseX, int mouseY, Form form);
        public abstract bool HandleMouseClick(int mouseX, int mouseY);
        public abstract bool HandleMouseClick(int mouseX, int mouseY, Form form);
        public abstract void HandleMouseDown(int mouseX, int mouseY);
        public abstract void HandleMouseDown(int mouseX, int mouseY, Form form);
        public abstract void HandleMouseUp(int mouseX, int mouseY, Form form);
        public abstract void RecalculateLayout();
        public abstract void Render(Graphics graphics);
        public abstract void Render(Graphics graphics, Form form);
    }
}

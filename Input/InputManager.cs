using System.Collections.Generic;
using System.Windows.Forms;

namespace WormholeGame.Input
{
    public class InputManager
    {
        private HashSet<Keys> pressedKeys;
        
        public InputManager()
        {
            pressedKeys = new HashSet<Keys>();
        }
        
        public void OnKeyDown(Keys key)
        {
            pressedKeys.Add(key);
        }
        
        public void OnKeyUp(Keys key)
        {
            pressedKeys.Remove(key);
        }
        
        public bool IsKeyPressed(Keys key)
        {
            return pressedKeys.Contains(key);
        }
        
        public (int deltaX, int deltaY) GetMovementInput(int speed)
        {
            int deltaX = 0, deltaY = 0;
            
            if (IsKeyPressed(Keys.W) || IsKeyPressed(Keys.Up))
                deltaY = -speed;
            if (IsKeyPressed(Keys.S) || IsKeyPressed(Keys.Down))
                deltaY = speed;
            if (IsKeyPressed(Keys.A) || IsKeyPressed(Keys.Left))
                deltaX = -speed;
            if (IsKeyPressed(Keys.D) || IsKeyPressed(Keys.Right))
                deltaX = speed;
                
            return (deltaX, deltaY);
        }
        
        public void Clear()
        {
            pressedKeys.Clear();
        }
    }
}

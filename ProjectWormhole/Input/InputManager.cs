using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ProjectWormhole.Input
{
    public class InputManager
    {
        private HashSet<Keys> pressedKeys;
        
        // Developer easter egg tracking
        private DateTime easterEggStartTime;
        private bool easterEggActive;
        private readonly Keys[] easterEggKeys = { Keys.LShiftKey, Keys.LMenu, Keys.D, Keys.RShiftKey, Keys.RMenu, Keys.L };
        private const double EASTER_EGG_HOLD_TIME = 3.0; // 3 seconds
        private int lastLoggedSecond = -1; // Track last logged second for countdown
        
        public InputManager()
        {
            pressedKeys = new HashSet<Keys>();
            easterEggActive = false;
        }
        
        public void OnKeyDown(Keys key)
        {
            pressedKeys.Add(key);
            CheckEasterEggActivation();
        }
        
        public void OnKeyUp(Keys key)
        {
            pressedKeys.Remove(key);
            CheckEasterEggDeactivation();
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
            easterEggActive = false;
        }
        
        // Check if the developer easter egg has been triggered (all keys held for 3 seconds)
        public bool CheckDeveloperEasterEgg()
        {
            if (easterEggActive && AreAllEasterEggKeysPressed())
            {
                double timeHeld = (DateTime.Now - easterEggStartTime).TotalSeconds;
                
                // Display countdown messages
                int secondsRemaining = (int)(EASTER_EGG_HOLD_TIME - timeHeld);
                if (secondsRemaining >= 1 && secondsRemaining <= 3)
                {
                    // Only log once per second to avoid spam
                    int currentSecond = (int)timeHeld;
                    if (currentSecond != lastLoggedSecond)
                    {
                        Console.WriteLine($"Self Destruct in {secondsRemaining}...");
                        lastLoggedSecond = currentSecond;
                    }
                }
                
                return timeHeld >= EASTER_EGG_HOLD_TIME;
            }
            return false;
        }
        
        private void CheckEasterEggActivation()
        {
            if (!easterEggActive && AreAllEasterEggKeysPressed())
            {
                easterEggActive = true;
                easterEggStartTime = DateTime.Now;
                lastLoggedSecond = -1; // Reset countdown logging
            }
        }
        
        private void CheckEasterEggDeactivation()
        {
            if (easterEggActive && !AreAllEasterEggKeysPressed())
            {
                easterEggActive = false;
                lastLoggedSecond = -1; // Reset countdown logging
            }
        }
        
        private bool AreAllEasterEggKeysPressed()
        {
            foreach (Keys key in easterEggKeys)
            {
                if (!pressedKeys.Contains(key))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

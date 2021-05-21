using Indieteur.GlobalHooks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Controls;
using Indieteur.GlobalHooks;
using System.Diagnostics;
using static WorkStatus.Common.GlobalHookAccess;
using System.Threading;

namespace WorkStatus.Common
{
    public class KeyBoardMouseActivityTracker
    {
        public GlobalKeyHook globalKeyHook;
        public GlobalMouseHook globalMouseHook;
        public static Stopwatch sw = new Stopwatch();
        int timeInSeconds = 1;
        
        public KeyBoardMouseActivityTracker()
        {
            
            sw.Start();
            if (sw.Elapsed.Seconds == timeInSeconds)
            {
                sw.Stop();
            }
        }

        

        #region KeyBoardActivity

        public void KeyBoardActivity(bool PlayStop)
        {
            if (PlayStop)
            {
                //We check if globalKeyHook is instantiated or not.
                if (globalKeyHook == null)
                {
                    //If the globalKeyhook isn't created, we instantiate it and subscribe to the available events.
                    globalKeyHook = new GlobalKeyHook();
                    globalKeyHook.OnKeyDown += GlobalKeyHook_OnKeyDown;
                    globalKeyHook.OnKeyPressed += GlobalKeyHook_OnKeyPressed;
                    globalKeyHook.OnKeyUp += GlobalKeyHook_OnKeyUp;

                }
                else
                {
                    //If the globablKeyHook is already created, we dispose the instance.
                    globalKeyHook.Dispose();
                    globalKeyHook = null; //Probably not needed but just to be sure.
                }
            }
            else
            {
                //If the globablKeyHook is already created, we dispose the instance.
                globalKeyHook.Dispose();
                globalKeyHook = null; //Probably not needed but just to be sure.
            }
        }
      
      
        private void GlobalKeyHook_OnKeyUp(object sender, GlobalKeyEventArgs e)
        {

            KeyBoardTrackingCount(e, KeyEventType.OnKeyUp);
            return;
        }
        private void GlobalKeyHook_OnKeyPressed(object sender, GlobalKeyEventArgs e)
        {
            KeyBoardTrackingCount(e, KeyEventType.OnKeyPress);
            return;
        }
        private void GlobalKeyHook_OnKeyDown(object sender, GlobalKeyEventArgs e)
        {
            KeyBoardTrackingCount(e, KeyEventType.OnKeyDown);
            return;
        }

        #endregion

        #region MouseActivity

        public void MouseActivity(bool PlayStop)
        {
            if (PlayStop)
            {
                //Let's check if the globablMouseHook is instantiated.
                if (globalMouseHook == null)
                {
                    //If it is not created, we instantiate it and subscribe to the available events.
                    globalMouseHook = new GlobalMouseHook();
                    globalMouseHook.OnMouseWheelScroll += GlobalMouseHook_OnMouseWheelScroll;
                    globalMouseHook.OnButtonDown += GlobalMouseHook_OnButtonDown;
                    globalMouseHook.OnButtonUp += GlobalMouseHook_OnButtonUp;
                    globalMouseHook.OnMouseMove += GlobalMouseHook_OnMouseMove;
                }
                else
                {
                    //If there's already an instance of globalMouseHook, we have to destroy it.
                    globalMouseHook.Dispose();
                    globalMouseHook = null; //Probably not needed but just to be sure.
                }
            }
        }
        private void GlobalMouseHook_OnMouseWheelScroll(object sender, GlobalMouseEventArgs e)
        {
            if (e.wheelRotation != WheelRotation.Backwards)
            {
                MouseTrackingCount(e, MouseEventType.OnMouseWheelScrollBackwards);
            }
            if (e.wheelRotation != WheelRotation.Forwards)
            {
                MouseTrackingCount(e, MouseEventType.OnMouseWheelScrollForwards);
            }
            return;
        }
        private void GlobalMouseHook_OnMouseMove(object sender, GlobalMouseEventArgs e)
        {
            MouseTrackingCount(e, MouseEventType.OnMouseMove);
            return;
        }
        private void GlobalMouseHook_OnButtonUp(object sender, GlobalMouseEventArgs e)
        {
            MouseTrackingCount(e, MouseEventType.OnButtonUp);
            return;
        }

        private void GlobalMouseHook_OnButtonDown(object sender, GlobalMouseEventArgs e)
        {
            MouseTrackingCount(e, MouseEventType.OnButtonDown);
            return;
        }

        #endregion
    }
}

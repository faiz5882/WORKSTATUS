using Indieteur.GlobalHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;
using WorkStatus.ViewModels;

namespace WorkStatus.Common
{
    public enum hookType
    {
        keyHook,
        mouseHook,
        All
    }
    public static class GlobalHookAccess
    {
        public static int trackingInSecond = DateTime.Now.Second;
        public enum KeyEventType
        {
            OnKeyDown,
            OnKeyPress,
            OnKeyUp
        }
        public enum MouseEventType
        {
            OnMouseWheelScrollBackwards,
            OnMouseWheelScrollForwards,
            OnButtonDown,
            OnButtonUp,
            OnMouseMove
        }

        public static int KeyBoardTrackingCount(GlobalKeyEventArgs e, KeyEventType keyEventType)
        {

            if (trackingInSecond != DateTime.Now.Second)
            {
                trackingInSecond = DateTime.Now.Second;
                int Keycount = 0;
                //if (KeyBoardMouseActivityTracker.sw.IsRunning)
                //{
                //Check which event called this method and set the appropriate text for it. The calling method passed on the event type to us via keyEventType argument.
                switch (keyEventType)
                {
                    case KeyEventType.OnKeyDown:
                        Keycount++;
                        Storage.KeyBoradEventCount += Keycount;
                        break;
                    case KeyEventType.OnKeyPress:
                        Keycount++;
                        Storage.KeyBoradEventCount += Keycount;
                        break;
                    case KeyEventType.OnKeyUp:
                        Keycount++;
                        Storage.KeyBoradEventCount += Keycount;
                        break;
                }
                Storage.AverageEventCount += Keycount;
                Storage.LastProjectEventCountTime = Storage.IdleProjectTime;
                Storage.LastToDoEventCountTime = Storage.IdleToDoTime;
                DateTime oCurrentDate = DateTime.Now;
                Storage.StopTimeForDB = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
               // LogFile.WriteaActivityLog( DateTime.Now +"\n" + "IdleProjectTime : " + Storage.IdleProjectTime + "\n" + "IdleToDoTime : " + Storage.IdleToDoTime + "\n" + Storage.StopTimeForDB);
                // }
            }
            return Storage.KeyBoradEventCount;
        }

        public static int MouseTrackingCount(GlobalMouseEventArgs e, MouseEventType mouseEventType)
        {
            if (trackingInSecond != DateTime.Now.Second)
            {

                trackingInSecond = DateTime.Now.Second;
                int Mousecount = 0;
                // if (KeyBoardMouseActivityTracker.sw.IsRunning)
                //{
                //Check which event called this method and set the appropriate text for it.
                //The calling method passed on the event type to us via mouseEventType argument.
                switch (mouseEventType)
                {
                    case MouseEventType.OnMouseWheelScrollBackwards:
                        Mousecount++;
                        Storage.MouseEventCount += Mousecount;
                        break;
                    case MouseEventType.OnMouseWheelScrollForwards:
                        Mousecount++;
                        Storage.MouseEventCount += Mousecount;
                        break;
                    case MouseEventType.OnMouseMove:
                        Mousecount++;
                        Storage.MouseEventCount += Mousecount;
                        break;
                    case MouseEventType.OnButtonUp:
                        Mousecount++;
                        Storage.MouseEventCount += Mousecount;
                        break;
                    case MouseEventType.OnButtonDown:
                        Mousecount++;
                        Storage.MouseEventCount += Mousecount;
                        break;
                }
                Storage.AverageEventCount += Mousecount;
                Storage.LastProjectEventCountTime = Storage.IdleProjectTime;
                Storage.LastToDoEventCountTime = Storage.IdleToDoTime;
                DateTime oCurrentDate = DateTime.Now;
                Storage.StopTimeForDB = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
               // LogFile.WriteaActivityLog(DateTime.Now + "\n" + "IdleProjectTime : " + Storage.IdleProjectTime + "\n" + "IdleToDoTime : " + Storage.IdleToDoTime + "\n" + Storage.StopTimeForDB);
                // }
            }
            return Storage.MouseEventCount;
        }
    }

}

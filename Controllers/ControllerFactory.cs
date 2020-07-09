using Communications;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Input
{
    public class ControllerFactory
    {
        public static Controller MakeDefault(Com com)
        {
            //ControllerKeyboard c = 
            //window.onKeyDown = (o, e) => { c.keyDown((KeyEventArgs)e); };
            //window.onKeyUp = (o, e) => { c.keyDown((KeyEventArgs)e); };
            return new ControllerKeyboard(com);
        }

        public static Controller MakeHandController(Com com)
        {
            return new ControllerHand(com);
        }

        public static Controller MakeKeyboardController(Com com)
        {
            return new ControllerKeyboard(com);
        }
    }
}

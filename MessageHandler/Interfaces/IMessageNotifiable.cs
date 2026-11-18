using SuperVisionObjectDesktop.Network.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperVisionObjectDesktop.MessageHandler.Interfaces
{
    public interface IMessageNotifiable
    {
        event EventHandler MessageReceived;
        void Notify(DialogMessage dialogMessage);
    }
}

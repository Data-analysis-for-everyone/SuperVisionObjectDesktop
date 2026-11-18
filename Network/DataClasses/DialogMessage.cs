using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Network.DataClasses
{
    public enum MessageType { INFO, ERROR, WARNING }
    public class DialogMessage : EventArgs
    {
        public string? Message { get; set; }
        public MessageType MessageType { get; set; }
        public string? Method { get; set; }
    }
}

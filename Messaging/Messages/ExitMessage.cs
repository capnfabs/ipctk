using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPCTK.Messaging.Messages
{
    /// <summary>
    /// Tell the external process to exit.
    /// </summary>
    [Serializable]
    class ExitMessage : Message
    {
        public override string ToString()
        {
            return "[ExitMessage]";
        }
    }
}

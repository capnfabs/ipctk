using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using IPCTK.Messaging.Messages;

namespace IPCTK.Messaging
{
    /// <summary>
    /// Handles receiving of messages.
    /// </summary>
    class MessageReceiver
    {
        Stream reader;
        BinaryFormatter formatter;

        public MessageReceiver(Stream reader)
        {
            this.reader = reader;
            formatter = new BinaryFormatter();
        }

        protected MessageReceiver() { }

        public Message WaitForMessage()
        {
            Message m = (Message)formatter.Deserialize(reader);
            return m;
        }
    }
}

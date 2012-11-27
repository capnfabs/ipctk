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
    /// Handles sending of messages.
    /// </summary>
    class MessageSender
    {
        Stream writer;
        BinaryFormatter formatter;

        protected MessageSender() { }

        public MessageSender(Stream stream)
        {
            writer = stream;
            formatter = new BinaryFormatter();
        }

        public void SendMessage(Message m)
        {
            formatter.Serialize(writer, m);
        }
    }
}

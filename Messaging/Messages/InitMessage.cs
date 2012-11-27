using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPCTK.Messaging.Messages
{
    /// <summary>
    /// Tell the external process to initialise.
    /// </summary>
    [Serializable]
    class InitMessage : Message
    {
        private InitMessage() { }
        public InitMessage(Type t, Object[] args)
        {
            InstanceType = t;
            Arguments = args;
        }

        public Type InstanceType { set; get; }
        public Object[] Arguments { set; get; }

        public override string ToString()
        {
            return String.Format("[InitMessage: <{0}> | {1}", InstanceType.ToString(), String.Join("/", Arguments.Select(arg => arg.ToString())));
        }
    }
}

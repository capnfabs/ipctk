using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace IPCTK.Messaging.Messages
{
    /// <summary>
    /// Tell the external process to run a method.
    /// </summary>
    [Serializable]
    class MethodCallMessage : Message
    {
        public MethodBase Method { set; get; }
        public Object[] Arguments { set; get; }

        protected MethodCallMessage() {}
        public MethodCallMessage(MethodBase method, Object[] args)
        {
            Method = method;
            Arguments = args;
        }

        public override string ToString()
        {
            return String.Format("[Method: {0} | {1}]", Method.Name, String.Join("/", Arguments.Select(arg => arg.ToString())));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPCTK.Messaging.Messages
{
    /// <summary>
    /// Tell the internal bits and pieces that we're giving them a return value.
    /// </summary>
    [Serializable]
    class ReturnValueMessage : Message
    {
        public Object ReturnValue { set; get; }

        public ReturnValueMessage(object retVal)
        {
            ReturnValue = retVal;
        }

        public override string ToString()
        {
            return "[ReturnVal: " + ReturnValue.ToString() + "]";
        }
    }
}

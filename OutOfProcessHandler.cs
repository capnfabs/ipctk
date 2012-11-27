using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using IPCTK.Messaging;
using IPCTK.Messaging.Messages;

namespace IPCTK
{
    /// <summary>
    /// From the external process, handles communication with the internal code.
    /// Use is as simple as writing <see cref="OutOfProcessHandler.Loop()"/>,
    /// passing <c>args[0]</c> and <c>args[1]</c> as parameters.
    /// </summary>
    public static class OutOfProcessHandler
    {
        // Pipes for communication with InProcessAgent.
        static PipeStream pipeIn;
        static PipeStream pipeOut;

        //Message sender and receiver.
        static MessageSender messageSender;
        static MessageReceiver messageReceiver;

        // indicates whether we should keep looping.
        static bool thinking = true;

        // the object we're managing in this process.
        static Object instanceObject;

        /// <summary>
        /// The main loop. Arguments are used to setup Inter-Process Communication, so follow the instructions!
        /// </summary>
        /// <param name="arg0">The first command line argument to the program.</param>
        /// <param name="arg1">The second command line argument to the program</param>
        public static void Loop(String arg0, String arg1)
        {
            pipeIn = new AnonymousPipeClientStream(PipeDirection.In, arg0);
            pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, arg1);
            messageReceiver = new MessageReceiver(pipeIn);
            messageSender = new MessageSender(pipeOut);

            while (thinking)
            {
                Message m = messageReceiver.WaitForMessage();
                if (m is InitMessage)
                    RunInitMessage((InitMessage)m);
                else if (m is ExitMessage)
                    RunExitMessage((ExitMessage)m);
                else if (m is MethodCallMessage)
                    RunMethodCallMessage((MethodCallMessage)m);
                else
                    throw new Exception("Shouldn't have received that kind of message!");
            }
        }

        // Handles execution of messages that require the running of a method.
        private static void RunMethodCallMessage(MethodCallMessage methodCallMessage)
        {
            Object retVal = methodCallMessage.Method.Invoke(instanceObject, methodCallMessage.Arguments);
            messageSender.SendMessage(new ReturnValueMessage(retVal));
        }

        //Handles execution of messages that call for external process exit.
        private static void RunExitMessage(ExitMessage exitMessage)
        {
            thinking = false;
        }

        //Handles execution of initialisation messages.
        private static void RunInitMessage(InitMessage initMessage)
        {
            if (instanceObject != null)
                throw new Exception("Can only be initialised once!");
            instanceObject = Activator.CreateInstance(initMessage.InstanceType, initMessage.Arguments);
        }
    }
}

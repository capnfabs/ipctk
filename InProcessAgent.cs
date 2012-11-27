using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

using System.Reflection;

using IPCTK.Messaging;
using IPCTK.Messaging.Messages;

namespace IPCTK
{
    /// <summary>
    /// Manages inter-process communication from the In-Process side.
    /// </summary>
    class InProcessAgent : IDisposable
    {
        /// <summary>
        /// A static directory that manages all links between IPCTK-enabled
        /// objects and InProcessAgents.
        /// </summary>
        private static Dictionary<Object, InProcessAgent> directory;

        /// <summary>
        /// Sends messages to the external process.
        /// </summary>
        private MessageSender messageSender;

        /// <summary>
        /// Receives messages from the external process.
        /// </summary>
        private MessageReceiver messageReceiver;

        /// <summary>
        /// Retrieve the <see cref="InProcessAgent"/> associated with the
        /// IPCTK-enabled object <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to retrieve the associated
        /// <see cref="InProcessAgent"/> for.</param>
        /// <returns>The assocated <see cref="InProcessAgent"/>.</returns>
        public static InProcessAgent GetIPCAgent(Object obj)
        {
            if (!directory.ContainsKey(obj))
            {
                throw new ArgumentException("That object has no IPCInProcessAgent assocated. Call NewIPCAgent first.", "obj");
            }
            return directory[obj];
        }

        /// <summary>
        /// Generate a new <see cref="InProcessAgent"/>, associated with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to assocated the <see cref="InProcessAgent"/> with.</param>
        /// <returns>A new <see cref="InProcessAgent"/>.</returns>
        public static InProcessAgent NewIPCAgent(Object obj)
        {
            var x = new InProcessAgent(obj.GetType());
            directory.Add(obj, x);
            return x;
        }

        //Static Constructor.
        static InProcessAgent()
        {
            directory = new Dictionary<object, InProcessAgent>();
        }

        //Disable the no-argument constructor.
        protected InProcessAgent() { }

        //The process that this InProcessAgent should communicate with.
        protected Process clientProcess;

        //Pipes used for communication with external process.
        protected AnonymousPipeServerStream pipeOut, pipeIn;

        /// <summary>
        /// Constructor. Starts external process.
        /// </summary>
        /// <param name="t">The type of the external class to manage.</param>
        protected InProcessAgent(Type t)
        {
            //Fire up instance of exe, with pipes set up.
            clientProcess = new Process();
            clientProcess.StartInfo.FileName = t.Assembly.Location;
            pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            clientProcess.StartInfo.Arguments = pipeOut.GetClientHandleAsString() + " " + pipeIn.GetClientHandleAsString();
            clientProcess.StartInfo.UseShellExecute = false;
            clientProcess.Start();
            pipeOut.DisposeLocalCopyOfClientHandle();
            pipeIn.DisposeLocalCopyOfClientHandle();

            //initialise an instance of the object

            messageSender = new MessageSender(pipeOut);
            messageReceiver = new MessageReceiver(pipeIn);
        }

        public Object Call(MethodBase method, Object[] args)
        {
            messageSender.SendMessage(new MethodCallMessage(method, args));
            Message returnValue = messageReceiver.WaitForMessage();
            if (!(returnValue is ReturnValueMessage))
                throw new Exception("Unexpected reply from out-of-process client");

            return (returnValue as ReturnValueMessage).ReturnValue;
        }

        /// <summary>
        /// Tell the external process to construct a new object of type <paramref name="t"/>.
        /// </summary>
        /// <param name="t">The type of the object to instantiate remotely.</param>
        /// <param name="args">Arguments to pass to the constructor.</param>
        public void Construct(Type t, Object[] args)
        {
            messageSender.SendMessage(new InitMessage(t, args));
        }


        #region Disposing
        private bool disposed = false;

        /// <summary>
        /// Free up resources. Politely exits the external process.
        /// </summary>
        public void Dispose()
        {
            Dispose(DisposeMethod.PoliteDispose);
        }

        /// <summary>
        /// Free up resources. Has the option to kill the external process.
        /// </summary>
        /// <param name="disposeMethod">A value indicating whether the external process should be exited politely (via a signal) or killed.</param>
        public void Dispose(DisposeMethod disposeMethod)
        {
            if (!disposed)
            {
                switch (disposeMethod)
                {
                    case DisposeMethod.PoliteDispose:
                        //send terminate signal.
                        messageSender.SendMessage(new ExitMessage());
                        //memory cleanup
                        pipeIn.Close();
                        pipeOut.Close();
                        break;
                    case DisposeMethod.KillDispose:
                        pipeIn.Close();
                        pipeOut.Close();
                        clientProcess.Kill();
                        break;
                }
                disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Indicates whether the external process should be exited politely (via a signal) or killed.
    /// </summary>
    enum DisposeMethod
    {
        /// <summary>
        /// Exit the external process politely (send a signal asking for exit).
        /// </summary>
        PoliteDispose,

        /// <summary>
        /// Exit the external process by forcefully terminating it.
        /// </summary>
        KillDispose
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using PostSharp.Aspects;
using PostSharp.Extensibility;

namespace IPCTK
{
    /// <summary>
    /// Mark your class that should run out-of-process with <see cref="RunOutOfProcessAttribute"/>,
    /// and all the hard work will be done for you ;)
    /// </summary>
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Method)]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RunOutOfProcessAttribute : MethodInterceptionAspect
    {
        //indicates whether this instance of the attribute is in-process or out of process.
        // determined at runtime initialisation.
        private static bool IsOutOfProcess;

        /// <summary>
        /// Intercepts all method calls.
        /// </summary>
        /// <param name="args"></param>
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            if (IsOutOfProcess) //if we're running out of process,
            {
                args.Proceed(); //just do what the code says.
                return;
            }

            InProcessAgent ipcAgent;

            //if the method we're processing is marked with [Initializer],
            if (MethodMarkedWith(args.Method, typeof(InitializerAttribute)))
            {
                ipcAgent = InProcessAgent.NewIPCAgent(args.Instance); // Start a new external process
                ipcAgent.Construct(args.Instance.GetType(), args.Arguments.ToArray()); //initialise the class on the other end.
                return;
            }

            //find the already existing InProcessAgent.
            ipcAgent = InProcessAgent.GetIPCAgent(args.Instance);

            if (MethodIsDispose(args.Method)) //If this method is the Dispose() Method,
            {
                // AND we've marked the Dispose() method with [KillExternalProcess],
                if (MethodMarkedWith(args.Method,typeof(KillExternalProcessAttribute)))
                {
                    ipcAgent.Dispose(DisposeMethod.KillDispose); //Kill the external process impolitely.
                }
                else //otherwise
                {
                    ipcAgent.Dispose(DisposeMethod.PoliteDispose); //Politely ask the external process to exit.
                }
                return;
            }
            // If we're not intialising or disposing, simply tell the external process to run the command.
            args.ReturnValue = ipcAgent.Call(args.Method, args.Arguments.ToArray());
        }

        //Tests whether the method is marked with the given attribute.
        private bool MethodMarkedWith(MethodBase methodInfo, Type markingAttribute)
        {
            return methodInfo.GetCustomAttributes(markingAttribute,false).Length > 0;
        }

        //Tests whether the method implements IDisposable.Dispose().
        private bool MethodIsDispose(MethodBase methodInfo)
        {
            return methodInfo.DeclaringType
                .GetInterfaceMap(typeof(IDisposable)).TargetMethods
                .Any(x => x == methodInfo);
        }

        //Determines whether this copy of the attribute is running in or out of process.
        public override void RuntimeInitialize(MethodBase method)
        {
            Assembly declaringAssembly = method.DeclaringType.Assembly;
            Assembly currentExe = System.Reflection.Assembly.GetEntryAssembly();
            IsOutOfProcess = declaringAssembly == currentExe;
            
            base.RuntimeInitialize(method);
        }

        //Applies the attribute only to:
        // classes implementing IDisposable,
        // only on public methods,
        // or on methods marked with [Initializer]
        public override bool CompileTimeValidate(MethodBase method)
        {
            if (method.DeclaringType.GetInterface("IDisposable") == null)
                throw new InvalidAnnotationException("Class must implement IDisposable " + method.DeclaringType);

            if (!method.Attributes.HasFlag(MethodAttributes.Public) && //if method is not public
                !MethodMarkedWith(method,typeof(InitializerAttribute))) //method is not initialiser
                return false; //silently ignore.

            return true;
        }
    }
}

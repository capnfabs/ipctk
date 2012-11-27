#IPCTK#

A PostSharp-driven toolkit for running code out-of-process.

##Features##

- Easy to set up and use
- Effectively isolates code sections from the rest of the process

##Why?##

I was trying to use a class from an old COM DLL in a multhreaded application, only to find that didn't support multithreading. The problem can be avoided by wrapping the COM class in an external process. Rather than clutter existing classes with interprocess communication (IPC) code, I wrote an Aspect-driven toolkit so that implementation is almost as simple as adding `[RunOutOfProcess]` to the top of the class definition.

##Obtaining a Copy##
Source code is available on [github](https://github.com/capnfabs/ipctk), and binaries are available with NuGet. Please note: IPCTK requires [PostSharp Starter Edition](http://www.sharpcrafters.com/postsharp/download), available free-of-charge.

##How do I set up a class to run out-of-process?##

1. Add a new Exe project to your solution.
3. Add classes to the Exe.
4. For each class that you want to run out-of-process:
4.a. Mark the class with the `[RunOutOfProcess]` attribute.
4.b. Make sure the class implements `IDisposable`.
4.c. Make sure that _no initialisation logic_ is in the constructor - instead, just put in a call to an `Init()` function with the same parameters as the constructor. (this is a workaround for a PostSharp quirk)
4.d. Mark your `Init()` function with the `[Initializer]` attribute.
5. In the Exe entry point, simply add a call to `IPCTK.OutOfProcessHandler.Loop(arg0, arg1);`, where `arg0` and `arg1` are the first and second command-line parameters respectively.

That's it! Now, you can add the Exe as a reference to your existing project and use the classes within. Every time your existing project attempts to communicate with your new class, IPCTK will take care of forwarding class instantiation, method calls, and disposal to a child process. Don't forget to `Dispose()` your class instance when you're done to free up any resources and terminate the external process.

##A Couple of Gotchas##
IPCTK uses .net `BinaryFormatter` serialization to send data to external processes. As such:

- All parameters to and return values from class methods need to be of types marked `[Serializable]`, and
- You should make sure that you setup the in-process/out-of-process boundary so that as little data is transferred as possible.

##Questions, comments, bugs?##

Get in touch with me at [capnfabs.net](http://www.capnfabs.net/contact/).
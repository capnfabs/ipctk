using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPCTK
{
    /// <summary>
    /// Move all initialization logic out of the constructor and into a
    /// a seperate function, marked with <see cref="InitializerAttribute"/>.
    /// Then, call this function from your constructor. IPCTK looks for
    /// this attribute on a class function, and without it, the external
    /// process will never be started.
    /// </summary>
    public class InitializerAttribute : Attribute
    {
    }
}

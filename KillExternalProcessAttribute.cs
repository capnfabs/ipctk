using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPCTK
{
    /// <summary>
    /// Mark your <see cref="IDisposable.Dispose"/> method with <see cref="KillExternalProcessAttribute"/>
    /// to indicate that you want the external process to be forcefully killed instead of politely asked
    /// to exit. Should only be used in extreme circumstances - use at your own risk!
    /// </summary>
    public class KillExternalProcessAttribute : Attribute
    {
    }
}

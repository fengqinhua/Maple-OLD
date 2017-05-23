using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple
{
    public static class ExceptionExtensions
    {
        public static bool IsFatal(this Exception ex)
        {
            return
                ex is StackOverflowException ||
                ex is OutOfMemoryException ||
                ex is AccessViolationException ||
                ex is AppDomainUnloadedException;
            //ex is ThreadAbortException 
            //ex is SecurityException 
            //ex is SEHException
            //ex is OrchardSecurityException 
        }
    }
}

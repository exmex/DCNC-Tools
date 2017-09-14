using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLibrary.PortableExecutable;
using InjectionLibrary;

namespace JLibrary
{
    class EXAMPLEUSAGE
    {
        public void inject(int pID, Byte[] dllbytes)
        {
            InjectionMethod method = InjectionMethod.Create(InjectionMethodType.Standard); //InjectionMethodType.Standard //InjectionMethodType.ManualMap //InjectionMethodType.ThreadHijack
            IntPtr zero = IntPtr.Zero;
            using (PortableExecutable.PortableExecutable executable = new PortableExecutable.PortableExecutable(dllbytes))
            {
                zero = method.Inject(executable, pID);
            }
            if (zero != IntPtr.Zero)
            {
                //BAIL HERE - ERROR
            }
            else if (method.GetLastError() != null)
            {
                //ERROR OCCURED
            }
            //SUCCESS
        }
    }
}

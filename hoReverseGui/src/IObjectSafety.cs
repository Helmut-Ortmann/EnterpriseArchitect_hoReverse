using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace hoReverse.Reverse
{
    [ComImport()]
    [Guid("8A382576-8FBC-4882-8BFE-8862CB3ED05F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IObjectSafety
    {
        [PreserveSig()]
        int GetInterfaceSafetyOptions(ref Guid riid, out int pdwSupportedOptions, out int pdwEnabledOptions);

        [PreserveSig()]
        int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions);
    }
}

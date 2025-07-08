using System;
using System.Runtime.InteropServices;

namespace SwapUpdater.Native
{
    internal static class NativeMethods
    {
        [DllImport("MetaQuotes.MT5ManagerAPI64.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern MTRetCode MTManagerVersion(ref uint version);
    }

    public enum MTRetCode : uint
    {
        MT_RET_OK = 0,
        MT_RET_ERROR = 1,
        // добавь при необходимости другие коды ошибок
    }
}

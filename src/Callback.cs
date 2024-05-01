using System.Runtime.InteropServices;

namespace ArmaExtensionDotNet
{
    internal class Callback
    {
        private const string ExtensionName = "ArmaExtensionDotNet";

        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        private ExtensionCallback? callback;

        public void Register(ExtensionCallback newCallback)
        {
            callback = newCallback;
        }

        public void Log(string log)
        {
            callback?.Invoke(ExtensionName, "writeLog", log);
        }

        public void ExecSqf(string code)
        {
            callback?.Invoke(ExtensionName, "execSqf", code);
        }
    }
}

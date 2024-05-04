using System.Runtime.InteropServices;

namespace ArmaExtensionDotNet
{
    internal class Client(string extensionName)
    {
        private readonly string extensionName = extensionName;

        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        private ExtensionCallback? callback;

        public void RegisterCallback(IntPtr functionPointer)
        {
            callback = Marshal.GetDelegateForFunctionPointer<ExtensionCallback>(functionPointer);
        }

        public void Log(string log)
        {
            callback?.Invoke(extensionName, "writeLog", log);
        }

        public string ExecSqf(string code)
        {
            var requestId = Guid.NewGuid().ToString();
            var payload = String.Format("\"{0}\"|{1}", requestId, code);
            callback?.Invoke(extensionName, "execSqf", payload);
            return requestId;
        }
    }
}

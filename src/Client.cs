using System.Runtime.InteropServices;

namespace ArmaExtensionDotNet
{
    internal class Client
    {
        public const string ExtensionName = "ArmaExtensionDotNet";

        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        private ExtensionCallback? callback;

        public void Register(IntPtr functionPointer)
        {
            callback = Marshal.GetDelegateForFunctionPointer<ExtensionCallback>(functionPointer);
        }

        public void Log(string log)
        {
            callback?.Invoke(ExtensionName, "writeLog", log);
        }

        public string ExecSqf(string code)
        {
            var requestId = Guid.NewGuid().ToString();
            // var payload = String.Format("[\"{0}\",\"{1}\"]", requestId, code);
            var payload = String.Format("\"{0}\"|{1}", requestId, code);
            callback?.Invoke(ExtensionName, "execSqf", payload);
            return requestId;
        }
    }
}

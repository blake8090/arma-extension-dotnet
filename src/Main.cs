using System.Runtime.InteropServices;
using System.Text;

namespace ArmaExtensionDotNet
{
    public class Main
    {
        private static int numCalls = 0;

        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        private static ExtensionCallback? callback;

        /// <summary>
        /// Called only once when Arma 3 loads the extension.
        /// </summary>
        /// <param name="func">Pointer to Arma 3's callback function</param>
        [UnmanagedCallersOnly(EntryPoint = "RVExtensionRegisterCallback")]
        public unsafe static void RVExtensionRegisterCallback(IntPtr func)
        {
            callback = Marshal.GetDelegateForFunctionPointer<ExtensionCallback>(func);
        }

        /// <summary>
        /// Called only once when Arma 3 loads the extension.
        /// The output will be written in the RPT logs.
        /// </summary>
        /// <param name="output">A pointer to the output buffer</param>
        /// <param name="outputSize">The maximum length of the buffer (always 32 for this particular method)</param>
        [UnmanagedCallersOnly(EntryPoint = "RVExtensionVersion")]
        public unsafe static void RVExtensionVersion(char* output, int outputSize)
        {
            WriteOutput(output, "ArmaExtensionDotNet v1.0");
        }

        /// <summary>
        /// The entry point for the default "callExtension" command.
        /// </summary>
        /// <param name="output">A pointer to the output buffer</param>
        /// <param name="outputSize">The maximum size of the buffer (20480 bytes)</param>
        /// <param name="function">The function identifier passed in "callExtension"</param>
        [UnmanagedCallersOnly(EntryPoint = "RVExtension")]
        public unsafe static void RVExtension(char* output, int outputSize, char* function)
        {
            numCalls++;

            string result = String.Format("Function: {0} - Total calls to extension: {1}", GetString(function), numCalls);
            WriteOutput(output, result);

            callback?.Invoke("ArmaExtensionDotNet", "myFunction", "example data");
        }

        /// <summary>
        /// The entry point for the "callExtension" command with function arguments.
        /// </summary>
        /// <param name="output">A pointer to the output buffer</param>
        /// <param name="outputSize">The maximum size of the buffer (20480 bytes)</param>
        /// <param name="function">The function identifier passed in "callExtension"</param>
        /// <param name="argv">The args passed to "callExtension" as a string array</param>
        /// <param name="argc">Number of elements in "argv"</param>
        /// <returns>The return code</returns>
        public unsafe static int RVExtensionArgs(char* output, int outputSize, char* function, char** argv, int argc)
        {
            numCalls++;

            List<String> parameters = [];
            for (int i = 0; i < argc; i++)
            {
                parameters.Add(GetString(argv[i]));
            }

            string result = String.Format("Function: {0} - Params: {1} - Total extension calls: {2}", GetString(function), SerializeList(parameters), numCalls);
            WriteOutput(output, result);

            return 0;
        }

        /// <summary>
        /// Reads a string from the given pointer. 
        /// If the pointer is null, a default value will be returned instead.
        /// </summary>
        /// <param name="pointer">The pointer to read</param>
        /// <param name="defaultValue">The string's default value</param>
        /// <returns>The marshalled string</returns>
        private unsafe static string GetString(char* pointer, string defaultValue = "")
        {
            return Marshal.PtrToStringAnsi((IntPtr)pointer) ?? defaultValue;
        }

        /// <summary>
        /// Serializes a list of strings into a string representing a valid Arma 3 array.
        /// </summary>
        /// <param name="list">The list of strings to serialize</param>
        /// <returns>A string representing an Arma 3 array</returns>
        private static string SerializeList(List<String> list)
        {
            var content = string.Join(",", [.. list]);
            return string.Format("[{0}]", content);
        }

        /// <summary>
        /// Writes the given payload to the output buffer using the provided pointer.
        /// Ensures that the payload string is always null terminated.
        /// </summary>
        /// <param name="output">A pointer to the output buffer</param>
        /// <param name="payload">The payload to write</param>
        private unsafe static void WriteOutput(char* output, string payload)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(payload + '\0');
            Marshal.Copy(bytes, 0, (IntPtr)output, bytes.Length);
        }
    }
}

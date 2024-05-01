using System.Runtime.InteropServices;
using System.Text;
using static ArmaExtensionDotNet.Callback;

namespace ArmaExtensionDotNet
{
    public class Main
    {
        private static int numCalls = 0;

        private static readonly Callback callback = new();

        /// <summary>
        /// Called only once when Arma 3 loads the extension.
        /// </summary>
        /// <param name="func">Pointer to Arma 3's callback function</param>
        [UnmanagedCallersOnly(EntryPoint = "RVExtensionRegisterCallback")]
        public unsafe static void RVExtensionRegisterCallback(IntPtr func)
        {
            callback.Register(Marshal.GetDelegateForFunctionPointer<ExtensionCallback>(func));
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

            var func = GetString(function);
            if (func == "runSqfTest")
            {
                Task.Run(() =>
                {
                    callback.Log("runSqfTest - begin");

                    callback.ExecSqf("systemChat \"runSqfTest - Sleeping for 2 seconds\"");
                    Thread.Sleep(2000);

                    callback.ExecSqf("getPos player");

                    callback.Log("runSqfTest - end");
                });
                WriteOutput(output, "started async task");
            }
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
        [UnmanagedCallersOnly(EntryPoint = "RVExtensionArgs")]
        public unsafe static int RVExtensionArgs(char* output, int outputSize, char* function, char** argv, int argc)
        {
            numCalls++;

            var func = GetString(function);

            List<String> parameters = [];
            for (int i = 0; i < argc; i++)
            {
                parameters.Add(GetString(argv[i]));
            }

            if (func == "sendResponse")
            {
                if (parameters.Count != 1)
                {
                    callback.Log("sendResponse - 1 parameter required");
                    return -1;
                }

                Task.Run(() =>
                {
                    callback.Log("sendResponse - received response " + parameters[0]);
                });
                WriteOutput(output, "received response");
            }
            else
            {
                string result = String.Format("Function: {0} - Params: {1} - Total extension calls: {2}", func, SerializeList(parameters), numCalls);
                WriteOutput(output, result);
            };

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

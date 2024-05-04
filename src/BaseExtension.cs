using ArmaExtensionDotNet.Sqf;

namespace ArmaExtensionDotNet
{
    internal abstract class BaseExtension
    {
        public abstract string Name { get; }
        public abstract string Version { get; }

        protected readonly Client client;
        protected readonly ResponseCache responseCache;
        protected readonly Invoker invoker;
        protected readonly Controller controller;

        public BaseExtension()
        {
            client = new(Name);
            responseCache = new();
            invoker = new(client, responseCache, Name);
            controller = new(client, responseCache);
        }

        public void Start()
        {
            controller.Start();
            OnStart();
        }

        public void RegisterCallback(IntPtr functionPointer)
        {
            client.RegisterCallback(functionPointer);
        }

        public Tuple<string, int> SendCommand(string command, List<string> parameters)
        {
            return controller.SendCommand(command, parameters);
        }

        protected abstract void OnStart();
    }
}

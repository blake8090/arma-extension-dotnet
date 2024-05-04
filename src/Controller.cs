using ArmaExtensionDotNet.Events;
using ArmaExtensionDotNet.Sqf;

namespace ArmaExtensionDotNet
{
    internal class Controller(Client client, ResponseCache responseCache)
    {
        public event EventHandler<HitEventArgs>? Hit;
        public event EventHandler<KilledEventArgs>? Killed;

        private readonly Client client = client;
        private readonly ResponseCache responseCache = responseCache;

        private readonly Dictionary<string, Action<List<string>>> commands = [];

        private readonly List<Task> tasks = [];
        private Task? backgroundTask;
        private bool running = false;

        public void Start()
        {
            if (running)
            {
                client.LogInfo("Controller already running");
            }

            running = true;
            backgroundTask = Task.Factory.StartNew(MonitorTasks, TaskCreationOptions.LongRunning);
        }

        public void RegisterCommand(string name, Action<List<string>> action)
        {
            if (commands.ContainsKey(name))
            {
                throw new ArgumentException($"Command {name} has already been registered");
            }
            commands.Add(name, action);
            client.LogDebug($"Controller - Registered command {name}");
        }

        public Tuple<string, int> SendCommand(string name, List<string> parameters)
        {
            if (!running)
            {
                client.LogError("Controller not started");
                return new("Controller not started", -1);
            }

            try
            {
                Task task = name switch
                {
                    "sendResponse" => Task.Run(() => SendResponse(parameters)),
                    "handleEvent" => HandleEvent(parameters),
                    "shutdown" => Task.Run(Shutdown),
                    _ => RunCommand(name, parameters)
                };
                tasks.Add(task);
                return new("success", 0);
            }
            catch (Exception e)
            {
                client.LogError(e.ToString());
                return new(e.Message, -1);
            }
        }

        private Task RunCommand(string name, List<string> parameters)
        {
            if (commands.TryGetValue(name, out Action<List<string>>? action))
            {
                return Task.Run(() => action(parameters));
            }
            else
            {
                throw new ArgumentException($"Command {name} does not exist");
            }
        }

        private void SendResponse(List<String> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new ArgumentException("Expected 2 parameters");
            }

            // deserialize from Arma 3 string which is always wrapped in double quotes
            var id = parameters[0].Replace("\"", "");
            var result = parameters[1];

            client.LogDebug($"Added response: '{result}' for request {id}");
            responseCache.AddResponse(id, result);
        }

        private Task HandleEvent(List<string> parameters)
        {
            if (parameters.Count < 1)
            {
                throw new ArgumentException("Expected at least 1 parameter");
            }

            var eventName = Serializer.ReadString(parameters[0]);
            return eventName switch
            {
                "hit" => Task.Run(() =>
                {
                    var unit = Serializer.ReadObject(parameters[1]);
                    Hit?.Invoke(this, new(unit));
                }),

                "killed" => Task.Run(() =>
                {
                    var unit = Serializer.ReadObject(parameters[1]);
                    Killed?.Invoke(this, new(unit));
                }),

                _ => throw new ArgumentException($"Unknown event '{eventName}'")
            };
        }

        private void Shutdown()
        {
            client.LogInfo("Controller - shutting down");
            running = false;
            backgroundTask?.Wait();
            client.LogInfo("Controller - successfully shut down");
        }

        private void MonitorTasks()
        {
            client.LogInfo("Controller - background service started");
            while (running)
            {
                Thread.Sleep(50);
                foreach (var item in tasks)
                {
                    CheckTaskExceptions(item);
                }
                tasks.RemoveAll(t => t.IsCompleted);
            }
            client.LogInfo("Controller - background service ended");
            // TODO: use cancellation token
        }

        private void CheckTaskExceptions(Task task)
        {
            if (!task.IsFaulted)
            {
                return;
            }

            foreach (var ex in task.Exception.InnerExceptions)
            {
                client.LogError(ex.ToString());
            }
        }
    }
}

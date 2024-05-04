namespace ArmaExtensionDotNet
{
    internal class Controller(Client client, ResponseCache responseCache)
    {
        private readonly Client client = client;
        private readonly ResponseCache responseCache = responseCache;

        private readonly Dictionary<string, Action<List<string>>> commands = [];
        private readonly List<Task> tasks = [];
        private Task? backgroundTask;

        private bool running = false;

        public void Start()
        {
            if (running) return;

            running = true;
            backgroundTask = Task.Factory.StartNew(() =>
            {
                client.Log("Controller background service started");
                MonitorTasks();
                client.Log("Controller background service ended");
            }, TaskCreationOptions.LongRunning);
        }

        public void RegisterCommand(string name, Action<List<string>> action)
        {
            if (commands.ContainsKey(name))
            {
                throw new ArgumentException($"Command {name} has already been registered");
            }
            commands.Add(name, action);
            client.Log($"DEBUG: Registered command {name}");
        }

        public Tuple<string, int> SendCommand(string name, List<string> parameters)
        {
            if (!running)
            {
                client.Log("ERROR: service not started");
                return new("nope", -1);
            }

            try
            {
                Task task = name switch
                {
                    "sendResponse" => Task.Run(() => SendResponse(parameters)),
                    "handleEvent" => Task.Run(() => HandleEvent(parameters)),
                    "shutdown" => Task.Run(Shutdown),
                    _ => RunCommand(name, parameters)
                };
                tasks.Add(task);
                return new("success", 0);
            }
            catch (Exception e)
            {
                client.Log($"ERROR: {e}");
                return new(e.Message, -1);
            }
        }

        private Task RunCommand(string name, List<string> parameters)
        {
            //if (name.Equals("sendResponse"))
            //{
            //    return Task.Run(() => SendResponse(parameters));
            //}
            //if (name.Equals("handleEvent"))
            //{
            //    return Task.Run(() => HandleEvent(parameters));
            //}
            //else if (name.Equals("shutdown"))
            //{
            //    Shutdown();
            //    return Task.CompletedTask;
            //}
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
                throw new ArgumentException("SendResponse: Expected 2 parameters");
            }

            // deserialize from Arma 3 string which is always wrapped in double quotes
            var id = parameters[0].Replace("\"", "");
            var result = parameters[1];

            client.Log($"Added response: '{result}' for request {id}");
            responseCache.AddResponse(id, result);
        }

        private void HandleEvent(List<string> parameters)
        {
            if (parameters.Count < 1)
            {
                throw new ArgumentException("Expected at least 1 parameter");
            }

            var eventName = parameters[0].Replace("\"", "");
            client.Log($"Handling event {eventName}");
        }

        private void Shutdown()
        {
            client.Log("shutting down");
            running = false;
            backgroundTask?.Wait();
            client.Log("successfully shut down");
        }

        private void MonitorTasks()
        {
            while (running)
            {
                Thread.Sleep(50);
                foreach (var item in tasks)
                {
                    CheckTaskExceptions(item);
                }
                tasks.RemoveAll(t => t.IsCompleted);
            }
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
                client.Log($"ERROR: {ex}");
            }
        }
    }
}

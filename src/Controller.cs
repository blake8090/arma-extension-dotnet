namespace ArmaExtensionDotNet
{
    internal class Controller(Client client, Invoker invoker, ResponseCache responseCache)
    {
        private readonly Client client = client;
        private readonly Invoker invoker = invoker;
        private readonly ResponseCache responseCache = responseCache;

        private readonly List<Task> tasks = [];

        public void Init()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    MonitorTasks();
                    await Task.Delay(500);
                }
            });
        }

        private void MonitorTasks()
        {
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                var task = tasks[i];
                if (task.IsCompleted)
                {
                    if (tasks[i].IsFaulted)
                    {
                        foreach (var ex in task.Exception?.InnerExceptions ?? new([]))
                        {
                            client.Log($"ERROR: {ex}");
                        }
                    }
                    tasks.RemoveAt(i);
                }
            }
        }

        public string Call(string functionName, List<String> parameters)
        {
            var task = functionName switch
            {
                "runSqfTest" => RunSqfTest(),
                "sendResponse" => SendResponse(parameters),
                _ => throw new ArgumentException($"Unknown function {functionName}"),
            };
            tasks.Add(task);

            return "accepted";
        }

        private Task RunSqfTest()
        {
            return Task.Run(() =>
            {
                client.Log("runSqfTest - begin");
                invoker.GetPlayerPos();
                client.Log("runSqfTest - end");
            });
        }

        private Task SendResponse(List<String> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new ArgumentException("SendResponse: Expected 2 parameters");
            }

            // deserialize from Arma 3 string which is always wrapped in double quotes
            var id = parameters[0].Replace("\"", "");
            var result = parameters[1];

            return Task.Run(() =>
            {
                client.Log($"Received response: '{result}' for request {id}");
                responseCache.AddResponse(id, result);
            });
        }
    }
}

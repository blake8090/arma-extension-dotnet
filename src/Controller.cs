namespace ArmaExtensionDotNet
{
    internal class Controller(Client client, Invoker invoker, ResponseCache responseCache)
    {
        private readonly Client client = client;
        private readonly Invoker invoker = invoker;
        private readonly ResponseCache responseCache = responseCache;

        private readonly List<Task> tasks = [];
        private Task BackgroundTask() => Task.Factory.StartNew(BackgroundService, TaskCreationOptions.LongRunning);
        private bool running = false;

        public void Start()
        {
            if (!running)
            {
                running = true;
                BackgroundTask();
            }
        }

        private void BackgroundService()
        {
            client.Log("Controller background service started");
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
            client.Log("Controller background service ended");
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

        public string Call(string functionName, List<String> parameters)
        {
            if (!running)
            {
                client.Log("ERROR: service not started");
                return "nope";
            }
            else if (functionName.Equals("shutdown"))
            {
                client.Log("shutting down");
                running = false;
                client.Log("successfully shut down");
                return "shutdown";
            }

            Task task = functionName switch
            {
                "runSqfTest" => Task.Run(RunSqfTest),
                "sendResponse" => Task.Run(() => SendResponse(parameters)),
                _ => throw new ArgumentException($"Unknown function {functionName}"),
            };
            tasks.Add(task);

            return "success";
        }

        private void RunSqfTest()
        {
            client.Log("runSqfTest - begin");
            invoker.GetPlayerPos();
            client.Log("runSqfTest - end");
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
    }
}

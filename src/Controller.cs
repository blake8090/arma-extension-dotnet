using ArmaExtensionDotNet.Events;
using ArmaExtensionDotNet.Sqf;

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

        public event EventHandler<HitEventArgs>? Hit;
        public event EventHandler<KilledEventArgs>? Killed;

        public void Start()
        {
            if (!running)
            {
                running = true;
                BackgroundTask();

                Hit += (sender, args) => client.Log($"Unit {args.Unit} was hit");
                Killed += (sender, args) => client.Log($"Unit {args.Unit} was killed");
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
                "handleEvent" => Task.Run(() => HandleEvent(parameters)),
                _ => throw new ArgumentException($"Unknown function {functionName}"),
            };
            tasks.Add(task);

            return "success";
        }

        private void RunSqfTest()
        {
            client.Log("runSqfTest - begin");

            A3Object player = invoker.GetPlayer();
            invoker.GetPos(player);
            invoker.IsKindOf(player, "Man");

            var leader = invoker.Leader(player);
            invoker.AddKilledEventHandler(leader);
            invoker.AddHitEventHandler(leader);

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

        private void HandleEvent(List<String> parameters)
        {
            if (parameters.Count < 1)
            {
                throw new ArgumentException("Expected at least 1 parameter");
            }

            var eventName = parameters[0].Replace("\"", "");

            if (eventName.Equals("hit"))
            {
                var unit = Serializer.ReadObject(parameters[1]);
                Hit?.Invoke(this, new(unit));
            }
            else if (eventName.Equals("killed"))
            {
                var unit = Serializer.ReadObject(parameters[1]);
                Killed?.Invoke(this, new(unit));
            }
            else
            {
                throw new ArgumentException($"Unknown event {eventName}");
            }
        }
    }
}

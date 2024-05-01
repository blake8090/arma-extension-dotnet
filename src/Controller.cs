namespace ArmaExtensionDotNet
{
    internal class Controller(Client client, Invoker invoker, ResponseCache responseCache)
    {
        private readonly Client client = client;
        private readonly Invoker invoker = invoker;
        private readonly ResponseCache responseCache = responseCache;

        public string Call(string functionName, List<String> parameters)
        {
            if (functionName.Equals("runSqfTest"))
            {
                Task.Run(() =>
                {
                    client.Log("runSqfTest - begin");

                    invoker.GetPlayerPos();

                    client.Log("runSqfTest - end");
                });
                return "started async task";
            }
            else if (functionName.Equals("sendResponse"))
            {
                // deserialize from Arma 3 string which is always wrapped in double quotes
                var id = parameters[0].Replace("\"", "");
                var result = parameters[1];

                // TODO: does this need to be a task?
                Task.Run(() =>
                {
                    client.Log($"Received response: '{result}' for request {id}");
                    responseCache.AddResponse(id, result);
                });
            }

            return "accepted";
        }
    }
}

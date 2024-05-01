using System.Diagnostics;

namespace ArmaExtensionDotNet
{
    internal class Invoker(Client client, ResponseCache responseCache)
    {
        private readonly Client client = client;
        private readonly ResponseCache responseCache = responseCache;

        public void GetPlayerPos()
        {
            var id = Guid.NewGuid().ToString();
            var code = "getPos player";
            client.ExecSqf(id, code);

            client.Log($"Waiting for a response to id {id}");
            Stopwatch stopwatch = Stopwatch.StartNew();
            while(stopwatch.Elapsed.TotalSeconds < 3)
            {
                if (responseCache.ContainsResponse(id))
                {
                    var response = responseCache.ConsumeResponse(id);
                    client.Log($"GOT A RESPONSE! {response}");
                }
            }
            stopwatch.Stop();
            client.Log("GetPlayerPos - timed out!");
        }
    }
}

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
            while(stopwatch.Elapsed.TotalSeconds < 2)
            {
                if (responseCache.ContainsResponse(id))
                {
                    var response = responseCache.ConsumeResponse(id);
                    stopwatch.Stop();
                    var elapsedMs = stopwatch.ElapsedMilliseconds;
                    client.Log($"Received a response {response} in {elapsedMs} ms");
                }
            }
            stopwatch.Stop();
            throw new InvalidOperationException($"GetPlayerPos - timed out for request {id}");
        }
    }
}

using System.Diagnostics;

namespace ArmaExtensionDotNet
{
    internal class Invoker(Client client, ResponseCache responseCache)
    {
        private const int TimeoutSeconds = 2;

        private readonly Client client = client;
        private readonly ResponseCache responseCache = responseCache;

        public String GetPlayerPos()
        {
            var id = Guid.NewGuid().ToString();
            client.ExecSqf(id, "getPos player");
            return WaitForResponse(id);
        }

        private String WaitForResponse(string requestId)
        {
            client.Log($"Waiting for a response to request {requestId}");

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < TimeoutSeconds)
            {
                if (responseCache.ContainsResponse(requestId))
                {
                    var response = responseCache.ConsumeResponse(requestId);
                    stopwatch.Stop();
                    var elapsedMs = stopwatch.ElapsedMilliseconds;
                    client.Log($"Received reponse '{response}' for request {requestId} in {elapsedMs} ms");
                    return response;
                }
            }
            stopwatch.Stop();

            throw new InvalidOperationException($"Timed out waiting for request {requestId}");
        }
    }
}

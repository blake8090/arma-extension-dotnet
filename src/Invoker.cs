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
            var requestId = client.ExecSqf("getPos player");
            return WaitForResponse(requestId);
        }

        public String GetPos(string unit)
        {
            //var requestId = client.ExecSqf($"getPos (\"{unit}\" call BIS_fnc_objectFromNetId)");
            var requestId = client.ExecSqf($"getPos {ObjectFromNetIdCode(unit)}");
            return WaitForResponse(requestId);
        }

        public String GetPlayer()
        {
            var requestId = client.ExecSqf("player call BIS_fnc_netId");
            var player = WaitForResponse(requestId).Replace("\"", "");
            return player;
        }

        public String IsKindOf(string unit, string kind)
        {
            //var requestId = client.ExecSqf($"(\"{unit}\" call BIS_fnc_objectFromNetId) isKindOf \"{kind}\"");
            var requestId = client.ExecSqf($"{ObjectFromNetIdCode(unit)} isKindOf \"{kind}\"");
            return WaitForResponse(requestId);
        }

        public String Leader(string unit)
        {
            //var requestId = client.ExecSqf($"(leader (\"{unit}\" call BIS_fnc_objectFromNetId)) ");
            var requestId = client.ExecSqf($"leader {ObjectFromNetIdCode(unit)} ");
            return WaitForResponse(requestId);
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

        private static String ObjectFromNetIdCode(string unit)
        {
            return $"(\"{unit}\" call BIS_fnc_objectFromNetId)";
        }
    }
}

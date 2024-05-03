using ArmaExtensionDotNet.Sqf;
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

        public String GetPos(A3Object unit)
        {
            var requestId = client.ExecSqf($"getPos {ObjectFromNetIdCode(unit)}");
            return WaitForResponse(requestId);
        }

        public A3Object GetPlayer()
        {
            var requestId = client.ExecSqf("player call BIS_fnc_netId");
            return Serializer.ReadObject(WaitForResponse(requestId));
        }

        public String IsKindOf(A3Object unit, string kind)
        {
            var requestId = client.ExecSqf($"{ObjectFromNetIdCode(unit)} isKindOf \"{kind}\"");
            return WaitForResponse(requestId);
        }

        public A3Object Leader(A3Object unit)
        {
            var requestId = client.ExecSqf($"(leader {ObjectFromNetIdCode(unit)}) call BIS_fnc_netId");
            return Serializer.ReadObject(WaitForResponse(requestId));
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

        private static String ObjectFromNetIdCode(A3Object unit)
        {
            return $"({Serializer.WriteObject(unit)} call BIS_fnc_objectFromNetId)";
        }
    }
}

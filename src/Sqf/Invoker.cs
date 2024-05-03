using System.Diagnostics;

namespace ArmaExtensionDotNet.Sqf
{
    internal class Invoker(Client client, ResponseCache responseCache)
    {
        private const int TimeoutSeconds = 2;

        private readonly Client client = client;
        private readonly ResponseCache responseCache = responseCache;

        public string GetPlayerPos()
        {
            var requestId = client.ExecSqf("getPos player");
            return WaitForResponse(requestId);
        }

        public string GetPos(A3Object unit)
        {
            var requestId = client.ExecSqf($"getPos {ObjectFromNetIdCode(unit)}");
            return WaitForResponse(requestId);
        }

        public A3Object GetPlayer()
        {
            var requestId = client.ExecSqf("player call BIS_fnc_netId");
            return Serializer.ReadObject(WaitForResponse(requestId));
        }

        public string IsKindOf(A3Object unit, string kind)
        {
            var requestId = client.ExecSqf($"{ObjectFromNetIdCode(unit)} isKindOf \"{kind}\"");
            return WaitForResponse(requestId);
        }

        public A3Object Leader(A3Object unit)
        {
            var requestId = client.ExecSqf($"(leader {ObjectFromNetIdCode(unit)}) call BIS_fnc_netId");
            return Serializer.ReadObject(WaitForResponse(requestId));
        }

        public void AddKilledEventHandler(A3Object unit)
        {
            var code = @$"
                {ObjectFromNetIdCode(unit)} addEventHandler [""Hit"", {{
	                params [""_unit"", ""_source"", ""_damage"", ""_instigator""];
	                ""{Client.ExtensionName}"" callExtension [""handleEvent"", [""hit"", {Serializer.WriteObject(unit)}, _source, _damage, _instigator]];
                }}];
               ";
            var requestId = client.ExecSqf(code);
            WaitForResponse(requestId);
        }

        public void AddHitEventHandler(A3Object unit)
        {
            var code = @$"
                {ObjectFromNetIdCode(unit)} addEventHandler [""Killed"", {{
	                params [""_unit"", ""_killer"", ""_instigator"", ""_useEffects""];
	                ""{Client.ExtensionName}"" callExtension [""handleEvent"", [""killed"", {Serializer.WriteObject(unit)}]];
                }}];
               ";
            var requestId = client.ExecSqf(code);
            WaitForResponse(requestId);
        }

        private string WaitForResponse(string requestId)
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

        private static string ObjectFromNetIdCode(A3Object unit)
        {
            return $"({Serializer.WriteObject(unit)} call BIS_fnc_objectFromNetId)";
        }
    }
}

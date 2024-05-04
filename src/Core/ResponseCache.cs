namespace ArmaExtensionDotNet.Core
{
    internal class ResponseCache
    {
        private readonly object cacheLock = new();
        private readonly Dictionary<string, string> cache = [];

        public void AddResponse(string requestId, string response)
        {
            lock (cacheLock)
            {
                cache.Add(requestId, response);
            }
        }

        public bool ContainsResponse(string requestId)
        {
            return cache.ContainsKey(requestId);
        }

        public string ConsumeResponse(string requestId) 
        {
            lock (cacheLock)
            {
                string response = cache[requestId];
                cache.Remove(requestId);
                return response;
            }
        }
    }
}

namespace ArmaExtensionDotNet.Core.Sqf
{
    internal class Serializer
    {
        public static A3Object ReadObject(string content)
        {
            var id = ReadString(content);
            return new A3Object(id);
        }

        public static string WriteObject(A3Object obj)
        {
            return $"\"{obj.Id}\"";
        }

        public static string ReadString(string content)
        {
            if (!content.StartsWith('"') || !content.EndsWith('"'))
            {
                throw new FormatException($"Invalid string format for content <{content}>");
            }
            return content.Replace("\"", "");
        }
    }
}

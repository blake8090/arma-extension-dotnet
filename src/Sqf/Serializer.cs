using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaExtensionDotNet.Sqf
{
    internal class Serializer
    {
        public static A3Object ReadObject(string content)
        {
            var id = content.Replace("\"", "");
            return new A3Object(id);
        }

        public static String WriteObject(A3Object obj)
        {
            return $"\"{obj.Id}\"";
        }
    }
}

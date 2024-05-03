using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaExtensionDotNet.Sqf
{
    internal class A3Object(string id) : A3Type
    {
        public string Id { get; set; } = id;
    }
}

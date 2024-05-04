using ArmaExtensionDotNet.Core.Sqf;

namespace ArmaExtensionDotNet.Core.Events
{
    internal class KilledEventArgs(A3Object unit) : EventArgs
    {
        public A3Object Unit { get; set; } = unit;
    }
}

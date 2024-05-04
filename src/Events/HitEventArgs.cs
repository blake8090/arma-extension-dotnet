using ArmaExtensionDotNet.Sqf;

namespace ArmaExtensionDotNet.Events
{
    internal class HitEventArgs(A3Object unit) : EventArgs
    {
        public A3Object Unit {  get; set; } = unit;
    }
}

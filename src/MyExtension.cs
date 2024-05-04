using ArmaExtensionDotNet.Sqf;

namespace ArmaExtensionDotNet
{
    internal class MyExtension : BaseExtension
    {
        public override string Name => "ArmaExtensionDotNet";
        public override string Version => "ArmaExtensionDotNet v1.0";

        protected override void OnStart()
        {
            client.LogInfo("MyExtension - start");
            controller.RegisterCommand("runSqfTest", RunSqfTest);

            controller.Hit += (sender, e) =>
            {
                client.LogDebug($"Unit {e.Unit} was hit");
            };

            controller.Killed += (sender, e) =>
            {
                client.LogDebug($"Unit {e.Unit} was killed");
            };
        }

        private void RunSqfTest(List<string> args)
        {
            client.LogDebug("runSqfTest - begin");

            A3Object player = invoker.GetPlayer();
            invoker.GetPos(player);
            invoker.IsKindOf(player, "Man");

            var leader = invoker.Leader(player);
            invoker.AddKilledEventHandler(leader);
            invoker.AddHitEventHandler(leader);

            client.LogDebug("runSqfTest - end");
        }
    }
}

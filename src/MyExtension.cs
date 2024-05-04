using ArmaExtensionDotNet.Sqf;

namespace ArmaExtensionDotNet
{
    internal class MyExtension : BaseExtension
    {
        public override string Name => "ArmaExtensionDotNet";
        public override string Version => "ArmaExtensionDotNet v1.0";

        protected override void OnStart()
        {
            client.Log("MyExtension - start");
            controller.RegisterCommand("runSqfTest", RunSqfTest);
        }

        private void RunSqfTest(List<string> args)
        {
            client.Log("runSqfTest - begin");

            A3Object player = invoker.GetPlayer();
            invoker.GetPos(player);
            invoker.IsKindOf(player, "Man");

            var leader = invoker.Leader(player);
            invoker.AddKilledEventHandler(leader);
            invoker.AddHitEventHandler(leader);

            client.Log("runSqfTest - end");
        }
    }
}

using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace HousingOptimize
{
    [HarmonyPatch]
    public class HousingOptimize : IModEntrypoint
    {
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Harmony harmony = new Harmony("housingoptimize");
            harmony.PatchAll();

            consoleWriter.LogInfo("Housing optimization mod loaded");
        }
    }
}

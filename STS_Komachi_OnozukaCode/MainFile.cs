using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode
{
    [ModInitializer(nameof(Initialize))]
    public partial class MainFile : Node
    {
        public const string ModId = "STS_Komachi_Onozuka"; //Used for resource filepath
        public const string ResPath = $"res://{ModId}";

        public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

        public static void Initialize()
        {
            //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
            //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

            Harmony harmony = new(ModId);

            harmony.PatchAll();

            foreach (var method in Harmony.GetAllPatchedMethods())
            {
                MainFile.Logger.LogMessage(LogLevel.Info, $"[PatchCheck] Patched: {method.DeclaringType?.Name}.{method.Name}", 0);
            }
        }

        public static void LogMessage(string message)
        {
            Logger.LogMessage(MegaCrit.Sts2.Core.Logging.LogLevel.Info, message, 0);
        }
    }
}

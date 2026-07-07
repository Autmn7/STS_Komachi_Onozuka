using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches
{
    public interface IPreExtraHoverTips
    {
        void PreExtraHoverTips();
    }

    /// <summary>
    /// Does stuff when hovering over a power before that power's vars and stuff gets locked in
    /// As overriding the normal extra hover tips would require a second hover for the powers to work
    /// Screw this chud game
    /// </summary>
    [HarmonyPatch(typeof(PowerModel), "get_HoverTips")]
    internal static class PreExtraHoverTips_Patch
    {
        [HarmonyPrefix]
        static void Prefix(PowerModel __instance)
        {
            if (__instance is IPreExtraHoverTips refreshable)
                refreshable.PreExtraHoverTips();
        }
    }
}

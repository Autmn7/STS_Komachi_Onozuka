using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches
{
    internal static class NPower_ThirdAmount_Patch
    {
        static readonly Dictionary<NPower, Action<CombatState>> _stateHandlers = new();

        [HarmonyPatch(typeof(NPower), "RefreshAmount")]
        [HarmonyPostfix]
        static void Postfix(NPower __instance)
        {
            if (!__instance.IsNodeReady()) return;
            if (__instance.Model is not IHasAmbientDamagePreview ambient) return;

            decimal? damage = ambient.GetAmbientPreviewDamage();

            if (!__instance.HasNode("Amount3Label"))
            {
                if (damage == null) return;
                var template = __instance.GetNode<MegaLabel>("%AmountLabel");
                var label3 = (MegaLabel)template.Duplicate((int)(Node.DuplicateFlags.Signals | Node.DuplicateFlags.Groups | Node.DuplicateFlags.Scripts | Node.DuplicateFlags.UseInstantiation));
                label3.Name = "Amount3Label";
                label3.UniqueNameInOwner = false;
                label3.SetAnchorsPreset(Control.LayoutPreset.TopLeft, keepOffsets: false);
                __instance.AddChild(label3, false, Node.InternalMode.Disabled);
                __instance.MoveChild(label3, template.GetIndex(false));
                label3.AddThemeColorOverride(ThemeConstants.Label.FontColor, StsColors.red);
            }

            var label = __instance.GetNode<MegaLabel>("Amount3Label");
            if (damage == null)
            {
                label.Visible = false;
                return;
            }

            label.Visible = true;
            label.SetTextAutoSize(damage.Value.ToString("0"));

            var amountLabel = __instance.GetNode<MegaLabel>("%AmountLabel");
            int fontSize = label.GetThemeFontSize(ThemeConstants.Label.FontSize, "Label");
            label.Position = amountLabel.Position + new Vector2(-(fontSize + 12), -(fontSize));
        }

        [HarmonyPatch(typeof(NPower), "SubscribeToModelEvents")]
        [HarmonyPostfix]
        static void Subscribe(NPower __instance)
        {
            if (__instance.Model is not IHasAmbientDamagePreview) return;
            if (_stateHandlers.ContainsKey(__instance)) return; // guard against double-subscribe

            Action<CombatState> handler = _ => __instance.RefreshAmount();
            CombatManager.Instance.StateTracker.CombatStateChanged += handler;
            _stateHandlers[__instance] = handler;
        }

        [HarmonyPatch(typeof(NPower), "UnsubscribeFromModelEvents")]
        [HarmonyPostfix]
        static void Unsubscribe(NPower __instance)
        {
            if (_stateHandlers.TryGetValue(__instance, out var handler))
            {
                CombatManager.Instance.StateTracker.CombatStateChanged -= handler;
                _stateHandlers.Remove(__instance);
            }
        }
    }
}

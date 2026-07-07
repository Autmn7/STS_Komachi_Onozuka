using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches
{
    /// <summary>
    /// Must be used with IHasSecondAmount
    /// Gives it a custom colour and a custom tweening condition.
    /// </summary>
    public interface IHasEmphasizedSecondAmount
    {
        Color SecondAmountColor { get; }
        bool ShouldEmphasizeSecondAmount { get; }
    }
    internal static class NPower_SecondAmountStyling_Patch
    {
        static readonly Dictionary<NPower, Tween> _activeTweens = new();

        [HarmonyPatch("RefreshAmount")]
        [HarmonyPostfix]
        static void Postfix(NPower __instance)
        {
            MainFile.LogMessage($"[SecondAmountStyle] Postfix fired. IsNodeReady={__instance.IsNodeReady()}");

            if (!__instance.IsNodeReady()) return;

            // Read the backing field directly instead of the Model property, since Model
            // throws if _model is null — and we don't yet know if that's happening here.
            PowerModel? model = __instance._model;
            MainFile.LogMessage($"[SecondAmountStyle] model={model?.GetType().Name ?? "null"}");
            if (model is not IHasEmphasizedSecondAmount emphasized)
            {
                MainFile.LogMessage("[SecondAmountStyle] Model does not implement IHasEmphasizedSecondAmount, bailing.");
                return;
            }

            MainFile.LogMessage("[SecondAmountStyle] Deferring ApplyStyling.");
            Callable.From(() => ApplyStyling(__instance, emphasized)).CallDeferred();
        }

        static void ApplyStyling(NPower instance, IHasEmphasizedSecondAmount emphasized)
        {
            MainFile.LogMessage("[SecondAmountStyle] ApplyStyling deferred call ran.");

            if (!GodotObject.IsInstanceValid(instance))
            {
                MainFile.LogMessage("[SecondAmountStyle] Instance invalid by the time deferred call ran.");
                return;
            }

            bool hasNode = instance.HasNode("Amount2Label");
            MainFile.LogMessage($"[SecondAmountStyle] HasNode(Amount2Label)={hasNode}");
            if (!hasNode) return;

            var label2 = instance.GetNode<MegaLabel>("Amount2Label");
            MainFile.LogMessage($"[SecondAmountStyle] Applying color {emphasized.SecondAmountColor}, emphasize={emphasized.ShouldEmphasizeSecondAmount}");

            label2.AddThemeColorOverride(ThemeConstants.Label.FontColor, emphasized.SecondAmountColor);
            SetEmphasis(instance, label2, emphasized.ShouldEmphasizeSecondAmount);
        }

        static void SetEmphasis(NPower instance, MegaLabel label, bool shouldEmphasize)
        {
            bool alreadyOn = _activeTweens.ContainsKey(instance);
            MainFile.LogMessage($"[SecondAmountStyle] SetEmphasis: shouldEmphasize={shouldEmphasize}, alreadyOn={alreadyOn}");
            if (shouldEmphasize == alreadyOn) return;

            if (shouldEmphasize)
            {
                var tween = label.CreateTween().SetLoops();
                tween.TweenProperty(label, "scale", Vector2.One * 1.3f, 0.3)
                    .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
                tween.TweenProperty(label, "scale", Vector2.One, 0.3)
                    .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
                _activeTweens[instance] = tween;
                MainFile.LogMessage("[SecondAmountStyle] Tween created.");
            }
            else
            {
                _activeTweens[instance].Kill();
                _activeTweens.Remove(instance);
                label.Scale = Vector2.One;
                MainFile.LogMessage("[SecondAmountStyle] Tween killed.");
            }
        }

        [HarmonyPatch(typeof(NPower), "_ExitTree")]
        [HarmonyPostfix]
        static void Cleanup(NPower __instance)
        {
            if (_activeTweens.TryGetValue(__instance, out var tween))
            {
                tween.Kill();
                _activeTweens.Remove(__instance);
            }
        }
    }
}

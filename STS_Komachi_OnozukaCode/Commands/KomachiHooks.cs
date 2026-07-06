using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands
{
    public static class KomachiHooks
    {
        private static async Task Dispatch<T>(PlayerChoiceContext choiceContext, ICombatState? combatState, Func<T, Task> invoke) where T : class
        {
            if (combatState == null)
            {
                return;
            }

            foreach (T item in combatState.IterateHookListeners().OfType<T>())
            {
                AbstractModel? model = item as AbstractModel;
                choiceContext.PushModel(model);
                await invoke(item);
                choiceContext.PopModel(model);
            }
        }

        public static Task OnDistanceChanged(PlayerChoiceContext choiceContext, DistanceChangedEventArgs args)
        {
            var dis = Dispatch(choiceContext, args.Target.CombatState, (IOnDistanceChangedListener m) => m.OnDistanceChanged(choiceContext, args));
            EnemyIntentDistancePreviewController.OnDistanceChanged(args.Target);
            return dis;
        }
        public static Task OnReleasing(PlayerChoiceContext choiceContext, ReleaseResult args)
        {
            return Dispatch(choiceContext, args.creature.CombatState,
                (IOnReleasingListener m) => m.OnReleasing(choiceContext, args));
        }
        public static Task OnDetonating(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        {
            return Dispatch(choiceContext, args.Target.CombatState,
                (IOnDetonatingListener m) => m.OnDetonating(choiceContext, args));
        }

        public static Task OnDetonatedEarly(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        => Dispatch(choiceContext, args.Target.CombatState, 
            (IOnDetonatedEarlyListener m) => m.OnDetonatedEarly(choiceContext, args));


        public static Task OnDetonated(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        {
            return Dispatch(choiceContext, args.Target.CombatState,
                (IOnDetonatedListener m) => m.OnDetonated(choiceContext, args));
        }
    }
}

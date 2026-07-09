using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{
    public class LonelyBoundSpiritPower : STS_Komachi_OnozukaPower, IOnDetonatedEarlyListener, IPreExtraHoverTips
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar("VengefulDamage", 2m, ValueProp.Move)
        ];

        public decimal BaseVengefulDamage => Amount * 2m;

        public decimal VengefulDamage
        {
            get
            {
                decimal baseDmg = BaseVengefulDamage;
                decimal modifiedDmg = baseDmg;
                if (Owner != null && Applier?.CombatState != null && Applier?.Player?.RunState != null)
                {
                    modifiedDmg = Hook.ModifyDamage(
                        Applier.Player.RunState,
                        Applier.CombatState,
                        Owner,
                        Applier,
                        baseDmg,
                        ValueProp.Move,
                        null,
                        ModifyDamageHookType.All,
                        CardPreviewMode.None,
                        out _
                    );
                }
                var damageVar = DynamicVars[nameof(VengefulDamage)];
                damageVar.BaseValue = baseDmg;
                damageVar.PreviewValue = modifiedDmg;
                return modifiedDmg;
            }
        }
        public void PreExtraHoverTips() => _ = VengefulDamage;

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power is not VengefulSpiritPower || power.Owner != Owner)
            {
                return;
            }
            Amount += (int)(amount);
        }

        public async Task OnDetonatedEarly(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        {
            if (args.Target != Owner || args.DetonatingPower is not VengefulSpiritPower || !args.DetonatedByEffect)
            {
                return;
            }
            args.BonusAmount += Amount;
            await DetonateCmd.DetonateRaw(choiceContext, this, Amount, args.Dealer, cardSource: null);
        }


        // Just in case safety
        private bool _isExploding = false;
        /// <summary>
        /// Reference to the current detonation args.
        /// Gets fed into it from DetonateCMD so that it can provide back the damage result before being removes.
        /// </summary>
        public DetonationEventArgs? currentDetonationArgs;
        /// <summary>
        /// Delays damage processing until the power instance is stripped from the creature container.
        /// </summary>
        public override async Task AfterRemoved(Creature oldOwner)
        {
            if (_isExploding) return;
            _isExploding = true;

            decimal finalDamage = VengefulDamage;

            var damageResults = await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, finalDamage, ValueProp.Move, Applier, null);

            if (currentDetonationArgs != null)
            {
                currentDetonationArgs.damageResult = damageResults.FirstOrDefault();
            }
        }

    }
}

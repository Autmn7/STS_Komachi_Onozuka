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
using MegaCrit.Sts2.Core.Models.Badges;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits
{
    public class VengefulSpiritPower : STS_Komachi_OnozukaPower, IHasSecondAmount
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new IntVar(nameof(Duration), 3),
            new DamageVar(nameof(VengefulDamage), 2m, ValueProp.Move)
        ];

        /// <summary>
        /// The base un-modified explosion damage (Amount * 2).
        /// </summary>
        public decimal BaseVengefulDamage => Amount * 2m;

        /// <summary>
        /// Evaluates the fully modified game damage using the combat engine hooks.
        /// </summary>
        public decimal VengefulDamage
        {
            get
            {
                decimal baseDmg = Amount * 2m;
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

                // Keep the underlying engine variable synced with calculations
                var damageVar = DynamicVars[nameof(VengefulDamage)];
                damageVar.BaseValue = baseDmg;
                damageVar.PreviewValue = modifiedDmg;

                return modifiedDmg;
            }
        }

        protected override IEnumerable<IHoverTip> ExtraHoverTips
        {
            get
            {
                // Accessing the property automatically refreshes its values inside DynamicVars
                _ = VengefulDamage;
                return base.ExtraHoverTips;
            }
        }

        public int Duration
        {
            get => DynamicVars[nameof(Duration)].IntValue;
            set
            {
                DynamicVars[nameof(Duration)].BaseValue = value;
                PowerExtensions.InvokeSecondAmountChanged(this);
            }
        }

        public string GetSecondAmount() => Duration.ToString();

        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (!participants.Contains(Owner))
            {
                return;
            }

            Duration--;

            if (Duration <= 0)
            {
                await DetonateCmd.Detonate(new ThrowingPlayerChoiceContext(), this);
            }
        }

        // Just in case safety
        private bool _isExploding = false;
        /// <summary>
        /// Reference to the current detonation args.
        /// Gets fed into it from DetonateCMD so that it can provide back the damage result before being removes.
        /// </summary>
        public DetonationEventArgs? currentDetonationArgs;
        public override async Task AfterRemoved(Creature oldOwner)
        {
            if (_isExploding) return;
            _isExploding = true;
            decimal damage = BaseVengefulDamage;
            var damageResults = await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), oldOwner, damage, ValueProp.Move, Applier, null);
            var dmgResult = damageResults.FirstOrDefault();
            if (currentDetonationArgs != null)
            {
                currentDetonationArgs.damageResult = dmgResult;
            }
            await base.AfterRemoved(oldOwner);
        }
    }
}

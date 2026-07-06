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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits
{
    public class GuidedSpiritPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar("GuidedDamage", 1m, ValueProp.Move),
            new StringVar("targetName", "None")
        ];

        // It targets the enemy with the lowest HP
        public Creature? AttackTarget
        {
            get
            {
                if (CombatState.HittableEnemies.Count <= 0)
                {
                    ((StringVar)DynamicVars["targetName"]).StringValue = "None";
                    return null;
                }
                var t = CombatState.HittableEnemies.OrderBy(c => c.CurrentHp).ToList()[0];
                ((StringVar) DynamicVars["targetName"]).StringValue = t.Name;
                return t;
            }
        }

        protected override IEnumerable<IHoverTip> ExtraHoverTips
        {
            get
            {
                decimal baseDmg = Amount;
                decimal modifiedDmg = baseDmg;
                // 1. Manually run the damage modifiers right before the UI draws
                if (Owner != null && Owner.CombatState != null && Owner.Player.RunState != null && AttackTarget != null)
                {
                    modifiedDmg = Hook.ModifyDamage(
                        Owner.Player.RunState,
                        Owner.CombatState,
                        AttackTarget,          
                        Owner,      
                        baseDmg,
                        ValueProp.Move,
                        null,
                        ModifyDamageHookType.All,
                        CardPreviewMode.None,
                        out _
                    );

                    MainFile.Logger.LogMessage(MegaCrit.Sts2.Core.Logging.LogLevel.Info, $"Modifying {baseDmg} damage into {modifiedDmg}.", 0);
                }

                var damageVar = DynamicVars["GuidedDamage"];
                damageVar.BaseValue = baseDmg;
                damageVar.PreviewValue = modifiedDmg;

                return base.ExtraHoverTips;
            }
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(Owner))
            {
                return;
            }

            if (AttackTarget != null) {
                // Change to attack later for animation and stuff
                await CreatureCmd.Damage(choiceContext, 
                    target: AttackTarget, 
                    amount: Amount, 
                    props: ValueProp.Move, 
                    dealer: Owner);
            }
            Amount--;
            if (Amount <= 0)
            {
                await PowerCmd.Remove(this);
            }
        }
    }
}

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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits
{
    public class GuidedSpiritPower : STS_Komachi_OnozukaPower, IPreExtraHoverTips, IHasAmbientDamagePreview
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar("GuidedDamage", 1m, ValueProp.Move),
            new StringVar("targetName", "None")
        ];

        // It targets the enemy with the lowest HP
        public Creature? DamageTarget
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
        public decimal BaseDamage => Amount;

        /// <summary>
        /// Previews what the damage should be against the damage target.
        /// </summary>
        public decimal ModifiedDamage => SpiritDamageHelper.FindDamageDealt(Owner, DamageTarget, BaseDamage, DynamicVars["GuidedDamage"]);
        public decimal? GetAmbientPreviewDamage() => DamageTarget == null ? null : ModifiedDamage;
        public void PreExtraHoverTips() => _ = ModifiedDamage;

        protected override IEnumerable<IHoverTip> ExtraHoverTips
        {
            get { 
                PreExtraHoverTips(); 
                return base.ExtraHoverTips; 
            }
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(Owner))
            {
                return;
            }


            if (DamageTarget != null) {
                // Change to attack later for animation and stuff
                await CreatureCmd.Damage(choiceContext, 
                    target: DamageTarget, 
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

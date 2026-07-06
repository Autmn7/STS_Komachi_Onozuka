using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Other
{
    public class EthicallySourcedBreakPower : STS_Komachi_OnozukaPower, IHasSecondAmount
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new IntVar(nameof(TurnsRemaining), 0),
            new EnergyVar(nameof(ManaGain), 2)
        ];

        public int TurnsRemaining
        {
            get => DynamicVars[nameof(TurnsRemaining)].IntValue;
            set
            {
                DynamicVars[nameof(TurnsRemaining)].BaseValue = value;
                this.InvokeSecondAmountChanged();
            }
        }

        public int ManaGain
        {
            get => DynamicVars[nameof(ManaGain)].IntValue;
            set => DynamicVars[nameof(ManaGain)].BaseValue = value;
        }

        public string GetSecondAmount() => TurnsRemaining.ToString();

        //public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
        //{
        //    TurnsRemaining = Amount;
        //    ManaGain = Amount * 2;
        //}

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power != this)
            {
                return;
            }

            TurnsRemaining += (int)amount;
            ManaGain = Amount * 2;
        }

        public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (!participants.Contains(Owner))
            {
                return;
            }

            Flash();

            if (TurnsRemaining > 0)
            {
                TurnsRemaining--;
                PlayerCmd.EndTurn(Owner.Player, false);
            }
            else
            {
                await PlayerCmd.GainEnergy(ManaGain, Owner.Player);
                await PowerCmd.Remove(this);
            }
        }
    }
}

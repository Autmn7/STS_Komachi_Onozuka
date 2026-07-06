using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class ResidualAether : STS_Komachi_OnozukaCard, IOnDetonatedListener
    {
        public ResidualAether()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {

            WithKeyword(CardKeyword.Retain);
            WithKeyword(CardKeyword.Exhaust);
            WithCostUpgradeBy(-1);
            // Just for tooltip.
            WithEnergy(1);
            WithTip(typeof(VengefulSpiritPower));
            WithKeyword(KomachiKeywords.Detonate);
        }

        public async Task OnDetonated(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        {
            if (DetonateCmd.IsDetonationSuccessful(args) && this.Pile.Type == PileType.Hand)
            {
                DynamicVars.Energy.BaseValue++;
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }
}

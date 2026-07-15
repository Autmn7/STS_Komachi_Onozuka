using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class Quintessence : STS_Komachi_OnozukaCard
    {
        public Quintessence() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
            WithCards(1);
            WithEnergy(1);
            WithPower<GuidedSpiritPower>(nameof(Value1), 2, 1);
            WithPower<DivineSpiritPower>(nameof(Value2), 2, 1);
            WithKeyword(KomachiKeywords.Barrier);
            WithKeyword(CardKeyword.Exhaust);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, target: Owner.Creature, Value1, Owner.Creature, this);
            await PowerCmd.Apply<DivineSpiritPower>(choiceContext, target: Owner.Creature, Value2, Owner.Creature, this);

        }
    }
}

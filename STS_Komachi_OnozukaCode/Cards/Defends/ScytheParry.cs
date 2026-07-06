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
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class ScytheParry : STS_Komachi_OnozukaCard
    {
        public ScytheParry()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
            WithBlock(9, 3);
            WithPower<ReflectPower>(nameof(Value1), 1);
            WithTip(typeof(ManipulateDistanceToken));
            WithKeyword(CardKeyword.Exhaust);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
            if (CombatState == null) return;

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

            await PowerCmd.Apply<ReflectPower>(choiceContext, Owner.Creature, Value1, Owner.Creature, this);

            var mandist = CombatState.CreateCard<ManipulateDistanceToken>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(mandist, PileType.Hand, Owner);
        }
    }
}

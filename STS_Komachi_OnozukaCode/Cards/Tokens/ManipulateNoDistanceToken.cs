using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens
{
    public class ManipulateNoDistanceToken : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;
        public ManipulateNoDistanceToken() : base(0, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
        {
            WithKeywords(CardKeyword.Retain, CardKeyword.Exhaust, KomachiKeywords.Displace);
            WithPower<DistancePower>(nameof(Value1), 0, 0);
        }

        /// <summary>
        /// If you somehow play this, it will just be for the lols.
        /// </summary>
        /// <param name="choiceContext"></param>
        /// <param name="cardPlay"></param>
        /// <returns></returns>
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var list = new List<CardModel>();
            if (IsUpgraded)
            {
                ManipulateNoDistanceToken Pull2 = CombatState.CreateCard<ManipulateNoDistanceToken>(Owner);
                Pull2.AltDescription = 1;
                list.Add(Pull2);
            }
            ManipulateNoDistanceToken Pull1 = CombatState.CreateCard<ManipulateNoDistanceToken>(Owner);
            Pull1.AltDescription = 1;
            list.Add(Pull1);
            ManipulateNoDistanceToken Push1 = CombatState.CreateCard<ManipulateNoDistanceToken>(Owner);
            Push1.AltDescription = 1;
            list.Add(Push1);
            if (IsUpgraded)
            {
                ManipulateNoDistanceToken Push2 = CombatState.CreateCard<ManipulateNoDistanceToken>(Owner);
                Push2.AltDescription = 1;
                list.Add(Push2);
            }

            CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                list,
                Owner,
                canSkip: true);


        }
    }
}

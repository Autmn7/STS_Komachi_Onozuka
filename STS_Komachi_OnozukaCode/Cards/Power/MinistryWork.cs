using MegaCrit.Sts2.Core.CardSelection;
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
using static BaseLib.Utils.BetaMainCompatibility;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class MinistryWork : STS_Komachi_OnozukaCard
    {
        public MinistryWork()
            : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
            // Energy refunded
            WithEnergy(1);
            WithCostUpgradeBy(-1);
            // Choices
            WithVar(nameof(Value1), 3, 2);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            if (CombatState == null) return;
            List<CardModel> cards = CardFactory.GetDistinctForCombat(
                base.Owner,
                base.Owner.Character.CardPool.
                    GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint),
                Value1,
                base.Owner.RunState.Rng.CombatCardGeneration).ToList();

            CardModel selectedCard;
            if (cards.Count <= 3)
            {
                selectedCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, base.Owner);

            }
            else selectedCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, base.Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();


            if (selectedCard != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, base.Owner);
                
                foreach (var card in cards)
                {
                    if (card != selectedCard)
                    {
                        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Exhaust, base.Owner);
                        await CardCmd.Exhaust(choiceContext,card);

                    }
                }
            }

            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            await PowerCmd.Apply<MinistryWorkPower>(choiceContext, Owner.Creature,
                1, Owner.Creature, this);
            

            
        }
    }
}

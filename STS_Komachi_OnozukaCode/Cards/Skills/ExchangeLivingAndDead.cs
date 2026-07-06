using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class ExchangeLivingAndDead : STS_Komachi_OnozukaCard
    {
        public ExchangeLivingAndDead()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithEnergy(1);
            // draw
            WithVar(nameof(Value1), 2);
            WithCostUpgradeBy(-1);
            WithKeyword(CardKeyword.Exhaust);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardPile drawPile = PileType.Draw.GetPile(Owner);

            // Ensure draw pile has a card if possible
            await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);

            var topCard = drawPile.Cards.FirstOrDefault();

            // if the player has completely run out of cards
            if (topCard == null)
            {
                // No card to exhaust means there's no "same type" target, trigger fallback draw immediately
                await CardPileCmd.Draw(choiceContext, Value1, Owner);
                return;
            }

            // Exhaust the top card
            await CardCmd.Exhaust(choiceContext, topCard);

            CardPile exhaust = PileType.Exhaust.GetPile(Owner);

            // Check for valid targets (excluding the freshly exhausted topCard)
            bool hasTarget = exhaust.Cards.Any(c => c.Type == topCard.Type && c != topCard);

            if (hasTarget)
            {
                // Actually capture the selection and move the chosen card to hand
                var selected = await CardSelectCmd.FromCombatPile(
                    choiceContext,
                    exhaust,
                    Owner,
                    new CardSelectorPrefs(SelectionScreenPrompt, 1),
                    (c) => c.Type == topCard.Type && c != topCard
                );

                var chosenCard = selected?.FirstOrDefault();
                if (chosenCard != null)
                {
                    await CardPileCmd.Add(chosenCard, PileType.Hand);
                }
            }
            else
            {
                await CardPileCmd.Draw(choiceContext, Value1, Owner);
            }
        }
    }
}

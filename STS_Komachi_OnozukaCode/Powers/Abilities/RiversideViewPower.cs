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

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{ 
    public class RiversideViewPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != base.Owner.Player)
            {
                return;
            }

            // How many tokens in the hand?
            CardPile hand = PileType.Hand.GetPile(player);
            int existingTokenCount = hand.Cards.Count(c => c.GetType() == typeof(SpiderLily));

            // Calculate the difference needed to reach the target amount
            int tokensToAdd = base.Amount - existingTokenCount;

            // If the hand already has enough or more tokens than the power amount, do nothing
            if (tokensToAdd <= 0)
            {
                return;
            }

            Flash();

            // Generate only the required number of tokens
            CardModel[] array = new CardModel[tokensToAdd];
            for (int i = 0; i < tokensToAdd; i++)
            {
                array[i] = combatState.CreateCard<SpiderLily>(player);
            }

            await CardPileCmd.AddGeneratedCardsToCombat(array, PileType.Hand, base.Owner.Player);
        }

        /// <summary>
        /// Value2 is the poison var.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public override async Task AfterCardEnteredCombat(CardModel card)
        {
            if (card is SpiderLily lily)
            {
                lily.Value2 = 1;
            }
        }
    }
}

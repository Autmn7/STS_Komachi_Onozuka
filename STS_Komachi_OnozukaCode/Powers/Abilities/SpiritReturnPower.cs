using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
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
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{ 
    public class SpiritReturnPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(KomachiKeywords.Clone)];
        /// <summary>
        /// Late so that HardWorking Shinigami works.
        /// </summary>
        public override async Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != base.Owner.Player)
            {
                return;
            }

            Flash();

            var pile = player.PlayerCombatState.ExhaustPile;

            var cardChoices = await CardSelectCmd.FromCombatPile(
                choiceContext,
                pile,
                player,
                new CardSelectorPrefs(SelectionScreenPrompt, 0, Amount), 
                c => c.EnergyCost.GetAmountToSpend() < 2
            );

            if (cardChoices == null) return;

            foreach(var card in cardChoices)
            {
                if (card.GetModifier<RemoveFromCombatCardModifier>() == null) CardModifier.AddModifier<RemoveFromCombatCardModifier>(card);
                card.AddKeyword(KomachiKeywords.Clone);
                await CardPileCmd.Add(card, PileType.Hand);
            }
        }
    }


    
}

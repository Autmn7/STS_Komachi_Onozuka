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
    public class InfernoTempest : STS_Komachi_OnozukaCard
    {
        public InfernoTempest()
            : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) 
        {
            // Also for vengeful spirits
            WithPower<GuidedSpiritPower>(nameof(Value1), 1, 1);
            WithTip(typeof(VengefulSpiritPower));
            WithPower<DivineSpiritPower>(nameof(Value3), 2, 1);
            // Cards to be returned
            WithVar(nameof(Value2), 4, 2);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            List<CardModel> toExile =
            [
                .. Owner.PlayerCombatState.Hand.Cards,
                .. Owner.PlayerCombatState.DrawPile.Cards,
                .. Owner.PlayerCombatState.DiscardPile.Cards,
            ];

            int handCount = 0, drawCount = 0, discardCount = 0;
            List<CardModel> choosable = new();
            List<CardModel> autoReturn = new();

            foreach (CardModel card in toExile)
            {
                PileType currentPile = card.Pile.Type;

                // skip if a side effect already exiled this card mid-loop.
                if (currentPile == PileType.Exhaust) continue;

                await CardCmd.Exhaust(choiceContext, card, skipVisuals:false);

                switch (currentPile)
                {
                    case PileType.Hand: handCount++; break;
                    case PileType.Draw: drawCount++; break;
                    case PileType.Discard: discardCount++; break;
                }
            }

            if (handCount > 0)
                await PowerCmd.Apply<DivineSpiritPower>(choiceContext, Owner.Creature, handCount * Value3, Owner.Creature, this);

            if (drawCount > 0)
                await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, Owner.Creature, drawCount * Value1, Owner.Creature, this);

            if (discardCount > 0)
            {
                foreach (Creature enemy in CombatState.Enemies)
                    await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, discardCount * Value1, Owner.Creature, this);
            }

            await PowerCmd.Apply<InfernoTempestPower>(choiceContext, Owner.Creature, Value2, Owner.Creature, this);
        }
    }
}

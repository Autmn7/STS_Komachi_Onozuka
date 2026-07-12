using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class CommercialTitanic : STS_Komachi_OnozukaRelic
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromCard<SpiderLily>(),
                HoverTipFactory.FromCard<ManipulateDistanceToken>(),
                HoverTipFactory.FromPower<GuidedSpiritPower>(),

            ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new GoldVar(4),
            new PowerVar<GuidedSpiritPower>("Value1", 4)
            ];

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != Owner)
                return;
            if (player.PlayerCombatState?.TurnNumber == 1)
            {
                CardModel created = combatState.CreateCard<ManipulateDistanceToken>(Owner);
                await CardPileCmd.Add(created, PileType.Hand);
                await PowerCmd.Apply<GuidedSpiritPower>(choiceContext,
                    Owner.Creature, DynamicVars["Value1"].BaseValue, Owner.Creature, null);
            }
            else if (player.PlayerCombatState?.TurnNumber == 3)
            {
                Flash();
                CardModel created = combatState.CreateCard<SpiderLily>(Owner);
                await CardPileCmd.Add(created, PileType.Hand);
            }
        }

        /// <summary>
        /// Gain Gold on kill
        /// </summary>
        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (creature.Side != base.Owner.Creature.Side)
            {
                if (!creature.IsSecondaryEnemy)
                {
                    await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
                }
            }
        }
    }
}

using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class ProtectiveSanzuBoat : STS_Komachi_OnozukaCard
    {
        public ProtectiveSanzuBoat()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithBlock(7);
            WithPower<DistancePower>(nameof(Value1), 1);
            // Summon per distance level
            WithVar(nameof(Value2), 1, 1);
            WithKeyword(KomachiKeywords.Displace);
            WithTip(StaticHoverTip.SummonStatic);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            await DistanceCmd.Displace(choiceContext, cardPlay.Target, 1, Owner.Creature, this);

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

            var enemyDisplacement = DistancePower.GetLevel(cardPlay.Target);
            await OstyCmd.Summon(choiceContext, Owner, enemyDisplacement * Value2, this);
        }
    }
}

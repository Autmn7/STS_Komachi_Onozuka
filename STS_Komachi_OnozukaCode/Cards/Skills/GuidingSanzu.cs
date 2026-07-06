using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class GuidingSanzu : STS_Komachi_OnozukaCard
    {
        public GuidingSanzu() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
            WithPower<GuidedSpiritPower>(nameof(Value1), 5, 2);
            // WithCostUpgradeBy(-1);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, target: Owner.Creature, Value1, Owner.Creature, this);
        }
        //protected override void OnUpgrade()
        //{
        //    base.DynamicVars.Block.UpgradeValueBy(2m);
        //    DynamicVars[nameof(GuidedApplication)].UpgradeValueBy(2);
        //}
    }
}

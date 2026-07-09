using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
      
    public class DivineProtection : STS_Komachi_OnozukaCard
    {
        public DivineProtection() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithBlock(8);
            WithPower<DivineSpiritPower>(nameof(Value1), 4, 2);
            // release cost
            WithVar(nameof(ReleaseCost), 4);
            WithKeyword(KomachiKeywords.Release);
            WithKeyword(CardKeyword.Exhaust);
            WithTip(StaticHoverTip.SummonStatic);
        }

        public override bool GainsBlock => true;
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardModel releaseChoice = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);
            await PowerCmd.Apply<DivineSpiritPower>(choiceContext, target: Owner.Creature, Value1, Owner.Creature, this);

            if (ReleaseCmd.ChoseRelease(releaseChoice))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, ReleaseCost, this);

                await PowerCmd.Apply<DivineSpiritPower>(choiceContext, target: Owner.Creature, Value1, Owner.Creature, this);
            }
        }
        //protected override void OnUpgrade()
        //{
        //    base.DynamicVars.Block.UpgradeValueBy(2m);
        //    DynamicVars[nameof(GuidedApplication)].UpgradeValueBy(2);
        //}
    }
}

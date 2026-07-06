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
      
    public class SpiritBarrier : STS_Komachi_OnozukaCard
    {
        public SpiritBarrier() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
            WithBlock(9, 3);
            WithKeyword(KomachiKeywords.Release);
            WithTip(StaticHoverTip.SummonStatic);

            // Player must have at least 1 spirit
            WithVar(nameof(ReleaseCost), 1);
        }

        public override bool GainsBlock => true;
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            CardModel releaseChoice = await ReleaseCmd.ChooseRelease(choiceContext, this, ReleaseCost);

            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay, false);


            int guided = Owner.Creature.GetPower<GuidedSpiritPower>()?.Amount ?? 0;
            int divine = Owner.Creature.GetPower<DivineSpiritPower>()?.Amount ?? 0;
            int total = guided + divine;

            if (ReleaseCmd.ChoseRelease(releaseChoice))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, total, this);

                await OstyCmd.Summon(choiceContext, Owner, total, this);
            }
        }

        //protected override void OnUpgrade()
        //{
        //    base.DynamicVars.Block.UpgradeValueBy(2m);
        //    DynamicVars[nameof(GuidedApplication)].UpgradeValueBy(2);
        //}
    }
}

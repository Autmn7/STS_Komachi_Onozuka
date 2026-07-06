using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Basics
{
      
    public class StrikeKomachi : STS_Komachi_OnozukaCard
    {
        public StrikeKomachi() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
        {
            WithTags(CardTag.Strike);
            WithDamage(6, 3);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        //protected override void OnUpgrade()
        //{
        //    DynamicVars.Damage.UpgradeValueBy(3);
        //}
    }
}

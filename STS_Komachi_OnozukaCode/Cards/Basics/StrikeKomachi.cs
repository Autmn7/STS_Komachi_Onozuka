using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
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
                .BeforeDamage(() => 
                    DanmakuCmd.FireAndWaitForHit
                    (_pattern, Owner.Creature, 
                    new[] { cardPlay.Target }, 
                    Owner.Creature.GetVfxContainer()))
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        //protected override void OnUpgrade()
        //{
        //    DynamicVars.Damage.UpgradeValueBy(3);
        //}

        static readonly List<DanmakuPiece> _pattern = new()
            {
                new DanmakuPiece
                {
                    SpritePath = "knife.png".BulletImagePath(),
                    Group = 1,
                    WayCount = 5f,
                    Range = 20f,
                    StartSpeed = 900f,
                    LifeSeconds = 2f,
                    RootType = DanmakuRootType.Shooter,
                    Aim = DanmakuAimType.PerGroup,
                    GatesDamage = true,
                },
                new DanmakuPiece
                {
                    SpritePath = "knife.png".BulletImagePath(),
                    Group = 3,
                    GIntervalSeconds = 0.08f,
                    WayCount = 1f,
                    StartSpeed = new GrowthValue { Base = 600f, PerGroup = 100f }, // speeds up each successive group
                    StartTimeSeconds = 0.1f,
                    RootType = DanmakuRootType.Shooter,
                    Aim = DanmakuAimType.FirstGroupAim,
                    GatesDamage = false,
                },
            };
    }
}

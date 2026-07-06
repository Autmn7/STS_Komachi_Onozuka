using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens
{

    [Pool(typeof(TokenCardPool))]
    public class ReleaseToken : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;
        public ReleaseToken() : base(0, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
        {
            WithKeywords(
                //CardKeyword.Retain, CardKeyword.Exhaust, don't need these
                KomachiKeywords.Release);
        }

        public override int MaxUpgradeLevel => 0;
    }
}

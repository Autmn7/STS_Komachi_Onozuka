using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Minions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Other;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class EthicallySourcedBreak : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;
        public EthicallySourcedBreak()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
            // Summon amount
            WithVar(nameof(Value1), 25, 10);
            WithKeyword(KomachiKeywords.Barrier);
            // Strength amount
            WithPower<StrengthPower>(nameof(Value2), 2, 1);
            WithHeal(5, 5);
            WithEnergy(2);
            WithKeyword(CardKeyword.Exhaust);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
            await DivineSpiritCmd.Summon(choiceContext, Owner, Value1, this);
            await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, Value2, base.Owner.Creature, this);
            await PowerCmd.Apply<EthicallySourcedBreakPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
            PlayerCmd.EndTurn(base.Owner, canBackOut: false);

            // Says "ZZZZzzzzzzzz...."
            TalkCmd.Play(RawExtraDescription1, Owner.Creature,
                MegaCrit.Sts2.Core.Nodes.Vfx.VfxColor.White,
                MegaCrit.Sts2.Core.Nodes.Vfx.VfxDuration.VeryLong);
        }
    }
}

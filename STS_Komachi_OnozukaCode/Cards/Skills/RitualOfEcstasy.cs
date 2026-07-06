using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class RitualOfEcstasy : STS_Komachi_OnozukaCard
    {
        public RitualOfEcstasy()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithTip(typeof(DistancePower));
            WithKeyword(KomachiKeywords.Displace);
            // Block on +
            WithBlock(6, 3);
            // Draw on -
            WithVar(nameof(Value1), 2, 1);

        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            // Displacement happens before block for certain powers that care about it.
            var enemyDisplacement = DistancePower.GetLevel(cardPlay.Target);

            // 3*2 - X inverts the number around 3. Check the maffs.
            var InvertedDistance = 6 - enemyDisplacement;

            var displacement = InvertedDistance - enemyDisplacement;

            if (displacement != 0)
            {
                var displacementAction = await DistanceCmd.Displace(choiceContext, cardPlay.Target, displacement, Owner.Creature);
                // Gain block
                if (displacementAction.Change > 0)
                {
                    await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
                }
                else if (displacementAction.Change < 0)
                {
                    await CardPileCmd.Draw(choiceContext, Value1, Owner);
                }
            }
            else
            {
                RitualOfEcstasy blockChoice = CombatState.CreateCard<RitualOfEcstasy>(Owner);
                blockChoice.AltDescription = 1;
                blockChoice.ExtraDescription1 = RawExtraDescription1.GetFormattedText();
                RitualOfEcstasy drawChoice = CombatState.CreateCard<RitualOfEcstasy>(Owner);
                drawChoice.AltDescription = 2;
                drawChoice.ExtraDescription2 = RawExtraDescription2.GetFormattedText();

                RitualOfEcstasy choice = (RitualOfEcstasy)await CardSelectCmd.FromChooseACardScreen(choiceContext, [blockChoice, drawChoice], Owner);

                if (choice.AltDescription == 1)
                {
                    await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
                }
                else if (choice.AltDescription == 2)
                {
                    await CardPileCmd.Draw(choiceContext, Value1, Owner);
                }
            }
        }
    }
}

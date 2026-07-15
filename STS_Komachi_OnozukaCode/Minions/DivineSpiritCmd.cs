using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MinionLib.Commands;
using MinionLib.Minion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Minions
{
    public static class DivineSpiritCmd
    {
        public static async Task Summon(
            PlayerChoiceContext choiceContext,
            Player owner,
            int amount,
            AbstractModel? source)
        {
            if (amount <= 0) return;

            var existing = owner.PlayerCombatState?.Pets
                .FirstOrDefault(p => p.Monster is DivineSpiritMinion);

            if (CombatManager.Instance.IsInProgress)
            {
                SfxCmd.Play("event:/sfx/characters/necrobinder/necrobinder_summon");
            }

            // Never summoned this combat: max hp = X.
            if (existing == null)
            {
                CardModel? cardSource = null;
                if (source is CardModel) cardSource = source as CardModel;
                _ = await MinionCmd.AddMinion<DivineSpiritMinion>(choiceContext, owner,
                    new MinionSummonOptions(
                        MaxHp: amount,
                        Source: cardSource,
                        Position: MinionPosition.Front));
            }
            // Dead but in combat
            else if (existing.CurrentHp <= 0)
            {
                await CreatureCmd.SetMaxHp(existing, amount);
                await CreatureCmd.Heal(existing, amount, true); // isReviving: true
            }
            // Alive
            else
            {
                await CreatureCmd.GainMaxHp(existing, amount);
            }
        }
    }
}

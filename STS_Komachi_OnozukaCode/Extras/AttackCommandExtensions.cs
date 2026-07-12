using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras
{
    public static class AttackCommandExtensions
    {
        public static AttackCommand WithDanmaku(this AttackCommand builder, List<DanmakuPiece> pattern)
        {
            return builder.BeforeDamage(async () =>
            {
                Creature? shooter = builder.Attacker;
                IReadOnlyList<Creature> targets = builder.GetPossibleTargets();
                if (shooter == null || targets.Count == 0) return;

                // Snapshot + clear whatever WithHitFx set, so AttackCommand.Execute's own
                // HitVfx playback (which happens earlier in its pipeline, before BeforeDamage
                // ever runs) doesn't fire it early. We replay it ourselves once the danmaku
                // actually resolves.
                string? deferredHitVfx = builder.HitVfx;
                builder.HitVfx = null;

                Control? container = shooter.GetVfxContainer();
                if (container == null) return;

                await DanmakuCmd.FireAndWaitForHit(
                    pattern,
                    shooter,
                    targets,
                    container,
                    onHitExtra: () =>
                    {
                        if (deferredHitVfx == null) return;
                        // Same play pattern AttackCommand itself would've used — single vs.
                        // multi target mirrors the _spawnVfxOnEachCreature branch in Execute.
                        if (targets.Count == 1)
                        {
                            VfxCmd.PlayOnCreatureCenter(targets[0], deferredHitVfx);
                        }
                        else
                        {
                            VfxCmd.PlayOnCreatureCenters(targets, deferredHitVfx);
                        }
                    });
            });
        }
    }
}

using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku
{
    public static class DanmakuCmd
    {
        public const float DefaultTimeoutSeconds = 1.5f;

        public static async Task FireAndWaitForHit(IReadOnlyList<DanmakuPiece> pieces, Creature shooter, IReadOnlyList<Creature> targets, Control container, float timeoutSeconds = DefaultTimeoutSeconds)
        {
            if (TestMode.IsOn || targets.Count == 0 || pieces.Count == 0) return;

            bool anyGates = pieces.Any(p => p.GatesDamage);
            var tcs = new TaskCompletionSource<bool>();

            var pieceTasks = pieces.Select(piece =>
                RunPiece(piece, shooter, targets, container,
                    (piece.GatesDamage || !anyGates) ? (() => tcs.TrySetResult(true)) : null)
            ).ToList();

            Task timeoutTask = Cmd.CustomScaledWait(timeoutSeconds, timeoutSeconds);
            await Task.WhenAny(tcs.Task, timeoutTask);
            // Intentionally NOT cleaning up bullets here. Gate resolving just means damage
            // is allowed to proceed — bullets keep flying and self-despawn on their own
            // lifetime/off-screen check in NDanmakuBullet._Process, same as any bullet that
            // never hit anything. Firing tasks (pieceTasks) also aren't awaited/cancelled
            // here on purpose: later Groups in a multi-Group Piece should keep emitting on
            // schedule even after damage has already resolved, rather than being cut short.
        }

        static async Task RunPiece(DanmakuPiece piece, Creature shooter, IReadOnlyList<Creature> targets, Control container, Action? onHit)
        {
            if (piece.StartTimeSeconds > 0f)
                await Cmd.CustomScaledWait(piece.StartTimeSeconds, piece.StartTimeSeconds);

            Creature primaryTarget = targets[0];
            float? firstGroupAimDeg = null;

            for (int group = 0; group < piece.Group; group++)
            {
                Vector2 rootPos = ResolveRoot(piece.RootType, shooter, primaryTarget);

                float aimDeg = piece.Aim switch
                {
                    DanmakuAimType.None => 0f,
                    DanmakuAimType.FirstGroupAim => firstGroupAimDeg ??= AimAngleDeg(rootPos, primaryTarget),
                    _ => AimAngleDeg(rootPos, primaryTarget),
                };

                int wayCount = Math.Max(1, (int)MathF.Round(piece.WayCount.Evaluate(group, 0)));
                float gangle = piece.GAngle.Evaluate(group, 0);
                float range = piece.Range.Evaluate(group, 0);
                float scale = piece.Scale.Evaluate(group, 0);
                if (scale == 0f) scale = 1f;

                Vector2 offset = new(piece.X.Evaluate(group, 0), piece.Y.Evaluate(group, 0));
                Vector2 basePos = rootPos + offset;

                for (int way = 0; way < wayCount; way++)
                {
                    float spreadDeg = wayCount <= 1 ? 0f : -range / 2f + range * ((float)way / (wayCount - 1));
                    float preRadiusAngleDeg = aimDeg + gangle + spreadDeg;
                    float preRadiusAngleRad = Mathf.DegToRad(preRadiusAngleDeg);

                    float radius = piece.Radius.Evaluate(group, way);
                    Vector2 spawnPos = basePos + radius * new Vector2(Mathf.Cos(preRadiusAngleRad), Mathf.Sin(preRadiusAngleRad));

                    float radiusA = piece.RadiusA.Evaluate(group, way);
                    float finalAngleRad = Mathf.DegToRad(preRadiusAngleDeg + radiusA);

                    float speed = piece.StartSpeed.Evaluate(group, way);
                    float acc = piece.StartAcc.Evaluate(group, way);
                    float accAngle = piece.StartAccAngle.Evaluate(group, way);

                    NDanmakuBullet bullet = NDanmakuBullet.Create(piece.SpritePath, scale, spawnPos, speed, finalAngleRad, acc, accAngle, piece.LifeSeconds, targets, onHit);
                    container.AddChildSafely(bullet);
                }

                if (group < piece.Group - 1 && piece.GIntervalSeconds > 0f)
                    await Cmd.CustomScaledWait(piece.GIntervalSeconds, piece.GIntervalSeconds);
            }
        }

        static Vector2 ResolveRoot(DanmakuRootType rootType, Creature shooter, Creature target) => rootType switch
        {
            DanmakuRootType.Shooter => shooter.GetCreatureNode()?.VfxSpawnPosition ?? Vector2.Zero,
            DanmakuRootType.Target => target.GetCreatureNode()?.VfxSpawnPosition ?? Vector2.Zero,
            DanmakuRootType.World => Vector2.Zero,
            _ => Vector2.Zero,
        };

        static float AimAngleDeg(Vector2 from, Creature target)
        {
            Vector2 targetPos = target.GetCreatureNode()?.VfxSpawnPosition ?? from;
            return Mathf.RadToDeg((targetPos - from).Angle());
        }
    }
}

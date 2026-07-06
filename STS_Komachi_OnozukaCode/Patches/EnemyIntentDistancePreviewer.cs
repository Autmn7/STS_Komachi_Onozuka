using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches
{
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
    public static class NCreature_UpdateIntent_DistancePreview_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCreature __instance, IEnumerable<Creature> targets)
            => EnemyIntentDistancePreviewController.OnIntentsUpdated(__instance, targets);
    }

    [HarmonyPatch(typeof(NCreature), "_ExitTree")]
    public static class NCreature_ExitTree_DistancePreviewCleanup_Patch
    {
        [HarmonyPostfix]
        static void Postfix(NCreature __instance)
            => EnemyIntentDistancePreviewController.Remove(__instance);
    }
    public static class EnemyIntentDistancePreviewController
    {
        static readonly Dictionary<NCreature, NEnemyIntentDistanceCluster> _activeClusters = new();
        static readonly Dictionary<NCreature, List<Creature>> _lastTargets = new();

        /// <summary>Called from the NCreature.UpdateIntent patch below — targets here are the
        /// same list the intent's own label text was just computed against.</summary>
        public static void OnIntentsUpdated(NCreature creatureNode, IEnumerable<Creature> targets)
        {
            var targetList = targets.ToList();
            _lastTargets[creatureNode] = targetList;
            Refresh(creatureNode, targetList);
        }

        /// <summary>Called directly from your KomachiHooks.OnDistanceChanged.</summary>
        public static void OnDistanceChanged(Creature creature)
        {
            NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (creatureNode == null) return;
            List<Creature> targets = _lastTargets.TryGetValue(creatureNode, out var cached)
                ? cached
                : creature.CombatState?.PlayerCreatures.ToList() ?? new List<Creature>();
            Refresh(creatureNode, targets);
        }

        static void Refresh(NCreature creatureNode, IReadOnlyList<Creature> targets)
        {
            if (_activeClusters.TryGetValue(creatureNode, out var existing))
            {
                existing.QueueFreeSafely();
                _activeClusters.Remove(creatureNode);
            }

            var preview = DistancePower.PreviewIntentDamageAcrossDistances(creatureNode.Entity, targets);
            if (preview == null) return;

            var cluster = NEnemyIntentDistanceCluster.Create(preview.Value.DamageByLevel, preview.Value.CurrentLevel);
            creatureNode.AddChildSafely(cluster);
            cluster.Position = ComputeLocalPosition(creatureNode, cluster);
            _activeClusters[creatureNode] = cluster;
        }

        static Vector2 ComputeLocalPosition(NCreature creatureNode, NEnemyIntentDistanceCluster cluster)
        {
            // Local space, same convention as IntentContainer.Position (both are direct
            // children of creatureNode) — this is what fixes the center-screen bug.
            Vector2 intentPos = creatureNode.IntentContainer.Position;
            Vector2 intentSize = creatureNode.IntentContainer.Size;
            return new Vector2(
                intentPos.X + intentSize.X * 0.5f - cluster.Size.X * 0.5f,
                intentPos.Y - cluster.Size.Y - 12f);
        }

        public static void Remove(NCreature creatureNode)
        {
            if (_activeClusters.TryGetValue(creatureNode, out var existing))
            {
                existing.QueueFreeSafely();
                _activeClusters.Remove(creatureNode);
            }
            _lastTargets.Remove(creatureNode);
        }

        public static void ClearAll()
        {
            foreach (var c in _activeClusters.Values) c.QueueFreeSafely();
            _activeClusters.Clear();
            _lastTargets.Clear();
        }
    }
    public partial class NEnemyIntentDistanceCluster : Control
    {
        const float ChipSize = 34f; // smaller than the 56f card-hover cluster — this is secondary/ambient info
        const float ChipGap = 4f;

        public static NEnemyIntentDistanceCluster Create(IReadOnlyDictionary<int, decimal> damageByLevel, int currentLevel)
        {
            var cluster = new NEnemyIntentDistanceCluster();
            cluster.Size = new Vector2(5 * ChipSize + 4 * ChipGap, ChipSize);

            for (int level = 1; level <= 5; level++)
            {
                cluster.AddChild(new Label
                {
                    Text = damageByLevel[level].ToString("0"),
                    Position = new Vector2((level - 1) * (ChipSize + ChipGap), 0),
                    Size = new Vector2(ChipSize, ChipSize),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = level == currentLevel ? StsColors.red : StsColors.halfTransparentWhite
                });
            }
            return cluster;
        }
    }
}

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers
{
    public interface IHasAmbientDamagePreview
    {
        /// Null = don't show a chip for this power right now (e.g. no valid target).
        decimal? GetAmbientPreviewDamage();
    }

    // Dont patch these failures
    //[HarmonyPatch(typeof(NCreature), "_Ready")]
    //internal static class NCreature_Ready_AmbientDamagePreview_Patch
    //{
    //    [HarmonyPostfix]
    //    static void Postfix(NCreature __instance) => AmbientDamagePreviewController.OnCreatureReady(__instance);
    //}

    //[HarmonyPatch(typeof(NCreature), "_ExitTree")]
    //internal static class NCreature_ExitTree_AmbientDamagePreview_Patch
    //{
    //    [HarmonyPostfix]
    //    static void Postfix(NCreature __instance) => AmbientDamagePreviewController.OnCreatureExit(__instance);
    //}

    public static class AmbientDamagePreviewController
    {
        static readonly Dictionary<Creature, NAmbientDamageRow> _enemyClusters = new();
        static readonly Dictionary<Creature, NAmbientDamageCluster> _playerClusters = new();
        static readonly Dictionary<Creature, Action> _unsubscribers = new();

        public static void OnCreatureReady(NCreature node)
        {
            Creature creature = node.Entity;
            void Refresh(CombatState _) => RefreshFor(node);
            void RefreshPower(PowerModel _) => RefreshFor(node);
            void RefreshIncrease(PowerModel _, int __, bool ___) => RefreshFor(node);

            creature.PowerApplied += RefreshPower;
            creature.PowerRemoved += RefreshPower;
            creature.PowerIncreased += RefreshIncrease;
            CombatManager.Instance.StateTracker.CombatStateChanged += Refresh;

            _unsubscribers[creature] = () =>
            {
                creature.PowerApplied -= RefreshPower;
                creature.PowerRemoved -= RefreshPower;
                creature.PowerIncreased -= RefreshIncrease;
                CombatManager.Instance.StateTracker.CombatStateChanged -= Refresh;
            };

            RefreshFor(node);
        }

        public static void OnCreatureExit(NCreature node)
        {
            Creature creature = node.Entity;
            if (_unsubscribers.TryGetValue(creature, out var unsub)) unsub();
            _unsubscribers.Remove(creature);
            RemoveCluster(creature);
        }

        static void RefreshFor(NCreature node)
        {
            if (node.Entity.IsPlayer)
            {
                RefreshPlayerCluster(node);
            }
            else
            {
                RefreshEnemyRow(node);
            }
        }

        static void RefreshPlayerCluster(NCreature node)
        {
            Creature creature = node.Entity;
            decimal total = creature.Powers
                .OfType<IHasAmbientDamagePreview>()
                .Select(p => p.GetAmbientPreviewDamage())
                .Where(d => d.HasValue)
                .Sum(d => d!.Value);

            // Cleanup existing if present
            if (_playerClusters.TryGetValue(creature, out var existing))
            {
                existing.QueueFreeSafely();
                _playerClusters.Remove(creature);
            }

            if (total <= 0m) return;

            // Create new
            var cluster = NAmbientDamageCluster.Create(total);
            node.AddChildSafely(cluster);
            cluster.Position = PositionAbovePlayer(node, cluster);
            _playerClusters[creature] = cluster;
        }

        static void RefreshEnemyRow(NCreature node)
        {
            Creature creature = node.Entity;
            var values = creature.Powers
                .OfType<IHasAmbientDamagePreview>()
                .Select(p => p.GetAmbientPreviewDamage())
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .ToList();

            // Cleanup existing if present
            if (_enemyClusters.TryGetValue(creature, out var existing))
            {
                existing.QueueFreeSafely();
                _enemyClusters.Remove(creature);
            }

            if (values.Count == 0) return;

            // Create new
            var row = NAmbientDamageRow.Create(values, node.Hitbox.Size.X);
            node.AddChildSafely(row);
            row.Position = PositionBelowEnemy(node, row);
            _enemyClusters[creature] = row;
        }

        static void RemoveCluster(Creature creature)
        {
            if (_enemyClusters.TryGetValue(creature, out var enemyRow))
            {
                enemyRow.QueueFreeSafely();
                _enemyClusters.Remove(creature);
            }

            if (_playerClusters.TryGetValue(creature, out var playerCluster))
            {
                playerCluster.QueueFreeSafely();
                _playerClusters.Remove(creature);
            }
        }

        static Vector2 PositionAbovePlayer(NCreature node, Control widget)
        {
            float margin = 40f;
            Vector2 hitboxTop = node.GetTopOfHitbox() - node.GlobalPosition; // to local space
            return new Vector2(hitboxTop.X - widget.Size.X * 0.5f, hitboxTop.Y - widget.Size.Y - margin);
        }

        static Vector2 PositionBelowEnemy(NCreature node, Control widget)
        {
            // We use a small base margin and multiply it by the creature's current scale.
            // This prevents the cluster from being too far from small enemies 
            // or clipping into the models of huge enemies.
            float baseMargin = 40f;
            float scaledMargin = baseMargin * node.Visuals.Scale.Y;
            MainFile.LogMessage("Visual scale of creature is " + node.Visuals.Scale.Y);
            Vector2 hitboxBottom = node.GetBottomOfHitbox() - node.GlobalPosition;

            return new Vector2(
                hitboxBottom.X - widget.Size.X * 0.5f,
                hitboxBottom.Y + scaledMargin
            );

        }
    }

    public partial class NAmbientDamageRow : Control
    {
        const float BaselineChipSize = 32f;
        const float BaselineHitboxWidth = 150f; // guessed reference width — tune against real creatures
        const float MinChipScale = 0.6f;

        public static NAmbientDamageRow Create(IReadOnlyList<decimal> damageValues, float hitboxWidth)
        {
            float scale = Mathf.Clamp(hitboxWidth / BaselineHitboxWidth, MinChipScale, 1f);
            float chipSize = BaselineChipSize * scale;
            float gap = 4f * scale;

            var row = new NAmbientDamageRow();
            row.MouseFilter = MouseFilterEnum.Ignore;
            float totalWidth = damageValues.Count * chipSize + Mathf.Max(0, damageValues.Count - 1) * gap;
            row.Size = new Vector2(Mathf.Max(totalWidth, hitboxWidth), chipSize);

            float startX = Mathf.Max(0f, (hitboxWidth - totalWidth) * 0.5f);
            for (int i = 0; i < damageValues.Count; i++)
            {
                row.AddChild(new Label
                {
                    Text = damageValues[i].ToString("0"),
                    Position = new Vector2(startX + i * (chipSize + gap), 0),
                    Size = new Vector2(chipSize, chipSize),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = StsColors.red,
                    MouseFilter = MouseFilterEnum.Ignore
                });
            }
            return row;
        }
    }

    public partial class NAmbientDamageCluster : Control
    {
        public static NAmbientDamageCluster Create(decimal damage)
        {
            var cluster = new NAmbientDamageCluster { Size = new Vector2(40f, 24f) };
            cluster.AddChild(new Label
            {
                Text = damage.ToString("0"),
                Size = cluster.Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = StsColors.red
            });
            return cluster;
        }
    }
}

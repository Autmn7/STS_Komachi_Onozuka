using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku
{
    public class DanmakuPiece
    {
        public required string SpritePath;

        public int Group = 1;
        public float GIntervalSeconds = 0f;

        /// <summary>Bullets per group. Rounded to int at use time.</summary>
        public GrowthValue WayCount = 1f;

        public GrowthValue GAngle = 0f;   // center angle of the group's spread, degrees
        public GrowthValue Range = 0f;    // total spread width across WayCount bullets, degrees
        public GrowthValue Scale = 1f;    // 0 is treated as 1, matching the guide's convention

        public float StartTimeSeconds = 0f;
        public float LifeSeconds = 5f;    // guide's "300 frames" default ≈ 5s at 60fps

        public GrowthValue StartSpeed = 1f;
        public GrowthValue StartAcc = 0f;       // change in speed, per second
        public GrowthValue StartAccAngle = 0f;  // change in angle, degrees per second

        public DanmakuRootType RootType = DanmakuRootType.Shooter;
        public GrowthValue X = 0f;
        public GrowthValue Y = 0f;
        public GrowthValue Radius = 0f;
        public GrowthValue RadiusA = 0f;

        public DanmakuAimType Aim = DanmakuAimType.PerGroup;

        /// <summary>
        /// If true, a bullet from this Piece hitting a target's hitbox is what allows
        /// AttackCommand's damage to proceed. If no Piece in an attack sets this true,
        /// every Piece's bullets gate damage (so a single-Piece attack works with zero config).
        /// Mirrors LBoL's LastWave, minus the multi-hit-tracking baggage — see notes.
        /// </summary>
        public bool GatesDamage = false;
    }

    /// <summary>
    /// Replaces LBoL's 4x2 ArrayCalculate matrix. Default linear behavior:
    ///   value = Base + PerGroup * groupId + PerWay * wayId
    /// Set CustomFunc for anything nonlinear (quadratic, easing, whatever) — it fully
    /// overrides the linear calculation when present.
    /// </summary>
    public class GrowthValue
    {
        public float Base;
        public float PerGroup;
        public float PerWay;
        public Func<int, int, float>? CustomFunc;

        public float Evaluate(int groupId, int wayId)
            => CustomFunc?.Invoke(groupId, wayId) ?? Base + PerGroup * groupId + PerWay * wayId;

        public static implicit operator GrowthValue(float constant) => new() { Base = constant };
    }

    public static class DanmakuTime
    {
        /// <summary>Convenience for porting values out of frame-based danmaku guides (assumes 60fps).</summary>
        public static float FramesToSeconds(float frames) => frames / 60f;
    }

    public enum DanmakuRootType { Shooter, Target, World }

    public enum DanmakuAimType
    {
        /// <summary>GAngle is absolute; 0° = right. No target-relative aiming.</summary>
        None,
        /// <summary>Recompute aim-to-target direction fresh for every group.</summary>
        PerGroup,
        /// <summary>Compute aim-to-target once for group 0, reuse for every later group.</summary>
        FirstGroupAim
    }
}

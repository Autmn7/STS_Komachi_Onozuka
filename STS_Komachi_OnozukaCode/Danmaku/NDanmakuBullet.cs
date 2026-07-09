using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku
{
    public partial class NDanmakuBullet : Sprite2D
    {
        float _speed;
        float _angleRad;
        float _acc;
        float _accAngleRad;
        float _elapsed;
        float _lifetimeSeconds;
        IReadOnlyList<Creature> _targets;
        Action? _onHit;

        public static NDanmakuBullet Create(string spritePath, float scale, Vector2 spawnPos, float speed, float angleRad, float acc, float accAngleDeg, float lifetimeSeconds, IReadOnlyList<Creature> targets, Action? onHit)
        {
            var bullet = new NDanmakuBullet
            {
                Texture = ResourceLoader.Load<Texture2D>(spritePath),
                Scale = Vector2.One * scale,
                GlobalPosition = spawnPos,
                _speed = speed,
                _angleRad = angleRad,
                _acc = acc,
                _accAngleRad = Mathf.DegToRad(accAngleDeg),
                _lifetimeSeconds = lifetimeSeconds,
                _targets = targets,
                _onHit = onHit,
                Rotation = angleRad
            };
            return bullet;
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;
            _elapsed += dt;
            _speed += _acc * dt;
            _angleRad += _accAngleRad * dt;
            Rotation = _angleRad;
            GlobalPosition += new Vector2(Mathf.Cos(_angleRad), Mathf.Sin(_angleRad)) * _speed * dt;

            Rect2 screenBounds = GetViewport().GetVisibleRect().Grow(64f);
            if (_elapsed > _lifetimeSeconds || !screenBounds.HasPoint(GlobalPosition))
            {
                this.QueueFreeSafely();
                return;
            }

            foreach (Creature target in _targets)
            {
                if (target.IsDead) continue;
                NCreature? node = target.GetCreatureNode();
                if (node != null && node.Hitbox.GetGlobalRect().HasPoint(GlobalPosition))
                {
                    _onHit?.Invoke();
                    this.QueueFreeSafely();
                    return;
                }
            }
        }
    }
}

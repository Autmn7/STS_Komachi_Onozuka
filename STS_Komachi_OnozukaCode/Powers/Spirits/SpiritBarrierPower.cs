using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits
{
    /// <summary>
    /// Power used for the Divine Spirit to block damage for you. 
    /// Essentially just a copy of Osty's Die for you.
    /// </summary>
    public sealed class SpiritBarrierPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override bool ShouldPlayVfx => false;

        public override Creature ModifyUnblockedDamageTarget(
            Creature target, decimal _, ValueProp props, Creature? __)
        {
            if (target != base.Owner.PetOwner?.Creature)
                return target;

            if (base.Owner.IsDead)
                return target;

            if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
                return target;

            return base.Owner;
        }

        public override bool ShouldAllowHitting(Creature creature) => creature.IsAlive;


        public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
    }
}

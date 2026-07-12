using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Potions
{
    [Pool(typeof(STS_Komachi_OnozukaPotionPool))]
    public abstract class STS_Komachi_OnozukaPotion : CustomPotionModel
    {
        public override string? CustomPackedImagePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
        public override string? CustomPackedOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".PotionImagePath();
    }
}
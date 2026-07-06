using BaseLib.Abstracts;
using BaseLib.Utils;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Potions
{
    [Pool(typeof(STS_Komachi_OnozukaPotionPool))]
    public abstract class STS_Komachi_OnozukaPotion : CustomPotionModel;
}
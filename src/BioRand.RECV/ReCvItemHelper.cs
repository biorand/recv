using IntelOrca.Biohazard;

namespace IntelOrca.Biohazard.BioRand.RECV;

public static class ReCvItemHelper
{
    public static ItemAttribute GetItemAttributes(byte item)
    {
        switch (item)
        {
            case ReCvItemIds.RocketLauncher:
            case ReCvItemIds.AssaultRifle:
            case ReCvItemIds.SniperRifle:
            case ReCvItemIds.Shotgun:
            case ReCvItemIds.HandgunGlock17:
            case ReCvItemIds.GrenadeLauncher:
            case ReCvItemIds.BowGun:
            case ReCvItemIds.CombatKnife:
            case ReCvItemIds.Handgun:
            case ReCvItemIds.CustomHandgun:
            case ReCvItemIds.LinearLauncher:
            case ReCvItemIds.M93RPart:
            case ReCvItemIds.Magnum:
            case ReCvItemIds.GoldLugers:
            case ReCvItemIds.SubMachineGun:
            case ReCvItemIds.DuraluminCaseM93RParts:
            case ReCvItemIds.M1P:
            case ReCvItemIds.EnhancedHandgun:
                return ItemAttribute.Weapon;

            case ReCvItemIds.HandgunBullets:
            case ReCvItemIds.MagnumBullets:
            case ReCvItemIds.ShotgunShells:
            case ReCvItemIds.GrenadeRounds:
            case ReCvItemIds.AcidRounds:
            case ReCvItemIds.FlameRounds:
            case ReCvItemIds.BowGunArrows:
            case ReCvItemIds.MagnumBulletsInsideCase:
            case ReCvItemIds.BOWGasRounds:
            case ReCvItemIds.MGunBullets:
            case ReCvItemIds.RifleBullets:
            case ReCvItemIds.ARifleBullets:
            case ReCvItemIds.DuraluminCaseMagnumRounds:
            case ReCvItemIds.CalicoBullets:
                return ItemAttribute.Ammo;

            case ReCvItemIds.BowGunPowder:
            case ReCvItemIds.GunPowderArrow:
            case ReCvItemIds.DuraluminCaseBowGunPowder:
            case ReCvItemIds.BowGunPowderUnused:
            case ReCvItemIds.ClementMixture:
                return ItemAttribute.Gunpowder;

            case ReCvItemIds.FAidSpray:
            case ReCvItemIds.GreenHerb:
            case ReCvItemIds.RedHerb:
            case ReCvItemIds.BlueHerb:
            case ReCvItemIds.MixedHerb2Green:
            case ReCvItemIds.MixedHerbRedGreen:
            case ReCvItemIds.MixedHerbBlueGreen:
            case ReCvItemIds.MixedHerb2GreenBlue:
            case ReCvItemIds.MixedHerb3Green:
            case ReCvItemIds.MixedHerbGreenBlueRed:
                return ItemAttribute.Heal;

            case ReCvItemIds.InkRibbon:
                return ItemAttribute.InkRibbon;
            case ReCvItemIds.SidePack:
                return ItemAttribute.Special;
            case ReCvItemIds.GasMask:
            case >= ReCvItemIds.AlexandersPierce and <= ReCvItemIds.CrestKeyG:
                return ItemAttribute.Key;

            default:
                return 0;
        }
    }

    public static byte GetMaxAmmoForAmmoType(byte type)
    {
        return type switch
        {
            ReCvItemIds.RocketLauncher => 5,
            ReCvItemIds.AssaultRifle => 255,
            ReCvItemIds.SniperRifle => 7,
            ReCvItemIds.Shotgun => 7,
            ReCvItemIds.HandgunGlock17 => 18,
            ReCvItemIds.GrenadeLauncher => 6,
            ReCvItemIds.BowGun => 30,
            ReCvItemIds.CombatKnife => 0,
            ReCvItemIds.Handgun => 15,
            ReCvItemIds.CustomHandgun => 20,
            ReCvItemIds.LinearLauncher => 5,
            ReCvItemIds.HandgunBullets => 30,
            ReCvItemIds.MagnumBullets => 12,
            ReCvItemIds.ShotgunShells => 14,
            ReCvItemIds.GrenadeRounds => 12,
            ReCvItemIds.AcidRounds => 12,
            ReCvItemIds.FlameRounds => 12,
            ReCvItemIds.BowGunArrows => 60,
            ReCvItemIds.M93RPart => 18,
            ReCvItemIds.MagnumBulletsInsideCase => 12,
            ReCvItemIds.InkRibbon => 6,
            ReCvItemIds.Magnum => 6,
            ReCvItemIds.GoldLugers => 15,
            ReCvItemIds.SubMachineGun => 255,
            ReCvItemIds.BOWGasRounds => 6,
            ReCvItemIds.MGunBullets => 255,
            ReCvItemIds.RifleBullets => 14,
            ReCvItemIds.ARifleBullets => 150,
            ReCvItemIds.EnhancedHandgun => 18,
            ReCvItemIds.CalicoBullets => 200,
            _ => 0,
        };
    }

}

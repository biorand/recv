using IntelOrca.Biohazard;

namespace IntelOrca.Biohazard.BioRand.RECV.Modifiers;

[Order(ModifierOrders.ItemQuantity)]
public sealed class ItemQuantityModifier : ICvModifier
{
    private const uint ItemQuantityTable = 0x35BCC0;

    public void Apply(ReCvRandomizerContext context, RandomizerLogger logger)
    {
        if (!context.Config.GetValueOrDefault<bool>("items/randomize-quantity", true))
            return;

        logger.LogLine("Randomizing item pickup quantities...");

        var multiplier = context.Config.GetValueOrDefault<int>("items/quantity-multiplier", 4);
        var rng = context.GetRng("item-quantity");

        for (var i = 0; i < 256; i++)
        {
            var itemType = (byte)i;
            var attributes = ReCvItemHelper.GetItemAttributes(itemType);

            if ((attributes & ItemAttribute.InkRibbon) == 0 &&
                (attributes & ItemAttribute.Ammo) == 0 &&
                itemType != ReCvItemIds.RocketLauncher)
            {
                continue;
            }

            var quantity = GetQuantity(itemType, multiplier, rng);

            var offset = ItemQuantityTable + (uint)(i * 16);
            for (var j = 0; j < 4; j++)
                context.Elf.WriteUInt32(offset + (uint)(j * 4), (uint)quantity);
        }

        logger.LogLine("Item pickup quantities randomized");
    }

    private static int GetQuantity(byte itemType, int multiplier, Rng rng)
    {
        if (itemType == ReCvItemIds.InkRibbon)
            return rng.Next(1, 3);

        var maxForType = ReCvItemHelper.GetMaxAmmoForAmmoType(itemType);
        if (maxForType <= 0)
            return 0;

        var scale = multiplier / 8.0;
        var max = (int)Math.Round(scale * maxForType);
        var min = Math.Max(1, max / 2);
        return rng.Next(min, max + 1);
    }
}

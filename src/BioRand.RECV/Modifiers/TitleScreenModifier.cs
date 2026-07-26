using System.Reflection;
using IntelOrca.Biohazard;
using QRCoder;

namespace IntelOrca.Biohazard.BioRand.RECV.Modifiers;

[Order(ModifierOrders.TitleScreen)]
public sealed class TitleScreenModifier : ICvModifier
{
    private const int AdvAfsTitleIndex = 2;
    private const int Tim2Offset = 0x460;
    private const int Tim2Size = 0x80960 - 0x460;

    private const string BgResourceName = "IntelOrca.Biohazard.BioRand.RECV.data.bg.bmp";

    // Bitmap font: 5x7 pixel characters, stored as 7 bytes each.
    // Bits 4-0 represent the 5 pixel columns (bit 4 = leftmost column).
    private const int FontCharWidth = 5;
    private const int FontCharHeight = 7;
    private const int FontScale = 3;
    private const int CanvasWidth = 1024;
    private const int CanvasHeight = 512;

    private static readonly byte[] FontEmpty = [0, 0, 0, 0, 0, 0, 0];

    private static readonly byte[] Font0 = [0b01110, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b01110];
    private static readonly byte[] Font1 = [0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110];
    private static readonly byte[] Font2 = [0b01110, 0b10001, 0b00001, 0b00010, 0b00100, 0b01000, 0b11111];
    private static readonly byte[] Font3 = [0b01110, 0b10001, 0b00001, 0b00110, 0b00001, 0b10001, 0b01110];
    private static readonly byte[] Font4 = [0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010];
    private static readonly byte[] Font5 = [0b11111, 0b10000, 0b11110, 0b00001, 0b00001, 0b10001, 0b01110];
    private static readonly byte[] Font6 = [0b01110, 0b10000, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110];
    private static readonly byte[] Font7 = [0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b01000, 0b01000];
    private static readonly byte[] Font8 = [0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110];
    private static readonly byte[] Font9 = [0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b00001, 0b01110];

    private static readonly byte[] FontA = [0b00100, 0b01010, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001];
    private static readonly byte[] FontB = [0b11110, 0b10001, 0b10001, 0b11110, 0b10001, 0b10001, 0b11110];
    private static readonly byte[] FontC = [0b01110, 0b10001, 0b10000, 0b10000, 0b10000, 0b10001, 0b01110];
    private static readonly byte[] FontD = [0b11110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b11110];
    private static readonly byte[] FontE = [0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b11111];
    private static readonly byte[] FontF = [0b11111, 0b10000, 0b10000, 0b11110, 0b10000, 0b10000, 0b10000];
    private static readonly byte[] FontG = [0b01110, 0b10001, 0b10000, 0b10111, 0b10001, 0b10001, 0b01110];
    private static readonly byte[] FontH = [0b10001, 0b10001, 0b10001, 0b11111, 0b10001, 0b10001, 0b10001];
    private static readonly byte[] FontI = [0b01110, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110];
    private static readonly byte[] FontN = [0b10001, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b10001];
    private static readonly byte[] FontO = [0b01110, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110];
    private static readonly byte[] FontR = [0b11110, 0b10001, 0b10001, 0b11110, 0b10100, 0b10010, 0b10001];
    private static readonly byte[] FontS = [0b01110, 0b10001, 0b10000, 0b01110, 0b00001, 0b10001, 0b01110];
    private static readonly byte[] FontT = [0b11111, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100, 0b00100];
    private static readonly byte[] FontU = [0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01110];
    private static readonly byte[] FontV = [0b10001, 0b10001, 0b10001, 0b10001, 0b10001, 0b01010, 0b00100];

    private static readonly byte[] FontLParen = [0b00010, 0b00100, 0b01000, 0b01000, 0b01000, 0b00100, 0b00010];
    private static readonly byte[] FontRParen = [0b01000, 0b00100, 0b00010, 0b00010, 0b00010, 0b00100, 0b01000];
    private static readonly byte[] FontHyphen = [0b00000, 0b00000, 0b00000, 0b11111, 0b00000, 0b00000, 0b00000];
    private static readonly byte[] FontDot = [0b00000, 0b00000, 0b00000, 0b00000, 0b00000, 0b01100, 0b01100];
    private static readonly byte[] FontColon = [0b00000, 0b01100, 0b01100, 0b00000, 0b01100, 0b01100, 0b00000];

    public void Apply(ReCvRandomizerContext context, RandomizerLogger logger)
    {
        var afs = context.AdvAfs
            ?? throw new InvalidOperationException("ADV.AFS not available");

        var versionInfo = GetVersionInfo();
        var seedText = context.Seed.ToString();

        logger.LogLine($"Overlaying: {versionInfo}");

        // Load bg.bmp from embedded resources as the replacement background
        // and composite version text + QR code onto it
        var argb = LoadBackgroundImage();

        DrawString(argb, versionInfo, 16, 16);
        DrawQrCode(argb, seedText, 640 - 16);

        WriteToTim2(argb, afs, context, logger);
    }

    private static int[] LoadBackgroundImage()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(BgResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{BgResourceName}' not found");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bmp = new Bmp(ms.ToArray());

        if (bmp.Width != CanvasWidth || bmp.Height != CanvasHeight)
            throw new InvalidDataException(
                $"Background image must be {CanvasWidth}x{CanvasHeight}, " +
                $"but got {bmp.Width}x{bmp.Height}");

        return bmp.GetArgb();
    }

    private static void WriteToTim2(int[] argb, AfsFile afs, ReCvRandomizerContext context, RandomizerLogger logger)
    {
        var entryData = afs.GetFileData(AdvAfsTitleIndex).ToArray();

        if (entryData.Length < Tim2Offset + Tim2Size)
            throw new InvalidDataException(
                $"Title screen AFS entry too small: {entryData.Length} bytes, " +
                $"expected at least {Tim2Offset + Tim2Size}");

        var tim2Data = new Memory<byte>(entryData, Tim2Offset, Tim2Size);
        var tim2 = new Tim2(tim2Data);

        if (tim2.PictureCount == 0)
            throw new InvalidDataException("No pictures in title screen TIM2");

        var tim2Builder = tim2.ToBuilder();
        var picBuilder = tim2Builder.Pictures[0].ToBuilder();
        picBuilder.Import(Array.ConvertAll(argb, x => (uint)x));
        tim2Builder.Pictures[0] = picBuilder.ToPicture();

        var newTim2 = tim2Builder.ToTim2();
        newTim2.Data.CopyTo(new Memory<byte>(entryData, Tim2Offset, Tim2Size));

        var afsBuilder = afs.ToBuilder();
        afsBuilder.Replace(AdvAfsTitleIndex, entryData);
        context.AdvAfs = afsBuilder.ToAfsFile();

        logger.LogLine("Title screen randomized");
    }

    private static string GetVersionInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version;
        var verStr = assemblyVersion is not null
            ? $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}"
            : "";
        var gitHash = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "";

        if (!string.IsNullOrEmpty(verStr) && !string.IsNullOrEmpty(gitHash))
            return $"BIORAND {verStr} ({gitHash})";
        if (!string.IsNullOrEmpty(gitHash))
            return $"BIORAND ({gitHash})";
        return "BIORAND RECV";
    }

    private static byte[] GetFontData(char c)
    {
        return c switch
        {
            ' ' => FontEmpty,
            '0' => Font0, '1' => Font1, '2' => Font2, '3' => Font3, '4' => Font4,
            '5' => Font5, '6' => Font6, '7' => Font7, '8' => Font8, '9' => Font9,
            'A' => FontA, 'B' => FontB, 'C' => FontC, 'D' => FontD, 'E' => FontE,
            'F' => FontF, 'G' => FontG, 'H' => FontH, 'I' => FontI,
            'N' => FontN, 'O' => FontO, 'R' => FontR, 'S' => FontS,
            'T' => FontT, 'U' => FontU, 'V' => FontV,
            '(' => FontLParen, ')' => FontRParen, '-' => FontHyphen,
            '.' => FontDot, ':' => FontColon,
            _ => FontEmpty,
        };
    }

    private static void DrawString(int[] argb, string text, int x, int y)
    {
        var charW = (FontCharWidth + 1) * FontScale;

        for (var ci = 0; ci < text.Length; ci++)
        {
            var ch = text[ci];
            if (ch >= 'a' && ch <= 'z')
                ch = (char)(ch - 32);

            var charData = GetFontData(ch);
            var cx = x + ci * charW;
            var cy = y;

            for (var row = 0; row < FontCharHeight; row++)
            {
                var rowData = charData[row];
                for (var col = 0; col < FontCharWidth; col++)
                {
                    if ((rowData & (1 << (4 - col))) == 0)
                        continue;

                    for (var sy = 0; sy < FontScale; sy++)
                    {
                        var py = cy + row * FontScale + sy;
                        if ((uint)py >= CanvasHeight)
                            continue;

                        for (var sx = 0; sx < FontScale; sx++)
                        {
                            var px = cx + col * FontScale + sx;
                            if ((uint)px >= CanvasWidth)
                                continue;

                            argb[py * CanvasWidth + px] = -1;
                        }
                    }
                }
            }
        }
    }

    private static void DrawQrCode(int[] argb, string text, int rightEdge)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
        var modules = qrData.ModuleMatrix;
        var qrSize = modules.Count;

        var maxQrWidth = Math.Min(200, CanvasWidth - 32);
        var moduleScale = Math.Max(1, maxQrWidth / qrSize);
        var qrPixelSize = qrSize * moduleScale;

        var startX = rightEdge - qrPixelSize;
        var startY = 16;

        if (startX < 0) startX = 0;

        for (var my = 0; my < qrSize; my++)
        {
            var row = modules[my];
            for (var mx = 0; mx < qrSize; mx++)
            {
                if (!row[mx])
                    continue;

                for (var sy = 0; sy < moduleScale; sy++)
                {
                    var py = startY + my * moduleScale + sy;
                    if ((uint)py >= CanvasHeight)
                        continue;

                    for (var sx = 0; sx < moduleScale; sx++)
                    {
                        var px = startX + mx * moduleScale + sx;
                        if ((uint)px >= CanvasWidth)
                            continue;

                        argb[py * CanvasWidth + px] = -1;
                    }
                }
            }
        }
    }
}

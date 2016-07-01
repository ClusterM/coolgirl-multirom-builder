using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace TilesConverter
{
    class Program
    {
        static Dictionary<uint, Color> NesPalette = new Dictionary<uint, Color>();

        static int Main(string[] args)
        {
            /*
            var mask = Image.FromFile(@"D:\Projects\NES\BroWord\keyboard_mask.png");
            var chromaKey = Color.FromArgb(0xFF, 0, 0xFF, 0);
            var maskResultX1 = new List<int>();
            var maskResultY1 = new List<int>();
            var maskResultX2 = new List<int>();
            var maskResultY2 = new List<int>();
            for (int y = 0; y < mask.Height; y++)
                for (int x = 0; x < mask.Width; x++)
                {
                    var c = (mask as Bitmap).GetPixel(x, y);
                    if (x > 0 && y > 0 && c == chromaKey && (mask as Bitmap).GetPixel(x - 1, y) != chromaKey && (mask as Bitmap).GetPixel(x, y - 1) != chromaKey)
                    {
                        for (int y2 = y + 1; y2 < mask.Height; y2++)
                            for (int x2 = x + 1; x2 < mask.Width; x2++)
                            {
                                if (x2 < mask.Width - 1 && y2 < mask.Height - 1 && (mask as Bitmap).GetPixel(x2, y2) == chromaKey && (mask as Bitmap).GetPixel(x2 + 1, y2) != chromaKey && (mask as Bitmap).GetPixel(x2, y2 + 1) != chromaKey)
                                {
                                    Console.WriteLine("{0} {1} {2} {3}", x, y, x2, y2);
                                    maskResultX1.Add(x);
                                    maskResultY1.Add(y);
                                    maskResultX2.Add(x2);
                                    maskResultY2.Add(y2);
                                    x2 = y2 = 256;
                                }
                            }

                    }
                }
            var maskResult = new StringBuilder();
            maskResult.Append("keyboard_coords_x1:");
            for (int i = 0; i < maskResultX1.Count; i++)
            {
                if ((i == 0) || (i % 16 == 0))
                    maskResult.Append("\r\n  .db ");
                if (i % 16 != 0) maskResult.Append(", ");
                maskResult.AppendFormat("{0}", maskResultX1[i]);
            }
            maskResult.Append("\r\n\r\nkeyboard_coords_y1:");
            for (int i = 0; i < maskResultY1.Count; i++)
            {
                if ((i == 0) || (i % 16 == 0))
                    maskResult.Append("\r\n  .db ");
                if (i % 16 != 0) maskResult.Append(", ");
                maskResult.AppendFormat("{0}", maskResultY1[i]);
            }
            File.WriteAllText(@"D:\Projects\NES\BroWord\keyboard_mask.asm", maskResult.ToString());
            maskResult.Append("\r\n\r\nkeyboard_coords_x2:");
            for (int i = 0; i < maskResultX2.Count; i++)
            {
                if ((i == 0) || (i % 16 == 0))
                    maskResult.Append("\r\n  .db ");
                if (i % 16 != 0) maskResult.Append(", ");
                maskResult.AppendFormat("{0}", maskResultX2[i]);
            }
            maskResult.Append("\r\n\r\nkeyboard_coords_y2:");
            for (int i = 0; i < maskResultY2.Count; i++)
            {
                if ((i == 0) || (i % 16 == 0))
                    maskResult.Append("\r\n  .db ");
                if (i % 16 != 0) maskResult.Append(", ");
                maskResult.AppendFormat("{0}", maskResultY2[i]);
            }
            maskResult.AppendFormat("\r\n\r\nkeyboard_key_count:\r\n  .db {0}", maskResultX1.Count);
            File.WriteAllText(@"D:\Projects\NES\BroWord\keyboard_mask.asm", maskResult.ToString());
            return 0;
            */

            if (args.Length < 4)
            {
                Console.WriteLine("NES image converter");
                Console.WriteLine("(c) Cluster, 2015");
                Console.WriteLine("http://clusterrr.com");
                Console.WriteLine("clusterrr@clusterrr.com");
                Console.WriteLine("Usage: tilesconverter.exe <input.png> <pattern.bin> <nametable.bin> <palette.bin> [palette_offset]");
                return 2;
            }

            try
            {
                NesPalette[0x00] = Color.FromArgb(0x7C, 0x7C, 0x7C);
                NesPalette[0x01] = Color.FromArgb(0x00, 0x00, 0xFC);
                NesPalette[0x02] = Color.FromArgb(0x00, 0x00, 0xBC);
                NesPalette[0x03] = Color.FromArgb(0x44, 0x28, 0xBC);
                NesPalette[0x04] = Color.FromArgb(0x94, 0x00, 0x84);
                NesPalette[0x05] = Color.FromArgb(0xA8, 0x00, 0x20);
                NesPalette[0x06] = Color.FromArgb(0xA8, 0x10, 0x00);
                NesPalette[0x07] = Color.FromArgb(0x88, 0x14, 0x00);
                NesPalette[0x08] = Color.FromArgb(0x50, 0x30, 0x00);
                NesPalette[0x09] = Color.FromArgb(0x00, 0x78, 0x00);
                NesPalette[0x0A] = Color.FromArgb(0x00, 0x68, 0x00);
                NesPalette[0x0B] = Color.FromArgb(0x00, 0x58, 0x00);
                NesPalette[0x0C] = Color.FromArgb(0x00, 0x40, 0x58);
                NesPalette[0x0F] = Color.FromArgb(0x00, 0x00, 0x00);

                NesPalette[0x10] = Color.FromArgb(0xBC, 0xBC, 0xBC);
                NesPalette[0x11] = Color.FromArgb(0x00, 0x78, 0xF8);
                NesPalette[0x12] = Color.FromArgb(0x00, 0x58, 0xF8);
                NesPalette[0x13] = Color.FromArgb(0x68, 0x44, 0xFC);
                NesPalette[0x14] = Color.FromArgb(0xD8, 0x00, 0xCC);
                NesPalette[0x15] = Color.FromArgb(0xE4, 0x00, 0x58);
                NesPalette[0x16] = Color.FromArgb(0xF8, 0x38, 0x00);
                NesPalette[0x17] = Color.FromArgb(0xE4, 0x5C, 0x10);
                NesPalette[0x18] = Color.FromArgb(0xAC, 0x7C, 0x00);
                NesPalette[0x19] = Color.FromArgb(0x00, 0xB8, 0x00);
                NesPalette[0x1A] = Color.FromArgb(0x00, 0xA8, 0x00);
                NesPalette[0x1B] = Color.FromArgb(0x00, 0xA8, 0x44);
                NesPalette[0x1C] = Color.FromArgb(0x00, 0x88, 0x88);
                //NesPalette[0x1D] = Color.FromArgb(0x08, 0x08, 0x08);

                //NesPalette[0x20] = Color.FromArgb(0xFC, 0xFC, 0xFC);
                NesPalette[0x21] = Color.FromArgb(0x3C, 0xBC, 0xFC);
                NesPalette[0x22] = Color.FromArgb(0x68, 0x88, 0xFC);
                NesPalette[0x23] = Color.FromArgb(0x98, 0x78, 0xF8);
                NesPalette[0x24] = Color.FromArgb(0xF8, 0x78, 0xF8);
                NesPalette[0x25] = Color.FromArgb(0xF8, 0x58, 0x98);
                NesPalette[0x26] = Color.FromArgb(0xF8, 0x78, 0x58);
                NesPalette[0x27] = Color.FromArgb(0xFC, 0xA0, 0x44);
                NesPalette[0x28] = Color.FromArgb(0xF8, 0xB8, 0x00);
                NesPalette[0x29] = Color.FromArgb(0xB8, 0xF8, 0x18);
                NesPalette[0x2A] = Color.FromArgb(0x58, 0xD8, 0x54);
                NesPalette[0x2B] = Color.FromArgb(0x58, 0xF8, 0x98);
                NesPalette[0x2C] = Color.FromArgb(0x00, 0xE8, 0xD8);
                NesPalette[0x2D] = Color.FromArgb(0x7C, 0x7C, 0x7C);

                NesPalette[0x30] = Color.FromArgb(0xFC, 0xFC, 0xFC);
                NesPalette[0x31] = Color.FromArgb(0xA4, 0xE4, 0xFC);
                NesPalette[0x32] = Color.FromArgb(0xB8, 0xB8, 0xF8);
                NesPalette[0x33] = Color.FromArgb(0xD8, 0xB8, 0xF8);
                NesPalette[0x34] = Color.FromArgb(0xF8, 0xB8, 0xF8);
                NesPalette[0x35] = Color.FromArgb(0xF8, 0xA4, 0xC0);
                NesPalette[0x36] = Color.FromArgb(0xF0, 0xD0, 0xB0);
                NesPalette[0x37] = Color.FromArgb(0xFC, 0xE0, 0xA8);
                NesPalette[0x38] = Color.FromArgb(0xF8, 0xD8, 0x78);
                NesPalette[0x39] = Color.FromArgb(0xD8, 0xF8, 0x78);
                NesPalette[0x3A] = Color.FromArgb(0xB8, 0xF8, 0xB8);
                NesPalette[0x3B] = Color.FromArgb(0xB8, 0xF8, 0xD8);
                NesPalette[0x3C] = Color.FromArgb(0x00, 0xFC, 0xFC);
                NesPalette[0x3D] = Color.FromArgb(0xD8, 0xD8, 0xD8);

                /*
                var image = Image.FromFile(@"D:\Temp\NES_palette.png");
                for (int y = 0; y < 4; y++)
                for (int x = 0; x < 14; x++)
                {
                    var color = ((Bitmap)image).GetPixel(x * 16 + 8, y * 16 + 8);
                    var index = x + y * 16;
                    Console.WriteLine(string.Format("NesPalette[0x{0:X2}] = Color.FromArgb(0x{1:X2}, 0x{2:X2}, 0x{3:X2});", index, color.R, color.G, color.B));
                }
                Console.ReadLine();
                 */

                //var image = Image.FromFile(@"D:\hello_world.png");
                Console.WriteLine("Converting {0}...", args[0]);
                var image = Image.FromFile(args[0]);
                image = new Bitmap(image);

                PaletteGroup[] palettes;
                if (!args[3].StartsWith("!"))
                {

                    // Приводим все цвета на рисунке к тем, что допустимы на NES
                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            var color = ((Bitmap)image).GetPixel(x, y);
                            var similarColor = NesPalette[findSimilarColor(NesPalette, color)];
                            ((Bitmap)image).SetPixel(x, y, similarColor);
                        }
                    }

                    Color bgColor = ((Bitmap)image).GetPixel(0, 0);
                    Dictionary<PaletteGroup, int> paletteGroupCounter = new Dictionary<PaletteGroup, int>(new PaletteGroupComparer());

                    // Перебираем все тайлы 16*16
                    for (int tileY = 0; tileY < image.Height / 16; tileY++)
                    {
                        for (int tileX = 0; tileX < image.Width / 16; tileX++)
                        {
                            // Создаём палитру
                            var paletteGroup = createPalette(image, tileX, tileY, bgColor);

                            // Считаем количество таких палитр
                            if (!paletteGroupCounter.ContainsKey(paletteGroup))
                                paletteGroupCounter[paletteGroup] = 0;
                            paletteGroupCounter[paletteGroup]++;
                        }
                    }
                    //image.Save(@"test0.png", ImageFormat.Png);

                    // Группируем палитры. Некоторые из них могут содержать все цвета других
                    var paletteGroupGrouped = new Dictionary<PaletteGroup, int>(paletteGroupCounter, new PaletteGroupComparer());
                    foreach (var palette2 in paletteGroupCounter.Keys)
                        foreach (var palette1 in paletteGroupCounter.Keys)
                        {
                            if (paletteGroupGrouped.ContainsKey(palette1) && !palette1.Equals(palette2) && palette2.Contains(palette1))
                            {
                                paletteGroupGrouped[palette2] += paletteGroupGrouped[palette1];
                                paletteGroupGrouped.Remove(palette1);
                            }
                        }

                    // Ну и выбираем наконец-то четыре самые часто используемые палитры

                    palettes = (from palette in paletteGroupGrouped.Keys
                                orderby paletteGroupGrouped[palette] descending
                                select palette).Take(4).ToArray();

                }
                // Загружаем уже готовую палитру
                else
                {
                    var paletteIn = File.ReadAllBytes(args[3].Substring(1));
                    palettes = new PaletteGroup[]
                    {
                        new PaletteGroup(new Color[] { NesPalette[paletteIn[0]], NesPalette[paletteIn[1]], NesPalette[paletteIn[2]], NesPalette[paletteIn[3]] }),
                        new PaletteGroup(new Color[] { NesPalette[paletteIn[4]], NesPalette[paletteIn[5]], NesPalette[paletteIn[6]], NesPalette[paletteIn[7]] }),
                        new PaletteGroup(new Color[] { NesPalette[paletteIn[8]], NesPalette[paletteIn[9]], NesPalette[paletteIn[10]], NesPalette[paletteIn[11]] }),
                        new PaletteGroup(new Color[] { NesPalette[paletteIn[12]], NesPalette[paletteIn[13]], NesPalette[paletteIn[14]], NesPalette[paletteIn[15]] })
                    };
                }

                // Иногда нужно сдвинуть палитру
                int paletteOffset = 0;
                if (args.Length >= 5)
                {
                    paletteOffset = int.Parse(args[4]);
                    var new_palettes = new PaletteGroup[4];
                    Array.Copy(palettes, new_palettes, palettes.Length);
                    palettes = new_palettes;
                    for (int i = 0; i < paletteOffset; i++)
                        palettes = new PaletteGroup[] { palettes[3], palettes[0], palettes[1], palettes[2] };
                }

                // Сохраним палитры в виде картинки, для отладки
                var paletteImage = new Bitmap(4, 4);
                for (int p = 0; p < palettes.Length; p++)
                {
                    int c = 0;
                    if (palettes[p] != null)
                        foreach (var color in palettes[p].Colors)
                        {
                            if (color != null)
                                paletteImage.SetPixel(c, p, color);
                            c++;
                        }
                }


                // Палитра
                var paletteRaw = new byte[16];
                paletteRaw[0] = paletteRaw[4] = paletteRaw[8] = paletteRaw[12] =
                    //paletteRaw[16] = paletteRaw[20] = paletteRaw[24] = paletteRaw[28] =
                    (byte)findSimilarColor(NesPalette, palettes[paletteOffset].Colors[0]);
                for (int p = 0; p < palettes.Length; p++)
                {
                    if (palettes[p] != null)
                        for (int c = 1; c < palettes[p].Colors.Count; c++)
                        {
                            paletteRaw[p * 4 + c] = (byte)findSimilarColor(NesPalette, palettes[p].Colors[c]);
                            // В спрайты скопируем тоже самое
                            //paletteRaw[p * 4 + c + 16] = (byte)findSimilarColor(NesPalette, palettes[p].Colors[c]);
                        }
                }
                File.WriteAllBytes(args[3]/*@"palette.dat"*/, paletteRaw);

                //paletteImage.Save("paltest.png", ImageFormat.Png);
                if (args[1] == "NUL" && args[2] == "NUL")
                {
                    Console.WriteLine("Palette created.");
                    return 0;
                }

                // Перебираем все тайлы 16*16
                var palleteIndexes = new byte[image.Width / 16, image.Height / 16];
                for (int tileY = 0; tileY < image.Height / 16; tileY++)
                {
                    for (int tileX = 0; tileX < image.Width / 16; tileX++)
                    {
                        long minDifference = long.MaxValue;
                        PaletteGroup bestPalette = null;
                        byte bestPaletteIndex = 0;
                        // Пробуем каждую палитру
                        for (byte paletteIndex = 0; paletteIndex < palettes.Length; paletteIndex++)
                        {
                            if (palettes[paletteIndex] == null) continue;
                            long difference = 0;
                            for (int y = 0; y < 16; y++)      // И применяем к каждому пикселю
                                for (int x = 0; x < 16; x++)
                                {
                                    var color = ((Bitmap)image).GetPixel(tileX * 16 + x, tileY * 16 + y);
                                    var similarColor = findSimilarColor(palettes[paletteIndex].Colors, color);
                                    // Вычисляем разницу в цвете с макисмально похожим цветом
                                    var delta = getColorDifference(color, similarColor);
                                    // И суммируем
                                    difference += delta;
                                }
                            // Ищем палитру, которая встанет с минимумом изменений
                            if (difference < minDifference)
                            {
                                minDifference = difference;
                                bestPalette = palettes[paletteIndex];
                                bestPaletteIndex = paletteIndex;
                            }
                        }
                        // Запоминаем номер палитры
                        palleteIndexes[tileX, tileY] = bestPaletteIndex;

                        // В итоге применяем эту палитру к тайлу
                        for (int y = 0; y < 16; y++)
                            for (int x = 0; x < 16; x++)
                            {
                                var color = ((Bitmap)image).GetPixel(tileX * 16 + x, tileY * 16 + y);
                                var similarColor = findSimilarColor(bestPalette.Colors, color);
                                ((Bitmap)image).SetPixel(tileX * 16 + x, tileY * 16 + y, similarColor);
                            }
                    }
                }

                // Осталось составить базу тайлов, теперь уже размером 8 на 8
                var patternTable = new List<PatternTableEntry>();
                var nameTable = new byte[32 * 30];
                for (int tileY = 0; tileY < image.Height / 8; tileY++)

                    for (int tileX = 0; tileX < image.Width / 8; tileX++)
                    {
                        var tileData = new byte[8, 8];
                        for (int y = 0; y < 8; y++)      // И применяем к каждому пикселю
                            for (int x = 0; x < 8; x++)
                            {
                                var color = ((Bitmap)image).GetPixel(tileX * 8 + x, tileY * 8 + y);
                                var palette = palettes[palleteIndexes[tileX / 2, tileY / 2]];
                                var colorIndex = (byte)palette.Colors.FindIndex(c => c == color);
                                tileData[x, y] = colorIndex;
                            }
                        // Создаём тайл на основе массива с номерами цветов (палитра при этом не важна)
                        var tile = new PatternTableEntry(tileData);
                        // Добавляем его в список, если его там ещё нет
                        if (!patternTable.Contains(tile))
                            patternTable.Add(tile);
                        // Запоминаем номер тайла
                        nameTable[tileX + tileY * 32] = (byte)patternTable.FindIndex(t => t.Equals(tile));
                    }

                //image.Save(@"test1.png", ImageFormat.Png);
                // Всё, осталось полученную информацию как-то сохранить в формате пригодном для NES
                if (patternTable.Count > 256) throw new Exception("Too many tiles: " + patternTable.Count);
                Console.WriteLine("Tiles count: " + patternTable.Count);

                // Сами тайлы
                var patternTableRaw = new byte[0x1000];
                for (int p = 0; p < patternTable.Count; p++)
                {
                    var pixels = patternTable[p].pixels;

                    for (int y = 0; y < 8; y++)
                    {
                        patternTableRaw[p * 16 + y] = 0;
                        patternTableRaw[p * 16 + y + 8] = 0;
                        for (int x = 0; x < 8; x++)
                        {
                            if ((pixels[x, y] & 1) != 0)
                                patternTableRaw[p * 16 + y] |= (byte)(1 << (7 - x));
                            if ((pixels[x, y] & 2) != 0)
                                patternTableRaw[p * 16 + y + 8] |= (byte)(1 << (7 - x));
                        }
                    }
                }

                // Ну и nametable
                var nametableRaw = new byte[1024];
                Array.Copy(nameTable, nametableRaw, 30 * 32);
                // В которой ещё attribute table
                for (int tileY = 0; tileY <= image.Height / 32; tileY++)
                    for (int tileX = 0; tileX < image.Width / 32; tileX++)
                    {
                        var topLeft = palleteIndexes[tileX * 2, tileY * 2];
                        var topRight = palleteIndexes[tileX * 2 + 1, tileY * 2];
                        var bottomLeft = tileY < 7 ? palleteIndexes[tileX * 2, tileY * 2 + 1] : 0;
                        var bottomRight = tileY < 7 ? palleteIndexes[tileX * 2 + 1, tileY * 2 + 1] : 0;

                        nametableRaw[0x3C0 + tileY * 8 + tileX] = (byte)
                            (topLeft // top left
                            | (topRight << 2) // top right
                            | (bottomLeft << 4) // bottom left
                            | (bottomRight << 6)); // bottom right
                    }

                //paletteImage.Save(@"palette.png", ImageFormat.Png);
                File.WriteAllBytes(args[1]/*@"pattern0.dat"*/, patternTableRaw);
                File.WriteAllBytes(args[2]/*@"nametable0.dat"*/, nametableRaw);
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + ex.StackTrace);
                return 1;
            }
            return 0;
        }

        static uint findSimilarColor(Dictionary<uint, Color> colors, Color color)
        {
            uint result = 0;
            int minDelta = int.MaxValue;
            foreach (var index in colors.Keys)
            {
                var color1 = color;
                var color2 = colors[index];
                var deltaR = Math.Abs((int)color1.R - (int)color2.R);
                var deltaG = Math.Abs((int)color1.G - (int)color2.G);
                var deltaB = Math.Abs((int)color1.B - (int)color2.B);
                var delta = deltaR + deltaG + deltaB;
                if (delta < minDelta)
                {
                    minDelta = delta;
                    result = index;
                }
            }
            return result;
        }

        static Color findSimilarColor(IEnumerable<Color> colors, Color color)
        {
            Color result = Color.Black;
            int minDelta = int.MaxValue;
            foreach (var c in colors)
            {
                var color1 = color;
                var color2 = c;
                var delta = getColorDifference(color1, color2);
                if (delta < minDelta)
                {
                    minDelta = delta;
                    result = c;
                }
            }
            return result;
        }

        static int getColorDifference(Color color1, Color color2)
        {
            var deltaR = Math.Abs((int)color1.R - (int)color2.R);
            var deltaG = Math.Abs((int)color1.G - (int)color2.G);
            var deltaB = Math.Abs((int)color1.B - (int)color2.B);
            var delta = deltaR + deltaG + deltaB;
            return delta;
        }

        static PaletteGroup createPalette(Image image, int tileX, int tileY, Color bgColor)
        {
            Dictionary<Color, int> colorCounter = new Dictionary<Color, int>();
            colorCounter[bgColor] = 0;
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    var color = ((Bitmap)image).GetPixel(tileX * 16 + x, tileY * 16 + y);
                    if (!colorCounter.ContainsKey(color)) colorCounter[color] = 0;
                    colorCounter[color]++;
                }

            // Пока их больше четырёх, надо удалить самый неиспользуемый
            while (colorCounter.Count > 4)
            {
                // Вычисляем самый малоиспользуемый цвет
                int min = int.MaxValue;
                Color minColor = Color.Black;
                List<Color> newColors = new List<Color>();
                foreach (var color in colorCounter.Keys)
                {
                    newColors.Add(color);
                    if (colorCounter[color] < min && color != bgColor)
                    {
                        minColor = color;
                        min = colorCounter[color];
                    }
                }

                // Удаляем его из списка цветов
                newColors.Remove(minColor);
                // Находим максимально похожий
                var similar = findSimilarColor(newColors, minColor);
                // Заменяем старый цвет на новый в каждом пикселе
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 16; x++)
                    {
                        var color = ((Bitmap)image).GetPixel(tileX * 16 + x, tileY * 16 + y);
                        if (color == minColor)
                            ((Bitmap)image).SetPixel(tileX * 16 + x, tileY * 16 + y, similar);
                    }

                colorCounter.Remove(minColor);

            }

            // Создаём палитру
            var paletteGroup = new PaletteGroup(colorCounter.Keys);
            return paletteGroup;
        }

        class PaletteGroup : IEquatable<PaletteGroup>
        {
            public List<Color> Colors;
            public PaletteGroup(IEnumerable<Color> colors)
            {
                Colors = new List<Color>();
                var bgColor = colors.First<Color>();
                var colorsList = new List<Color>();
                colorsList.AddRange(colors);
                colorsList.Sort((x, y) => (x.ToArgb() == y.ToArgb() ? 0 : (x.ToArgb() > y.ToArgb() ? 1 : -1)));
                colorsList.Remove(bgColor);
                colorsList.Insert(0, bgColor);
                foreach (var color in colorsList)
                {
                    Colors.Add(color);
                    if (Colors.Count == 4) break;
                }
            }

            public bool Equals(PaletteGroup other)
            {
                if (Colors.Count != other.Colors.Count) return false;
                for (int i = 0; i < Colors.Count; i++)
                    if (Colors[i] != other.Colors[i]) return false;
                return true;
            }

            public bool Contains(PaletteGroup other)
            {
                foreach (var color in other.Colors)
                {
                    if (!Colors.Contains(color))
                        return false;
                }
                return true;
            }

            /*
            public int GetDifference(PaletteGroup targetPalette)
            {
                int result = 0;
                foreach (var sourceColor in Colors)
                {
                    var simirar = findSimilarColor(targetPalette.Colors, sourceColor);
                    result += getColorDifference(sourceColor, simirar);
                }
                return result;
            }
             */
        }

        class PaletteGroupComparer : IEqualityComparer<PaletteGroup>
        {
            public bool Equals(PaletteGroup x, PaletteGroup y)
            {
                if (x.Colors.Count != y.Colors.Count) return false;
                for (int i = 0; i < x.Colors.Count; i++)
                    if (x.Colors[i] != y.Colors[i]) return false;
                return true;
            }

            public int GetHashCode(PaletteGroup obj)
            {
                StringBuilder r = new StringBuilder();
                foreach (var color in obj.Colors)
                {
                    r.Append(color.ToArgb());
                }
                return r.ToString().GetHashCode();
            }
        }

        class PatternTableEntry : IEquatable<PatternTableEntry>
        {
            public byte[,] pixels;

            public PatternTableEntry(byte[,] data)
            {
                pixels = data;
            }

            public bool Equals(PatternTableEntry other)
            {
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        if (pixels[x, y] != other.pixels[x, y]) return false;
                return true;
            }
        }
    }
}

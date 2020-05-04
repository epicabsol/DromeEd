using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace DromeEd.Drome
{
    public class Texture
    {
        private const string MAGIC_PCT1 = "1TCP";
        private const string MAGIC_PCT2 = "2TCP";
        public string Magic;

        public enum BitmapType : uint
        {
            RGBA,
            BGRA,
            Paletted,
            DXT3,
            Unknown = 0xFFFFFFFF
        }

        public enum MapType : uint // eMapType
        {
            Base,                             // 1   Base texture map
            Specular,                         // 2   Specular and environment maps for overlaying detail
            Gloss,                            // 4   Gloss map using alpha to control shininess
            Bump,                             // 8   Bump map
            Add,                              // 16  Add texture to a base texture
            Modulate,                         // 32  Light maps and multiplying textures together
            AlphaMorph,                       // 64  Blending two textures using an alpha channel
            Morph,                            // 128 Blending two textures using a constant alpha factor
            Subtract,                         // 256 Subtract texture from a base texture
            Reflect,                          // 512 Reflective cubic environment map
            ReflectPlane,
            Opacity,
            Unknown = 0xFFFFFFFF
        }

        public int Width;
        public int Height;
        public int BitsPerPixel;
        public int OriginalBitsPerPixel;
        public int MipCount;
        public int PaletteEntryCount;
        public int CompressionCandidate;
        public int CompressionAllowed;
        public int UsesAlpha;
        public int UsesChromaKey;
        public BitmapType Type;
        public uint Padding0;
        public uint Padding1;
        public uint Padding2;
        public uint Padding3;

        public byte[] PaletteData;
        public byte[] PixelData;

        public Texture(BinaryReader reader)
        {
            Magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            BitsPerPixel = reader.ReadInt32();
            OriginalBitsPerPixel = reader.ReadInt32();
            MipCount = reader.ReadInt32();
            PaletteEntryCount = reader.ReadInt32();
            CompressionCandidate = reader.ReadInt32();
            CompressionAllowed = reader.ReadInt32();
            UsesAlpha = reader.ReadInt32();
            UsesChromaKey = reader.ReadInt32();
            Type = (BitmapType)reader.ReadUInt32();
            Padding0 = reader.ReadUInt32();
            Padding1 = reader.ReadUInt32();
            Padding2 = reader.ReadUInt32();
            Padding3 = reader.ReadUInt32();

            if (PaletteEntryCount > 0 && Type != BitmapType.DXT3)
            {
                PaletteData = reader.ReadBytes(4 * PaletteEntryCount);
            }

            int bytes = 0;

            if ((Magic == MAGIC_PCT2 && (Type == BitmapType.RGBA || Type == BitmapType.BGRA)) || (Magic == MAGIC_PCT1 && PaletteEntryCount == 0))
            {
                bytes = (int)(Width * Height * BitsPerPixel / 8);
                PixelData = reader.ReadBytes(bytes);
            }
            else if ((Magic == MAGIC_PCT2 && Type == BitmapType.Paletted) || (Magic == MAGIC_PCT1 && PaletteEntryCount > 0))
            {
                bytes = (int)(Width * Height * BitsPerPixel / 8);
                PixelData = reader.ReadBytes(bytes);
            }
            else if (Magic == MAGIC_PCT2 && Type == BitmapType.DXT3)
            {
                bytes = ((int)Width / 4) * ((int)Height / 4) * 16;
                PixelData = reader.ReadBytes(bytes);
            }

            if ((PixelData?.Length ?? 0) != bytes)
            {
                //System.Diagnostics.Debugger.Break();
                //Console.WriteLine(Unk08.ToString("X8") + " [BAD]");
                Console.WriteLine("[ERROR]: " + ToString());
                throw new NotSupportedException("No pixel data!");
            }
        }

        /// <summary>
        /// Dumps the image pixels to a 32bpp bgra array.
        /// </summary>
        /// <returns></returns>
        public byte[] DumpPixelData()
        {
            if (Magic == MAGIC_PCT1 || (Magic == MAGIC_PCT2 && Type != BitmapType.DXT3))
            {
                MemoryStream ms = new MemoryStream(32 * (int)Width * (int)Height);

                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if ((Magic == MAGIC_PCT1 && PaletteEntryCount == 0) || Magic == MAGIC_PCT2 && Type == BitmapType.BGRA)
                            {
                                // BGRA data, direct copy each channel
                                for (int channel = 0; channel < OriginalBitsPerPixel / 8; channel++)
                                {
                                    writer.Write(PixelData[(y * Width + x) * BitsPerPixel / 8 + channel]);
                                }
                                // Fill in missing channels
                                for (int channel = OriginalBitsPerPixel / 8; channel < 4; channel++)
                                {
                                    writer.Write((byte)(channel == 3 ? 0xFF : 0x00)); // Write 255 if in alpha channel, otherwise 0
                                }
                            }
                            else if (Magic == MAGIC_PCT2 && Type == BitmapType.RGBA)
                            {
                                // RGBA data, copy each channel in the right order
                                // Dest B channel
                                if (OriginalBitsPerPixel >= 24)
                                {
                                    writer.Write(PixelData[(y * Width + x) * BitsPerPixel / 8 + 2]);
                                }
                                else
                                {
                                    writer.Write((byte)0); // Dummy data because the source didn't have a B value
                                }
                                // Dest G channel
                                if (OriginalBitsPerPixel >= 16)
                                {
                                    writer.Write(PixelData[(y * Width + x) * BitsPerPixel / 8 + 1]);
                                }
                                else
                                {
                                    writer.Write((byte)0);
                                }
                                // Dest R channel
                                writer.Write(PixelData[(y * Width + x) * BitsPerPixel / 8]);
                                // Dest A channel
                                if (OriginalBitsPerPixel >= 32)
                                {
                                    writer.Write(PixelData[(y * Width + x) * BitsPerPixel / 8 + 3]);
                                }
                                else
                                {
                                    writer.Write((byte)255); // Full alpha
                                }
                            }
                            else if ((Magic == MAGIC_PCT1 && PaletteEntryCount > 0) || (Magic == MAGIC_PCT2 && Type == BitmapType.Paletted))
                            {
                                // Read the index into the palette
                                byte entryStart = PixelData[(y * Width + x)];
                                // Copy all 4 channels of palette data
                                for (int channel = 0; channel < 4; channel++)
                                {
                                    writer.Write(PaletteData[entryStart * 4 + channel]);
                                }
                            }
                            else
                            {
                                throw new NotImplementedException("Invalid texture format!");
                            }
                        }
                    }
                    byte[] data = ms.ToArray();
                    ms.Dispose();
                    return data;
                }
            }
            else if (Magic == MAGIC_PCT2 && Type == BitmapType.DXT3)
            {
                byte[] uncompressedData = new byte[Width * Height * 4];
                for (int y = 0; y < Height / 4; y++)
                {
                    for (int x = 0; x < Width / 4; x++)
                    {
                        int blockStart = (y * ((int)Width / 4) + x) * 16;
                        ulong alphaTable = BitConverter.ToUInt64(PixelData, blockStart); // Read 8 bytes
                        Color[] color = new Color[4];
                        color[0] = ColorFrom565(BitConverter.ToUInt16(PixelData, blockStart + 8)); // Read 2 bytes
                        color[1] = ColorFrom565(BitConverter.ToUInt16(PixelData, blockStart + 10)); // Read 2 bytes
                        color[2] = LerpColor(color[0], color[1], 1 / 3.0f);
                        color[3] = LerpColor(color[0], color[1], 2 / 3.0f);
                        uint colorTable = BitConverter.ToUInt32(PixelData, blockStart + 12); // Read 4 bytes

                        for (int py = 0; py < 4; py++)
                        {
                            for (int px = 0; px < 4; px++)
                            {
                                int i = py * 4 + px;
                                uint tvalue = colorTable >> ((i * 2)) & 0x03;
                                byte alpha = (byte)((alphaTable >> ((i * 4))) & 0xF);
                                alpha = (byte)((alpha * 255) / 15); // Rescale from 0-15 to 0-255
                                Color c = Color.FromArgb(alpha, color[tvalue]);

                                int start = ((y * 4 + py) * (int)Width + x * 4 + px) * 4;
                                uncompressedData[start] = c.B;
                                uncompressedData[start + 1] = c.G;
                                uncompressedData[start + 2] = c.R;
                                uncompressedData[start + 3] = c.A;
                            }
                        }
                    }
                }
                return uncompressedData;
            }
            else
            {
                throw new BadImageFormatException();
            }
        }

        /// <summary>
        /// Dumps the image data to a 32bpp TGA file.
        /// </summary>
        /// <param name="filename"></param>
        public void DumpTGA(string filename)
        {
            using (System.IO.FileStream stream = new FileStream(filename, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Write TARGA header
                writer.Write((byte)0); // ID length
                writer.Write((byte)0); // Color map? (none)
                writer.Write((byte)2); // Encoding type (Uncompressed rgb data)
                writer.Write((ushort)0); // Index of first color map entry
                writer.Write((ushort)0); // Color map entry count
                writer.Write((byte)0); // Bits per color map entry
                writer.Write((ushort)0); // X origin
                writer.Write((ushort)0); // Y origin
                writer.Write((ushort)Width);
                writer.Write((ushort)Height);
                writer.Write((byte)32); // Bits per pixel
                writer.Write((byte)8); // Image descriptor (extra fancy stuff). 8 is alpha bits per channel

                // Write pixel data as BGRA
                writer.Write(DumpPixelData());
            }
        }

        private static Color ColorFrom565(ushort fsf)
        {
            int r = (fsf >> 11) & 0x1F;
            int g = (fsf >> 5) & 0x3F;
            int b = (fsf >> 0) & 0x1F;
            r <<= 3;
            g <<= 2;
            b <<= 3;
            r |= r >> 5;
            g |= g >> 6;
            b |= b >> 5;
            return Color.FromArgb(r, g, b);
        }

        private static Color LerpColor(Color a, Color b, float d)
        {
            return Color.FromArgb((int)(a.A + d * (b.A - a.A)), (int)(a.R + d * (b.R - a.R)), (int)(a.G + d * (b.G - a.G)), (int)(a.B + d * (b.B - a.B)));
        }


        public override string ToString()
        {
            return "Size:" + Width + "x" + Height + ",PaletteEntryCount:" + PaletteEntryCount + ",PCTFormat:" + Type.ToString() + ",PixelSize:" + BitsPerPixel.ToString() + ",BitDepth:" + OriginalBitsPerPixel.ToString() + ",MipCount:" + MipCount.ToString() + ",IsCompressed:" + CompressionCandidate.ToString() + "\nUnknowns:" + CompressionAllowed.ToString("X8") + " " + UsesAlpha.ToString("X8") + " " + UsesChromaKey.ToString("X8") + " " + Padding0.ToString("X8") + " " + Padding1.ToString("X8") + " " + Padding2.ToString("X8") + " " + Padding3.ToString("X8");
        }
    }
}

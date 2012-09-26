using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Checksums;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using ICSharpCode.SharpZipLib.Core;

namespace IPA_Web_Installer_Generator.Utilities
{
    public class Decrunch
    {
        const int MAX_CHUNKS = 40;

        static long PNG_Header = BitConverter.ToInt64(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0);
        static string PNG_DataChunk = "IDAT";
        static string PNG_EndChunk = "IEND";
        static string PNG_CgBIChunk = "CgBI";
        static string PNG_HeaderChunk = "IHDR";

        enum PNG_Filter
        {
            None,
            Sub,
            Up,
            Average,
            Paeth
        }

        class PNG_Chunk
        {
            public string Name;
            public byte[] Data;
            public uint CRC;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PNG_IHDR
        {
            public int Width;
            public int Height;
            public byte BitDepth;
            public byte ColorType;
            public byte CompressionMethod;
            public byte FilterMethod;
            public byte InterlanceMethod;

            static public PNG_IHDR FromBytes(byte[] bytes)
            {
                int size = Marshal.SizeOf(typeof(PNG_IHDR));
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                PNG_IHDR result = (PNG_IHDR)Marshal.PtrToStructure(ptr, typeof(PNG_IHDR));
                Marshal.FreeHGlobal(ptr);
                result.Width = IPAddress.HostToNetworkOrder(result.Width);
                result.Height = IPAddress.HostToNetworkOrder(result.Height);
                return result;
            }
        }

        static byte[] ReadData(MemoryStream stream, int length)
        {
            byte[] data = new byte[length];
            stream.Read(data, 0, length);
            return data;
        }

        static byte[] ReadData(MemoryStream stream, int length, bool BE)
        {
            byte[] data = ReadData(stream, length);
            if (!(BitConverter.IsLittleEndian ^ BE))
                Array.Reverse(data);
            return data;
        }

        static int ReadInt32BE(MemoryStream stream)
        {
            return BitConverter.ToInt32(ReadData(stream, 4, true), 0);
        }

        static uint ReadUInt32BE(MemoryStream stream)
        {
            return BitConverter.ToUInt32(ReadData(stream, 4, true), 0);
        }

        static long ReadInt64LE(MemoryStream stream)
        {
            return BitConverter.ToInt64(ReadData(stream, 8), 0);
        }

        static string ReadString4(MemoryStream stream)
        {
            return ASCIIEncoding.ASCII.GetString(ReadData(stream, 4));
        }

        static void WriteData(MemoryStream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        static void WriteData(MemoryStream stream, byte[] data, bool BE)
        {
            if (!(BitConverter.IsLittleEndian ^ BE))
                Array.Reverse(data);
            WriteData(stream, data);
        }

        static void WriteInt32BE(MemoryStream stream, int value)
        {
            WriteData(stream, BitConverter.GetBytes(value), true);
        }

        static void WriteUInt32BE(MemoryStream stream, uint value)
        {
            WriteData(stream, BitConverter.GetBytes(value), true);
        }

        static void WriteInt64LE(MemoryStream stream, long value)
        {
            WriteData(stream, BitConverter.GetBytes(value));
        }

        static void WriteString4(MemoryStream stream, string value)
        {
            WriteData(stream, ASCIIEncoding.ASCII.GetBytes(value));
        }

        static PNG_Chunk[] ReadChunks(MemoryStream data)
        {
            List<PNG_Chunk> chunkList = new List<PNG_Chunk>();

            if (ReadInt64LE(data) != PNG_Header)
                return chunkList.ToArray();

            int i = 0;
            do
            {
                PNG_Chunk chunk = new PNG_Chunk();

                int length = ReadInt32BE(data);
                chunk.Name = ReadString4(data);
                chunk.Data = ReadData(data, length);
                chunk.CRC = ReadUInt32BE(data);

                chunkList.Add(chunk);

                if (chunk.Name.Equals(PNG_EndChunk))
                    break;

            } while (i++ < MAX_CHUNKS);

            return chunkList.ToArray();
        }

        static void WriteChunks(MemoryStream data, PNG_Chunk[] chunks)
        {
            WriteInt64LE(data, PNG_Header);
            foreach (PNG_Chunk chunk in chunks)
            {
                WriteInt32BE(data, chunk.Data.Length);
                WriteString4(data, chunk.Name);
                WriteData(data, chunk.Data);
                WriteUInt32BE(data, chunk.CRC);
            }
        }

        static PNG_Chunk[] ProcessChunks(PNG_Chunk[] chunks)
        {
            List<PNG_Chunk> result = new List<PNG_Chunk>();

            PNG_IHDR header = new PNG_IHDR();

            using (MemoryStream rawData = new MemoryStream())
            {
                foreach (PNG_Chunk chunk in chunks)
                {
                    if (chunk.Name.Equals(PNG_CgBIChunk))
                        continue;

                    if (chunk.Name.Equals(PNG_HeaderChunk))
                        header = PNG_IHDR.FromBytes(chunk.Data);

                    if (chunk.Name.Equals(PNG_DataChunk))
                    {
                        rawData.Write(chunk.Data, 0, chunk.Data.Length);
                        continue;
                    }

                    if (chunk.Name.Equals(PNG_EndChunk))
                    {
                        const int dataChunkSize = 16 * 1024;

                        rawData.Position = 0;
                        using (MemoryStream buffer = new MemoryStream())
                        {
                            using (InflaterInputStream inflate = new InflaterInputStream(rawData, new Inflater(true)))
                            {
                                inflate.IsStreamOwner = false;
                                inflate.CopyTo(buffer);
                            }

                            buffer.Position = 0;
                            rawData.SetLength(0);
                            buffer.CopyTo(rawData);
                        }

                        {
                            byte[] imageData = rawData.GetBuffer();

                            int scanlineSize = header.Width * 4 + 1;
                            int scanlinePos = 0;

                            int bpp = 4;

                            for (int y = 0; y < header.Height; ++y, scanlinePos += scanlineSize)
                            {
                                PNG_Filter filter = (PNG_Filter)imageData[scanlinePos];

                                switch (filter)
                                {
                                    case PNG_Filter.Sub:
                                        for (int x = 1 + bpp; x < scanlineSize; ++x)
                                            imageData[scanlinePos + x] = (byte)((int)imageData[scanlinePos + x] + (int)imageData[scanlinePos + x - bpp]);
                                        break;

                                    case PNG_Filter.Up:
                                        if (y > 0)
                                            for (int x = 1; x < scanlineSize; ++x)
                                                imageData[scanlinePos + x] = (byte)((int)imageData[scanlinePos + x] + (int)imageData[scanlinePos + x - bpp - scanlineSize]);
                                        break;

                                    case PNG_Filter.Average:
                                        if (y > 0)
                                        {
                                            for (int x = 1 + bpp; x < scanlineSize; ++x)
                                                imageData[scanlinePos + x] = (byte)
                                                    (((int)imageData[scanlinePos + x - bpp] +
                                                    (int)imageData[scanlinePos + x - bpp - scanlineSize]) >> 1);
                                        }
                                        break;

                                    case PNG_Filter.Paeth:
                                        if (y > 0)
                                        {
                                            for (int x = 1 + bpp; x < scanlineSize; ++x)
                                                imageData[scanlinePos + x] =
                                                    (byte)FilterPaethPredictor(
                                                        imageData[scanlinePos + x - bpp],
                                                        imageData[scanlinePos + x - bpp - scanlineSize],
                                                        imageData[scanlinePos + x - scanlineSize]);
                                        }
                                        break;
                                }

                                for (int x = 0; x < header.Width; ++x)
                                {
                                    byte r = imageData[scanlinePos + x * 4 + 1];
                                    byte g = imageData[scanlinePos + x * 4 + 2];
                                    byte b = imageData[scanlinePos + x * 4 + 3];
                                    byte a = imageData[scanlinePos + x * 4 + 4];

                                    if (a > 0)
                                    {
                                        r = (byte)(255 * r / a);
                                        g = (byte)(255 * g / a);
                                        b = (byte)(255 * b / a);
                                    }

                                    imageData[scanlinePos + x * 4 + 1] = b;
                                    imageData[scanlinePos + x * 4 + 2] = g;
                                    imageData[scanlinePos + x * 4 + 3] = r;
                                    imageData[scanlinePos + x * 4 + 4] = a;
                                }

                                imageData[scanlinePos] = (byte)PNG_Filter.None;

                                //imageData[scanlinePos] = (byte)PNG_Filter.Sub;
                                //for (int x = 1 + bpp; x < scanlineSize; ++x)
                                //{
                                //    byte c = imageData[scanlinePos + x];
                                //    byte l = imageData[scanlinePos + x - bpp];
                                //    imageData[scanlinePos + x] = (byte)(l - c);
                                //}
                            }
                        }

                        Deflater deflater = new Deflater(9);

                        Crc32 crc32 = new Crc32();

                        rawData.Position = 0;
                        long dataLeft = rawData.Length;
                        while (dataLeft > 0)
                        {
                            byte[] chunkData = new byte[dataChunkSize];

                            int dataInChunk = rawData.Read(chunkData, 0, dataChunkSize);

                            dataLeft -= dataInChunk;

                            byte[] deflatedChunkData = new byte[dataChunkSize];
                            deflater.SetInput(chunkData, 0, dataInChunk);
                            if (dataLeft > 0)
                                deflater.Flush();
                            else
                                deflater.Finish();
                            int deflatedBytes = deflater.Deflate(deflatedChunkData, 0, dataInChunk);

                            PNG_Chunk dataChunk = new PNG_Chunk();
                            dataChunk.Name = PNG_DataChunk;
                            dataChunk.Data = new byte[deflatedBytes];

                            for (int i = 0; i < deflatedBytes; ++i)
                                dataChunk.Data[i] = deflatedChunkData[i];

                            crc32.Reset();
                            crc32.Update(ASCIIEncoding.ASCII.GetBytes(dataChunk.Name));
                            crc32.Update(dataChunk.Data);
                            dataChunk.CRC = (uint)crc32.Value;

                            result.Add(dataChunk);
                        }
                    }

                    result.Add(chunk);
                }
            }

            return result.ToArray();
        }

        public static int FilterPaethPredictor(int a, int b, int c)
        {
            // from http://www.libpng.org/pub/png/spec/1.2/PNG-Filters.html
            // a = left, b = above, c = upper left
            int p = a + b - c;// ; initial estimate
            int pa = p >= a ? p - a : a - p;
            int pb = p >= b ? p - b : b - p;
            int pc = p >= c ? p - c : c - p;
            // ; return nearest of a,b,c,
            // ; breaking ties in order a,b,c.
            if (pa <= pb && pa <= pc)
                return a;
            else if (pb <= pc)
                return b;
            else
                return c;
        }

        static public Stream Process(Stream reader)
        {
            MemoryStream output = new MemoryStream();

            using (MemoryStream input = new MemoryStream())
            {
                reader.CopyTo(input);
                input.Position = 0;
                PNG_Chunk[] chunks = ReadChunks(input);
                chunks = ProcessChunks(chunks);
                WriteChunks(output, chunks);
                output.Position = 0;
            }

            return output;
        }
    }
}

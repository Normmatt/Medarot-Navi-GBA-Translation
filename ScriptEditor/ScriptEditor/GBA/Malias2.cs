﻿using System.Collections.Generic;
using System.IO;

namespace Nintenlord.GBA.Compressions
{
    static unsafe class Malias2
    {
        #region Don't edit this!!!

        private const int SlidingWindowSize = 4096;
        private const int ReadAheadBufferSize = 18;
        private const int BlockSize = 8;

        private enum ScanDepth : byte
        {
            Byte = 1,
            HalfWord = 2,
            Word = 4
        };

        private const ScanDepth scanDepth = ScanDepth.Word;
        private const int sizeMultible = 0x20;
        private const int maxSize = 0x8000;

        #endregion Don't edit this!!!

        /// <summary>
        /// Scans the stream for potential LZ77 compressions
        /// </summary>
        /// <param name="br">Stream to scan</param>
        /// <returns>An array of offsets relative to the beginning of scan area</returns>
        static public int[] Scan(BinaryReader br, int offset, int amount, int sizeMultible, int minSize, int maxSize)
        {
            br.BaseStream.Position = offset;
            byte[] area = br.ReadBytes(amount);
            fixed (byte* pointer = &area[0])
            {
                return Scan(pointer, amount, sizeMultible, minSize, maxSize);
            }
        }

        /// <summary>
        /// Scans the data for potential LZ77 compressions
        /// </summary>
        /// <param name="data">Data to scan</param>
        /// <param name="offset">Starting offset of are to scan</param>
        /// <param name="amount">Size of the area to scan</param>
        /// <returns></returns>
        static public int[] Scan(byte[] data, int offset, int amount, int sizeMultible, int minSize, int maxSize)
        {
            fixed (byte* ptr = &data[offset])
            {
                return Scan(ptr, amount, sizeMultible, minSize, maxSize);
            }
        }

        /// <summary>
        /// Scans an area in memory for potential LZ77 compressions
        /// </summary>
        /// <param name="pointer">Pointer to start of area to scan</param>
        /// <param name="amount">Size of the area to scan in bytes</param>
        /// <returns>An array of offsets relative to the beginning of scan area</returns>
        static public int[] Scan(byte* pointer, int amount, int sizeMultible, int minSize, int maxSize)
        {
            List<int> results = new List<int>();

            for (int i = 0; i < amount; i += (int)scanDepth)
            {
                if (pointer[i] == 0x4C && pointer[i + 1] == 0x65)
                {
                    int length = pointer[i + 2] | pointer[i + 3] << 8 | pointer[i + 4] << 16;
                    if ((length > minSize) && (length < maxSize))
                    {
                        results.Add(i);
                    }
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// Decompresses Malias2 data
        /// </summary>
        /// <param name="data">Data where compressed data is</param>
        /// <param name="offset">Offset of the compressed data</param>
        /// <returns>Decompressed data or null if decompression fails</returns>
        static public byte[] Decompress(byte[] data, int offset)
        {
            fixed (byte* ptr = &data[offset])
            {
                return Decompress(ptr);
            }
        }

        /// <summary>
        /// Decompresses Malias2 data
        /// </summary>
        /// <param name="source">Pointer to data to decompress</param>
        /// <returns>Decompressed data or null if decompression fails</returns>
        static public byte[] Decompress(byte* source)
        {
            byte[] uncompressedData = null;
            if (*source == 0x4C && *(source + 1) == 0x65)
            {
                int length = *(source + 2) | *(source + 3) << 8 | *(source + 4) << 16;
                if (length > 0 && length < 0x100000)
                {
                    uncompressedData = new byte[length];
                    fixed (byte* destination = &uncompressedData[0])
                    {
                        if (!Decompress(source, destination))
                            uncompressedData = null;
                    }
                }
            }
            return uncompressedData;
        }

        /// <summary>
        /// Decompresses Malias2 data
        /// </summary>
        /// <param name="source">Pointer to compressed data</param>
        /// <param name="target">Pointer to where uncompressed data goes</param>
        /// <returns>True if successful, else false</returns>
        static public bool Decompress(byte* source, byte* target)
        {
            int length = *(source + 2) | *(source + 3) << 8 | *(source + 4) << 16;

            source += 6;

            while (length > 0)
            {
                byte command = *source++;
                int numCommands = 0;

                while (numCommands < 4 && length > 0)
                {
                    switch (command & 3)
                    {
                        case 0: //Mode 0: "Far" LZ77 Copy
                            {
                                int src = *source++ | *source++ << 8;
                                byte* copySrc = target - ((src & 0xFFF) + 5);
                                length -= (src >> 12) + 3;
                                int copyLength = (src >> 12) + 2;

                                if (copyLength == -1) break;

                                while (copyLength > -1)
                                {
                                    *target++ = *copySrc++;
                                    copyLength--;
                                }
                            }
                            break;
                        case 1: //Mode 1: "RLE" LZ77 Copy
                            {
                                int src = *source++;
                                byte* copySrc = target - ((src & 0x3) + 1);
                                length -= (src >> 2) + 2;
                                int copyLength = (src >> 2) + 1;

                                if (copyLength == -1) break;

                                while (copyLength > -1)
                                {
                                    *target++ = *copySrc++;
                                    copyLength--;
                                }
                            }
                            break;
                        case 2: //Mode 2: One uncompressed byte
                            *target++ = *source++;
                            length--;
                            break;
                        case 3: //Mode 3: Three uncompressed bytes
                            *target++ = *source++;
                            *target++ = *source++;
                            *target++ = *source++;
                            length -= 3;
                            break;
                    }
                    numCommands++;
                    command >>= 2;
                }
            }
            return true;
        }
    }
}
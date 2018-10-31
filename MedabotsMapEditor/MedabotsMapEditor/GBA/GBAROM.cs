using System.Collections.Generic;
using System.IO;
using Nintenlord.GBA.Compressions;

namespace Nintenlord.GBA
{
    class GBAROM : AbstractROM
    {
        public GBAROM()
        {
            maxLength = 0x2000000;
        }

        public override void OpenROM(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));
            if (maxLength > 0 && br.BaseStream.Length > maxLength)
                ROMdata = new byte[maxLength];
            else
                ROMdata = new byte[br.BaseStream.Length];
            br.Read(ROMdata, 0, ROMdata.Length);
            br.Close();
            this.edited = false;
            this.path = path;
        }

        public override void CloseROM()
        {
            this.edited = false;
            this.path = null;
            this.ROMdata = null;
        }

        public override void SaveROM(string path)
        {
            Path.ChangeExtension(path, ".gba");
            BinaryWriter rw = new BinaryWriter(File.Open(path, FileMode.Create));
            rw.Write(ROMdata);
            rw.Close();
            edited = false;
        }

        public override void SaveBackup()
        {
            Path.ChangeExtension(path, ".bak");
            BinaryWriter rw = new BinaryWriter(File.Open(path, FileMode.Create));
            rw.Write(ROMdata);
            rw.Close();
        }

        #region GBA pointers

        public int[] SearchForPointer(int offset)
        {
            if (offset < maxLength)
            {
                int[] values = this.SearchForValue(offset + 0x8000000);
                List<int> result = new List<int>(values);
                result.RemoveAll(IsInvalidIntOffset);
                return result.ToArray();
            }
            else
            {
                return new int[0];
            }
        }

        public int[] ReplacePointers(int oldOffset, int newOffset)
        {
            int[] offsets = SearchForPointer(oldOffset);
            for (int i = 0; i < offsets.Length; i++)
            {
                this.InsertData(offsets[i], newOffset + 0x8000000);
            }
            return offsets;
        }

        #endregion GBA pointers

        #region LZ77 compression

        public void InsertLZ77CompressedData(int offset, byte[] data)
        {
            InsertLZ77CompressedData(offset, data, 0, data.Length);
        }

        public void InsertLZ77CompressedData(int offset, byte[] data, int index, int length)
        {
            byte[] compressedData = LZ77.Compress(data, index, length);
            this.InsertData(offset, compressedData);
        }

        public byte[] DecompressLZ77CompressedData(int offset)
        {
            return LZ77.Decompress(ROMdata, offset);
        }

        public int[] ScanForLZ77CompressedData(int offset, int length, int maxSize, int minSize, int sizeMultible)
        {
            return LZ77.Scan(ROMdata, offset, length, sizeMultible, minSize, maxSize);
        }

        public bool CanBeLZ77Decompressed(int offset, int maxSize, int minSize)
        {
            return LZ77.CanBeUnCompressed(ROMdata, offset, minSize, maxSize);
        }

        public int LZ77CompressedDataLength(int offset)
        {
            return LZ77.GetCompressedDataLenght(ROMdata, offset);
        }

        #endregion LZ77 compression

        #region Malias2 compression

        public byte[] DecompressMalias2CompressedData(int offset)
        {
            return Malias2.Decompress(ROMdata, offset);
        }

        public int[] ScanForMalias2CompressedData(int offset, int length, int maxSize, int minSize, int sizeMultible)
        {
            return Malias2.Scan(ROMdata, offset, length, sizeMultible, minSize, maxSize);
        }

        #endregion Malias2 compression
    }
}
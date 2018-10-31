using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Nintenlord
{
    abstract class AbstractROM
    {
        protected bool edited;
        protected byte[] ROMdata;
        protected int maxLength;
        protected string path;
        public bool Edited
        {
            get { return edited; }
        }
        public int Length
        {
            get
            {
                if (ROMdata == null)
                    return 0;
                else
                    return ROMdata.Length; 
            }
            set { ChangeROMSize(value); }
        }
        public bool Opened
        {
            get { return ROMdata != null && path != null; }
        }
        public string ROMPath
        {
            get { return path; }
        }

        public abstract void OpenROM(string path);
        public abstract void CloseROM();
        public void SaveROM()
        {
            SaveROM(path);
        }
        public abstract void SaveROM(string path);
        public abstract void SaveBackup();

        public virtual void InsertData(int offset, int value)
        {
            InsertData(offset, BitConverter.GetBytes(value));
        }
        public virtual void InsertData(int offset, short value)
        {
            InsertData(offset, BitConverter.GetBytes(value));
        }
        public virtual void InsertData(int offset, byte value)
        {
            InsertData(offset, BitConverter.GetBytes(value));
        }
        public virtual void InsertData(int offset, byte[] data)
        {
            InsertData(offset, data, 0, data.Length);
        }
        public virtual void InsertData(int offset, byte[] data, int index)
        {
            InsertData(offset, data, index, data.Length);
        }
        public virtual void InsertData(int offset, byte[] data, int index, int length)
        {
            edited = true;
            if (length == 0 || index + length > data.Length)
                length = data.Length - index;
            if (offset + length > ROMdata.Length)
            {
                ChangeROMSize(offset + length);
                length = this.ROMdata.Length - offset;
            }
            Array.Copy(data, index, ROMdata, offset, length);
        }        

        private void ChangeROMSize(int newSize)
        {
            if (newSize > maxLength)
                newSize = maxLength;
            Array.Resize(ref ROMdata, newSize);
        }

        public virtual byte[] GetData(uint offset, int length)
        {
            return GetData((int)offset, length);
        }

        public virtual byte[] GetData(int offset, int length)
        {
            if (length == 0 || offset + length > ROMdata.Length)
                length = ROMdata.Length - offset;
            byte[] data = new byte[length];
            Array.Copy(ROMdata, offset, data, 0, length);
            return data;
        }

        public virtual sbyte Gets8(int offset)
        {
            return (sbyte)ROMdata[offset];
        }

        public virtual byte GetU8(int offset)
        {
            return ROMdata[offset];
        }

        public virtual short GetS16(int offset)
        {
            return BitConverter.ToInt16(ROMdata, offset);
        }

        public virtual ushort GetU16(int offset)
        {
            return BitConverter.ToUInt16(ROMdata,offset);
        }

        public virtual uint GetU32(uint offset)
        {
            return GetU32((int) offset);
        }

        public virtual uint GetU32(int offset)
        {
            return BitConverter.ToUInt32(ROMdata, offset);
        }

        public virtual int GetS32(int offset)
        {
            return BitConverter.ToInt32(ROMdata, offset);
        }

        public virtual int[] SearchForValue(int value)
        {
            return SearchForValue(BitConverter.GetBytes(value));
        }
        public virtual int[] SearchForValue(int value, int offset, int area)
        {
            return SearchForValue(BitConverter.GetBytes(value), offset, area);
        }
        public virtual int[] SearchForValue(short value)
        {
            return SearchForValue(BitConverter.GetBytes(value));
        }
        public virtual int[] SearchForValue(short value, int offset, int area)
        {
            return SearchForValue(BitConverter.GetBytes(value), offset, area);
        }
        public virtual int[] SearchForValue(byte[] value)
        {
            return SearchForValue(value, 0, ROMdata.Length - value.Length); 
        }
        public virtual int[] SearchForValue(byte[] value, int offset, int area)
        {
            List<int> offsets = new List<int>();
            int index = offset;
            int maxIndex = area + offset;
            while (index < maxIndex)
            {
                int foundIndex = 0;
                while (index + foundIndex < ROMdata.Length
                    && foundIndex < value.Length
                    && ROMdata[index + foundIndex] == value[foundIndex])
                {
                    foundIndex++;
                }
                if (foundIndex == value.Length)
                    offsets.Add(index);

                index++;
            }
            return offsets.ToArray();
        }

        public static bool IsValidIntOffset(int value)
        {
            return (value & 3) == 0;
        }
        public static bool IsInvalidIntOffset(int value)
        {
            return !IsValidIntOffset(value);
        }
        public static bool IsValidShortOffset(int value)
        {
            return (value & 1) == 0;
        }
        public static bool IsInvalidShortOffset(int value)
        {
            return !IsValidShortOffset(value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Facsimile
{
    class CharacterInfo
    {
        public string TextString { get; set; }

        public int Length { get; set; }
    }

    class Table
    {
        private int m_LongestHex = 0;
        private string m_termination = "`";
        private Dictionary<string, string> table;
        private Dictionary<string, string> insTable; //Used for insertion
        public Encoding encoding = Encoding.UTF8; //SJIS

        public int LongestHex
        {
            get { return m_LongestHex; }
            set { }
        }

        public Dictionary<string, string> TableMapping
        {
            get { return table; }
            set { table = value; }
        }

        public Dictionary<string, string> InsertionTableMapping
        {
            get { return insTable; }
            set { insTable = value; }
        }

        public Table(Stream fs)
        {
            try
            {
                StreamReader sr = new StreamReader(fs, encoding);
                string[] lines = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                table = new Dictionary<string, string>();
                insTable = new Dictionary<string, string>();
                int cur_line = 0;
                foreach (string str in lines)
                {
                    if (str == "") continue;
                    if (str.StartsWith("//")) continue;

                    if (str.StartsWith("/"))
                    {
                        m_termination = str.Substring(1, str.Length - 1);
                        continue;
                    }

                    int i = str.IndexOf("=", StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        if ((str.Length - 1) != i)
                        {
                            string sByte = str.Substring(0, i).ToUpper();
                            string sVal = str.Substring(i + 1);
                            //System.Diagnostics.Trace.WriteLine(sVal);

                            if (!table.ContainsKey(sByte))
                                table.Add(sByte, sVal);
                            else
                                table[sByte] = sVal;

                            if(!insTable.ContainsKey(sVal))
                                insTable.Add(sVal, sByte);
                            else
                                insTable[sVal] = sByte;

                            if (sByte.Length > LongestHex)
                                m_LongestHex = sByte.Length;
                        }
                        cur_line++;
                    }
                    else
                    {
                        throw new Exception("Invalid table file.");
                    }
                }
            }
            catch
            {
                throw;// new Exception("Error accessing file.");
            }
        }

        public List<byte> HandleControlCodes(string text,ref int index)
        {
            List<byte> data = new List<byte>();

            var endIndex = text.IndexOf('>', index + 1) + 1;

            var ctr = text.Substring(index, endIndex - index);
            var code = ctr.Substring(1, ctr.Length - 2);

            /*if (ctr.Contains(":"))
            {
                //parameters
            }
            else*/
            {
                //no parameters
                if (insTable.ContainsKey(ctr))
                {
                    for (int i = 0; i < insTable[ctr].Length/2; i++)
                        data.Add(Convert.ToByte(insTable[ctr].Substring(i*2,2), 16));
                }
            }

            index = endIndex;

            return data;
        }

        public byte[] EncodeString(string text)
        {
            List<byte> data = new List<byte>();

            int i = 0;
            while (i < text.Length)
            {
                var chr = text[i];
                switch (chr)
                {
                    case '\r': //Windows bullshit carrage return (useless)
                        i++;
                        break;
                    case '\n': //New Line
                        data.Add(0x02);
                        i++;
                        break;
                    case '<':
                        data.AddRange(HandleControlCodes(text, ref i));
                        break;
                    default:
                        if (insTable.ContainsKey(chr.ToString()))
                        {
                            data.Add(Convert.ToByte(insTable[chr.ToString()], 16));
                            i++;
                        }
                        break;
                }
            }

            //Null terminate string
            data.Add(0x00);

            return data.ToArray();
        }

        public byte[] EncodeCharacter(string character)
        {
            List<byte> data = new List<byte>();

            if (insTable.ContainsKey(character))
            {
                for (int i = 0; i < insTable[character].Length / 2; i++)
                    data.Add(Convert.ToByte(insTable[character].Substring(i * 2, 2), 16));
            }

            return data.ToArray();
        }

        public CharacterInfo DecodeCharacter(byte[] raw, int curAdr = 0)
        {
            StringBuilder sb = new StringBuilder();

            int hexSize = LongestHex/2;

            while (hexSize > 0)
            {
                String hexString = "";
                for (int i = curAdr; i < curAdr + hexSize && i < raw.Length; i++)
                    hexString += String.Format("{0:X2}", raw[i]);

                if (TableMapping.ContainsKey(hexString))
                {
                    String strToWrite = TableMapping[hexString];

                    strToWrite = strToWrite.Replace("\\n", "\n");

                    sb.Append(strToWrite);
                    break;
                }

                if (--hexSize <= 0)
                {
                    sb.AppendFormat("<${0}>", hexString);
                    hexSize = 1;
                    break;
                }
            }

            var temp = new CharacterInfo();
            temp.TextString = sb.ToString();
            temp.Length = hexSize;
            return temp;
        }

        public string DecodeString(byte[] raw, int curAdr = 0)
        {
            StringBuilder sb = new StringBuilder();

            bool finished = false;
            while (curAdr < raw.Length && !finished)
            {
                int hexSize = LongestHex / 2;

                while (hexSize > 0)
                {
                    String hexString = "";
                    for (int i = curAdr; i < curAdr + hexSize && i < raw.Length ; i++)
                        hexString += String.Format("{0:X2}", raw[i]);

                    if (hexString == m_termination)
                    {
                        finished = true;
                        break;
                    }

                    if (TableMapping.ContainsKey(hexString))
                    {
                        String strToWrite = TableMapping[hexString];

                        //Seek first
                        curAdr += hexSize;

                        strToWrite = strToWrite.Replace("\\n", "\n");

                        sb.Append(strToWrite);
                        hexSize = 0;
                    }
                    else
                    {
                        if (--hexSize <= 0)
                        {
                            sb.AppendFormat("<${0}>", hexString);
                            curAdr++;
                        }
                    };
                }
            }
            return sb.ToString();
        }
    }
}
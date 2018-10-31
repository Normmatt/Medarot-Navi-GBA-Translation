using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Facsimile;
using Nintenlord.GBA;
using ScriptEditor.Properties;

namespace ScriptEditor
{
    public partial class Form1 : Form
    {
        private GBAROM ROM;
        private Table tbl = new Table(new MemoryStream(Resources.navi));
        private List<int> pointerList;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtOriginal.LanguageOption = RichTextBoxLanguageOptions.DualFont;

            ROM = new GBAROM();
            ROM.OpenROM("output.gba");

            LoadScript((int) numericUpDown1.Value, (int) numericUpDown2.Value);

        }

        void LoadScript(int id, int subID)
        {
            //var adr = 0x5FBCF0;
            var adr = ROM.GetU32(0x629728 + (4*id)) - 0x08000000;
            var size = 0x20000;

            var data = ROM.GetData((int) adr, size);
            ScriptBytecode sc = new ScriptBytecode(data, id, (int) adr);

            numericUpDown2.Maximum = sc.GetNumberOfSubScripts();

            sc.Parse(subID);

            var debug = sc.ParseDebug(subID);
            txtDebug.Text = debug;
            //File.WriteAllText("debug_"+id+"_"+subID+".txt", debug);

            pointerList = sc.GetStringPointers();

            listBox1.Items.Clear();
            foreach (var ptr in pointerList)
            {
                listBox1.Items.Add(DumpString(ref data, ptr));
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index > -1)
            {
                var item = (listBox1.Items[e.Index]);
                string s = item.ToString();

                /*Normal items*/
                if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
                {
                    Color clr = SystemColors.Window;
                    if ((e.Index & 1) == 1)
                        clr = SystemColors.ControlLight;
                    //else if (item.Final)
                    //    clr = Color.Goldenrod;

                    e.Graphics.FillRectangle(
                        new SolidBrush(clr),
                        e.Bounds);
                    e.Graphics.DrawString(s, Font,
                        new SolidBrush(SystemColors.WindowText),
                        e.Bounds);
                }
                else /*Selected item, needs highlighting*/
                {
                    e.Graphics.FillRectangle(
                        new SolidBrush(Color.SlateBlue),
                        e.Bounds);
                    e.Graphics.DrawString(s, Font,
                        new SolidBrush(SystemColors.HighlightText),
                        e.Bounds);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (listBox1.Items[listBox1.SelectedIndex]);
            txtOriginal.Text = item.ToString();
            label6.Text = String.Format("Offset: {0:X8}", pointerList[listBox1.SelectedIndex]);
        }

        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            string s = listBox1.Items[e.Index].ToString();
            SizeF sf = e.Graphics.MeasureString(s, Font, Width);
            int htex = 5;
            e.ItemHeight = (int)sf.Height + htex;
            e.ItemWidth = Width;
        }

        private string DumpString(ref byte[] data, int curAdr = 0)
        {
            StringBuilder sb = new StringBuilder();
            byte currentChar = 0;
            bool finished = false;
            while (!finished && ((currentChar = data[curAdr]) != 0))
            {
                CharacterInfo temp;
                switch (currentChar)
                {
                    case 0xE5: //Set Delay
                        sb.AppendFormat("<SET_DELAY:{0:X4}>", ROM.GetU8(curAdr + 1) << 8 | ROM.GetU8(curAdr + 2));
                        curAdr += 3;
                        break;
                    case 0xE6: //Restart Music
                        sb.AppendFormat("<RESTART_MUSIC>");
                        curAdr++;
                        break;
                    case 0xF1: //New Line
                        sb.Append("<NL>\r\n");
                        curAdr++;
                        break;

                    case 0xF2: //New Line
                        sb.Append("<WAIT>\r\n\r\n");
                        curAdr++;
                        break;

                    case 0xF3: //Clear String Pointer (End string?)
                        sb.Append("<F3>");
                        finished = true;
                        break;

                    case 0xF4: //Clear String Pointer and Clear tiles (End string?)
                        sb.Append("<CLR>");
                        curAdr++;
                        break;

                    case 0xF5: //Play Sound Effect
                        sb.AppendFormat("<PLAY_SFX:{0:X2}>", ROM.GetU8(curAdr + 1));
                        curAdr += 2;
                        break;

                    case 0xF6: //Face
                    case 0xF7: //Face
                        sb.AppendFormat("<FACE:{0:X2}>", ROM.GetU8(curAdr + 1));
                        curAdr += 2;
                        break;
                   
                    default:
                        temp = tbl.DecodeCharacter(data, curAdr);
                        sb.Append(temp.TextString);
                        curAdr += temp.Length;
                        break;
                }
            }

            return sb.ToString();
        }

        private string DumpWikiString(int pos)
        {
            StringBuilder sb = new StringBuilder();
            byte currentChar = 0;
            bool finished = false;
            while (!finished && ((currentChar = ROM.GetU8(pos)) != 0))
            {
                CharacterInfo temp;
                switch (currentChar)
                {
                    case 0xE5: //Set Delay
                        sb.AppendFormat("<SET_DELAY:{0:X4}>", ROM.GetU8(pos + 1) << 8 | ROM.GetU8(pos + 2));
                        pos += 3;
                        break;
                    case 0xE6: //Restart Music
                        sb.AppendFormat("<RESTART_MUSIC>");
                        pos++;
                        break;
                    case 0xF1: //New Line
                        sb.Append("<NL>\r\n");
                        pos++;
                        break;

                    case 0xF2: //New Line
                        sb.Append("<WAIT>\r\n\r\n");
                        pos++;
                        break;

                    case 0xF3: //Clear String Pointer (End string?)
                        sb.Append("<F3>");
                        finished = true;
                        break;

                    case 0xF4: //Clear String Pointer and Clear tiles (End string?)
                        sb.Append("<CLR>");
                        pos++;
                        break;

                    case 0xF5: //Play Sound Effect
                        sb.AppendFormat("<PLAY_SFX:{0:X2}>", ROM.GetU8(pos + 1));
                        pos += 2;
                        break;

                    case 0xF6: //Face
                    case 0xF7: //Face
                        sb.AppendFormat("<[[File:Navi F-{0:X2}.gif]]>", ROM.GetU8(pos + 1));
                        pos += 2;
                        break;

                    //case 0x01:
                    default:
                        temp = tbl.DecodeCharacter(ROM.GetData(pos, tbl.LongestHex / 2));
                        sb.Append(temp.TextString);
                        pos += temp.Length;
                        break;
                }
            }

            return sb.ToString();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown2.Value = 0; //Force it to 0 as that is safe
            LoadScript((int)numericUpDown1.Value, (int)numericUpDown2.Value);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            LoadScript((int)numericUpDown1.Value, (int)numericUpDown2.Value);
        }

        private int GetRealLength(int ofs)
        {
            const int maxStringLength = 4096;
            var data = ROM.GetData(ofs, maxStringLength);
            var realLength = 0;
            while (data[realLength] != 0)
            {
                switch (data[realLength])
                {
                    case 0xE0: //Kanji
                    case 0xE1: //Kanji
                    case 0xF5: //Play Sound Effect
                    case 0xF6: //Face
                        realLength += 2;
                        break;
                    case 0xE5: //Set Delay
                        realLength += 3;
                        break;
                    case 0xF3: //Clear String Pointer (End string?)
                        return realLength+1;
                    default:
                        realLength++;
                        break;
                }
            }

            return realLength;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = Directory.CreateDirectory("..\\..\\asm\\script");
            DirectoryInfo diwiki = Directory.CreateDirectory("..\\..\\wiki");
            var ids_with_text = new List<int>();
            var base_addresses = new List<uint>();
            var misc_files = new List<string>();
            const int maxID = 370;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < maxID; i++)
            {
                var adr = ROM.GetU32(0x629728 + (4*i)) - 0x08000000;
                var size = 0x20000;

                if (!base_addresses.Contains(adr))
                {
                    base_addresses.Add(adr);

                    var data = ROM.GetData((int) adr, size);
                    ScriptBytecode sc = new ScriptBytecode(data, i, (int) adr);

                    var numSubScripts = sc.GetNumberOfSubScripts();

                    for (int s = 0; s < numSubScripts; s++)
                    {
                        sc.Parse(s);
                    }

                    var dict = sc.GetLookupDictionary();

                    if (dict.Count > 0)
                    {
                        ids_with_text.Add(i);
                        DirectoryInfo sdi = di.CreateSubdirectory("script_" + i);

                        File.WriteAllText(sdi.FullName + "\\pointers.asm", MakeScriptPointerString(i, dict));
                        File.WriteAllText(sdi.FullName + "\\strings.asm", MakeScriptString(i, dict));
                        File.WriteAllText(diwiki.FullName + String.Format("\\script_{0}.txt",i), MakeScriptStringWiki(i, dict));
                        sb.Append(MakeScriptStringWiki(i, dict));
                    }
                }
            }

            //Menu strings
            var menu_pointers = new List<uint>()
            {
                0x07C910,
                0x07C924,
                0x07C92C,
                0x07C958,
                0x07C968,
                0x07C978,

                0x7EB9A0,
                0x7EB9A4,
                0x7EB9A8,
                0x7EB9AC,
                0x7EB9B0,
                0x7EB9B4,

                0x7EB9E4,
                0x7EB9E8,

                0x7EB9EC,

                0x7EB9F0,

                0x7EB9F4,
                0x7EB9F8,
                0x7EB9FC,
                0x7EBA00,

                0x7EBA04,
                0x7EBA08,
                0x7EBA0C,
                0x7EBA10,

                0x7EBA14,

                0x7EBA18,

                0x7EBA2C,
                0x7EBA30,
                0x7EBA34,
                0x7EBA38,

            };
            DirectoryInfo menu_sdi = di.CreateSubdirectory("menu");
            File.WriteAllText(menu_sdi.FullName + "\\pointers.asm", MakePointersFile(menu_pointers));
            File.WriteAllText(menu_sdi.FullName + "\\strings.asm", MakeStringsFile(menu_pointers));
            File.WriteAllText(diwiki.FullName + "\\menu.txt", MakeStringsFileWiki(menu_pointers));
            sb.Append(MakeStringsFileWiki(menu_pointers));
            misc_files.Add("menu");

            //medaforce
            DirectoryInfo medaforce_sdi = di.CreateSubdirectory("medaforce");
            File.WriteAllText(medaforce_sdi.FullName + "\\pointers.asm", MakeMedaforcePointerString());
            File.WriteAllText(medaforce_sdi.FullName + "\\strings.asm", MakeMedaforceString());
            File.WriteAllText(diwiki.FullName + "\\medaforce.txt", MakeMedaforceStringWiki());
            sb.Append(MakeMedaforceStringWiki());
            misc_files.Add("medaforce");

            //
            File.WriteAllText(di.FullName + "\\robattle_team_info.asm", DumpRobattleTeamInfo());

            //Write master file
            File.WriteAllText(di.FullName + "\\script.asm", MakeMainFile(ids_with_text, misc_files));

            File.WriteAllText(diwiki.FullName + "\\master.txt", sb.ToString());
        }

        #region Pointer Dictionary
        private string MakeScriptPointerString(int id, Dictionary<int, string> dict)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("; Medarot Navi Script {0} Pointers", id).AppendLine().AppendLine();

            const uint engControlCode = 0xE3;
            const ushort engControlCode2 = 0xE4; //Some sort of pointer table maybe?
            foreach (var t in dict)
            {
                var realLength = GetRealLength(t.Key);

                if (realLength > 1)
                {
                    sb.AppendFormat(".org 0x{0:X8}", t.Key + 0x08000000).AppendLine();
                    if (realLength <= 2)
                    {
                        sb.AppendFormat("\t; .halfword 0x{0:X4} ;TODO: FIX THIS PROPERLY", engControlCode2).AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat("\t.word 0x{0:X2}|(({1}-0x08000000)<<8)", engControlCode, t.Value).AppendLine();
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string MakeScriptString(int id, Dictionary<int, string> dict)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("; Medarot Navi Script {0} Strings", id).AppendLine().AppendLine();

            foreach (var t in dict)
            {
                var realLength = GetRealLength(t.Key);
                var data = ROM.GetData(t.Key, realLength);

                if (realLength > 1)
                {
                    sb.AppendFormat("{0}:", t.Value).AppendLine();
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string MakeScriptStringWiki(int id, Dictionary<int, string> dict)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{{| class=wikitable").AppendLine();
            sb.AppendFormat("|-").AppendLine();
            sb.AppendFormat("! ID").AppendLine();
            sb.AppendFormat("! Japanese").AppendLine();
            sb.AppendFormat("! English").AppendLine();

            foreach (var t in dict)
            {
                var realLength = GetRealLength(t.Key);
                //var data = ROM.GetData(t.Key, realLength);

                if (realLength > 1)
                {
                    var str = DumpWikiString(t.Key);

                    sb.AppendFormat("|-").AppendLine();
                    sb.AppendFormat("| {0}", t.Value).AppendLine();
                    sb.AppendFormat("| {0}", str).AppendLine();
                    sb.AppendFormat("| ").AppendLine();
                }
            }
            sb.AppendFormat("|}}").AppendLine().AppendLine();
            sb.AppendFormat("[[Category:Medarot Navi Translation Project]]").AppendLine();

            return sb.ToString();
        }
        #endregion

        #region Generic Pointer List
        private string MakePointersFile(List<uint> pointers)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("; Medarot Navi Pointers").AppendLine().AppendLine();

            foreach (var t in pointers)
            {
                var realAdr = ROM.GetU32(t) - 0x08000000;
                sb.AppendFormat(".org 0x{0:X8}", t + 0x08000000).AppendLine();
                sb.AppendFormat("\t.word str_{0:X8}", realAdr).AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string MakeStringsFile(List<uint> pointers)
        {
            var sb = new StringBuilder();
            var dumpedList = new List<string>();

            sb.AppendFormat("; Medarot Navi Pointers").AppendLine().AppendLine();

            foreach (var t in pointers)
            {
                var realAdr = ROM.GetU32(t) - 0x08000000;
                var name = String.Format("str_{0:X8}:", realAdr);
                if (!dumpedList.Contains(name))
                {
                    var realLength = GetRealLength((int) realAdr);
                    var data = ROM.GetData(realAdr, realLength);

                    sb.AppendFormat(name).AppendLine();
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendLine();

                    dumpedList.Add(name);
                }
            }

            return sb.ToString();
        }

        private string MakeStringsFileWiki(List<uint> pointers)
        {
            var sb = new StringBuilder();
            var dumpedList = new List<string>();

            sb.AppendFormat("{{| class=wikitable").AppendLine();
            sb.AppendFormat("|-").AppendLine();
            sb.AppendFormat("! ID").AppendLine();
            sb.AppendFormat("! Japanese").AppendLine();
            sb.AppendFormat("! English").AppendLine();

            foreach (var t in pointers)
            {
                var realAdr = ROM.GetU32(t) - 0x08000000;
                var name = String.Format("str_{0:X8}:", realAdr);
                if (!dumpedList.Contains(name))
                {
                    var realLength = GetRealLength((int)realAdr);
                    //var data = ROM.GetData(realAdr, realLength);
                    var str = DumpWikiString((int)realAdr);

                    sb.AppendFormat("|-").AppendLine();
                    sb.AppendFormat("| {0}", name.Substring(0,name.Length-1)).AppendLine();
                    sb.AppendFormat("| {0}", str).AppendLine();
                    sb.AppendFormat("| ").AppendLine();

                    dumpedList.Add(name);
                }
            }

            sb.AppendFormat("|}}").AppendLine().AppendLine();
            sb.AppendFormat("[[Category:Medarot Navi Translation Project]]").AppendLine();

            return sb.ToString();
        }
        #endregion

        #region Medaforce
        private string MakeMedaforcePointerString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("; Medarot Navi Medaforce Pointers").AppendLine().AppendLine();

            var name_ptrs = ROM.SearchForPointer(0x90C0C);
            var desc1_ptrs = ROM.SearchForPointer(0x90C1C);
            var desc2_ptrs = ROM.SearchForPointer(0x90C50);

            foreach (var ptr in name_ptrs)
            {
                sb.AppendFormat(".org 0x{0:X8}", ptr + 0x08000000).AppendLine();
                sb.AppendFormat("\t.word medaforce_1_name").AppendLine();
                sb.AppendLine();
            }

            foreach (var ptr in desc1_ptrs)
            {
                sb.AppendFormat(".org 0x{0:X8}", ptr + 0x08000000).AppendLine();
                sb.AppendFormat("\t.word medaforce_1_desc1").AppendLine();
                sb.AppendLine();
            }

            foreach (var ptr in desc2_ptrs)
            {
                sb.AppendFormat(".org 0x{0:X8}", ptr + 0x08000000).AppendLine();
                sb.AppendFormat("\t.word medaforce_1_desc2").AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string MakeMedaforceString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("; Medarot Navi Medaforce Strings").AppendLine().AppendLine();

            const int medaforce_name_base = 0x90C0C;
            const int medaforce_desc1_base = 0x90C1C;
            const int medaforce_desc2_base = 0x90C50;
            const int medaforce_info_size = 0x78;
            for (int i = 0; i < 60; i++)
            {
                //names
                {
                    var adr = medaforce_name_base + medaforce_info_size*i;
                    var realLength = GetRealLength(adr);
                    var data = ROM.GetData(adr, realLength);

                    //if (realLength > 1)
                    {
                        sb.AppendFormat("medaforce_{0}_name:", i + 1).AppendLine();
                        var str = tbl.DecodeString(data);
                        sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                        sb.AppendFormat("\t.fill (16-(.-medaforce_{0}_name))", i + 1).AppendLine();
                        sb.AppendLine();
                    }
                }
                //desc1
                {
                    var adr = medaforce_desc1_base + medaforce_info_size * i;
                    var realLength = GetRealLength(adr);
                    var data = ROM.GetData(adr, realLength);

                    //if (realLength > 1)
                    {
                        sb.AppendFormat("medaforce_{0}_desc1:", i + 1).AppendLine();
                        var str = tbl.DecodeString(data);
                        sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                        sb.AppendFormat("\t.fill (52-(.-medaforce_{0}_desc1))", i + 1).AppendLine();
                        sb.AppendLine();
                    }
                }
                //desc1
                {
                    var adr = medaforce_desc2_base + medaforce_info_size * i;
                    var realLength = GetRealLength(adr);
                    var data = ROM.GetData(adr, realLength);

                    //if (realLength > 1)
                    {
                        sb.AppendFormat("medaforce_{0}_desc2:", i + 1).AppendLine();
                        var str = tbl.DecodeString(data);
                        sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                        sb.AppendFormat("\t.fill (52-(.-medaforce_{0}_desc2))", i + 1).AppendLine();
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString();
        }

        private string MakeMedaforceStringWiki()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{{| class=wikitable").AppendLine();
            sb.AppendFormat("|-").AppendLine();
            sb.AppendFormat("! ID").AppendLine();
            sb.AppendFormat("! Japanese").AppendLine();
            sb.AppendFormat("! English").AppendLine();

            const int medaforce_name_base = 0x90C0C;
            const int medaforce_desc1_base = 0x90C1C;
            const int medaforce_desc2_base = 0x90C50;
            const int medaforce_info_size = 0x78;
            for (int i = 0; i < 60; i++)
            {
                //names
                {
                    var adr = medaforce_name_base + medaforce_info_size * i;
                    var realLength = GetRealLength(adr);

                    //if (realLength > 1)
                    {
                        var str = DumpWikiString(adr);

                        sb.AppendFormat("|-").AppendLine();
                        sb.AppendFormat("| medaforce_{0}_name:", i + 1).AppendLine();
                        sb.AppendFormat("| {0}", str).AppendLine();
                        sb.AppendFormat("| ").AppendLine();
                    }
                }
                //desc1
                {
                    var adr = medaforce_desc1_base + medaforce_info_size * i;
                    var realLength = GetRealLength(adr);

                    //if (realLength > 1)
                    {
                        var str = DumpWikiString(adr);

                        sb.AppendFormat("|-").AppendLine();
                        sb.AppendFormat("| medaforce_{0}_desc1:", i + 1).AppendLine();
                        sb.AppendFormat("| {0}", str).AppendLine();
                        sb.AppendFormat("| ").AppendLine();
                    }
                }
                //desc1
                {
                    var adr = medaforce_desc2_base + medaforce_info_size * i;
                    var realLength = GetRealLength(adr);

                    //if (realLength > 1)
                    {
                        var str = DumpWikiString(adr);

                        sb.AppendFormat("|-").AppendLine();
                        sb.AppendFormat("| medaforce_{0}_desc2:", i + 1).AppendLine();
                        sb.AppendFormat("| {0}", str).AppendLine();
                        sb.AppendFormat("| ").AppendLine();
                    }
                }
            }

            sb.AppendFormat("|}}").AppendLine().AppendLine();
            sb.AppendFormat("[[Category:Medarot Navi Translation Project]]").AppendLine();

            return sb.ToString();
        }
        #endregion

        private string MakeMainFile(List<int> idList, List<string> miscList )
        {
            var sb = new StringBuilder();

            sb.AppendFormat("; Medarot Navi Translation").AppendLine();
            sb.AppendFormat("; Number of Scripts {0}", idList.Count).AppendLine().AppendLine();

            sb.AppendFormat(".gba\t\t\t\t; Set the architecture to GBA").AppendLine();
            sb.AppendFormat(".open \"rom/output.gba\",0x08000000\t; Open input.gba for output.").AppendLine();
            sb.AppendFormat("\t\t\t\t\t\t; 0x08000000 will be used as the").AppendLine();
            sb.AppendFormat("\t\t\t\t\t\t; header size").AppendLine().AppendLine();
            sb.AppendFormat(".relativeinclude on").AppendLine().AppendLine();
            sb.AppendFormat(".table \"navi.tbl\"").AppendLine().AppendLine();

            sb.AppendFormat("; Include all the files with pointer adjustments").AppendLine();
            foreach (var id in idList)
            {
                sb.AppendFormat(".include \"script_{0}/pointers.asm\"", id).AppendLine();
            }

            foreach (var id in miscList)
            {
                sb.AppendFormat(".include \"{0}/pointers.asm\"", id).AppendLine();
            }

            sb.AppendLine();

            const uint position = 0x08900000;

            sb.AppendLine().AppendFormat("; Include all the files with strings").AppendLine();
            sb.AppendFormat("\t.org 0x{0:X8}", position).AppendLine();

            foreach (var id in idList)
            {
                sb.AppendFormat(".include \"script_{0}/strings.asm\"", id).AppendLine();
            }

            foreach (var id in miscList)
            {
                sb.AppendFormat(".include \"{0}/strings.asm\"", id).AppendLine();
            }

            foreach (var id in idList)
            {
                sb.AppendFormat(".include \"robattle_team_info.asm\"", id).AppendLine();
            }

            sb.AppendLine().AppendLine(".close").AppendLine();

            sb.AppendLine(" ; make sure to leave an empty line at the end");

            return sb.ToString();
        }

        private string DumpRobattleTeamInfo()
        {
            var sb = new StringBuilder();
            var numTeams = 147;

            sb.AppendFormat("; Medarot Navi Translation").AppendLine();
            sb.AppendFormat("; Number of teams", numTeams).AppendLine().AppendLine();

            var pointers = new List<uint>()
            {
                0x008470,
                0x008524,
                0x07E410,
                0x07FA7C,
            };

            foreach (var t in pointers)
            {
                var realAdr = ROM.GetU32(t) - 0x08000000;
                sb.AppendFormat(".org 0x{0:X8}", t + 0x08000000).AppendLine();
                sb.AppendFormat("\t.word str_{0:X8}", realAdr).AppendLine();
                sb.AppendLine();
            }
            sb.AppendLine();

            var robattle_team_info_base = 0x86650;
            var robattle_team_info_size = 0xC0;
            var desc_pointer_names = new List<string>();
            var desc_pointers = new List<int>();
            for (int i = 0; i < numTeams; i++)
            {
                var adr = robattle_team_info_base + robattle_team_info_size * i;
                sb.AppendFormat("robattle_team_{0}_info:", i + 1).AppendLine();

                //name
                {
                    var data = ROM.GetData(adr, 8);
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("robattle_team_{0}_info_name: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendFormat("\t.fill (8-(.-robattle_team_{0}_info_name))", i + 1).AppendLine();
                }
                adr += 8;

                //leader
                {
                    var data = ROM.GetData(adr, 8);
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("robattle_team_{0}_info_leader: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendFormat("\t.fill (8-(.-robattle_team_{0}_info_leader))", i + 1).AppendLine();
                }
                adr += 8;

                //medarot1
                {
                    var data = ROM.GetData(adr, 8);
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("robattle_team_{0}_info_medarot1: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendFormat("\t.fill (8-(.-robattle_team_{0}_info_medarot1))", i + 1).AppendLine();
                }
                adr += 8;

                //medarot2
                {
                    var data = ROM.GetData(adr, 8);
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("robattle_team_{0}_info_medarot2: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendFormat("\t.fill (8-(.-robattle_team_{0}_info_medarot2))", i + 1).AppendLine();
                }
                adr += 8;

                //medarot3
                {
                    var data = ROM.GetData(adr, 8);
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("robattle_team_{0}_info_medarot3: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendFormat("\t.fill (8-(.-robattle_team_{0}_info_medarot3))", i + 1).AppendLine();
                }
                adr += 8;

                //medarot4
                {
                    var data = ROM.GetData(adr, 8);
                    var str = tbl.DecodeString(data);
                    sb.AppendFormat("robattle_team_{0}_info_medarot4: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                    sb.AppendFormat("\t.fill (8-(.-robattle_team_{0}_info_medarot4))", i + 1).AppendLine();
                }
                adr += 8;

                //encounter1_desc_ptr
                {
                    var ptr = ROM.GetS32(adr);
                    var name = String.Format("robattle_team_{0}_info_encounter1_desc", i + 1);
                    desc_pointer_names.Add(name);
                    desc_pointers.Add(ptr);
                    sb.AppendFormat("robattle_team_{0}_info_encounter1_desc_ptr: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.word {0}", name).AppendLine();
                }
                adr += 4;

                //encounter2_desc_ptr
                {
                    var ptr = ROM.GetS32(adr);
                    var name = String.Format("robattle_team_{0}_info_encounter2_desc", i + 1);
                    desc_pointer_names.Add(name);
                    desc_pointers.Add(ptr);
                    sb.AppendFormat("robattle_team_{0}_info_encounter2_desc_ptr: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.word {0}", name).AppendLine();
                }
                adr += 4;

                //encounter3_desc_ptr
                {
                    var ptr = ROM.GetS32(adr);
                    var name = String.Format("robattle_team_{0}_info_encounter3_desc", i + 1);
                    desc_pointer_names.Add(name);
                    desc_pointers.Add(ptr);
                    sb.AppendFormat("robattle_team_{0}_info_encounter3_desc_ptr: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.word {0}", name).AppendLine();
                }
                adr += 4;

                //character_portrait
                {
                    var val = ROM.GetU8(adr);
                    sb.AppendFormat("robattle_team_{0}_info_character_portrait: ", i + 1).AppendLine();
                    sb.AppendFormat("\t.byte {0}", val).AppendLine();
                }
                adr += 4;

                sb.AppendLine().AppendLine();
            }

            for (int i = 0; i < desc_pointers.Count; i++)
            {
                var name = desc_pointer_names[i];
                var ptr = desc_pointers[i] - 0x08000000;
                var realLength = GetRealLength(ptr);
                var data = ROM.GetData(ptr, realLength);

                var str = tbl.DecodeString(data);
                sb.AppendFormat("{0}: ", name).AppendLine();
                sb.AppendFormat("\t.string \"{0}\"", str).AppendLine();
                sb.AppendFormat("\t.align 4").AppendLine();
            }

            return sb.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.WriteAllText("..\\..\\asm\\script\\robattle_team_info.asm", DumpRobattleTeamInfo());
        }
    }
}

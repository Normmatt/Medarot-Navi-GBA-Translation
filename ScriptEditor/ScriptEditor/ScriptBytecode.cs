using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScriptEditor
{
    class ScriptBytecode
    {
        private byte[] data;
        private List<int> pointers_in_script;
        private List<int> pointers;
        private Dictionary<int, string> pointers_to_string; 
        private int numSubScripts;
        private int baseAdr;
        private int id;

        public ScriptBytecode(byte[] data, int id, int baseAdr=0)
        {
            this.data = data;
            this.baseAdr = baseAdr;
            this.id = id;

            pointers_in_script = new List<int>();
            pointers = new List<int>();
            pointers_to_string = new Dictionary<int, string>();

            numSubScripts = 0;
            while (data[numSubScripts * 9] <= 0x80)
            {
                numSubScripts++;
            }
        }

        public int GetNumberOfSubScripts()
        {
            return numSubScripts;
        }

        public List<int> GetStringPointers()
        {
            return pointers;
        }

        public Dictionary<int, string> GetLookupDictionary()
        {
            return pointers_to_string;
        }

        public void Parse(int subID)
        {
            var startAdr = subID*9;
            var idx = data[startAdr + 4] << 8 | data[startAdr + 3]; //start of actual bytecode

            Parse_(idx, subID);
        }

        public void Parse_(int idx, int subID=0)
        {
            var maxIdx = data.Length;
            var numBytesParsed = 0;
            while (idx < maxIdx)
            {
                if (data[idx] > 0x43)
                {
                    idx += 1;
                    continue;
                }

                if (numBytesParsed > maxIdx)
                {
                    return;
                }

                switch (data[idx])
                {
                    case 0x00: //End
                    case 0x16: //End
                    case 0x1D: //End
                    case 0x2C: //??
                    case 0x2D: //End
                    case 0x2E: //End
                    case 0x2F: //End
                    case 0x35: //End
                    case 0x36: //End
                        return;

                    case 0x2A: //Jump to new script?
                        return;

                    case 0x01: //start text
                    {
                        int strPtr = (data[idx + 1] | data[idx + 2] << 8);
                        if (strPtr < maxIdx)
                        {
                            maxIdx = strPtr;
                        }
                        pointers_in_script.Add(idx + 1);
                        pointers.Add(strPtr);

                        if (!pointers_to_string.ContainsKey(baseAdr + strPtr))
                        {
                            var str = String.Format("id_{0}_subid_{1}_string{2}", id, subID, pointers.Count);
                            pointers_to_string.Add(baseAdr + strPtr, str);
                        }

                        idx += 3;
                        numBytesParsed += 3;
                        break;
                    }
                    case 0x22: //start text (hide textbox)
                    {
                        //TODO: Are these strings ever actually used?
                        /*int strPtr = (data[idx + 1] | data[idx + 2] << 8);
                        if (strPtr < maxIdx)
                        {
                            maxIdx = strPtr;
                        }
                        pointers_in_script.Add(idx+1);
                        pointers.Add(strPtr);

                        if (!pointers_to_string.ContainsKey(baseAdr+strPtr))
                        {
                            var str = String.Format("id_{0}_subid_{1}_cmd0x{2:X2}_string{3}", id, subID, data[idx], pointers.Count);
                            pointers_to_string.Add(baseAdr + strPtr, str);
                        }*/

                        idx += 3;
                        numBytesParsed += 3;
                        break;
                    }

                    case 0x0A: //4 way CALL based on current direction of player
                    {
                        //int flag = data[(subID*9) + 2] & 3;
                        int unk = data[idx + 1];
                        int up_call = data[idx + 2] | (data[idx + 3] << 8);
                        int down_call = data[idx + 4] | (data[idx + 5] << 8);
                        int left_call = data[idx + 6] | (data[idx + 7] << 8);
                        int right_call = data[idx + 8] | (data[idx + 9] << 8);

                        if (up_call > idx) Parse_(up_call, subID);
                        if (down_call > idx) Parse_(down_call, subID);
                        if (left_call > idx) Parse_(left_call, subID);
                        if (right_call > idx) Parse_(right_call, subID);

                        idx += 10;
                        numBytesParsed += 10;
                        return;
                    }

                    case 0x10:
                    {
                        idx++;
                        numBytesParsed++;
                        while (data[idx] == 5)
                        {
                            idx += 6;
                            numBytesParsed += 6;
                        }
                        while (data[idx] == 6)
                        {
                            idx += 3;
                            numBytesParsed += 3;
                        }
                        break;
                    }

                    case 0x05:
                    case 0x0C:
                    case 0x23:
                        idx += 6;
                        numBytesParsed += 6;
                        break;

                    case 0x03: //Jump?
                        {
                            int jumpTarget = (data[idx + 1] | data[idx + 2] << 8);
                            if(jumpTarget > idx)
                                idx = jumpTarget;
                            else
                            {
                                return;
                            }
                            numBytesParsed += 3;
                            break;
                        }

                    case 0x04:
                    case 0x06:
                    case 0x08: //Fade palette maybe?
                    case 0x0B:
                    case 0x18:
                    case 0x19:
                    case 0x1C:
                    case 0x1F:
                    case 0x30:
                    case 0x32:
                    case 0x33:
                    case 0x37:
                    case 0x40:
                        idx += 3;
                        numBytesParsed += 3;
                        break;

                    case 0x07:
                        idx += 5;
                        numBytesParsed += 5;
                        break;

                    case 0x31:
                    {
                        int jumpTarget = (data[idx + 3] | data[idx + 4] << 8);
                        Parse_(jumpTarget, subID);
                        idx += 5;
                        numBytesParsed += 5;
                        break;
                    }

                    case 0x34:
                    {
                        int jumpTarget = (data[idx + 2] | data[idx + 3] << 8);
                        Parse_(jumpTarget, subID);
                        idx += 4;
                        numBytesParsed += 4;
                        break;
                    }

                    case 0x0D:
                    case 0x2B:
                        idx += 4;
                        numBytesParsed += 4;
                        break;

                    case 0x11:
                    case 0x17:
                    case 0x24:
                    case 0x25: //Display medal name select screen
                    case 0x39:
                    case 0x3C:
                    case 0x3F:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                        idx += 1;
                        numBytesParsed += 1;
                        break;

                    case 0x02:
                    case 0x09:
                    case 0x0E:
                    case 0x0F:
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    case 0x15:
                    case 0x1A:
                    case 0x1B:
                    case 0x1E:
                    case 0x20:
                    case 0x21:
                    case 0x26: //Display medarot name select screen
                    case 0x27:
                    case 0x28:
                    case 0x29:
                    case 0x38:
                    case 0x3A:
                    case 0x3B:
                    case 0x3D: //Start Robattle (second byte is battle id?)
                    case 0x3E:
                        idx += 2;
                        numBytesParsed += 2;
                        break;

                    default:
                        Debug.WriteLine("Unknown Opcode 0x" + Convert.ToString(data[idx], 16) + " / " + data[idx]);
                        return;
                }
            }
        }

        public string ParseDebug(int subID, int indent = 0)
        {
            var startAdr = subID * 9;
            var idx = data[startAdr + 4] << 8 | data[startAdr + 3]; //start of actual bytecode

            return ParseDebug_(idx, subID, indent);
        }

        private string DebugFormat(int idx, int indent, string str, params object[] args)
        {
            StringBuilder sb = new StringBuilder();

            //sb.AppendFormat("{0:X8}:\t", idx - baseAdr);
            sb.AppendFormat("{0:X8}:\t", idx);

            for (int ind = 0; ind < indent; ind++)
                sb.AppendFormat("\t");

            sb.AppendFormat(str, args);

            return sb.ToString();
        }

        public string ParseDebug_(int idx, int subID, int indent=0)
        {
            var sb = new StringBuilder();
            var maxIdx = data.Length;
            var finished = false;
            var baseIdx = idx;
            while (idx < maxIdx && !finished)
            {
                var size = 0;
                var handled = false;

                if (data[idx] > 0x43)
                {
                    idx += 1;
                    continue;
                }
                //sb.AppendFormat("{0:X8}:\t", baseAdr+idx);

                var curSbLen = sb.Length;

                switch (data[idx])
                {
                    case 0x00: //End
                    case 0x1D: //End
                    case 0x2C: //??
                    case 0x2D: //End
                    case 0x2E: //End
                    case 0x2F: //End
                    case 0x36: //End
                        handled = true;
                        finished = true;
                        break;
                    case 0x2A: //Jump to new script?
                        handled = true;
                        finished = true;
                        break;

                    case 0x01: //start text
                    case 0x22: //start text (hide textbox)
                        {
                            int strPtr = (data[idx + 1] | data[idx + 2] << 8);
                            sb.Append(DebugFormat(baseAdr + idx, indent,"<TEXT:{0:X4}>", strPtr));
                            if (strPtr < maxIdx)
                            {
                                maxIdx = strPtr;
                            }
                            size = 3;
                            handled = true;
                            break;
                        }
                    case 0x02: //Delay
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<DELAY:{0:X2}>", data[idx + 1]));
                        size = 2;
                        handled = true;
                        break;

                    case 0x03: //Jump?
                    {
                        int jumpTarget = (data[idx + 1] | data[idx + 2] << 8);
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<JUMP:{0:X4}>", jumpTarget)).AppendLine();
                        idx = jumpTarget;
                        size = 3;
                        handled = true;
                        finished = true;
                        break;
                    }

                    case 0x05:
                    {
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<MOVE_OBJ:{0:X2},X={1:X2},Y={2:X2},SPEED={3:X2},UNK1={4:X2}>",
                            data[idx + 1],
                            data[idx + 2],
                            data[idx + 3],
                            data[idx + 4],
                            data[idx + 5]));
                        handled = true;
                        size = 6;
                        break;
                    }

                    case 0x07:
                        sb.Append(DebugFormat(baseAdr + idx, indent,"<CREATE_OBJ:{0:X2},X={1:X2},Y={2:X2},UNK={3:X2}>",
                            data[idx + 1], data[idx + 2], data[idx + 3], data[idx + 4]));
                        size = 5;
                        handled = true;
                        break;

                    case 0x0A: //4 way CALL based on current direction of player
                    {
                        //int flag = data[(subID*9) + 2] & 3;
                        int unk = data[idx + 1];
                        int up_call = data[idx + 2] | (data[idx + 3] << 8);
                        int down_call = data[idx + 4] | (data[idx + 5] << 8);
                        int left_call = data[idx + 6] | (data[idx + 7] << 8);
                        int right_call = data[idx + 8] | (data[idx + 9] << 8);

                        if (up_call > idx)
                        {
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<IF_PLAYER_DIRECTION_UP>\r\n"));
                            sb.Append(ParseDebug_(up_call, subID, indent + 1));
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<END_IF>\r\n"));
                        }

                        if (down_call > idx)
                        {
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<IF_PLAYER_DIRECTION_DOWN>\r\n"));
                            sb.Append(ParseDebug_(down_call, subID, indent + 1));
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<END_IF>\r\n"));
                        }

                        if (left_call > idx)
                        {
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<IF_PLAYER_DIRECTION_LEFT>\r\n"));
                            sb.Append(ParseDebug_(left_call, subID, indent + 1));
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<END_IF>\r\n"));
                        }

                        if (right_call > idx)
                        {
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<IF_PLAYER_DIRECTION_RIGHT>\r\n"));
                            sb.Append(ParseDebug_(right_call, subID, indent + 1));
                            sb.Append(DebugFormat(baseAdr + idx, indent, "<END_IF>\r\n"));
                        }

                        size = 10;
                        handled = true;
                        finished = true; //Is this correct?
                        break;
                    }

                    case 0x0E: //Show Object
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<SHOW_OBJ:{0:X2}>", data[idx + 1]));
                        size = 2;
                        handled = true;
                        break;

                    case 0x0F: //Hide Object
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<HIDE_OBJ:{0:X2}>", data[idx + 1]));
                        size = 2;
                        handled = true;
                        break;

                    case 0x10:
                    {
                        size = 1;
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<SET_SUB_ROUTINE>"));
                        while (data[idx + size] == 5)
                        {
                            sb.AppendFormat("<MOVE_OBJ:{0:X2},X={1:X2},Y={2:X2},SPEED={3:X2},UNK1={4:X2}>", 
                                data[idx + size + 1],
                                data[idx + size + 2],
                                data[idx + size + 3],
                                data[idx + size + 4],
                                data[idx + size + 5]);
                            size += 6;
                        }
                        while (data[idx + size] == 6)
                        {
                            sb.AppendFormat("<{0:X2},{1:X2},{2:X2}>",
                                data[idx + size],
                                data[idx + size + 1],
                                data[idx + size + 2]);
                            size += 3;
                        }
                        handled = true;
                        break;
                    }

                    case 0x11:
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<CALL_SUB_ROUTINE>"));
                        size = 1;
                        handled = true;
                        break;

                    case 0x20: //Play Sound Effect
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<PLAY_SFX:{0:X2}>", data[idx + 1]));
                        size = 2;
                        handled = true;
                        break;

                    case 0x25: //Display medal name select screen
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<MEDAL_NAME_SELECT>"));
                        size = 1;
                        handled = true;
                        break;

                    case 0x26: //Display medarot name select screen
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<MEDAROT_NAME_SELECT:{0:X2}>", data[idx + 1]));
                        size = 2;
                        handled = true;
                        break;

                    case 0x0C:
                    case 0x23:
                        size = 6;
                        break;

                    case 0x16:
                    case 0x17:
                    case 0x24:
                    case 0x35:
                    case 0x39:
                    case 0x3C:
                    case 0x3F:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                        size = 1;
                        break;

                    case 0x0D:
                    case 0x2B:
                    case 0x34:
                        size = 4;
                        break;

                    case 0x31:
                        size = 5;
                        break;

                    case 0x04:
                    case 0x06:
                    case 0x08: //Pan camera
                    case 0x0B:
                    case 0x18:
                    case 0x19:
                    case 0x1C:
                    case 0x1F:
                    case 0x30:
                    case 0x32:
                    case 0x33:
                    case 0x37:
                    case 0x40:
                        size = 3;
                        break;

                    case 0x09:
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    case 0x15:
                    case 0x1A:
                    case 0x1B:
                    case 0x1E:
                    case 0x21:
                    case 0x27:
                    case 0x28:
                    case 0x29:
                    case 0x38:
                    case 0x3A:
                    case 0x3B:
                    case 0x3E:
                        size = 2;
                        break;
                    case 0x3D: //Start Robattle (second byte is battle id?)
                        sb.Append(DebugFormat(baseAdr + idx, indent, "<START_ROBATTLE:{0:X2}>", data[idx + 1]));
                        size = 2;
                        handled = true;
                        break;
                    default:
                        sb.Append(DebugFormat(baseAdr + idx, indent, "Unknown Opcode 0x{0:X2}", data[idx]));
                        finished = true;
                        break;
                }

                if (!handled)
                {
                    sb.Append(DebugFormat(baseAdr + idx, indent, ""));
                    for (int i = idx; i < (idx+size) && i < data.Length; i++)
                        sb.AppendFormat("<{0:X2}>", data[i]);
                }

                if(!finished)
                    sb.AppendLine();

                idx += size;
            }
            return sb.ToString();
        }
    }
}

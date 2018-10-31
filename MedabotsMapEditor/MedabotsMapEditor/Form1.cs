using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Nintenlord.GBA;

namespace MedabotsMapEditor
{
    public partial class Form1 : Form
    {
        private GBAROM ROM;
        private Color[] PALfilePalette;
        private Color[] GrayScalePalette;
        private Color[] WhiteToBlackPalette;
        private byte[] rawGraphics; //raw tileset graphics
        private byte[] rawTSA;
        private byte[] rawTilemap;
        private Bitmap mapBitmap;
        private Graphics mapGraphics;
        private Bitmap[] overworldTiles;

        //Medarot Navi - Kabuto (Japan)
        const int Level_Info = 0x085D617C - 0x08000000;

        const int NumberOfMaps = 75;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        public void SaveBitmap(string path)
        {
            ImageFormat im;
            switch (Path.GetExtension(path).ToUpper())
            {
                case ".PNG":
                    im = ImageFormat.Png;
                    break;
                case ".BMP":
                    im = ImageFormat.Bmp;
                    break;
                case ".GIF":
                    im = ImageFormat.Gif;
                    break;
                default:
                    MessageBox.Show("Wrong image format.");
                    return;
            }
            mapBitmap.Save(path, im);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        public void LoadPalFile(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));
            br.BaseStream.Position = 0x18;
            byte[] data = new byte[br.BaseStream.Length - br.BaseStream.Position];
            data = br.ReadBytes(data.Length);
            br.Close();

            PALfilePalette = new Color[data.Length / 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                PALfilePalette[i / 4] = Color.FromArgb(data[i], data[i + 1], data[i + 2]);
            }
        }

        public void LoadRawPalFile(int offset)
        {
            byte[] rawPal = ROM.GetData(offset, 0x200);

            PALfilePalette = new Color[0x100];

            Color[] temp = GBAGraphics.toPalette(rawPal, 0, 0x100);
            temp.CopyTo(PALfilePalette, 0x00);

            for (int i = 0; i < 0x100; i+=16)
            {
                PALfilePalette[i] = Color.Transparent;
            }
        }

        private void DrawTile(int tileIndex, int x, int y, Graphics g)
        {
            int length = 0x20;
            byte[] graphics = new byte[0x20];
            GraphicsMode mode = GraphicsMode.Tile4bit;
            ushort tile = (ushort)tileIndex;
            int tileNum = Math.Max(0,(tile & 0x3FF)-0x180);
            int tileAdr = Math.Min(tileNum * 0x20, rawGraphics.Length - 0x20);

            if((tileNum*0x20) <= tileAdr)
                Array.Copy(rawGraphics, tileAdr, graphics, 0, 0x20);

            int palIndex = tile >> 12;
            Color[] palette = new Color[16];
            for (int j = 0; j < palette.Length; j++)
                palette[j] = PALfilePalette[j + 16 * palIndex];

            int empty;

            Bitmap tileBitmap = GBAGraphics.ToBitmap(graphics, length, 0, palette, 8, mode, out empty);

            int flipX = (tile >> 10) & 1;
            int flipY = (tile >> 11) & 1;

            if (flipX != 0)
            {
                tileBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            if (flipY != 0)
            {
                tileBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            //Debug.WriteLine(String.Format("X: {0} Y: {1}", x*8, y*8));
            g.DrawImage(tileBitmap, new Point(x * 8, y * 8));
        }

        private void DrawMap(int id)
        {
            int mapinfo_adr = Level_Info + (0x20*id);

            int tilemap_adr = mapinfo_adr;
            uint tilemap = ROM.GetU32(tilemap_adr) - 0x08000000;

            int tileset_adr = mapinfo_adr + 4;
            uint tileset = ROM.GetU32(tileset_adr) - 0x08000000;

            int pal_adr = mapinfo_adr + 8;
            uint pal = ROM.GetU32(pal_adr) - 0x08000000;

            byte[] tilemap_data = ROM.DecompressMalias2CompressedData((int)tilemap);

            byte[] layer1_data = new byte[0x2000];
            byte[] layer2_data = new byte[0x2000];
            byte[] layer3_data = new byte[0x2000];

            Array.Copy(tilemap_data, 0x0000, layer1_data, 0, 0x2000);
            Array.Copy(tilemap_data, 0x2000, layer2_data, 0, 0x2000);
            Array.Copy(tilemap_data, 0x4000, layer3_data, 0, 0x2000);

            rawGraphics = ROM.DecompressMalias2CompressedData((int)tileset);
            LoadRawPalFile((int)pal);

            int mapX_ = ROM.GetS16(mapinfo_adr + 12);
            int mapY_ = ROM.GetS16(mapinfo_adr + 14);
            uint unk1 = ROM.GetU32(mapinfo_adr + 16);
            uint unk2 = ROM.GetU32(mapinfo_adr + 20);
            uint unk3 = ROM.GetU32(mapinfo_adr + 24);
            uint unk4 = ROM.GetU32(mapinfo_adr + 28);

            int mapX = Math.Max(mapX_, mapY_);
            int mapY = mapY_;//Math.Min(mapX_, mapY_);

            if (checkBox1.Checked) //Double map size flag?
            {
                mapX *= 2;
                mapY *= 2;
            }

            mapBitmap = new Bitmap(mapX * 8, mapY * 8);
            mapGraphics = Graphics.FromImage(mapBitmap);

            //File.WriteAllBytes("rawGraphics.bin", rawGraphics);
            //File.WriteAllBytes("TileMap.bin", tilemap_data);
            //File.WriteAllBytes("Layer1.bin", layer1_data);
            //File.WriteAllBytes("Layer2.bin", layer2_data);
            //File.WriteAllBytes("Layer3.bin", layer3_data);

            for (int y = 0; y < mapY; y++)
            {
                for (int x = 0; x < mapX; x++)
                {
                    int layer_idx = ((x + (y * mapX))*2);
                    //Debug.WriteLine(String.Format("X: {0} Y: {1}", x, y));
                    int index = (int)BitConverter.ToUInt16(layer3_data, layer_idx);
                    DrawTile(index, x, y, mapGraphics);
                    //drawTile(location, mapTiles[x + (y * 32)], x, y, mapGraphics);
                }
            }

            for (int y = 0; y < mapY; y++)
            {
                for (int x = 0; x < mapX; x++)
                {
                    int layer_idx = ((x + (y * mapX)) * 2);
                    int index = (int)BitConverter.ToUInt16(layer2_data, layer_idx);
                    DrawTile(index, x, y, mapGraphics);
                    //drawTile(location, mapTiles[x + (y * 32)], x, y, mapGraphics);
                }
            }

            for (int y = 0; y < mapY; y++)
            {
                for (int x = 0; x < mapX; x++)
                {
                    int layer_idx = ((x + (y * mapX)) * 2);
                    int index = (int)BitConverter.ToUInt16(layer1_data, layer_idx);
                    DrawTile(index, x, y, mapGraphics);
                    //drawTile(location, mapTiles[x + (y * 32)], x, y, mapGraphics);
                }
            }

            DrawTileMap();

            if (checkBox1.Checked) //Double map size flag?
            {
                mapX /= 2;
                mapY /= 2;
            }

            pictureBox1.Image = mapBitmap.Clone(new Rectangle(0, 0, mapX * 8, mapY * 8), mapBitmap.PixelFormat);
        }

        private void DrawTileMap()
        {
            int tile_id = 0;
            var mapX = 31;
            var mapY = ((rawGraphics.Length/0x20)/mapX)+1;

            var mapBitmap1 = new Bitmap(mapX * 8, mapY * 8);
            var mapGraphics1 = Graphics.FromImage(mapBitmap1);

            for (int y = 0; y < mapY; y++)
            {
                for (int x = 0; x < mapX; x++)
                {
                    DrawTile(tile_id++, x, y, mapGraphics1);
                    //drawTile(location, mapTiles[x + (y * 32)], x, y, mapGraphics);
                }
            }

            pictureBox2.Image = mapBitmap1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(numericUpDown1.Value);
            DrawMap(id);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ROM = new GBAROM();
            //mapBitmap = new Bitmap(64 * 8, 64 * 8);
            //mapGraphics = Graphics.FromImage(mapBitmap);
            ROM.OpenROM("input.gba");

            GrayScalePalette = new Color[0x100];
            WhiteToBlackPalette = new Color[0x100];

            for (int x = 0; x < 0x10; x++)
            {
                for (int y = 0; y < 0x10; y++)
                {
                    int value = x * 0x10 + y;
                    int value2 = ((0x10 - x) * 0x10 + (0x10 - y)) & 0xFF;

                    GrayScalePalette[x + y * 0x10] = Color.FromArgb(value, value, value);

                    WhiteToBlackPalette[x + y * 0x10] = Color.FromArgb(value2, value2, value2);
                }
            }

            numericUpDown1.Maximum = NumberOfMaps - 1;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(numericUpDown1.Value);
            DrawMap(id);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("maps");
            for (var i = 0; i < NumberOfMaps; i++)
            {
                DrawMap(i);
                pictureBox1.Image.Save("./maps/"+i+".png",ImageFormat.Png);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("tilesets");
            for (var i = 0; i < NumberOfMaps; i++)
            {
                DrawMap(i);
                pictureBox2.Image.Save("./tilesets/" + i + ".png", ImageFormat.Png);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("maps");
            var id = Convert.ToInt32(numericUpDown1.Value);
            DrawMap(id);
            pictureBox1.Image.Save("./maps/" + id + ".png", ImageFormat.Png);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("decompressed");

            var offsets = ROM.ScanForMalias2CompressedData(0,ROM.Length,0x1000000,0,0);

            for (int i = 0; i < offsets.Length; i++)
            {
                var data = ROM.DecompressMalias2CompressedData(offsets[i]);
                if (data != null)
                    File.WriteAllBytes(String.Format("./decompressed/{0:X8}.bin", offsets[i]), data);
            }

        }
    }
}
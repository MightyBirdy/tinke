﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fonts
{
    public partial class MapChar : Form
    {
        List<sNFTR.PAMC> maps;


        public MapChar(List<sNFTR.PAMC> maps)
        {
            InitializeComponent();
            this.maps = maps;
            lblTotalSec.Text = "of " + (maps.Count - 1).ToString();
            numericSection.Maximum = maps.Count - 1;
            MapInfo();
        }

        private void MapInfo()
        {
            sNFTR.PAMC map = maps[(int)numericSection.Value];

            numericFirstChar.Value = map.first_char;
            numericLastChar.Value = map.last_char;
            numericType.Value = map.type_section;
            dataGridMapInfo.Rows.Clear();

            switch (map.type_section)
            {
                case 0:
                    sNFTR.PAMC.Type0 type0 = (sNFTR.PAMC.Type0)map.info;
                    dataGridMapInfo.Rows.Add(type0.fist_char_code.ToString(), "0");
                    break;

                case 1:
                    sNFTR.PAMC.Type1 type1 = (sNFTR.PAMC.Type1)map.info;
                    for (int i = 0; i < type1.char_code.Length; i++)
                        dataGridMapInfo.Rows.Add(type1.char_code[i].ToString(), i.ToString());
                    break;

                case 2:
                    sNFTR.PAMC.Type2 type2 = (sNFTR.PAMC.Type2)map.info;
                    for (int i = 0; i < type2.num_chars; i++)
                        dataGridMapInfo.Rows.Add(type2.chars[i].ToString(), type2.chars_code[i].ToString());
                    break;
            }
        }

        public List<sNFTR.PAMC> Maps
        {
            get
            {
                Recalculate_Size();
                return maps;
            }
        }
        private void Recalculate_Size()
        {
            for (int i = 0; i < maps.Count; i++)
            {
                sNFTR.PAMC map = maps[i];
                uint size = 0x14;

                switch (map.type_section)
                {
                    case 0:
                        size += 2;
                        break;

                    case 1:
                        sNFTR.PAMC.Type1 type1 = (sNFTR.PAMC.Type1)map.info;
                        size += (uint)type1.char_code.Length * 2;
                        break;

                    case 2:
                        sNFTR.PAMC.Type2 type2 = (sNFTR.PAMC.Type2)map.info;
                        size += 2;
                        size += (uint)type2.chars_code.Length * 2;
                        size += (uint)type2.chars.Length * 2;
                        break;
                }
                size += 2;  // 0x00 terminator

                map.block_size = size;
                maps[i] = map;
            }
        }

        private void MapChar_Resize(object sender, EventArgs e)
        {
            groupBox1.Height = this.Height - 80;
            btnSave.Location = new Point(this.Width - 91, 7);
            dataGridMapInfo.Height = groupBox1.Height - 82;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void numericSection_ValueChanged(object sender, EventArgs e)
        {
            MapInfo();
            numericType.ReadOnly = true;
        }

        private void btnRemoveSec_Click(object sender, EventArgs e)
        {
            maps.RemoveAt((int)numericSection.Value);
            numericSection.Maximum = maps.Count - 1;

            if (maps.Count == 0)
                MessageBox.Show("There aren't more sections.");
            else
                numericSection.Value = 0;

            lblTotalSec.Text = "of " + (maps.Count - 1).ToString();
        }
        private void btnAddSect_Click(object sender, EventArgs e)
        {
            sNFTR.PAMC map = new sNFTR.PAMC();
            map.type = new char[] { 'P', 'A', 'M', 'C' };
            map.first_char = 0x0000;
            map.last_char = 0xFFFF;
            map.type_section = 2;
            numericType.ReadOnly = false;
            sNFTR.PAMC.Type2 type2 = new sNFTR.PAMC.Type2();
            type2.num_chars = 0;
            type2.chars = new ushort[0];
            type2.chars_code = new ushort[0];
            map.info = type2;
            maps.Add(map);

            lblTotalSec.Text = "of " + (maps.Count - 1).ToString();
            numericSection.Maximum = maps.Count - 1;
            numericSection.Value = maps.Count - 1;
        }


        private void numericChar_ValueChanged(object sender, EventArgs e)
        {
            sNFTR.PAMC pamc = maps[(int)numericSection.Value];
            pamc.first_char = (ushort)numericFirstChar.Value;
            pamc.last_char = (ushort)numericLastChar.Value;
            maps[(int)numericSection.Value] = pamc;
        }

        private void dataGridMapInfo_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            sNFTR.PAMC map = maps[(int)numericSection.Value];

            if (map.type_section == 2)
            {
                sNFTR.PAMC.Type2 type2 = (sNFTR.PAMC.Type2)map.info;
                if (e.ColumnIndex == 0) 
                {
                    if ((e.RowIndex + 1) > type2.chars.Length)  
                    {
                        List<ushort> values = new List<ushort>();

                        values.AddRange(type2.chars);
                        values.Add(Convert.ToUInt16(dataGridMapInfo.Rows[e.RowIndex].Cells[0].Value));
                        type2.chars = values.ToArray();

                        values.Clear();
                        values.AddRange(type2.chars_code);
                        values.Add(0);
                        type2.chars_code = values.ToArray();

                        type2.num_chars++;
                    }
                    else if (dataGridMapInfo.RowCount < type2.chars.Length)
                    {
                        List<ushort> values = new List<ushort>();

                        values.AddRange(type2.chars);
                        values.RemoveAt(e.RowIndex);
                        type2.chars = values.ToArray();

                        values.Clear();
                        values.AddRange(type2.chars_code);
                        values.RemoveAt(e.RowIndex);
                        type2.chars_code = values.ToArray();

                        type2.num_chars--;
                    }
                    else
                        type2.chars[e.RowIndex] = Convert.ToUInt16(dataGridMapInfo.Rows[e.RowIndex].Cells[0].Value);

                }
                else if (e.ColumnIndex == 1)  
                {
                    if ((e.RowIndex + 1) > type2.chars_code.Length)
                    {
                        List<ushort> values = new List<ushort>();

                        values.AddRange(type2.chars_code);
                        values.Add(Convert.ToUInt16(dataGridMapInfo.Rows[e.RowIndex].Cells[1].Value));
                        type2.chars_code = values.ToArray();

                        values.Clear();
                        values.AddRange(type2.chars);
                        values.Add(0);
                        type2.chars = values.ToArray();

                        type2.num_chars++;
                    }
                    else if (dataGridMapInfo.RowCount < type2.chars.Length)
                    {
                        List<ushort> values = new List<ushort>();

                        values.AddRange(type2.chars_code);
                        values.RemoveAt(e.RowIndex);
                        type2.chars_code = values.ToArray();

                        values.Clear();
                        values.AddRange(type2.chars);
                        values.RemoveAt(e.RowIndex);
                        type2.chars = values.ToArray();

                        type2.num_chars--;
                    }
                    else
                        type2.chars_code[e.RowIndex] = Convert.ToUInt16(dataGridMapInfo.Rows[e.RowIndex].Cells[1].Value);
                }
                map.info = type2;
            }
            else if (map.type_section == 1)
            {
                sNFTR.PAMC.Type1 type1 = (sNFTR.PAMC.Type1)map.info;

                if (e.ColumnIndex == 0)
                {
                    if ((e.RowIndex + 1) > type1.char_code.Length)
                    {
                        List<ushort> values = new List<ushort>();
                        values.AddRange(type1.char_code);
                        values.Add(Convert.ToUInt16(dataGridMapInfo.Rows[e.RowIndex].Cells[0].Value));
                        type1.char_code = values.ToArray();
                    }
                    else if (dataGridMapInfo.RowCount < type1.char_code.Length)
                    {
                        List<ushort> values = new List<ushort>();
                        values.AddRange(type1.char_code);
                        values.RemoveAt(e.RowIndex);
                        type1.char_code = values.ToArray();
                    }
                    else
                        type1.char_code[e.RowIndex] = Convert.ToUInt16(dataGridMapInfo.Rows[e.RowIndex].Cells[0].Value);
                }

                map.info = type1;
            }
            else
            {
                sNFTR.PAMC.Type0 type0 = (sNFTR.PAMC.Type0)map.info;

                if (e.ColumnIndex == 0 && e.RowIndex == 0 && dataGridMapInfo.RowCount > 0)
                {
                    type0.fist_char_code = Convert.ToUInt16(dataGridMapInfo.Rows[e.RowIndex].Cells[0].Value);
                }

                map.info = type0;
            }

            maps[(int)numericSection.Value] = map;
        }

        private void numericType_ValueChanged(object sender, EventArgs e)
        {
            sNFTR.PAMC map = maps[(int)numericSection.Value];
            map.type_section = (uint)numericType.Value;
            maps[(int)numericSection.Value] = map;
        }


    }
}

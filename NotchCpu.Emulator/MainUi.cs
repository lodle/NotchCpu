using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace NotchCpu.Emulator
{
    public partial class MainUi : Form
    {
        Thread _Thread;

        Registers _Reg;

        public MainUi()
        {
            InitializeComponent();
        }

        private void MainUi_Load(object sender, EventArgs e)
        {
            for (int x = 0; x < 32; x++)
            {
                var col = new DataGridViewTextBoxColumn();

                col.HeaderText = (x + 1).ToString();
                col.MaxInputLength = 1;
                col.MinimumWidth = 15;
                col.Name = "C" + (x+1).ToString();
                col.ReadOnly = true;
                col.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                col.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                col.Width = 15;

                TextGridView.Columns.Add(col);
            }

            for (int x = 0; x < 12; x++)
            {
                var row = new DataGridViewRow();

                for (int y = 0; y < 32; y++)
                {
                    var cell = new DataGridViewTextBoxCell();
                    
                    cell.Style.ForeColor = GetColor((char)0);
                    cell.Style.BackColor = GetColor((char)0);
                    row.Cells.Add(cell);
                }

                TextGridView.Rows.Add(row);
            }
        }

        delegate void ConsoleTextHandler(ushort x, ushort y, ushort value);
        public void SetConsoleText(ushort x, ushort y, ushort val)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ConsoleTextHandler(SetConsoleText), new object[] { x, y, val });
            }
            else
            {
                if (y * x >= 0x180)
                    return;

                var cell = TextGridView.Rows[x].Cells[y];

                if (val != 0)
                    cell.Value = "" + (Char)(val & 0x00FF);
                else
                    cell.Value = "";

                cell.Style.ForeColor = GetColor((char)((val & 0xF000) >> 12));
                cell.Style.BackColor = GetColor((char)((val & 0x0F00) >> 8));
            }
        }

        private Color GetColor(char p)
        {
            var b = (p & 0x01) > 0 ? 255 : 0;
            var g = (p & 0x02) > 0 ? 255 : 0;
            var r = (p & 0x04) > 0 ? 255 : 0;

            return Color.FromArgb(r, g, b);
        }

        private void ClearConsoleText()
        {
            for (ushort x = 0; x < 12; x++)
            {
                for (ushort y = 0; y < 32; y++)
                    SetConsoleText(x, y, 0);
            }
        }

        private void ButStartToggle_Click(object sender, EventArgs e)
        {
            if (_Thread != null)
            {
                Log("Canceling Emulator");
                _Thread.Abort();
            }
            else
            {
                Log("Starting Emulator");

                ButStartToggle.Text = "Stop";
                ClearConsoleText();

                _Thread = new Thread(() =>
                {
                    var start = DateTime.Now;

                    _Reg = new Registers();

                    _Reg.MemUpdateEvent += new MemUpdateHandler(OnMemUpdate);

                    try
                    {
                        Emu.Run(_Reg);
                    }
                    catch (Exception ex)
                    {
                        Log("Emulator threw exception: " + ex.Message);
                    }

                    var time = DateTime.Now - start;

                    Log("Done in " + time.ToString());
                    FinishedEmulation();
                });

                _Thread.Start();
            }
        }

        void OnMemUpdate(ushort loc, ushort value)
        {
            if (loc < 0x8000 || loc > 0x8180)
                return;

            var x = (loc - 0x8000)/32;
            var y = (loc - 0x8000)%32;

            SetConsoleText((ushort)x, (ushort)y, value);
        }

        delegate void FinishedHandler();
        private void FinishedEmulation()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new FinishedHandler(FinishedEmulation));
            }
            else
            {
                _Thread = null;

                ClearConsoleText();
                ButStartToggle.Text = "Start";
            }
        }

        delegate void LogHandler(String msg);
        private void Log(String msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new LogHandler(Log), new object[] { msg });
            }
            else
            {
                TBLog.AppendText(msg + Environment.NewLine);
            }
        }

        internal void Log(string p, params object[] parm)
        {
            Log(String.Format(p, parm));
        }
    }
}

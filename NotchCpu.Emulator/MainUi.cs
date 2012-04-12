using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace NotchCpu.Emulator
{
    public partial class MainUi : Form
    {
        Thread _Thread;

        Registers _Reg;

        String[] _RegNames = new string[] {"A", "B", "C", "X", "Y", "Z", "I", "J", "PC", "SP", "O" };
        double[] _Speeds = new double[] 
        { 
            0.5,    // 200 khz
            1.0,    // 100 khz
            2.0,    // 50 khz
            10.0,   // 10 khz
            100.0,  // 1 khz
            200.0,  // 500 hz 
            10000.0 // 10 hz
        };

        char _TickCount;
        long _AvgTicks;
        long _AvgMs;

        Emu _Emu;

        int _LastReg = 0;
        private bool _IgnoreHightlight;

        private bool ShowSelect;

        public MainUi()
        {
            InitializeComponent();

            foreach (var reg in _RegNames)
            {
                var item = new System.Windows.Forms.ListViewItem(new string[] { reg, "0x0000" }, -1);
                this.listView1.Items.Add(item);
            }

            CBSpeed.SelectedIndex = _Speeds.Length-1;
            Emu.SpeedMultiplier = _Speeds[CBSpeed.SelectedIndex];
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
                    cell.Style.Font = new Font(new FontFamily("Courier New"), 11, FontStyle.Regular);

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

                if (ShowSelect)
                {
                    TextGridView.ClearSelection();
                    cell.Selected = true;
                }
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

            TextGridView.ClearSelection();
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

                if (_Emu == null)
                    InitProg();

                _Thread = new Thread(() =>
                {
                    var start = DateTime.Now;

                    try
                    {
                        _Emu.RunProgram();

                        var time = DateTime.Now - start;

                        Log("Done in " + time.ToString());
                        FinishedEmulation();
                    }
                    catch (Exception ex)
                    {
                        Log("Emulator threw exception: " + ex.Message);
                        FinishedEmulation();
                    }
                });

                _Thread.Start();
            }
        }

        private void InitProg()
        {
            _Reg = new Registers();
            _Emu = new Emu(_Reg);

            _Emu.StepCompleteEvent += new StepCompleteHandler(OnStepComplete);
            _Reg.MemUpdateEvent += new MemUpdateHandler(OnMemUpdate);

            _LastReg = 0;

            TBLog.Text = "";

            OnSpeedChanged(null, null);
            ClearConsoleText();

            if (_LastReg != -1)
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    item.BackColor = Color.White;
                    item.SubItems[1].Text = String.Format("0x{0,4:X4}", 0);
                }
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

        void OnRegUpdate(ushort loc, ushort value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MemUpdateHandler(OnRegUpdate), new object[] { loc, value });
            }
            else
            {
                if (_LastReg == -1)
                {
                    foreach (ListViewItem item in listView1.Items)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", 0);
                }

                if (!_IgnoreHightlight)
                {
                    if (_LastReg != -1)
                        listView1.Items[_LastReg].BackColor = Color.White;

                    listView1.Items[loc].BackColor = Color.LightBlue;
                    _LastReg = loc;
                }

                listView1.Items[loc].SubItems[1].Text = String.Format("0x{0,4:X4}", value);
            }
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
                int x = 0;
                foreach (ListViewItem item in listView1.Items)
                {
                    if (x < 8)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Reg.Reg[x]);
                    else if (x == 8)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Reg.PC);
                    else if (x == 9)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Reg.SP);
                    else if (x == 10)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Reg.O);

                    x++;
                }

                _Thread = null;
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

        private void ButStep_Click(object sender, EventArgs e)
        {
            if (_Emu == null)
                InitProg();

            _Emu.Step();

            _IgnoreHightlight = true;

            OnRegUpdate((ushort)8, _Reg.PC);
            OnRegUpdate((ushort)7, _Reg.SP);
            OnRegUpdate((ushort)9, _Reg.O);

            _IgnoreHightlight = false;
        }

        private void ButReset_Click(object sender, EventArgs e)
        {
            if (_Thread != null)
                ButStartToggle_Click(null, null);

            InitProg();
        }

        private void OnSpeedChanged(object sender, EventArgs e)
        {
            Emu.SpeedMultiplier = _Speeds[CBSpeed.SelectedIndex];
            _AvgTicks = 0;

            ShowSelect = CBSpeed.SelectedIndex >= _Speeds.Length - 2;

            if (!ShowSelect)
            {
                if (_LastReg != -1)
                {
                    foreach (ListViewItem item in listView1.Items)
                    {
                        item.BackColor = Color.White;
                        item.SubItems[1].Text = "";
                    }

                    _LastReg = -1;
                }

                if (_Reg != null)
                    _Reg.RegUpdateEvent -= new MemUpdateHandler(OnRegUpdate);
            }
            else if (_Reg != null)
                _Reg.RegUpdateEvent += new MemUpdateHandler(OnRegUpdate);
        }

        private void OnStepComplete(long ticks, long instruct)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StepCompleteHandler(OnStepComplete), new object[] { ticks, instruct });
            }
            else
            {
                if (_AvgTicks != 0)
                {
                    var temp = (long)(ticks / Emu.SpeedMultiplier / instruct);

                    _AvgTicks += temp;
                    _AvgTicks /= 2;
                }
                else
                {
                    _AvgTicks += ticks;
                }

                var mod = (10000 / _Speeds[CBSpeed.SelectedIndex]);

                if (_TickCount % mod == 0)
                {
                    long herz = (long)(TimeSpan.TicksPerSecond / (_AvgTicks * Emu.SpeedMultiplier));

                    if (herz > 1000)
                        TBSpeed.Text = String.Format("{0} kHz", herz / 1000);
                    else
                        TBSpeed.Text = String.Format("{0} Hz", herz);
                }

                _TickCount++;
            }
        }
    }
}

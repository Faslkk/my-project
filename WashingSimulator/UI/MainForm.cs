using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WashingSimulator.Logic;

namespace WashingSimulator.UI
{
    public class MainForm : Form
    {
        private MachineManager _manager;
        private WashingMachine _selectedMachine;
        private Timer _simulationTimer;
        private int _timerInterval = 400; 

        private Panel _panelLeft;
        private Panel _panelRight;
        private Panel _panelTop;
        private Panel _panelBottom;

        private ListBox _listMachines;
        private Label _lblMachineTitle;
        private Label _lblMachineDesc;
        private Label _lblType;
        private Label _lblCapacity;
        private Label _lblSpinSpeed;

        private ComboBox _cmbMode;
        private NumericUpDown _nudLoad;
        private Button _btnStart;
        private Button _btnReset;
        private Button _btnAdd;
        private Button _btnRemove;

        private ProgressBar _progressBar;
        private Label _lblProgress;
        private Label _lblState;
        private Panel _panelDrum;

        private RichTextBox _rtbLog;
        private Label _lblStats;

        private Label _lblModeDescription;

        private float _drumAngle = 0f;
        private Timer _drumAnimTimer;
        private bool _isRunning = false;

        public MainForm()
        {
            _manager = MachineManager.CreateDemo();
            InitializeComponent();
            RefreshMachineList();
            RefreshStats();
        }

        private void InitializeComponent()
        {
            Text = "Симулятор пральних машин";
            Size = new Size(1000, 700);
            MinimumSize = new Size(900, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(240, 244, 248);
            Font = new Font("Segoe UI", 9f);

            _panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(33, 97, 180),
                Padding = new Padding(16, 0, 16, 0)
            };
            var lblTitle = new Label
            {
                Text = "🫧  Симулятор пральних машин",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(14, 12)
            };
            _panelTop.Controls.Add(lblTitle);

            _lblStats = new Label
            {
                ForeColor = Color.FromArgb(180, 210, 255),
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(600, 20)
            };
            _panelTop.Controls.Add(_lblStats);

            _panelLeft = new Panel
            {
                Width = 230,
                Dock = DockStyle.Left,
                BackColor = Color.White,
                Padding = new Padding(8)
            };

            var lblListTitle = MakeLabel("Пральні машини", new Font("Segoe UI", 10f, FontStyle.Bold),
                Color.FromArgb(40, 60, 90), new Point(8, 8));

            _listMachines = new ListBox
            {
                Location = new Point(8, 32),
                Size = new Size(214, 340),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 42
            };
            _listMachines.DrawItem += ListMachines_DrawItem;
            _listMachines.SelectedIndexChanged += ListMachines_SelectedIndexChanged;

            _btnAdd = MakeButton("+ Додати", new Point(8, 382), new Size(100, 30),
                Color.FromArgb(39, 174, 96), BtnAdd_Click);
            _btnRemove = MakeButton("✕ Видалити", new Point(122, 382), new Size(100, 30),
                Color.FromArgb(192, 57, 43), BtnRemove_Click);

            _panelLeft.Controls.AddRange(new Control[] {
                lblListTitle, _listMachines, _btnAdd, _btnRemove
            });

            _panelRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 244, 248),
                Padding = new Padding(12)
            };

            var cardMachine = MakeCard(new Point(12, 12), new Size(430, 110));

            _lblMachineTitle = MakeLabel("Оберіть машину зі списку",
                new Font("Segoe UI", 13f, FontStyle.Bold),
                Color.FromArgb(33, 97, 180), new Point(10, 8));
            _lblType = MakeLabel("", new Font("Segoe UI", 9f), Color.Gray, new Point(10, 36));
            _lblCapacity = MakeLabel("", new Font("Segoe UI", 9f), Color.DimGray, new Point(10, 54));
            _lblSpinSpeed = MakeLabel("", new Font("Segoe UI", 9f), Color.DimGray, new Point(10, 72));
            _lblMachineDesc = MakeLabel("", new Font("Segoe UI", 8.5f, FontStyle.Italic), Color.Gray, new Point(10, 90));
            cardMachine.Controls.AddRange(new Control[] {
                _lblMachineTitle, _lblType, _lblCapacity, _lblSpinSpeed, _lblMachineDesc
            });

            var cardControl = MakeCard(new Point(12, 130), new Size(430, 175));
            var lblControl = MakeLabel("Запуск прання", new Font("Segoe UI", 10f, FontStyle.Bold),
                Color.FromArgb(40, 60, 90), new Point(10, 8));

            var lblMode = MakeLabel("Режим:", new Font("Segoe UI", 9f), Color.DimGray, new Point(10, 34));
            _cmbMode = new ComboBox
            {
                Location = new Point(80, 30),
                Size = new Size(200, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            _cmbMode.SelectedIndexChanged += CmbMode_SelectedIndexChanged;

            _lblModeDescription = MakeLabel("", new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Color.FromArgb(100, 130, 180), new Point(10, 58));
            _lblModeDescription.Size = new Size(410, 16);

            var lblLoad = MakeLabel("Кг білизни:", new Font("Segoe UI", 9f), Color.DimGray, new Point(10, 82));
            _nudLoad = new NumericUpDown
            {
                Location = new Point(100, 78),
                Size = new Size(80, 24),
                DecimalPlaces = 1,
                Increment = 0.5m,
                Minimum = 0.5m,
                Maximum = 15m,
                Value = 3m,
                Font = new Font("Segoe UI", 9f)
            };

            _btnStart = MakeButton("▶  Запустити", new Point(10, 112), new Size(140, 36),
                Color.FromArgb(33, 97, 180), BtnStart_Click);
            _btnStart.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

            _btnReset = MakeButton("↺  Скинути", new Point(160, 112), new Size(120, 36),
                Color.FromArgb(120, 120, 130), BtnReset_Click);

            cardControl.Controls.AddRange(new Control[] {
                lblControl, lblMode, _cmbMode, _lblModeDescription,
                lblLoad, _nudLoad, _btnStart, _btnReset
            });

            var cardProgress = MakeCard(new Point(12, 315), new Size(430, 130));
            var lblProgTitle = MakeLabel("Прогрес", new Font("Segoe UI", 10f, FontStyle.Bold),
                Color.FromArgb(40, 60, 90), new Point(10, 8));

            _lblState = MakeLabel("Стан: Очікування", new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Color.FromArgb(33, 97, 180), new Point(10, 32));

            _progressBar = new ProgressBar
            {
                Location = new Point(10, 56),
                Size = new Size(410, 24),
                Minimum = 0,
                Maximum = 100,
                Style = ProgressBarStyle.Continuous
            };

            _lblProgress = MakeLabel("0%", new Font("Segoe UI", 9f), Color.Gray, new Point(10, 86));

            cardProgress.Controls.AddRange(new Control[] {
                lblProgTitle, _lblState, _progressBar, _lblProgress
            });

            _panelDrum = new Panel
            {
                Location = new Point(456, 12),
                Size = new Size(200, 200),
                BackColor = Color.Transparent
            };
            _panelDrum.Paint += PanelDrum_Paint;

            var cardLog = MakeCard(new Point(456, 220), new Size(400, 230));
            var lblLog = MakeLabel("Журнал подій", new Font("Segoe UI", 10f, FontStyle.Bold),
                Color.FromArgb(40, 60, 90), new Point(10, 8));

            _rtbLog = new RichTextBox
            {
                Location = new Point(10, 30),
                Size = new Size(380, 188),
                ReadOnly = true,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 8.5f),
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            cardLog.Controls.AddRange(new Control[] { lblLog, _rtbLog });

            _panelRight.Controls.AddRange(new Control[] {
                cardMachine, cardControl, cardProgress, _panelDrum, cardLog
            });

            _simulationTimer = new Timer { Interval = _timerInterval };
            _simulationTimer.Tick += SimulationTimer_Tick;

            _drumAnimTimer = new Timer { Interval = 40 };
            _drumAnimTimer.Tick += DrumAnimTimer_Tick;


            _drumAnimTimer.Start();


            Controls.AddRange(new Control[] { _panelRight, _panelLeft, _panelTop });
        }

        private void DrumAnimTimer_Tick(object sender, EventArgs e)
        {
            if (_isRunning) { _drumAngle += 4f; _panelDrum.Invalidate(); }
        }

        private void PanelDrum_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = _panelDrum.Width / 2;
            int cy = _panelDrum.Height / 2;
            int r = 85;

            g.FillEllipse(new SolidBrush(Color.White), cx - r, cy - r, r * 2, r * 2);
            g.DrawEllipse(new Pen(Color.FromArgb(180, 200, 220), 6), cx - r, cy - r, r * 2, r * 2);

            int ri = r - 14;
            g.DrawEllipse(new Pen(Color.FromArgb(200, 210, 225), 2), cx - ri, cy - ri, ri * 2, ri * 2);

            if (_isRunning)
            {
                for (int i = 0; i < 6; i++)
                {
                    double angle = (_drumAngle + i * 60) * Math.PI / 180.0;
                    int hx = cx + (int)(50 * Math.Cos(angle));
                    int hy = cy + (int)(50 * Math.Sin(angle));
                    g.FillEllipse(new SolidBrush(Color.FromArgb(200, 210, 230)), hx - 9, hy - 9, 18, 18);
                    g.DrawEllipse(new Pen(Color.FromArgb(150, 170, 200), 2), hx - 9, hy - 9, 18, 18);
                }

                var random = new Random(42);
                for (int i = 0; i < 8; i++)
                {
                    double a = (_drumAngle * 1.5 + i * 45) * Math.PI / 180.0;
                    double dist = 20 + (i % 3) * 12;
                    int bx = cx + (int)(dist * Math.Cos(a));
                    int by = cy + (int)(dist * Math.Sin(a));
                    int br = 4 + i % 4;
                    g.FillEllipse(new SolidBrush(Color.FromArgb(180, 220, 240, 255)), bx - br, by - br, br * 2, br * 2);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    double angle = i * 60 * Math.PI / 180.0;
                    int hx = cx + (int)(50 * Math.Cos(angle));
                    int hy = cy + (int)(50 * Math.Sin(angle));
                    g.FillEllipse(new SolidBrush(Color.FromArgb(220, 225, 235)), hx - 9, hy - 9, 18, 18);
                    g.DrawEllipse(new Pen(Color.FromArgb(180, 190, 210), 1), hx - 9, hy - 9, 18, 18);
                }
            }

            int lockR = 14;
            Color lockColor = _isRunning ? Color.FromArgb(33, 150, 243) : Color.FromArgb(180, 190, 200);
            g.FillEllipse(new SolidBrush(lockColor), cx - lockR, cy - lockR, lockR * 2, lockR * 2);
            g.DrawEllipse(new Pen(Color.White, 2), cx - lockR, cy - lockR, lockR * 2, lockR * 2);

            string stateText = _selectedMachine != null
                ? GetStateEmoji(_selectedMachine.State)
                : "⬜";
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(stateText, new Font("Segoe UI", 10f), Brushes.DimGray,
                new RectangleF(cx - 50, cy + r - 8, 100, 20), sf);
        }

        private string GetStateEmoji(MachineState s) => s switch
        {
            MachineState.Idle     => "Очікування",
            MachineState.Washing  => "🫧 Прання",
            MachineState.Rinsing  => "💧 Полоскання",
            MachineState.Spinning => "🌀 Віджим",
            MachineState.Done     => "✅ Готово",
            MachineState.Error    => "❌ Помилка",
            _ => ""
        };

       private void ListMachines_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _manager.Machines.Count) return;
            var machine = _manager.Machines[e.Index];

            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color bg = selected ? Color.FromArgb(33, 97, 180) : Color.White;
            Color fg = selected ? Color.White : Color.FromArgb(30, 40, 60);
            Color fg2 = selected ? Color.FromArgb(200, 220, 255) : Color.Gray;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

            Color typeColor = machine switch
            {
                FrontLoadMachine => Color.FromArgb(33, 97, 180),
                TopLoadMachine   => Color.FromArgb(39, 174, 96),
                PortableMachine  => Color.FromArgb(230, 126, 34),
                _ => Color.Gray
            };
            e.Graphics.FillRectangle(new SolidBrush(typeColor),
                e.Bounds.X, e.Bounds.Y + 4, 4, e.Bounds.Height - 8);

            Color stateColor = machine.State switch
            {
                MachineState.Idle     => Color.LightGray,
                MachineState.Washing  => Color.FromArgb(33, 150, 243),
                MachineState.Rinsing  => Color.FromArgb(0, 188, 212),
                MachineState.Spinning => Color.FromArgb(156, 39, 176),
                MachineState.Done     => Color.FromArgb(76, 175, 80),
                MachineState.Error    => Color.FromArgb(244, 67, 54),
                _ => Color.Gray
            };
            e.Graphics.FillEllipse(new SolidBrush(stateColor),
                e.Bounds.Right - 18, e.Bounds.Y + 14, 10, 10);

            e.Graphics.DrawString(machine.Name,
                new Font("Segoe UI", 9.5f, FontStyle.Bold), new SolidBrush(fg),
                new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 4, e.Bounds.Width - 30, 20));

            e.Graphics.DrawString(machine.MachineType,
                new Font("Segoe UI", 8f), new SolidBrush(fg2),
                new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 22, e.Bounds.Width - 30, 16));

            e.Graphics.DrawLine(new Pen(Color.FromArgb(230, 230, 230)),
                e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void ListMachines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_listMachines.SelectedIndex < 0) return;
            _selectedMachine = _manager.Machines[_listMachines.SelectedIndex];
            RefreshMachineCard();
            RefreshModeCombo();
            _panelDrum.Invalidate();
        }

        private void CmbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedMachine == null || _cmbMode.SelectedIndex < 0) return;
            var mode = (WashingMode)_cmbMode.SelectedItem;
            _lblModeDescription.Text = _selectedMachine.GetModeDescription(mode);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (_selectedMachine == null) { Warn("Оберіть машину."); return; }
            if (_cmbMode.SelectedItem == null) { Warn("Оберіть режим прання."); return; }
            if (_manager.WorkingCount > 0 && !_isRunning)
            {
                Warn("Зачекайте — інша машина ще працює. Симулятор виконує один цикл прання за раз.");
                return;
            }
            var mode = (WashingMode)_cmbMode.SelectedItem;
            float loadKg = (float)_nudLoad.Value;

            var result = _selectedMachine.StartWashing(loadKg, mode);
            if (!result.IsSuccess)
            {
                Warn(result.Message);
                return;
            }

            _isRunning = true;
            _simulationTimer.Start();
            UpdateButtons();
            RefreshStats();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (_selectedMachine == null) return;
            _simulationTimer.Stop();
            _isRunning = false;
            _selectedMachine.Reset();
            _progressBar.Value = 0;
            _lblProgress.Text = "0%";
            UpdateStateLabel();
            UpdateButtons();
            _panelDrum.Invalidate();
            RefreshMachineList();
            RefreshStats();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using var dlg = new AddMachineDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _manager.AddMachine(dlg.CreatedMachine);
                    RefreshMachineList();
                    RefreshStats();
                }
                catch (Exception ex)
                {
                    Warn(ex.Message);
                }
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_selectedMachine == null) { Warn("Оберіть машину."); return; }
            if (MessageBox.Show($"Видалити «{_selectedMachine.Name}»?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                _manager.RemoveMachine(_selectedMachine.Name);
                _selectedMachine = null;
                RefreshMachineList();
                RefreshStats();
                ClearMachineCard();
            }
            catch (Exception ex) { Warn(ex.Message); }
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (_selectedMachine == null) return;

            if (_selectedMachine.State == MachineState.Done ||
                _selectedMachine.State == MachineState.Error)
            {
                _simulationTimer.Stop();
                _isRunning = false;
                UpdateButtons();
                RefreshMachineList();
                RefreshStats();
                return;
            }

            _selectedMachine.AdvanceStep();
            _progressBar.Value = _selectedMachine.ProgressPercent;
            _lblProgress.Text = $"{_selectedMachine.ProgressPercent}%";
            UpdateStateLabel();
            RefreshMachineList();
            RefreshStats();
        }

        private WashingMachine _subscribedMachine;

        private void SubscribeToMachine(WashingMachine machine)
        {
            if (_subscribedMachine != null)
            {
                _subscribedMachine.StatusUpdated -= Machine_StatusUpdated;
                _subscribedMachine.WashingCompleted -= Machine_WashingCompleted;
            }
            _subscribedMachine = machine;
            if (machine != null)
            {
                machine.StatusUpdated += Machine_StatusUpdated;
                machine.WashingCompleted += Machine_WashingCompleted;
            }
        }

        private void Machine_StatusUpdated(object sender, MachineEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => LogMessage(e.Message, e.State))); return; }
            LogMessage(e.Message, e.State);
        }

        private void Machine_WashingCompleted(object sender, MachineEventArgs e)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnWashingComplete())); return; }
            OnWashingComplete();
        }

        private void OnWashingComplete()
        {
            _simulationTimer.Stop();
            _isRunning = false;
            _progressBar.Value = 100;
            _lblProgress.Text = "100%";
            UpdateStateLabel();
            UpdateButtons();
            RefreshMachineList();
            RefreshStats();
            _panelDrum.Invalidate();
        }

        private void RefreshMachineList()
        {
            int sel = _listMachines.SelectedIndex;
            _listMachines.Items.Clear();
            foreach (var m in _manager.Machines)
                _listMachines.Items.Add(m);
            if (sel >= 0 && sel < _listMachines.Items.Count)
                _listMachines.SelectedIndex = sel;
            _listMachines.Invalidate();
        }

        private void RefreshMachineCard()
        {
            if (_selectedMachine == null) { ClearMachineCard(); return; }
            SubscribeToMachine(_selectedMachine);

            _lblMachineTitle.Text = _selectedMachine.Name;
            _lblType.Text = $"Тип: {_selectedMachine.MachineType}";
            _lblCapacity.Text = $"Місткість: {_selectedMachine.LoadCapacityKg:F1} кг";
            _lblSpinSpeed.Text = _selectedMachine.GetSpinSpeedRpm() > 0
                ? $"Віджим: {_selectedMachine.GetSpinSpeedRpm()} об/хв"
                : "Без функції віджиму";
            _lblMachineDesc.Text = _selectedMachine.Description;

            _progressBar.Value = _selectedMachine.ProgressPercent;
            _lblProgress.Text = $"{_selectedMachine.ProgressPercent}%";
            UpdateStateLabel();
            UpdateButtons();

            _isRunning = _selectedMachine.State == MachineState.Washing ||
                         _selectedMachine.State == MachineState.Rinsing ||
                         _selectedMachine.State == MachineState.Spinning;
        }

        private void RefreshModeCombo()
        {
            _cmbMode.Items.Clear();
            if (_selectedMachine == null) return;
            foreach (var mode in _selectedMachine.GetSupportedModes())
                _cmbMode.Items.Add(mode);
            if (_cmbMode.Items.Count > 0) _cmbMode.SelectedIndex = 0;
            _cmbMode.DisplayMember = null;
            _cmbMode.Format += (s, e) =>
                e.Value = WashingMachine.GetModeLocalName((WashingMode)e.ListItem);
        }

        private void ClearMachineCard()
        {
            _lblMachineTitle.Text = "Оберіть машину зі списку";
            _lblType.Text = _lblCapacity.Text = _lblSpinSpeed.Text = _lblMachineDesc.Text = "";
            _lblState.Text = "Стан: —";
            _progressBar.Value = 0;
            _lblProgress.Text = "0%";
            _cmbMode.Items.Clear();
            _lblModeDescription.Text = "";
        }

        private void UpdateStateLabel()
        {
            if (_selectedMachine == null) return;
            string stateStr = _selectedMachine.State switch
            {
                MachineState.Idle     => "⏸ Очікування",
                MachineState.Washing  => "🫧 Прання...",
                MachineState.Rinsing  => "💧 Полоскання...",
                MachineState.Spinning => "🌀 Віджим...",
                MachineState.Done     => "✅ Завершено",
                MachineState.Error    => "❌ Помилка",
                _ => ""
            };
            _lblState.Text = $"Стан: {stateStr}";
            _lblState.ForeColor = _selectedMachine.State switch
            {
                MachineState.Done  => Color.FromArgb(39, 174, 96),
                MachineState.Error => Color.FromArgb(192, 57, 43),
                MachineState.Idle  => Color.Gray,
                _ => Color.FromArgb(33, 97, 180)
            };
        }

        private void UpdateButtons()
        {
            bool canStart = _selectedMachine != null &&
                (_selectedMachine.State == MachineState.Idle || _selectedMachine.State == MachineState.Done);
            _btnStart.Enabled = canStart;
            _btnReset.Enabled = _selectedMachine != null &&
                _selectedMachine.State != MachineState.Idle;
        }

        private void RefreshStats()
        {
            _lblStats.Text =
                $"Всього: {_manager.TotalMachines}  |  " +
                $"Працюють: {_manager.WorkingCount}  |  " +
                $"Готові: {_manager.DoneCount}  |  " +
                $"Вільні: {_manager.IdleCount}";
        }

        private void LogMessage(string message, MachineState state)
        {
            Color c = state switch
            {
                MachineState.Done     => Color.FromArgb(39, 174, 96),
                MachineState.Error    => Color.FromArgb(192, 57, 43),
                MachineState.Spinning => Color.FromArgb(130, 40, 180),
                MachineState.Rinsing  => Color.FromArgb(0, 130, 180),
                MachineState.Washing  => Color.FromArgb(33, 97, 180),
                _ => Color.DimGray
            };
            _rtbLog.SelectionStart = _rtbLog.TextLength;
            _rtbLog.SelectionColor = Color.Gray;
            _rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
            _rtbLog.SelectionColor = c;
            _rtbLog.AppendText(message + "\n");
            _rtbLog.SelectionStart = _rtbLog.TextLength;
            _rtbLog.ScrollToCaret();
        }

        private static void Warn(string msg) =>
            MessageBox.Show(msg, "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private static Panel MakeCard(Point loc, Size size)
        {
            var p = new Panel
            {
                Location = loc,
                Size = size,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            p.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(220, 225, 235), 1.5f);
                var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                g.DrawRectangle(pen, rect);
            };
            return p;
        }

        private static Label MakeLabel(string text, Font font, Color color, Point loc)
        {
            return new Label
            {
                Text = text, Font = font, ForeColor = color,
                Location = loc, AutoSize = true, BackColor = Color.Transparent
            };
        }

        private static Button MakeButton(string text, Point loc, Size size, Color backColor, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text, Location = loc, Size = size,
                BackColor = backColor, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }
    }
}

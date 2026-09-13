using System;
using System.Drawing;
using System.Windows.Forms;
using WashingSimulator.Logic;

namespace WashingSimulator.UI
{
    public class AddMachineDialog : Form
    {
        public WashingMachine CreatedMachine { get; private set; }

        private ComboBox _cmbType;
        private TextBox _txtName;
        private NumericUpDown _nudCapacity;
        private NumericUpDown _nudSpinSpeed;
        private CheckBox _chkExtra;
        private Label _lblExtraFeature;
        private Button _btnOk;
        private Button _btnCancel;

        public AddMachineDialog()
        {
            Text = "Додати машину";
            Size = new Size(360, 310);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            int y = 16;
            int lx = 14, cx = 150, cw = 180;

            Controls.Add(MakeLbl("Тип машини:", lx, y));
            _cmbType = new ComboBox { Location = new Point(cx, y), Size = new Size(cw, 24),
                DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbType.Items.AddRange(new[] { "Фронтальна", "Вертикальна", "Портативна" });
            _cmbType.SelectedIndex = 0;
            _cmbType.SelectedIndexChanged += CmbType_Changed;
            Controls.Add(_cmbType);

            y += 34;
            Controls.Add(MakeLbl("Назва:", lx, y));
            _txtName = new TextBox { Location = new Point(cx, y), Size = new Size(cw, 24) };
            _txtName.Text = "Нова машина";
            Controls.Add(_txtName);

            y += 34;
            Controls.Add(MakeLbl("Місткість (кг):", lx, y));
            _nudCapacity = new NumericUpDown
            {
                Location = new Point(cx, y), Size = new Size(80, 24),
                Minimum = 1, Maximum = 12, Value = 6, DecimalPlaces = 1, Increment = 0.5m
            };
            Controls.Add(_nudCapacity);

            y += 34;
            Controls.Add(MakeLbl("Швидкість (об/хв):", lx, y));
            _nudSpinSpeed = new NumericUpDown
            {
                Location = new Point(cx, y), Size = new Size(80, 24),
                Minimum = 0, Maximum = 1600, Value = 1200, Increment = 100
            };
            Controls.Add(_nudSpinSpeed);

            y += 34;
            _lblExtraFeature = MakeLbl("Функція пару:", lx, y);
            Controls.Add(_lblExtraFeature);
            _chkExtra = new CheckBox { Location = new Point(cx, y + 2), AutoSize = true };
            Controls.Add(_chkExtra);

            y += 44;
            var sep = new Panel { Location = new Point(0, y), Size = new Size(360, 1),
                BackColor = Color.FromArgb(220, 220, 225) };
            Controls.Add(sep);

            y += 10;
            _btnOk = new Button
            {
                Text = "✔ Додати", Location = new Point(cx, y), Size = new Size(80, 30),
                BackColor = Color.FromArgb(33, 97, 180), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += BtnOk_Click;

            _btnCancel = new Button
            {
                Text = "✕ Скасувати", Location = new Point(cx + 90, y), Size = new Size(90, 30),
                BackColor = Color.FromArgb(150, 150, 155), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            Controls.AddRange(new Control[] { _btnOk, _btnCancel });
            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            CmbType_Changed(null, EventArgs.Empty);
        }

        private void CmbType_Changed(object sender, EventArgs e)
        {
            switch (_cmbType.SelectedIndex)
            {
                case 0: 
                    _lblExtraFeature.Text = "Функція пару:";
                    _nudSpinSpeed.Enabled = true;
                    _nudSpinSpeed.Value = 1200;
                    _nudCapacity.Maximum = 12;
                    break;
                case 1: 
                    _lblExtraFeature.Text = "Активатор:";
                    _nudSpinSpeed.Enabled = false;
                    _nudSpinSpeed.Value = 800;
                    _nudCapacity.Maximum = 10;
                    break;
                case 2: 
                    _lblExtraFeature.Text = "Є віджим:";
                    _nudSpinSpeed.Enabled = false;
                    _nudSpinSpeed.Value = 400;
                    _nudCapacity.Maximum = 3.5m;
                    if (_nudCapacity.Value > 3.5m) _nudCapacity.Value = 3.5m;
                    break;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            string name = _txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введіть назву машини.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            float cap = (float)_nudCapacity.Value;
            int spin = (int)_nudSpinSpeed.Value;
            bool extra = _chkExtra.Checked;

            CreatedMachine = _cmbType.SelectedIndex switch
            {
                0 => new FrontLoadMachine(name, cap, spin, extra),
                1 => new TopLoadMachine(name, cap, extra),
                2 => new PortableMachine(name, cap, extra),
                _ => throw new InvalidOperationException()
            };
        }

        private static Label MakeLbl(string text, int x, int y) =>
            new Label { Text = text, Location = new Point(x, y + 3), AutoSize = true,
                        ForeColor = Color.FromArgb(60, 70, 90) };
    }
}

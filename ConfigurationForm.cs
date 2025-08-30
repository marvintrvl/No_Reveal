using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NoReveal
{
    public partial class ConfigurationForm : Form
    {
        private Configuration _config;
        private bool _configChanged = false;

        public event Action<Configuration>? OnConfigurationChanged;

        // Controls
        private CheckBox _enabledCheckBox = null!;
        private NumericUpDown _blockDistanceNumeric = null!;
        private CheckBox _topEdgeCheckBox = null!;
        private CheckBox _bottomEdgeCheckBox = null!;
        private CheckBox _leftEdgeCheckBox = null!;
        private CheckBox _rightEdgeCheckBox = null!;
        private CheckBox _startMinimizedCheckBox = null!;
        private CheckBox _showNotificationsCheckBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;

        public ConfigurationForm(Configuration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            LoadConfiguration();
        }

    private const string EdgeTop = "Top";
    private const string EdgeBottom = "Bottom";
    private const string EdgeLeft = "Left";
    private const string EdgeRight = "Right";

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form properties
            Text = "No_Reveal Configuration";
            Size = new Size(400, 350);
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;

            var y = 20;
            var labelWidth = 120;
            // int controlLeft = labelWidth + 10; // no longer used after switching to TableLayoutPanel

            // Enabled checkbox
            _enabledCheckBox = new CheckBox
            {
                Text = "Enable blocking",
                Location = new Point(20, y),
                Size = new Size(200, 23),
                Checked = _config.IsEnabled
            };
            Controls.Add(_enabledCheckBox);
            y += 35;

            // Block distance (DPI-safe layout using FlowLayoutPanel)
            var distanceRow = new FlowLayoutPanel
            {
                Location = new Point(20, y),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent
            };

            var distanceLabel = new Label
            {
                Text = "Block distance:",
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            };
            distanceRow.Controls.Add(distanceLabel);

            _blockDistanceNumeric = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 50,
                Value = _config.BlockDistance,
                Width = 90,
                Margin = new Padding(0, 0, 8, 0),
                TextAlign = HorizontalAlignment.Center,
                UpDownAlign = LeftRightAlignment.Right
            };
            distanceRow.Controls.Add(_blockDistanceNumeric);

            var pixelsLabel = new Label
            {
                Text = "pixels",
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };
            distanceRow.Controls.Add(pixelsLabel);

            Controls.Add(distanceRow);
            // Add spacing below the row using preferred size to avoid large gaps
            distanceRow.PerformLayout();
            int rowHeight = distanceRow.PreferredSize.Height;
            if (rowHeight <= 0)
            {
                rowHeight = distanceRow.Height > 0 ? distanceRow.Height : 30;
            }
            y += rowHeight + 8;

            // Restricted edges
            var edgesLabel = new Label
            {
                Text = "Block edges:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            Controls.Add(edgesLabel);
            y += 22;

            _topEdgeCheckBox = new CheckBox
            {
                Text = EdgeTop,
                Location = new Point(40, y),
                Size = new Size(80, 23),
                Checked = _config.RestrictedEdges.Contains(EdgeTop)
            };
            Controls.Add(_topEdgeCheckBox);

            _bottomEdgeCheckBox = new CheckBox
            {
                Text = EdgeBottom,
                Location = new Point(130, y),
                Size = new Size(80, 23),
                Checked = _config.RestrictedEdges.Contains(EdgeBottom)
            };
            Controls.Add(_bottomEdgeCheckBox);
            y += 26;

            _leftEdgeCheckBox = new CheckBox
            {
                Text = EdgeLeft,
                Location = new Point(40, y),
                Size = new Size(80, 23),
                Checked = _config.RestrictedEdges.Contains(EdgeLeft)
            };
            Controls.Add(_leftEdgeCheckBox);

            _rightEdgeCheckBox = new CheckBox
            {
                Text = EdgeRight,
                Location = new Point(130, y),
                Size = new Size(80, 23),
                Checked = _config.RestrictedEdges.Contains(EdgeRight)
            };
            Controls.Add(_rightEdgeCheckBox);
            y += 28;

            // Additional options
            _startMinimizedCheckBox = new CheckBox
            {
                Text = "Start minimized",
                Location = new Point(20, y),
                Size = new Size(200, 23),
                Checked = _config.StartMinimized
            };
            Controls.Add(_startMinimizedCheckBox);
            y += 30;

            _showNotificationsCheckBox = new CheckBox
            {
                Text = "Show notifications",
                Location = new Point(20, y),
                Size = new Size(200, 23),
                Checked = _config.ShowNotifications
            };
            Controls.Add(_showNotificationsCheckBox);
            y += 28;

            // Buttons
            _saveButton = new Button
            {
                Text = "Save",
                Location = new Point(200, y),
                Size = new Size(75, 30),
                UseVisualStyleBackColor = true
            };
            _saveButton.Click += SaveButton_Click;
            Controls.Add(_saveButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(285, y),
                Size = new Size(75, 30),
                UseVisualStyleBackColor = true
            };
            _cancelButton.Click += CancelButton_Click;
            Controls.Add(_cancelButton);

            // Set Accept and Cancel buttons
            AcceptButton = _saveButton;
            CancelButton = _cancelButton;

            ResumeLayout(false);
        }

        private void LoadConfiguration()
        {
            _enabledCheckBox.Checked = _config.IsEnabled;
            _blockDistanceNumeric.Value = _config.BlockDistance;
            _topEdgeCheckBox.Checked = _config.RestrictedEdges.Contains(EdgeTop);
            _bottomEdgeCheckBox.Checked = _config.RestrictedEdges.Contains(EdgeBottom);
            _leftEdgeCheckBox.Checked = _config.RestrictedEdges.Contains(EdgeLeft);
            _rightEdgeCheckBox.Checked = _config.RestrictedEdges.Contains(EdgeRight);
            _startMinimizedCheckBox.Checked = _config.StartMinimized;
            _showNotificationsCheckBox.Checked = _config.ShowNotifications;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate at least one edge is selected
                if (!_topEdgeCheckBox.Checked && !_bottomEdgeCheckBox.Checked &&
                    !_leftEdgeCheckBox.Checked && !_rightEdgeCheckBox.Checked)
                {
                    MessageBox.Show("Please select at least one edge to block.",
                        "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update configuration
                _config.IsEnabled = _enabledCheckBox.Checked;
                _config.BlockDistance = (int)_blockDistanceNumeric.Value;
                _config.StartMinimized = _startMinimizedCheckBox.Checked;
                _config.ShowNotifications = _showNotificationsCheckBox.Checked;

                // Update restricted edges
                _config.RestrictedEdges.Clear();
                if (_topEdgeCheckBox.Checked) _config.RestrictedEdges.Add(EdgeTop);
                if (_bottomEdgeCheckBox.Checked) _config.RestrictedEdges.Add(EdgeBottom);
                if (_leftEdgeCheckBox.Checked) _config.RestrictedEdges.Add(EdgeLeft);
                if (_rightEdgeCheckBox.Checked) _config.RestrictedEdges.Add(EdgeRight);

                // Save configuration
                _config.Save();

                // Notify about changes
                OnConfigurationChanged?.Invoke(_config);

                _configChanged = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to save configuration: {ex.Message}");
                MessageBox.Show($"Failed to save configuration: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_configChanged && DialogResult != DialogResult.OK)
            {
                DialogResult = DialogResult.Cancel;
            }
            base.OnFormClosing(e);
        }
    }
}

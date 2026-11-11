namespace SameGame.View
{
    partial class GameForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            _menuStrip = new MenuStrip();
            _menuNewGame = new ToolStripMenuItem();
            _menuEasy = new ToolStripMenuItem();
            _menuMedium = new ToolStripMenuItem();
            _menuHard = new ToolStripMenuItem();
            _menuExit = new ToolStripMenuItem();
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel();
            _tablePanel = new Panel();
            _menuStrip.SuspendLayout();
            _statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // _menuStrip
            //
            _menuStrip.Items.AddRange(new ToolStripItem[] { _menuNewGame, _menuExit });
            _menuStrip.Location = new Point(0, 0);
            _menuStrip.Name = "_menuStrip";
            _menuStrip.TabIndex = 0;
            //
            // _menuNewGame
            //
            _menuNewGame.DropDownItems.AddRange(new ToolStripItem[] { _menuEasy, _menuMedium, _menuHard });
            _menuNewGame.Name = "_menuNewGame";
            _menuNewGame.Text = "Új játék";
            //
            // _menuEasy
            //
            _menuEasy.Name = "_menuEasy";
            _menuEasy.Text = "Könnyű (3 szín)";
            _menuEasy.Click += MenuEasy_Click;
            //
            // _menuMedium
            //
            _menuMedium.Name = "_menuMedium";
            _menuMedium.Text = "Közepes (5 szín)";
            _menuMedium.Click += MenuMedium_Click;
            //
            // _menuHard
            //
            _menuHard.Name = "_menuHard";
            _menuHard.Text = "Nehéz (7 szín)";
            _menuHard.Click += MenuHard_Click;
            //
            // _menuExit
            //
            _menuExit.Name = "_menuExit";
            _menuExit.Text = "Kilépés";
            _menuExit.Click += MenuExit_Click;
            //
            // _statusStrip
            //
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel });
            _statusStrip.Name = "_statusStrip";
            _statusStrip.SizingGrip = false;
            _statusStrip.TabIndex = 1;
            //
            // _statusLabel
            //
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Text = "Új játék";
            //
            // _tablePanel
            //
            _tablePanel.Dock = DockStyle.Fill;
            _tablePanel.Name = "_tablePanel";
            _tablePanel.TabIndex = 2;
            //
            // GameForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_tablePanel); // a Dock = Fill vezérlőt vesszük fel először
            Controls.Add(_statusStrip);
            Controls.Add(_menuStrip);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MainMenuStrip = _menuStrip;
            MaximizeBox = false;
            Name = "GameForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SameGame";
            _menuStrip.ResumeLayout(false);
            _menuStrip.PerformLayout();
            _statusStrip.ResumeLayout(false);
            _statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip _menuStrip;
        private ToolStripMenuItem _menuNewGame;
        private ToolStripMenuItem _menuEasy;
        private ToolStripMenuItem _menuMedium;
        private ToolStripMenuItem _menuHard;
        private ToolStripMenuItem _menuExit;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private Panel _tablePanel;
    }
}

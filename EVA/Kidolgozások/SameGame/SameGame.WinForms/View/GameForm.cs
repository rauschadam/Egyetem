using SameGame.Model;

namespace SameGame.View
{
    /// <summary>
    /// A játék ablaka (nézet). Csak megjelenít és továbbítja a kattintásokat a modellnek.
    /// </summary>
    public partial class GameForm : Form
    {
        private const int FieldSize = 34; // egy mező mérete logikai pixelben

        // szín index -> megjelenített szín (legfeljebb 7 szín kell)
        private static readonly Color[] FieldColors =
        [
            Color.Crimson, Color.RoyalBlue, Color.Gold, Color.ForestGreen,
            Color.DarkOrange, Color.MediumOrchid, Color.Turquoise,
        ];

        private readonly SameGameModel _model;
        private readonly Button[,] _buttonGrid;

        public GameForm()
        {
            InitializeComponent();

            _model = new SameGameModel();
            _model.TableChanged += Model_TableChanged;
            _model.StepPerformed += Model_StepPerformed;
            _model.InvalidStep += Model_InvalidStep;
            _model.GameOver += Model_GameOver;

            _buttonGrid = new Button[_model.TableSize, _model.TableSize];
            GenerateTable();

            _model.NewGame(GameDifficulty.Easy); // indításkor rögtön legyen 15x15-ös tábla
        }

        #region Table

        /// <summary>
        /// A gombrácsot egyszer hozzuk létre; új játéknál csak a színeket frissítjük.
        /// </summary>
        private void GenerateTable()
        {
            int size = LogicalToDeviceUnits(FieldSize);

            _tablePanel.SuspendLayout();

            for (int x = 0; x < _model.TableSize; x++)
            {
                for (int y = 0; y < _model.TableSize; y++)
                {
                    Button button = new()
                    {
                        Location = new Point(y * size, x * size), // y = oszlop -> vízszintes
                        Size = new Size(size, size),
                        FlatStyle = FlatStyle.Flat,
                        Tag = (x, y), // a gomb koordinátája
                        TabStop = false,
                    };
                    button.FlatAppearance.BorderColor = Color.White;
                    button.Click += FieldButton_Click; // közös eseménykezelő

                    _buttonGrid[x, y] = button;
                    _tablePanel.Controls.Add(button);
                }
            }

            _tablePanel.ResumeLayout();

            ClientSize = new Size(
                _model.TableSize * size,
                _menuStrip.Height + (_model.TableSize * size) + _statusStrip.Height);
        }

        private void RefreshTable()
        {
            for (int x = 0; x < _model.TableSize; x++)
            {
                for (int y = 0; y < _model.TableSize; y++)
                {
                    _buttonGrid[x, y].BackColor = FieldColors[_model[x, y]];
                }
            }
        }

        private void SetTableEnabled(bool enabled)
        {
            foreach (Button button in _buttonGrid)
            {
                button.Enabled = enabled;
            }
        }

        private void FieldButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button { Tag: (int x, int y) })
            {
                _model.Step(x, y);
            }
        }

        #endregion

        #region Model event handlers

        private void Model_TableChanged(object? sender, EventArgs e)
        {
            RefreshTable();
        }

        private void Model_StepPerformed(object? sender, SameGameEventArgs e)
        {
            _statusLabel.Text = $"Az utolsó lépés {e.GroupSize} korongot törölt. Összesen: {e.Score}";
        }

        private void Model_InvalidStep(object? sender, SameGameEventArgs e)
        {
            _statusLabel.Text =
                $"Szabálytalan lépés: legalább {SameGameModel.MinimumGroupSize} összefüggő korong kell " +
                $"(ez a csoport {e.GroupSize} korongos).";
        }

        private void Model_GameOver(object? sender, SameGameEventArgs e)
        {
            SetTableEnabled(false);
            MessageBox.Show(
                $"Nincs több szabályos lépés, vége a játéknak!{Environment.NewLine}Törölt korongok: {e.Score}",
                "SameGame",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        #region Menu event handlers

        private void StartNewGame(GameDifficulty difficulty)
        {
            SetTableEnabled(true);
            _statusLabel.Text = "Új játék";
            _model.NewGame(difficulty); // ha rögtön nincs lépés, a GameOver újra letiltja a táblát
        }

        private void MenuEasy_Click(object? sender, EventArgs e) => StartNewGame(GameDifficulty.Easy);

        private void MenuMedium_Click(object? sender, EventArgs e) => StartNewGame(GameDifficulty.Medium);

        private void MenuHard_Click(object? sender, EventArgs e) => StartNewGame(GameDifficulty.Hard);

        private void MenuExit_Click(object? sender, EventArgs e) => Close();

        #endregion
    }
}

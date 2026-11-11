namespace SameGame.Model
{
    /// <summary>
    /// A SameGame játék modellje. Nem ismeri a Windows Formst:
    /// minden állapotváltozást eseménnyel jelez a nézetnek.
    /// </summary>
    public class SameGameModel
    {
        #region Constants

        public const int DefaultTableSize = 15;
        public const int MinimumGroupSize = 3; // jeles szint: csak 3+ méretű csoport törölhető
        public const int EmptyField = -1;

        #endregion

        #region Fields

        private readonly Random _random;
        private int[,] _table; // _table[sor, oszlop] = szín indexe (0..ColorCount-1)

        #endregion

        #region Properties

        public int TableSize => _table.GetLength(0);

        public GameDifficulty Difficulty { get; private set; }

        public int ColorCount => (int)Difficulty;

        public int LastRemovedCount { get; private set; }

        public int Score { get; private set; }

        public bool IsGameOver { get; private set; }

        /// <summary>
        /// Mező színének lekérdezése (csak olvasható: a nézet nem írhat a táblába).
        /// </summary>
        public int this[int x, int y] => _table[x, y];

        #endregion

        #region Events

        /// <summary>A tábla tartalma megváltozott (új játék vagy lépés után).</summary>
        public event EventHandler? TableChanged;

        /// <summary>Sikeres lépés történt.</summary>
        public event EventHandler<SameGameEventArgs>? StepPerformed;

        /// <summary>Szabálytalan lépést kísérelt meg a játékos (3-nál kisebb csoport).</summary>
        public event EventHandler<SameGameEventArgs>? InvalidStep;

        /// <summary>Nincs több szabályos lépés.</summary>
        public event EventHandler<SameGameEventArgs>? GameOver;

        #endregion

        #region Constructors

        public SameGameModel()
            : this(new Random())
        {
        }

        /// <summary>
        /// A véletlengenerátort kívülről is megadhatjuk (tesztekben fix seeddel).
        /// </summary>
        public SameGameModel(Random random)
        {
            _random = random;
            _table = new int[DefaultTableSize, DefaultTableSize];
            Difficulty = GameDifficulty.Easy;
        }

        #endregion

        #region Public methods

        public void NewGame(GameDifficulty difficulty)
        {
            Difficulty = difficulty;
            _table = new int[DefaultTableSize, DefaultTableSize];

            for (int x = 0; x < TableSize; x++)
            {
                for (int y = 0; y < TableSize; y++)
                {
                    _table[x, y] = RandomColor();
                }
            }

            LastRemovedCount = 0;
            Score = 0;
            IsGameOver = false;

            OnTableChanged();
            CheckGameOver(); // elvileg már az induló táblán sem lehet lépés
        }

        public void Step(int x, int y)
        {
            if (IsGameOver)
            {
                return;
            }

            if (x < 0 || x >= TableSize || y < 0 || y >= TableSize)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "A mező a táblán kívül esik.");
            }

            List<(int X, int Y)> group = FindGroup(x, y);

            if (group.Count < MinimumGroupSize)
            {
                OnInvalidStep(group.Count);
                return;
            }

            foreach ((int gx, int gy) in group)
            {
                _table[gx, gy] = EmptyField;
            }

            ApplyGravity();
            FillEmptyFields();

            LastRemovedCount = group.Count;
            Score += group.Count;

            OnTableChanged();
            OnStepPerformed();
            CheckGameOver();
        }

        /// <summary>
        /// Van-e még legalább MinimumGroupSize méretű egyszínű csoport a táblán.
        /// </summary>
        public bool HasValidStep()
        {
            bool[,] visited = new bool[TableSize, TableSize];

            for (int x = 0; x < TableSize; x++)
            {
                for (int y = 0; y < TableSize; y++)
                {
                    if (!visited[x, y] && FindGroup(x, y, visited).Count >= MinimumGroupSize)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Internal methods (tesztekhez)

        /// <summary>
        /// Előre megadott tábla betöltése (csak a tesztprojekt látja, InternalsVisibleTo).
        /// </summary>
        internal void SetTable(int[,] table, GameDifficulty difficulty)
        {
            Difficulty = difficulty;
            _table = (int[,])table.Clone();
            LastRemovedCount = 0;
            Score = 0;
            IsGameOver = false;
        }

        #endregion

        #region Private methods

        private int RandomColor() => _random.Next(ColorCount);

        private List<(int X, int Y)> FindGroup(int x, int y)
        {
            return FindGroup(x, y, new bool[TableSize, TableSize]);
        }

        /// <summary>
        /// Szélességi bejárás (BFS) a vízszintesen/függőlegesen szomszédos, azonos színű mezőkön.
        /// </summary>
        private List<(int X, int Y)> FindGroup(int x, int y, bool[,] visited)
        {
            List<(int X, int Y)> group = [];
            int color = _table[x, y];

            if (color == EmptyField)
            {
                return group;
            }

            Queue<(int X, int Y)> queue = new();
            queue.Enqueue((x, y));
            visited[x, y] = true;

            while (queue.Count > 0)
            {
                (int cx, int cy) = queue.Dequeue();
                group.Add((cx, cy));

                foreach ((int nx, int ny) in Neighbours(cx, cy))
                {
                    if (!visited[nx, ny] && _table[nx, ny] == color)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            return group;
        }

        private IEnumerable<(int X, int Y)> Neighbours(int x, int y)
        {
            if (x > 0)
            {
                yield return (x - 1, y);
            }

            if (x < TableSize - 1)
            {
                yield return (x + 1, y);
            }

            if (y > 0)
            {
                yield return (x, y - 1);
            }

            if (y < TableSize - 1)
            {
                yield return (x, y + 1);
            }
        }

        /// <summary>
        /// Oszloponként alulról felfelé "lejjebb csúsztatjuk" a nem üres mezőket.
        /// </summary>
        private void ApplyGravity()
        {
            for (int y = 0; y < TableSize; y++)
            {
                int writeRow = TableSize - 1;

                for (int x = TableSize - 1; x >= 0; x--)
                {
                    if (_table[x, y] != EmptyField)
                    {
                        _table[writeRow, y] = _table[x, y];
                        writeRow--;
                    }
                }

                for (int x = writeRow; x >= 0; x--)
                {
                    _table[x, y] = EmptyField;
                }
            }
        }

        private void FillEmptyFields()
        {
            for (int x = 0; x < TableSize; x++)
            {
                for (int y = 0; y < TableSize; y++)
                {
                    if (_table[x, y] == EmptyField)
                    {
                        _table[x, y] = RandomColor();
                    }
                }
            }
        }

        private void CheckGameOver()
        {
            if (HasValidStep())
            {
                return;
            }

            IsGameOver = true;
            OnGameOver();
        }

        #endregion

        #region Event raisers

        private void OnTableChanged() => TableChanged?.Invoke(this, EventArgs.Empty);

        private void OnStepPerformed() =>
            StepPerformed?.Invoke(this, new SameGameEventArgs(LastRemovedCount, Score));

        private void OnInvalidStep(int groupSize) =>
            InvalidStep?.Invoke(this, new SameGameEventArgs(groupSize, Score));

        private void OnGameOver() =>
            GameOver?.Invoke(this, new SameGameEventArgs(LastRemovedCount, Score));

        #endregion
    }
}

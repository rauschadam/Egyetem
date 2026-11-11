using SameGame.Model;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace SameGame.Test
{
    [TestClass]
    public sealed class SameGameModelTest
    {
        private const int N = SameGameModel.DefaultTableSize;

        private SameGameModel _model = null!;

        [TestInitialize]
        public void Initialize()
        {
            _model = new SameGameModel(new Random(42)); // fix seed: ismételhető teszt
        }

        /// <summary>
        /// Sakktábla-mintás 15x15-ös tábla: sehol nincs két szomszédos azonos szín.
        /// </summary>
        private static int[,] Checkerboard()
        {
            int[,] table = new int[N, N];
            for (int x = 0; x < N; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    table[x, y] = (x + y) % 2;
                }
            }

            return table;
        }

        [TestMethod]
        [DataRow(GameDifficulty.Easy)]
        [DataRow(GameDifficulty.Medium)]
        [DataRow(GameDifficulty.Hard)]
        public void NewGame_FillsTableWithValidColors(GameDifficulty difficulty)
        {
            bool tableChanged = false;
            _model.TableChanged += (_, _) => tableChanged = true;

            _model.NewGame(difficulty);

            Assert.IsTrue(tableChanged);
            Assert.AreEqual(N, _model.TableSize);
            Assert.AreEqual((int)difficulty, _model.ColorCount);
            Assert.AreEqual(0, _model.Score);
            for (int x = 0; x < N; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    Assert.IsTrue(_model[x, y] >= 0 && _model[x, y] < _model.ColorCount);
                }
            }
        }

        [TestMethod]
        public void Step_SmallGroup_IsInvalidAndTableUnchanged()
        {
            int[,] table = Checkerboard();
            table[0, 0] = 2;
            table[0, 1] = 2; // csak 2 korongos csoport
            _model.SetTable(table, GameDifficulty.Easy);

            int reportedSize = -1;
            bool tableChanged = false;
            _model.InvalidStep += (_, e) => reportedSize = e.GroupSize;
            _model.TableChanged += (_, _) => tableChanged = true;

            _model.Step(0, 0);

            Assert.AreEqual(2, reportedSize);
            Assert.IsFalse(tableChanged);
            Assert.AreEqual(0, _model.Score);
            Assert.AreEqual(2, _model[0, 0]);
        }

        [TestMethod]
        public void Step_ValidGroup_RemovesGroupAndDropsFieldsAbove()
        {
            int[,] table = Checkerboard();
            // az utolsó oszlop alsó 3 mezője egy 3 korongos (2-es színű) csoport
            table[N - 1, N - 1] = 2;
            table[N - 2, N - 1] = 2;
            table[N - 3, N - 1] = 2;
            table[N - 4, N - 1] = 0; // ez fölötte van, ennek kell leesnie a legaljára
            table[N - 4, N - 2] = 1;
            _model.SetTable(table, GameDifficulty.Easy);

            int removed = -1;
            _model.StepPerformed += (_, e) => removed = e.GroupSize;

            _model.Step(N - 2, N - 1);

            Assert.AreEqual(3, removed);
            Assert.AreEqual(3, _model.LastRemovedCount);
            Assert.AreEqual(3, _model.Score);
            Assert.AreEqual(0, _model[N - 1, N - 1]); // a felette lévő 0-s korong leesett
            Assert.AreEqual(table[0, N - 1], _model[3, N - 1]); // a legfelső eredeti korong 3 sorral lejjebb
            for (int x = 0; x < 3; x++)
            {
                Assert.IsTrue(_model[x, N - 1] >= 0 && _model[x, N - 1] < 3); // a teteje újra ki van töltve
            }

            Assert.AreEqual(table[N - 1, 0], _model[N - 1, 0]); // a többi oszlop nem változott
        }

        /// <summary>
        /// Olyan sakktábla, amelyen a 0. sor első három mezője egy 3 korongos (2-es színű) csoport.
        /// A lépés után a modell ezt a 3 mezőt pótolja; a FixedRandom a sakktábla eredeti
        /// színeit adja vissza, így a lépés után nem marad szabályos lépés.
        /// </summary>
        private SameGameModel ModelWithLastGroup()
        {
            SameGameModel model = new(new FixedRandom(0, 1, 0));
            int[,] table = Checkerboard();
            table[0, 0] = 2;
            table[0, 1] = 2;
            table[0, 2] = 2;
            model.SetTable(table, GameDifficulty.Easy);
            return model;
        }

        [TestMethod]
        public void Step_LastValidStep_RaisesGameOver()
        {
            SameGameModel model = ModelWithLastGroup();
            int finalScore = -1;
            model.GameOver += (_, e) => finalScore = e.Score;

            Assert.IsTrue(model.HasValidStep());
            model.Step(0, 1);

            Assert.IsTrue(model.IsGameOver);
            Assert.AreEqual(3, finalScore);
            Assert.IsFalse(model.HasValidStep());
        }

        [TestMethod]
        public void Step_AfterGameOver_DoesNothing()
        {
            SameGameModel model = ModelWithLastGroup();
            model.Step(0, 0);
            Assert.IsTrue(model.IsGameOver);

            bool anyEvent = false;
            model.TableChanged += (_, _) => anyEvent = true;
            model.StepPerformed += (_, _) => anyEvent = true;
            model.InvalidStep += (_, _) => anyEvent = true;

            model.Step(5, 5);

            Assert.IsFalse(anyEvent);
            Assert.AreEqual(3, model.Score);
        }

        [TestMethod]
        public void HasValidStep_Checkerboard_IsFalse()
        {
            _model.SetTable(Checkerboard(), GameDifficulty.Easy);
            Assert.IsFalse(_model.HasValidStep());
        }

        [TestMethod]
        public void Step_OutsideTable_Throws()
        {
            _model.NewGame(GameDifficulty.Easy);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _model.Step(N, 0));
        }
    }
}

namespace SameGame.Model
{
    /// <summary>
    /// Lépéssel kapcsolatos eseményargumentum (push stílus: viszi az adatot).
    /// </summary>
    public class SameGameEventArgs : EventArgs
    {
        /// <summary>
        /// Az érintett csoport mérete (érvényes lépésnél a törölt korongok száma).
        /// </summary>
        public int GroupSize { get; }

        /// <summary>
        /// Az összes eddig törölt korong száma.
        /// </summary>
        public int Score { get; }

        public SameGameEventArgs(int groupSize, int score)
        {
            GroupSize = groupSize;
            Score = score;
        }
    }
}

namespace SameGame.Test
{
    /// <summary>
    /// Előre megadott értékeket visszaadó "véletlen" generátor a determinisztikus tesztekhez.
    /// A Random.Next(int) virtuális, ezért felüldefiniálható.
    /// </summary>
    internal sealed class FixedRandom : Random
    {
        private readonly int[] _values;
        private int _index;

        public FixedRandom(params int[] values)
        {
            _values = values;
        }

        public override int Next(int maxValue)
        {
            int value = _values[_index % _values.Length];
            _index++;
            return value % maxValue;
        }
    }
}

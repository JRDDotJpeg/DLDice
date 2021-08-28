namespace DLDice
{
    internal class DiceSide
    {
        public int Value { get; }
        public bool Explodes { get; }

        public DiceSide(int value, bool explodes)
        {
            Value = value;
            Explodes = explodes;
        }
    }
}

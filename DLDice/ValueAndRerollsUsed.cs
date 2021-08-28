namespace DLDice
{
    public class ValueAndRerollsUsed
    {
        public int Successes { get; set; }

        public int RerollsUsed { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ValueAndRerollsUsed target)
            {
                return target.Successes == Successes && target.RerollsUsed == RerollsUsed;
            }
            return base.Equals(obj);
        }

        protected bool Equals(ValueAndRerollsUsed other)
        {
            return Successes == other.Successes && RerollsUsed == other.RerollsUsed;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Successes * 397) ^ RerollsUsed;
            }
        }
    }
}

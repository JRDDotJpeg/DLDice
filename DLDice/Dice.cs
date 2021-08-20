using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    class Dice
    {
        public List<DiceSide> Sides { get; } = new List<DiceSide>();

        private const int NumberOfSides = 6;
        public const decimal ProbabilityOfAnyGivenSide = 1 / (decimal) NumberOfSides;

        public Dice(int hitOn, diceColour colour)
        {
            var profile = new DiceColourProfile(colour);
            for (var i = 1; i <= 6; i++)
            {
                var value = 0;
                if (i >= hitOn) value = 1;
                if (i >= profile.WorthTwoOnYPlus) value = 2;
                var explodes = i >= profile.ExplodesOnZPlus;
                Sides.Add(new DiceSide(value, explodes));
            }

        }

    }

    public enum diceColour
    {
        black,
        blue,
        red
    };
}

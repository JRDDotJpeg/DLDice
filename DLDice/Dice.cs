using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    internal class Dice
    {
        public List<DiceSide> Sides { get; } = new List<DiceSide>();

        public decimal ProbabilityOfAnyGivenSide;


        /// <summary>
        /// Do not call directly, use the dice factory.
        /// </summary>
        /// <param name="hitOn"></param>
        /// <param name="colour"></param>
        /// <param name="numberOfSides"></param>
        public Dice(int hitOn, DiceColourProfile profile, int numberOfSides)
        {
            ProbabilityOfAnyGivenSide = 1 / (decimal)numberOfSides;
            
            for (var i = 1; i <= numberOfSides; i++)
            {
                var value = 0;
                var explodes = false;
                if (i >= hitOn)
                {
                    value = 1;
                    if (i >= profile.WorthTwoOnYPlus) value = 2;
                    explodes = i >= profile.ExplodesOnZPlus;
                }
                Sides.Add(new DiceSide(value, explodes));
            }
        }
    }

    public enum DiceColour
    {
        black,
        blue,
        red
    };
}

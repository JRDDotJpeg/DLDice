using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    internal interface IDiceFactory
    {
        Dice CreateDice(int numberOfSides, int hitOn, DiceColour colour);
    }
    internal class DiceFactory : IDiceFactory
    {
        public Dice CreateDice(int numberOfSides, int hitOn, DiceColour colour)
        {
            var profile = new DiceColourProfile(colour);
            return new Dice(hitOn,profile, numberOfSides);
        }
    }
}

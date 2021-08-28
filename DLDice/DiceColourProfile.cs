using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    internal class DiceColourProfile
    {
        public int WorthTwoOnYPlus = 7;
        public int ExplodesOnZPlus = 7;
        public DiceColourProfile(DiceColour colour)
        {
            if (colour == DiceColour.blue)
            {
                WorthTwoOnYPlus = 6;
            }
            if (colour == DiceColour.red)
            {
                WorthTwoOnYPlus = 6;
                ExplodesOnZPlus = 6;
            }
        }
    }
}

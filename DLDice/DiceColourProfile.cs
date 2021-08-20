using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    class DiceColourProfile
    {
        public int WorthTwoOnYPlus = 7;
        public int ExplodesOnZPlus = 7;
        public DiceColourProfile(diceColour colour)
        {
            if (colour == diceColour.blue)
            {
                WorthTwoOnYPlus = 6;
            }
            if (colour == diceColour.red)
            {
                WorthTwoOnYPlus = 6;
                ExplodesOnZPlus = 6;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    public struct DicePool
    {
        public int NumberOfDice { get; set; }

        public int HitOn { get; set; }

        public DiceColour DiceColour { get; set; }

        public int ReRolls { get; set; }
    }
}

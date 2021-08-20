using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    class DiceSide
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

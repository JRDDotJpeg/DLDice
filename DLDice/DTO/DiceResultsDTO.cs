using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice.DTO
{
    public class DiceResultsDTO
    {
        public Dictionary<int, decimal> Results { get; set; }

        public bool FoundError { get; set; } = false;

        public ErrorTypes ErrorType { get; set; }

        public string ErrorDetails { get; set; }
    }

    public enum ErrorTypes
    {
        TooManyDice,
        BadJson
}
}

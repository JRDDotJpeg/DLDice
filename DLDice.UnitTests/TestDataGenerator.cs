using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLDice.DTO;
using Newtonsoft.Json;

namespace DLDice.UnitTests
{
    /// <summary>
    /// This class is intended to be run manually when confidence in the DLDice lib is high.
    /// </summary>
    internal class TestDataGenerator
    {
        DLDice _dice = new DLDice();


        public void GenerateData()
        {

            var dto = new DiceDTO
            {
                DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = DiceColour.red
            });
            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = DiceColour.blue
            });
            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = DiceColour.black
            });
            

            var dtoAsJson = JsonConvert.SerializeObject(dto);

            var results = _dice.Calculator.CalculateResults(dto, 100);
            var resultsAsJson = JsonConvert.SerializeObject(results);
            var resultsBackFromJson = (DiceResultsDTO)JsonConvert.DeserializeObject(resultsAsJson, typeof(DiceResultsDTO));


            if (results.Results.Any(entry => results.Results[entry.Key] != resultsBackFromJson.Results[entry.Key]))
            {
                throw new Exception("Dictionaries do not match");
            }

            //var savedResultsAsJson = (Dictionary<int, decimal>)JsonConvert.DeserializeObject(ExpectedResultsFromDiceCalculatorService.TenBlackThreePlus, typeof(Dictionary<int, decimal>));
            //if (results.Results.Any(entry => results.Results[entry.Key] != savedResultsAsJson[entry.Key]))
            //{
            //    throw new Exception("Dictionaries do not match");
            //}
        }

        public static decimal AverageOfBlackRerollableDice()
        {
            return ((decimal)(0.5 + 0.5 + 0.5 + 1 + 1 + 1)) / (decimal) 6;
        }

        public static decimal AverageOfBlueRerollableDice()
        {
            return ((decimal)(((decimal)2 / (decimal)3) + ((decimal)2 / (decimal)3) + ((decimal)2/(decimal)3) + 1 + 1 + 2)) / (decimal)6;
        }

        public static decimal AverageOfRedRerollableDice()
        {
            return ((decimal)(0.8 + 0.8 + 0.8 + 1 + 1 + 2.8)) / (decimal)6;
        }

        public static string JsonPayload10OfEachDice4Plus =
            "{\"DicePools\":[{\"NumberOfDice\":10,\"HitOn\":4,\"DiceColour\":2,\"ReRolls\":0},{\"NumberOfDice\":10,\"HitOn\":4,\"DiceColour\":1,\"ReRolls\":0},{\"NumberOfDice\":10,\"HitOn\":4,\"DiceColour\":0,\"ReRolls\":0}]}";

    }
}

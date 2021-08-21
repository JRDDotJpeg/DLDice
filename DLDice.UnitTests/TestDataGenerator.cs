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


        public void GenerateData(DiceDTO dataDto = null)
        {
            // Update this object to the data required.
            if (dataDto is null)
            {
                dataDto = new DiceDTO
                {
                    BlackDice = 10,
                    BlueDice = 10,
                    RedDice = 10,
                    BlackDiceHitOn = 4,
                    BlueDiceHitOn = 4,
                    RedDiceHitOn = 4
                };
            }

            var results = _dice.Calculator.CalculateResults(dataDto);
            var resultsAsJson = JsonConvert.SerializeObject(results);
            var resultsBackFromJson = (DiceResultsDTO)JsonConvert.DeserializeObject(resultsAsJson, typeof(DiceResultsDTO));
            var savedResultsAsJson = (Dictionary<int,decimal>)JsonConvert.DeserializeObject(ExpectedResultsFromDiceCalculatorService.TenBlackFourPlus,typeof(Dictionary<int,decimal>));


            if (results.Results.Any(entry => results.Results[entry.Key] != resultsBackFromJson.Results[entry.Key]))
            {
                throw new Exception("Dictionaries do not match");
            }

            if (results.Results.Any(entry => results.Results[entry.Key] != savedResultsAsJson[entry.Key]))
            {
                throw new Exception("Dictionaries do not match");
            }
        }
    }
}

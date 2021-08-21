using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLDice.DTO;

namespace DLDice.API
{
    public interface IDiceCalculator
    {
        /// <summary>
        /// Calculates the possible Results of the specified dice.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        DiceResultsDTO CalculateResults(DiceDTO dto);
    }

    public class DiceCalculator : IDiceCalculator
    {
        private readonly IDiceCalculatorService _calculatorService;

        public DiceCalculator(IDiceCalculatorService service)
        {
            _calculatorService = service;
        }
        public DiceResultsDTO CalculateResults(DiceDTO dto)
        {
            var resultsBlack = _calculatorService.ResultsOfNDice(new DicePool { NumberOfDice = dto.BlackDice, HitOn = dto.BlackDiceHitOn, DiceColour = diceColour.black });
            var resultsBlue = _calculatorService.ResultsOfNDice(new DicePool { NumberOfDice = dto.BlueDice, HitOn = dto.BlueDiceHitOn, DiceColour = diceColour.blue});
            var resultsRed = _calculatorService.ResultsOfNDice(new DicePool { NumberOfDice = dto.RedDice, HitOn = dto.RedDiceHitOn, DiceColour = diceColour.red});

            var result = CombineResults(dto.BlackDice > 0, dto.BlueDice > 0, dto.RedDice > 0, resultsBlack, resultsBlue,
                resultsRed);

            return new DiceResultsDTO
            {
                Results = result
            };
        }

        private Dictionary<int, decimal> CombineResults(bool rollingBlack, bool rollingBlue, bool rollingRed, Dictionary<int, decimal> resultsBlack, Dictionary<int, decimal> resultsBlue, Dictionary<int, decimal> resultsRed)
        {
            Dictionary<int, decimal> result = null;
            if (rollingBlack)
            {
                result = resultsBlack;
                if (rollingBlue)
                {
                    result = HelperFunctions.Combine(result, resultsBlue);
                }
                if (rollingRed)
                {
                    result = HelperFunctions.Combine(result, resultsRed);
                }
            }
            else if (rollingBlue)
            {
                result = resultsBlue;
                if (rollingRed)
                {
                    result = HelperFunctions.Combine(result, resultsRed);
                }
            }
            else if (rollingRed)
            {
                result = resultsRed;
            }
            return result;
        }
    }
}

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
            var uncombinedResults = dto.DicePools.Select(dtoDicePool => _calculatorService.ResultsOfDicePool(dtoDicePool)).ToList();
            Dictionary<int, decimal> combinedResults = null;

            foreach (var resultSet in uncombinedResults)
            {
                if(combinedResults is null)
                {
                    combinedResults = resultSet;
                    continue;
                }

                combinedResults = HelperFunctions.Combine(combinedResults, resultSet);
            }

            return new DiceResultsDTO
            {
                Results = combinedResults
            };
        }
    }
}

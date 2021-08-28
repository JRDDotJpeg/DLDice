using DLDice.DTO;
using System.Collections.Generic;
using System.Linq;

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

    internal class DiceCalculator : IDiceCalculator
    {
        private readonly IDiceCalculatorService _calculatorService;

        public DiceCalculator(IDiceCalculatorService service)
        {
            _calculatorService = service;
        }

        public DiceResultsDTO CalculateResults(DiceDTO dto)
        {
            var uncombinedResults = dto.DicePools.Select(
                dtoDicePool => _calculatorService.ResultsOfDicePool(dtoDicePool))
                .ToList();

            var combinedResults =
                uncombinedResults.Aggregate(HelperFunctions.Combine);

            return new DiceResultsDTO
            {
                Results = combinedResults
            };
        }
    }
}

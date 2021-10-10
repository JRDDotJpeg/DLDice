using System;
using DLDice.DTO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

        /// <summary>
        /// Calculates the possible Results of the specified dice.
        /// </summary>
        /// <param name="json">Json object containing dice pools. See readme for details.</param>
        /// <returns></returns>
        DiceResultsDTO CalculateResults(string json);
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
            try
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
            catch (Exception e)
            {
                return new DiceResultsDTO
                {
                    FoundError = true,
                    ErrorDetails = e.ToString()
                };
            }
        }

        public DiceResultsDTO CalculateResults(string json)
        {
            try
            {
                var dto = (DiceDTO) JsonConvert.DeserializeObject(json, typeof(DiceDTO));
                return CalculateResults(dto);
            }
            catch (Exception ex)
            {
                return new DiceResultsDTO
                {
                    FoundError = true,
                    ErrorDetails = ex.ToString()
                };
            }
        }
    }
}

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
        /// <param name="maximumNumberOfDice"></param>
        /// <returns></returns>
        DiceResultsDTO CalculateResults(DiceDTO dto, int maximumNumberOfDice =100);

        /// <summary>
        /// Calculates the possible Results of the specified dice.
        /// </summary>
        /// <param name="json">Json object containing dice pools. See readme for details.</param>
        /// <param name="maximumNumberOfDice"></param>
        /// <returns></returns>
        DiceResultsDTO CalculateResults(string json, int maximumNumberOfDice = 100);
    }

    internal class DiceCalculator : IDiceCalculator
    {
        private readonly IDiceCalculatorService _calculatorService;

        public DiceCalculator(IDiceCalculatorService service)
        {
            _calculatorService = service;
        }

        public DiceResultsDTO CalculateResults(DiceDTO dto, int maximumNumberOfDice = 100)
        {
            try
            {
                var totalNumberOfDice = dto.DicePools.Sum(x => x.NumberOfDice);
                if (totalNumberOfDice > maximumNumberOfDice)
                {
                    return new DiceResultsDTO
                    {
                        FoundError = true,
                        ErrorType = ErrorTypes.TooManyDice,
                        ErrorDetails =
                            $"Total number of dice to be rolled{totalNumberOfDice} is greater than the maximum of {maximumNumberOfDice}"
                    };
                }

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

        public DiceResultsDTO CalculateResults(string json, int maximumNumberOfDice = 100)
        {
            try
            {
                var dto = (DiceDTO) JsonConvert.DeserializeObject(json, typeof(DiceDTO));
                return CalculateResults(dto, maximumNumberOfDice);
            }
            catch (Exception ex)
            {
                return new DiceResultsDTO
                {
                    FoundError = true,
                    ErrorType = ErrorTypes.BadJson,
                    ErrorDetails = ex.ToString()
                };
            }
        }
    }
}

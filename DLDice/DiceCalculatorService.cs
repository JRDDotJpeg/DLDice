using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DLDice
{
    internal interface IDiceCalculatorService
    {
        Dictionary<int, decimal> ResultsOfDicePool(DicePool dicePool);
    }

    internal class DiceCalculatorService : IDiceCalculatorService
    {
        private const int c_maxRolls = 10;
        private const int c_maxDicePerPool = 50;
        private readonly IDiceFactory _diceFactory;

        public DiceCalculatorService(IDiceFactory diceFactory)
        {
            _diceFactory = diceFactory;
        }

        /// <summary>
        /// Keys are all possible Results, corresponding values are the probability of that result
        /// </summary>
        /// <param name="dicePool"></param>
        /// <returns></returns>
        public Dictionary<int, decimal> ResultsOfDicePool(DicePool dicePool)
        {
            if (dicePool.NumberOfDice < 1 || dicePool.NumberOfDice > c_maxDicePerPool)
            {
                throw new InvalidDataException($"Number of dice specified outside of allowed range, maximum dice per pool is: {c_maxDicePerPool}");
            }
            if (dicePool.HitOn > 6 || dicePool.HitOn < 2)
            {
                throw new InvalidDataException($"Successes for HitOn out side of acceptable range, value:{dicePool.HitOn}");
            }
            
            // Rerollable dice aren't independent from each other.
            // Non rerollable dice are, as such we can use a much simpler algorithm
            // for non rerollable dice.
            return dicePool.ReRolls > 0 ?
                CreateResultsForRerollableDice(dicePool) :
                CreateResultsForNonrerollableDice(dicePool); 
        }

        private Dictionary<int, decimal> CreateResultsForRerollableDice(DicePool dicePool)
        {
            var dice = _diceFactory.CreateDice(6, dicePool.HitOn, dicePool.DiceColour);
            var resultsOfASingleDice = ResultsOfASingleDice(dice, false);
            var outComesUsingReroll = CreateRerollableDiceOutcomes(true, resultsOfASingleDice);
            var outComesNotUsingReroll = CreateRerollableDiceOutcomes(false, resultsOfASingleDice);

            var results = CreateRerollableDiceOutcomes(true, resultsOfASingleDice);
            for (var i = 2; i <= dicePool.NumberOfDice; i++)
            {
                var newOutcomes = new List<Dictionary<ValueAndRerollsUsed, decimal>>();
                foreach (var outcome in results)
                {
                    if (outcome.Key.RerollsUsed < dicePool.ReRolls)
                    {
                        //combine with reroll able dice
                        newOutcomes.Add(HelperFunctions.CombineSingleOutcomeAndNumberOfRerolls(outComesUsingReroll,
                            outcome.Key, outcome.Value));
                    }
                    else
                    {
                        //combine with non rerollable dice
                        newOutcomes.Add(HelperFunctions.CombineSingleOutcomeAndNumberOfRerolls(outComesNotUsingReroll,
                            outcome.Key, outcome.Value));
                    }
                }
                results = newOutcomes.Aggregate(HelperFunctions.AddToDictionaryOrSumWithExisting);
            }

            HelperFunctions.CheckProbability(results.Values);
            var convertedResults = RemoveInformationAboutRerollsUsedFromData(results);

            HelperFunctions.CheckProbability(convertedResults.Values);
            return convertedResults;
        }

        private static Dictionary<int, decimal> RemoveInformationAboutRerollsUsedFromData(Dictionary<ValueAndRerollsUsed, decimal> results)
        {
            var convertedResults = new Dictionary<int, decimal>();
            foreach (var pair in results)
            {
                HelperFunctions.AddToDictionaryOrSumWithExisting(convertedResults, pair.Key.Successes,
                    pair.Value);
            }

            return convertedResults;
        }

        private Dictionary<int, decimal> CreateResultsForNonrerollableDice(DicePool dicePool)
        {
            var dice = _diceFactory.CreateDice(6, dicePool.HitOn, dicePool.DiceColour);
            var results = ResultsOfASingleDice(dice, false);
            var resultsOfASingleDice = ResultsOfASingleDice(dice, false);

            for (var i = 2; i <= dicePool.NumberOfDice; i++)
            {
                results = HelperFunctions.Combine(results, resultsOfASingleDice);
            }

            HelperFunctions.CheckProbability(results.Values);
            return results;
        }

        private static Dictionary<ValueAndRerollsUsed, decimal> CreateRerollableDiceOutcomes(
            bool canUseReroll,
            Dictionary<int,decimal> resultsOfASingleDice)
        {
            var newOutcomes = new List<Dictionary<ValueAndRerollsUsed, decimal>>();
            foreach (var outcome in resultsOfASingleDice)
            {
                Dictionary<ValueAndRerollsUsed, decimal> newOutcome;
                if (outcome.Key > 0) // good result don't reroll
                {
                    newOutcome = new Dictionary<ValueAndRerollsUsed, decimal>
                    {
                        {
                            new ValueAndRerollsUsed
                            {
                                Successes = outcome.Key,
                                RerollsUsed = 0
                            },
                            outcome.Value
                        }
                    };
                }
                else if (canUseReroll)
                {
                    newOutcome = HelperFunctions.CombineSingleProbabilityAndNumberOfRerolls(resultsOfASingleDice,
                        0, outcome.Value, 1);
                }
                else
                {
                    //can't reroll
                    newOutcome = new Dictionary<ValueAndRerollsUsed, decimal>
                    {
                        {
                            new ValueAndRerollsUsed
                            {
                                Successes = outcome.Key,
                                RerollsUsed = 0
                            }, 
                            outcome.Value
                        }
                    };
                }
                newOutcomes.Add(newOutcome);
            }

            var combinedNewOutcomes = new Dictionary<ValueAndRerollsUsed, decimal>();
            combinedNewOutcomes = newOutcomes.Aggregate(combinedNewOutcomes, HelperFunctions.AddToDictionaryOrSumWithExisting);
            return combinedNewOutcomes;
        }

        /// <summary>
        /// Keys are all possible Results, corresponding values are the probability of that result
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, decimal> ResultsOfASingleDice(Dice dice, bool canReroll)
        {
            return RollOneDice(dice, 1, 0, canReroll);
        }

        /// <summary>
        /// Recursive function, should be called with 1,0. 
        /// </summary>
        /// <param name="dice"></param>
        /// <param name="numberOfRolls"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        private Dictionary<int, decimal> RollOneDice(Dice dice, int numberOfRolls, int currentValue, bool canReroll)
        {
            var results = new Dictionary<int, decimal>();
            foreach (var side in dice.Sides)
            {
                var newValue = currentValue + side.Value;
                if (side.Value == 0 && canReroll)
                {
                    AddResultsOfDiceProducedByTheExplosion(numberOfRolls, results, newValue, 1, dice);
                } 
                else if (side.Explodes && numberOfRolls < c_maxRolls)
                {
                    var numberOfDiceProducedByExplosion = 1; // This is left incase we want to readd this feature in future.
                    AddResultsOfDiceProducedByTheExplosion(numberOfRolls, results, newValue, numberOfDiceProducedByExplosion, dice);
                }
                else
                {
                    HelperFunctions.AddToDictionaryOrSumWithExisting(results, newValue, HelperFunctions.Power(dice.ProbabilityOfAnyGivenSide, numberOfRolls));
                }
            }
            return results;
        }

        /// <summary>
        /// This allows dice to explode into more than one dice
        /// </summary>
        /// <param name="numberOfRolls"></param>
        /// <param name="results"></param>
        /// <param name="newValue"></param>
        /// <param name="numberOfDice"></param>
        /// <param name="dice"></param>
        private void AddResultsOfDiceProducedByTheExplosion(int numberOfRolls, Dictionary<int, Decimal> results, int newValue, int numberOfDice, Dice dice)
        {
            var probabilityFactorForSplittingDice = (decimal)1 / (decimal)numberOfDice;
            for (var i = 1; i <= numberOfDice; i++)
            {
                foreach (var pair in RollOneDice(dice, numberOfRolls + 1, newValue, false))
                {
                    var modifiedProbability = pair.Value * probabilityFactorForSplittingDice;
                    HelperFunctions.AddToDictionaryOrSumWithExisting(results, pair.Key, modifiedProbability);
                }
            }
        }
    }
}

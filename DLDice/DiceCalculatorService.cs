using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    public interface IDiceCalculatorService
    {
        Dictionary<int, decimal> ResultsOfDicePool(DicePool dicePool);
    }

    public class DiceCalculatorService : IDiceCalculatorService
    {
        private const int c_maxRolls = 10;
        private static Dictionary<int, decimal> _resultsOfASingleDice; // This is not thread safe and needs to be reworked.

        /// <summary>
        /// Keys are all possible Results, corresponding values are the probability of that result
        /// </summary>
        /// <param name="dicePool"></param>
        /// <returns></returns>
        public Dictionary<int, decimal> ResultsOfDicePool(DicePool dicePool)
        {
            if (dicePool.NumberOfDice < 1 || dicePool.NumberOfDice > 50) return new Dictionary<int, decimal>();
            if (dicePool.HitOn > 6 || dicePool.HitOn < 1)
            {
                throw new InvalidDataException($"Successes for HitOn out side of acceptable range, value:{dicePool.HitOn}");
            }

            var dice = new Dice(dicePool.HitOn, dicePool.DiceColour);
            
            _resultsOfASingleDice = ResultsOfASingleDice(dice, false); // TODO remove
            return dicePool.ReRolls > 0 ?
                CreateResultsForRerollableDice(dicePool) :
                CreateResultsForNonrerollableDice(dicePool, dice);
        }

        private static Dictionary<int, decimal> CreateResultsForRerollableDice(DicePool dicePool)
        {
            var outComesUsingReroll = CreateRerollableDiceOutcomes(true);
            var outComesNotUsingReroll = CreateRerollableDiceOutcomes(false);

            var results = CreateRerollableDiceOutcomes(true);
            for (var i = 2; i <= dicePool.NumberOfDice; i++)
            {
                var cases = new List<Dictionary<ValueAndRerollsUsed, decimal>>();
                foreach (var outcome in results)
                {
                    if (outcome.Key.RerollsUsed < dicePool.ReRolls)
                    {
                        //combine with reroll able dice
                        cases.Add(HelperFunctions.CombineSingleOutcomeAndNumberOfRerolls(outComesUsingReroll,
                            outcome.Key, outcome.Value));
                    }
                    else
                    {
                        //combine with non rerollable dice
                        cases.Add(HelperFunctions.CombineSingleOutcomeAndNumberOfRerolls(outComesNotUsingReroll,
                            outcome.Key, outcome.Value));
                    }
                }
                results = cases.Aggregate(HelperFunctions.AddToDictionaryOrSumWithExisting);
            }

            HelperFunctions.CheckProbability(results.Values);
            var convertedResults = new Dictionary<int, decimal>();
            foreach (var pair in results)
            {
                HelperFunctions.AddToDictionaryOrSumWithExisting(convertedResults, pair.Key.Successes,
                    pair.Value);
            }

            HelperFunctions.CheckProbability(convertedResults.Values);
            return convertedResults;
        }

        private Dictionary<int, decimal> CreateResultsForNonrerollableDice(DicePool dicePool, Dice dice)
        {
            var results = ResultsOfASingleDice(dice, false);
            for (var i = 2; i <= dicePool.NumberOfDice; i++)
            {
                results = HelperFunctions.Combine(results, _resultsOfASingleDice);
            }

            HelperFunctions.CheckProbability(results.Values);
            return results;
        }

        private static Dictionary<ValueAndRerollsUsed, decimal> CreateRerollableDiceOutcomes(bool canUseReroll)
        {
            var cases = new List<Dictionary<ValueAndRerollsUsed, decimal>>();
            foreach (var outcome in _resultsOfASingleDice)
            {
                Dictionary<ValueAndRerollsUsed, decimal> myCase;
                if (outcome.Key > 0) // good result don't reroll
                {
                    myCase = new Dictionary<ValueAndRerollsUsed, decimal>
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
                    // reroll
                    // There's time saving to be made in replacing this functional call with a local implementation.
                    // The function is defensive while local code could be optimistic.
                    myCase = HelperFunctions.CombineSingleProbabilityAndNumberOfRerolls(_resultsOfASingleDice,
                        0, outcome.Value, 1);
                }
                else
                {
                    //can't reroll
                    myCase = new Dictionary<ValueAndRerollsUsed, decimal>
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
                cases.Add(myCase);
            }

            var combinedCases = new Dictionary<ValueAndRerollsUsed, decimal>();
            combinedCases = cases.Aggregate(combinedCases, HelperFunctions.AddToDictionaryOrSumWithExisting);
            return combinedCases;
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
                    var numberOfDiceProducedByExplosion = 1; //todo remove
                    AddResultsOfDiceProducedByTheExplosion(numberOfRolls, results, newValue, numberOfDiceProducedByExplosion, dice);
                }
                else
                {
                    HelperFunctions.AddToDictionaryOrSumWithExisting(results, newValue, HelperFunctions.Power(Dice.ProbabilityOfAnyGivenSide, numberOfRolls));
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

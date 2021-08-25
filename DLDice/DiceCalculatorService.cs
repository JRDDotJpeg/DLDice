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
        public Dictionary<int, decimal> ResultsOfDicePool(DicePool dicePool, bool useRecursiveFunction = false)
        {
            if (dicePool.NumberOfDice < 1 || dicePool.NumberOfDice > 50 || dicePool.ReRolls > 0 && dicePool.NumberOfDice > 20) return new Dictionary<int, decimal>();

            if (dicePool.HitOn > 6 || dicePool.HitOn < 1)
            {
                throw new InvalidDataException($"Successes for HitOn out side of acceptable range, value:{dicePool.HitOn}");
            }

            var dice = new Dice(dicePool.HitOn, dicePool.DiceColour);
            
            _resultsOfASingleDice = ResultsOfASingleDice(dice, false);
            if (dicePool.ReRolls > 0)
            {
                if (useRecursiveFunction) {
                    var resultsOfADiceThatIsRerollable = ResultsOfASingleDice(dice, true);
                    var finalResult = new Dictionary<int, decimal>();


                    RerollFun(ref finalResult, dicePool.ReRolls, dicePool.NumberOfDice);



                    HelperFunctions.CheckProbability(finalResult.Values);
                    return finalResult;
                }
                else
                {
                    var nonRecursiveResult = new Dictionary<ValueAndRerollsUsed, decimal>();
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
                                cases.Add(HelperFunctions.CombineSingleOutcomeAndNumberOfRerolls(outComesUsingReroll, outcome.Key, outcome.Value));

                            }
                            else
                            {
                                //combine with non rerollable dice
                                cases.Add(HelperFunctions.CombineSingleOutcomeAndNumberOfRerolls(outComesNotUsingReroll, outcome.Key, outcome.Value));
                            }
                        }
                        var combinedCases = new Dictionary<ValueAndRerollsUsed, decimal>();
                        combinedCases = cases.Aggregate(HelperFunctions.AddToDictionaryOrSumWithExisting);
                        results = combinedCases;

                    }
                    HelperFunctions.CheckProbability(results.Values);
                    var convertedResults = new Dictionary<int,decimal>();
                    foreach (var pair in results)
                    {
                        HelperFunctions.AddToDictionaryOrSumWithExisting(convertedResults, pair.Key.Successes,
                            pair.Value);
                    }
                    HelperFunctions.CheckProbability(convertedResults.Values);

                    return convertedResults;
                }
            }
            else
            {
                var results = ResultsOfASingleDice(dice, false);
                for (var i = 2; i <= dicePool.NumberOfDice; i++)
                {
                    results = HelperFunctions.Combine(results, _resultsOfASingleDice);
                }

                HelperFunctions.CheckProbability(results.Values);
                return results;
            }
            
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

        private static void RerollFun(ref Dictionary<int, decimal> currentResults, int rerollsLeft, int diceLeft)
        {
            if(diceLeft == 0) return;
            diceLeft--;
            var cases = new List<Dictionary<int, decimal>>();
            foreach (var outcome in _resultsOfASingleDice) //todo class member
            {
                Dictionary<int,decimal> myCase;
                if (outcome.Key > 0)
                {
                    // Add outcome
                    myCase = new Dictionary<int, decimal> {{outcome.Key, outcome.Value } };
                    // Move on to next dice
                    if(diceLeft > 0) RerollFun(ref myCase, rerollsLeft, diceLeft);
                }
                else // result we want to reroll
                {
                    if (rerollsLeft > 0)
                    {
                        // add the results of the reroll
                        rerollsLeft--;
                        myCase = HelperFunctions.CombineSingleProbability(_resultsOfASingleDice, 0, outcome.Value);
                        // move on to next dice in pool
                        if (diceLeft > 0) RerollFun(ref myCase, rerollsLeft, diceLeft);
                    }
                    else
                    {
                        myCase = new Dictionary<int, decimal> { { outcome.Key, outcome.Value } };
                        for (var i = 1; i <= diceLeft; i++)
                        {
                            myCase = HelperFunctions.Combine(myCase, _resultsOfASingleDice);
                        }
                    }
                }
                cases.Add(myCase);
            }


            var combinedCases = new Dictionary<int, decimal>();
            combinedCases = cases.Aggregate(combinedCases, HelperFunctions.AddToDictionaryOrSumWithExisting);

            currentResults = AddAndMultiply(currentResults, combinedCases);

        }

        private static Dictionary<int, decimal> AddAndMultiply(Dictionary<int, decimal> target, Dictionary<int, decimal> extraData)
        {
            var result = new Dictionary<int,decimal>();
            if (!target.Any())
            {
                foreach (var outcome in extraData)
                {
                    result.Add(outcome.Key, outcome.Value);
                }
                return result;
            }
            else
            {
                foreach (var pair in target)
                {
                    foreach (var outcome in extraData)
                    {
                        HelperFunctions.AddToDictionaryOrSumWithExisting(result,
                            pair.Key + outcome.Key, pair.Value * outcome.Value);
                    }
                }

                return result;
            }
        }

        private static Dictionary<ValueAndRerollsUsed, decimal> AddAndMultiply(Dictionary<ValueAndRerollsUsed, decimal> target, Dictionary<ValueAndRerollsUsed, decimal> extraData)
        {
            var result = new Dictionary<ValueAndRerollsUsed, decimal>();
            if (!target.Any())
            {
                foreach (var outcome in extraData)
                {
                    result.Add(outcome.Key, outcome.Value);
                }
                return result;
            }
            else
            {
                foreach (var pair in target)
                {
                    foreach (var outcome in extraData)
                    {
                        var newKey = new ValueAndRerollsUsed
                        {
                            Successes = pair.Key.Successes + outcome.Key.Successes,
                            RerollsUsed = pair.Key.RerollsUsed + outcome.Key.RerollsUsed,
                        };
                        HelperFunctions.AddToDictionaryOrSumWithExisting(
                            result, newKey, pair.Value * outcome.Value);
                    }
                }

                return result;
            }
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

        public Dictionary<int, decimal> ResultsOfDicePool(DicePool dicePool)
        {
            return ResultsOfDicePool(dicePool, false);
        }
    }
}

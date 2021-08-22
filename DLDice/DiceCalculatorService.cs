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
        private const int c_maxRolls = 3;

        /// <summary>
        /// Keys are all possible Results, corresponding values are the probability of that result
        /// </summary>
        /// <param name="dicePool"></param>
        /// <returns></returns>
        public Dictionary<int, decimal> ResultsOfDicePool(DicePool dicePool)
        {
            if (dicePool.NumberOfDice < 1 || dicePool.NumberOfDice > 50 || dicePool.ReRolls > 0 && dicePool.NumberOfDice > 20) return new Dictionary<int, decimal>();

            if (dicePool.HitOn > 6 || dicePool.HitOn < 1)
            {
                throw new InvalidDataException($"Value for HitOn out side of acceptable range, value:{dicePool.HitOn}");
            }

            var dice = new Dice(dicePool.HitOn, dicePool.DiceColour);
            var results = ResultsOfASingleDice(dice, false);
            var resultsOfASingleDiceThatCantBeRerolled = ResultsOfASingleDice(dice, false);
            if (dicePool.ReRolls > 0)
            {
                var resultsOfADiceThatIsRerollable = ResultsOfASingleDice(dice, true);
                var finalResult = new Dictionary<int, decimal>();


                RerollFun(resultsOfASingleDiceThatCantBeRerolled,ref finalResult, dicePool.ReRolls, dicePool.NumberOfDice);



                HelperFunctions.CheckProbability(finalResult);
                return finalResult;
            }
            else
            {
                for (var i = 2; i <= dicePool.NumberOfDice; i++)
                {
                    results = HelperFunctions.Combine(results, resultsOfASingleDiceThatCantBeRerolled);
                }

                HelperFunctions.CheckProbability(results);
                return results;
            }
            
        }

        private static void RerollFun(Dictionary<int, decimal> resultsOfASingleDiceThatCantBeRerolled, ref Dictionary<int, decimal> currentResults, int rerollsLeft, int diceLeft)
        {
            if(diceLeft == 0) return;
            diceLeft--;
            var cases = new List<Dictionary<int, decimal>>();
            foreach (var outcome in resultsOfASingleDiceThatCantBeRerolled) //todo class member
            {
                Dictionary<int,decimal> myCase;
                if (outcome.Key > 0)
                {
                    // Add outcome
                    myCase = new Dictionary<int, decimal> {{outcome.Key, outcome.Value } };
                    // Move on to next dice
                    RerollFun(resultsOfASingleDiceThatCantBeRerolled, ref myCase, rerollsLeft, diceLeft);
                }
                else // result we want to reroll
                {
                    if (rerollsLeft > 0)
                    {
                        // add the results of the reroll
                        rerollsLeft--;
                        myCase = HelperFunctions.CombineSingleProbability(resultsOfASingleDiceThatCantBeRerolled, 0, outcome.Value);
                        // move on to next dice in pool
                        RerollFun(resultsOfASingleDiceThatCantBeRerolled, ref myCase, rerollsLeft, diceLeft);
                    }
                    else
                    {
                        myCase = new Dictionary<int, decimal> { { outcome.Key, outcome.Value } };
                        RerollFun(resultsOfASingleDiceThatCantBeRerolled, ref myCase, rerollsLeft, diceLeft);
                    }
                }
                cases.Add(myCase);
            }


            var combinedCases = new Dictionary<int, decimal>();
            foreach (var casee in cases)
            {
                combinedCases = HelperFunctions.AddToDictionaryOrSumWithExisting(combinedCases, casee);
            }
                
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

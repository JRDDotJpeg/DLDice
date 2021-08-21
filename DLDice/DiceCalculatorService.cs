﻿using System;
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
        Dictionary<int, decimal> ResultsOfNDice(DicePool dicePool);
    }

    public class DiceCalculatorService : IDiceCalculatorService
    {
        private const int c_maxRolls = 10;

        /// <summary>
        /// Keys are all possible Results, corresponding values are the probability of that result
        /// </summary>
        /// <param name="dicePool"></param>
        /// <returns></returns>
        public Dictionary<int, decimal> ResultsOfNDice(DicePool dicePool)
        {
            if (dicePool.NumberOfDice < 1 || dicePool.NumberOfDice > 50) return new Dictionary<int, decimal>();

            if (dicePool.HitOn > 6 || dicePool.HitOn < 1)
            {
                throw new InvalidDataException($"Value for HitOn out side of acceptable range, value:{dicePool.HitOn}");
            }

            var dice = new Dice(dicePool.HitOn, dicePool.DiceColour);
            var results = ResultsOfASingleDice(dice);
            var resultsFromASingleDice = ResultsOfASingleDice(dice);

            for (var i = 2; i <= dicePool.NumberOfDice; i++)
            {
                results = HelperFunctions.Combine(results, resultsFromASingleDice);
            }

            HelperFunctions.CheckProbability(results);
            return results;
        }

        /// <summary>
        /// Keys are all possible Results, corresponding values are the probability of that result
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, decimal> ResultsOfASingleDice(Dice dice)
        {
            return RollOneDice(dice, 1, 0);
        }

        /// <summary>
        /// Recursive function, should be called with 1,0. 
        /// </summary>
        /// <param name="dice"></param>
        /// <param name="numberOfRolls"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        private Dictionary<int, decimal> RollOneDice(Dice dice, int numberOfRolls = 1, int currentValue = 0)
        {
            var results = new Dictionary<int, decimal>();
            foreach (var side in dice.Sides)
            {
                var newValue = currentValue + side.Value;

                if (side.Explodes && numberOfRolls < c_maxRolls)
                {
                    var numberOfDiceProducedByExplosion = 1;
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
                foreach (var pair in RollOneDice(dice, numberOfRolls + 1, newValue))
                {
                    var modifiedProbability = pair.Value * probabilityFactorForSplittingDice;
                    HelperFunctions.AddToDictionaryOrSumWithExisting(results, pair.Key, modifiedProbability);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    public interface IDiceCalculatorService
    {
        Dictionary<int, decimal> ResultsOfNDice(int n, int hitOn, diceColour colour);
    }

    public class DiceCalculatorService : IDiceCalculatorService
    {
        private const int m_maxRolls = 10;
        private const bool m_devastatingOrdnance = false; //toto remove

        /// <summary>
        /// Keys are all possible results, corresponding values are the probability of that result
        /// </summary>
        /// <param name="n"></param> //todo pass in dice
        /// <param name="hitOn"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public Dictionary<int, decimal> ResultsOfNDice(int n, int hitOn, diceColour colour)
        {
            if (n == 0) return new Dictionary<int, decimal>();

            var dice = new Dice(hitOn, colour);
            var results = ResultsOfASingleDice(dice);
            var resultsFromASingleDice = ResultsOfASingleDice(dice);

            for (var i = 2; i <= n; i++)
            {
                results = HelperFunctions.Combine(results, resultsFromASingleDice);
            }

            HelperFunctions.CheckProbability(results);
            return results;
        }
        
        //private static List<DiceSide> CreateDice(int hitOn, diceColour colour)
        //{
        //    var profile = new DiceColourProfile(colour);
        //    var sides = new List<DiceSide>();
        //    for (var i = 1; i <= 6; i++)
        //    {
        //        var value = 0;
        //        if (i >= hitOn) value = 1;
        //        if (i >= profile.WorthTwoOnYPlus) value = 2;
        //        var explodes = i >= profile.ExplodesOnZPlus;
        //        sides.Add(new DiceSide(value, explodes));
        //    }
        //    return sides;
        //}


        /// <summary>
        /// Keys are all possible results, corresponding values are the probability of that result
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

                if (side.Explodes && numberOfRolls < m_maxRolls)
                {
                    var numberOfDiceProducedByExplosion = 1;
                    if (m_devastatingOrdnance && numberOfRolls == 1) numberOfDiceProducedByExplosion = 2;
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

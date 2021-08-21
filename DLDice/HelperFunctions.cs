using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLDice.DTO;

namespace DLDice
{
    public class HelperFunctions
    {
        /// <summary>
        /// Checks if the target contains the passed key if so add the passed value to the existing value.
        /// Otherwise adds the key and value to the dictionary
        /// </summary>
        /// <param name="target"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// 
        public static void AddToDictionaryOrSumWithExisting(Dictionary<int, decimal> target, int key, decimal value)
        {
            if (!target.ContainsKey(key))
            {
                target.Add(key, value);
            }
            else
            {
                target[key] += value;
            }

        }

        /// <summary>
        /// Like math.pow but simpler and works for decimals
        /// </summary>
        /// <param name="value"></param>
        /// <param name="toThe"></param>
        /// <returns></returns>
        public static decimal Power(decimal value, int toThe)
        {
            var result = value;
            for (var i = 1; i < toThe; i++)
            {
                result *= value;
            }
            return result;
        }


        /// <summary>
        /// Checks that the combined probabilities sum to between 0.999 and 1.001
        /// </summary>
        /// <param name="results"></param>
        public static void CheckProbability(Dictionary<int, decimal> results)
        {
            decimal totalProbability = 0;
            foreach (var val in results.Values) totalProbability += val;
            if (totalProbability > (decimal)1.001 || totalProbability < (decimal)0.999) throw new Exception("Total probability did not total to 1 probability was: " + totalProbability.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Combines two probability dictionaries into a single result.
        /// </summary>
        public static Dictionary<int, decimal> Combine(Dictionary<int, decimal> resultsA, Dictionary<int, decimal> resultsB)
        {
            var tempResults = new Dictionary<int, decimal>();
            foreach (var resultsPair in resultsA)
            {
                foreach (var dicePair in resultsB)
                {
                    var combinedValue = resultsPair.Key + dicePair.Key;
                    var combinedProbability = resultsPair.Value * dicePair.Value;
                    AddToDictionaryOrSumWithExisting(tempResults, combinedValue, combinedProbability);
                }
            }
            return tempResults;
        }
    }
}

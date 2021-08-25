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

        public static void AddToDictionaryOrSumWithExisting(Dictionary<ValueAndRerollsUsed, decimal> target, ValueAndRerollsUsed key, decimal value)
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
        /// For each key in dictionary 2
        /// Checks if the target contains the passed key if so add the passed value to the existing value.
        /// Otherwise adds the key and value to the dictionary
        /// </summary>
        /// <param name="target"></param>
        /// 
        public static Dictionary<int, decimal> AddToDictionaryOrSumWithExisting(Dictionary<int, decimal> target, Dictionary<int, decimal> data)
        {
            foreach (var keyValuePair in data)
            {
                AddToDictionaryOrSumWithExisting(target, keyValuePair.Key, keyValuePair.Value);
            }

            return target;
        }
        public static Dictionary<ValueAndRerollsUsed, decimal> AddToDictionaryOrSumWithExisting(Dictionary<ValueAndRerollsUsed, decimal> target, Dictionary<ValueAndRerollsUsed, decimal> data)
        {
            foreach (var keyValuePair in data)
            {
                AddToDictionaryOrSumWithExisting(target, keyValuePair.Key, keyValuePair.Value);
            }

            return target;
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
        public static void CheckProbability(IEnumerable<decimal> results)
        {
            var totalProbability = results.Sum();
            if (totalProbability > (decimal)1.0001 || totalProbability < (decimal)0.9999) throw new Exception("Total probability did not total to 1 probability was: " + totalProbability.ToString(CultureInfo.InvariantCulture));
        }


        /// <summary>
        /// Combines two probability dictionaries into a single result.
        /// </summary>
        public static Dictionary<int, decimal> Combine(Dictionary<int, decimal> resultsA, Dictionary<int, decimal> resultsB)
        {
            var tempResults = new Dictionary<int, decimal>();
            if (!resultsA.Any())
            {
                foreach (var dicePair in resultsB)
                {
                    tempResults.Add(dicePair.Key, dicePair.Value);
                }
            }
            else
            {
                foreach (var resultsPair in resultsA)
                {
                    foreach (var dicePair in resultsB)
                    {
                        var combinedValue = resultsPair.Key + dicePair.Key;
                        var combinedProbability = resultsPair.Value * dicePair.Value;
                        //AddToDictionaryOrSumWithExisting(tempResults, combinedValue, combinedProbability);
                        if (tempResults.ContainsKey(combinedValue))
                        {
                            tempResults[combinedValue] += combinedProbability;
                        }
                        else
                        {
                            tempResults.Add(combinedValue, combinedProbability);
                        }
                    }
                }
            }
            return tempResults;
        }

        /// <summary>
        /// Combines two probability dictionaries into a single result.
        /// If the dictionary is empty, adds the value prob pair as the only item.
        /// </summary>
        public static Dictionary<int, decimal> CombineSingleProbability(Dictionary<int, decimal> resultsA, int value,decimal prob)
        {
            var tempResults = new Dictionary<int, decimal>();

            if (!resultsA.Any())
            {
                tempResults.Add(value, prob);
            }
            else
            {
                foreach (var resultsPair in resultsA)
                {
                    var combinedValue = resultsPair.Key + value;
                    var combinedProbability = resultsPair.Value * prob;
                    AddToDictionaryOrSumWithExisting(tempResults, combinedValue, combinedProbability);
                }
            }
            return tempResults;
        }

        public static Dictionary<ValueAndRerollsUsed, decimal> CombineSingleProbabilityAndNumberOfRerolls(Dictionary<int, decimal> resultsA, int value, decimal prob, int rerollsUsed)
        {
            var tempResults = new Dictionary<ValueAndRerollsUsed, decimal>();

            if (!resultsA.Any())
            {
                tempResults.Add(
                    new ValueAndRerollsUsed
                    {
                        Successes = value,
                        RerollsUsed = rerollsUsed
                    }, prob);
            }
            else
            {
                foreach (var resultsPair in resultsA)
                {
                    var combinedValue = resultsPair.Key + value;
                    var combinedProbability = resultsPair.Value * prob;
                    var newKey = new ValueAndRerollsUsed
                    {
                        Successes = combinedValue,
                        RerollsUsed = rerollsUsed
                    };
                    if (tempResults.ContainsKey(newKey))
                    {
                        tempResults[newKey] += combinedProbability;
                    }
                    else
                    {
                        tempResults.Add(newKey, combinedProbability);
                    }
                }
            }
            return tempResults;
        }

        public static Dictionary<ValueAndRerollsUsed, decimal>
            CombineSingleOutcomeAndNumberOfRerolls(
                Dictionary<ValueAndRerollsUsed, decimal> resultsA,
                ValueAndRerollsUsed outcome,
                decimal prob)
        {
            var tempResults = new Dictionary<ValueAndRerollsUsed, decimal>();

            if (!resultsA.Any())
            {
                tempResults.Add(
                    outcome, prob);
            }
            else
            {
                foreach (var resultsPair in resultsA)
                {
                    var combinedSuccesses = resultsPair.Key.Successes + outcome.Successes;
                    var combinedProbability = resultsPair.Value * prob;
                    var combinedRerollsUsed = resultsPair.Key.RerollsUsed + outcome.RerollsUsed;
                    var newKey = new ValueAndRerollsUsed
                    {
                        Successes = combinedSuccesses,
                        RerollsUsed = combinedRerollsUsed
                    };
                    if (tempResults.ContainsKey(newKey))
                    {
                        tempResults[newKey] += combinedProbability;
                    }
                    else
                    {
                        tempResults.Add(newKey, combinedProbability);
                    }
                }
            }
            return tempResults;
        }

        public static decimal CalculateAverage(Dictionary<int, decimal> data)
        {
            return data.Sum(pair => pair.Value * pair.Key);
        }
    }
}

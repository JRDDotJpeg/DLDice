using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using DLDice;

namespace DLDice.UnitTests
{
    [TestClass]
    public class DiceCalculatorServiceTests
    {
        private static bool CompareResults(Dictionary<int, decimal> res, string expectedResultsJson)
        {
            var expectedResultsDto = JsonConvert.DeserializeObject<Dictionary<int, decimal>>(expectedResultsJson);
            return res.All(entry =>
                res[entry.Key] == expectedResultsDto[entry.Key]);
        }

        private static bool CompareResults(decimal resultA, decimal resultB, int roundToThisManyDP = 5)
        {
            resultA = Math.Round(resultA, decimals: roundToThisManyDP);
            resultB = Math.Round(resultB, decimals: roundToThisManyDP);
            return resultA == resultB;

        }

        [TestMethod]
        public void ResultsOfNDice_10Black4Plus()
        {
            var tenBlack = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = DiceColour.black };
            
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(tenBlack);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculatorService.TenBlackFourPlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void ResultsOfNDice_10Blue4Plus()
        {
            var tenBlue = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = DiceColour.blue };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(tenBlue);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculatorService.TenBlueFourPlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void ResultsOfNDice_10Red4Plus()
        {
            var tenRed = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = DiceColour.red };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(tenRed);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculatorService.TenRedFourPlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void ResultsOfNDice_10Black3Plus()
        {
            var tenBlack3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = DiceColour.black };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(tenBlack3Plus);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculatorService.TenBlackThreePlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void ResultsOfNDice_10Blue3Plus()
        {
            var tenBlue3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = DiceColour.blue };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(tenBlue3Plus);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculatorService.TenBlueThreePlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void ResultsOfNDice_10Red3Plus()
        {
            var tenRed3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = DiceColour.red };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(tenRed3Plus);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculatorService.TenRedThreePlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void CheckHitOnValidationWorks()
        {
            var pool = new DicePool { NumberOfDice = 10, HitOn = 0, DiceColour = DiceColour.red };
            var service = new DiceCalculatorService();
            Assert.ThrowsException<InvalidDataException>(() => service.ResultsOfDicePool(pool));
            pool.HitOn = -1;
            Assert.ThrowsException<InvalidDataException>(() => service.ResultsOfDicePool(pool));
            pool.HitOn = 7;
            Assert.ThrowsException<InvalidDataException>(() => service.ResultsOfDicePool(pool));
        }

        //todo add test for 1 dice and for 1 and 1 reroll and for rerolls > dice

        [TestMethod]
        public void Rerolls12BlackDiceWith12Rerolls()
        {
            // In this test and those based on it we leverage the fact that if all the dice can be rerolled
            // Then n dice has the same average as n * 1 rerollable dice.
            var thisManyDice = 12;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = DiceColour.black, ReRolls = thisManyDice};
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(pool);

            var average = HelperFunctions.CalculateAverage(res);
            var expectedAverage = thisManyDice * TestDataGenerator.AverageOfBlackRerollableDice();
            Assert.IsTrue(CompareResults(average, expectedAverage));
        }

        [TestMethod]
        public void Rerolls12BlueDiceWith12Rerolls()
        {
            var thisManyDice = 12;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = DiceColour.blue, ReRolls = thisManyDice };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(pool);

            var average = HelperFunctions.CalculateAverage(res);
            var expectedAverage = thisManyDice * TestDataGenerator.AverageOfBlueRerollableDice();
            Assert.IsTrue(CompareResults(average, expectedAverage));
        }

        [TestMethod]
        public void Rerolls12RedDiceWith12Rerolls()
        {
            var thisManyDice = 12;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = DiceColour.red, ReRolls = thisManyDice };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(pool);

            var average = HelperFunctions.CalculateAverage(res);
            var expectedAverage = thisManyDice * TestDataGenerator.AverageOfRedRerollableDice();
            Assert.IsTrue(CompareResults(average, expectedAverage));
        }


        [TestMethod]
        public void Rerolls3BlackDiceWith2Rerolls()
        {
            var pool = new DicePool
            {
                NumberOfDice = 3,
                HitOn = 4,
                DiceColour = DiceColour.black,
                ReRolls = 2
            };
            TestAgainstSimulator(pool);

            // Special case for this particular dice pool
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(pool);
            var averageWithRerolls = HelperFunctions.CalculateAverage(res);
            // I sat down and mapped out the full possibility space.
            // This is the average that I calculated.
            var averageNoRerolls = (decimal) 2.1875;
            Assert.IsTrue(CompareResults(averageWithRerolls, averageNoRerolls));
        }

        [TestMethod]
        public void Rerolls12BlackDiceWith4Rerolls()
        {
            var pool = new DicePool
            {
                NumberOfDice = 12,
                HitOn = 4,
                DiceColour = DiceColour.black,
                ReRolls = 2
            };
            TestAgainstSimulator(pool);
        }

        [TestMethod]
        public void Rerolls12BlueDiceWith4Rerolls()
        {
            var pool = new DicePool
            {
                NumberOfDice = 12,
                HitOn = 4,
                DiceColour = DiceColour.blue,
                ReRolls = 2
            };
            TestAgainstSimulator(pool);
        }


        [TestMethod]
        public void Rerolls12RedDiceWith4Rerolls()
        {
            var pool = new DicePool
            {
                NumberOfDice = 6,
                HitOn = 4,
                DiceColour = DiceColour.red,
                ReRolls = 2
            };
            TestAgainstSimulator(pool);
        }



        public void TestAgainstSimulator(DicePool pool)
        {
            var service = new DiceCalculatorService();
            var resultsFromService = service.ResultsOfDicePool(pool);
            var averageOfResultsFromService = HelperFunctions.CalculateAverage(resultsFromService);
            
            var resultsFromSimulator = GenerateTestDataUsingSimulator(pool);
            var averageOfResultsFromSimulator = HelperFunctions.CalculateAverage(resultsFromSimulator);

            // TODO compare more than just the average

            Assert.IsTrue(CompareResults(
                    averageOfResultsFromService,
                    averageOfResultsFromSimulator,
                    2));
        }
        
        public Dictionary<int, decimal> GenerateTestDataUsingSimulator(DicePool pool)
        {
            var diceToRollPerTrial = 5000000;
            var numberOfTrials = 10;
            
            var trialResults = new List<Dictionary<int, int>>();
            var watch = new Stopwatch();
            watch.Start();
            
            // Switch to regular for if debugging.
            //for(var i = 1; i <= numberOfTrials; i++)
            Parallel.For(0, numberOfTrials, i =>
            {
                var simulator = new DiceSimulator
                {
                    DicePool = pool,
                    NumberOfTrials = diceToRollPerTrial
                };
                trialResults.Add(simulator.GenerateResults());
            });
            // }

            var compiledResults = new Dictionary<int,decimal>();
            foreach (var trialResult in trialResults)
            {
                foreach (var outcome in trialResult)
                {
                    HelperFunctions.AddToDictionaryOrSumWithExisting(compiledResults, outcome.Key, outcome.Value);
                }
            }
            

            var results = new Dictionary<int, decimal>();
            foreach (var outcome in compiledResults)
            {
                var probability = outcome.Value / (decimal) (diceToRollPerTrial * numberOfTrials);
                results.Add(outcome.Key, probability);
            }
            HelperFunctions.CheckProbability(results.Values);
            watch.Stop();
            return results;
        }
    }
}

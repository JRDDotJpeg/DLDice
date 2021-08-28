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
        private DiceFactory _diceFactory = new DiceFactory();

        [TestMethod]
        public void ResultsOfNDice_10Black4Plus()
        {
            var tenBlack = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = DiceColour.black };
            
            var service = new DiceCalculatorService(_diceFactory);
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
            var service = new DiceCalculatorService(_diceFactory);
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
            var service = new DiceCalculatorService(_diceFactory);
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
            var service = new DiceCalculatorService(_diceFactory);
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
            var service = new DiceCalculatorService(_diceFactory);
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
            var service = new DiceCalculatorService(_diceFactory);
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
            var service = new DiceCalculatorService(_diceFactory);
            Assert.ThrowsException<InvalidDataException>(() => service.ResultsOfDicePool(pool));
            pool.HitOn = -1;
            Assert.ThrowsException<InvalidDataException>(() => service.ResultsOfDicePool(pool));
            pool.HitOn = 7;
            Assert.ThrowsException<InvalidDataException>(() => service.ResultsOfDicePool(pool));
        }

        [TestMethod]
        public void Rerolls12BlackDiceWith12Rerolls()
        {
            var thisManyDice = 12;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = DiceColour.black, ReRolls = thisManyDice};
            TestRerollableDiceWhenNumberOfDiceEqualsNumberOfRerolls(pool);
        }

        private void TestRerollableDiceWhenNumberOfDiceEqualsNumberOfRerolls(DicePool pool)
        {
            // Leverage the fact that if all the dice can be rerolled
            // Then n dice has the same average as n * 1 rerollable dice.
            var service = new DiceCalculatorService(_diceFactory);
            var res = service.ResultsOfDicePool(pool);

            var averageCalculatedByService = HelperFunctions.CalculateAverage(res);
            decimal averageForOneDiceOfThisColour;
            switch (pool.DiceColour)
            {
                case DiceColour.black:
                    averageForOneDiceOfThisColour = TestDataGenerator.AverageOfBlackRerollableDice();
                    break;
                case DiceColour.blue:
                    averageForOneDiceOfThisColour = TestDataGenerator.AverageOfBlueRerollableDice();
                    break;
                case DiceColour.red:
                    averageForOneDiceOfThisColour = TestDataGenerator.AverageOfRedRerollableDice();
                    break;
                default:
                    throw new InvalidDataException("Dice Colour not specified. colour: " + pool.DiceColour);
            }

            var expectedAverage = pool.NumberOfDice * averageForOneDiceOfThisColour;
            Assert.IsTrue(CompareResults(averageCalculatedByService, expectedAverage));
        }

        [TestMethod]
        public void Rerolls12BlueDiceWith12Rerolls()
        {
            var thisManyDice = 12;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = DiceColour.blue, ReRolls = thisManyDice };
            TestRerollableDiceWhenNumberOfDiceEqualsNumberOfRerolls(pool);
        }

        [TestMethod]
        public void Rerolls12RedDiceWith12Rerolls()
        {
            var pool = new DicePool
            {
                NumberOfDice = 12,
                HitOn = 4,
                DiceColour = DiceColour.red,
                ReRolls = 12
            };
            TestRerollableDiceWhenNumberOfDiceEqualsNumberOfRerolls(pool);
        }

        [TestMethod]
        public void Rerolls1RedDiceWith1Reroll()
        {
            var pool = new DicePool
            {
                NumberOfDice = 1,
                HitOn = 4,
                DiceColour = DiceColour.red,
                ReRolls = 1
            };
            TestRerollableDiceWhenNumberOfDiceEqualsNumberOfRerolls(pool);
        }

        [TestMethod]
        public void Rerolls1BlueDiceWith1Reroll()
        {
            var thisManyDice = 1;
            var diceColour = DiceColour.blue;
            var rerolls = 1;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour, ReRolls = rerolls };
            TestRerollableDiceWhenNumberOfDiceEqualsNumberOfRerolls(pool);
        }

        [TestMethod]
        public void Rerolls1RedDiceWith5Rerolls()
        {
            var thisManyDice = 1;
            var diceColour = DiceColour.red;
            var rerolls = 5;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour, ReRolls = rerolls };
            TestRerollableDiceWhenNumberOfDiceEqualsNumberOfRerolls(pool);
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
            var service = new DiceCalculatorService(_diceFactory);
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

        [TestMethod]
        public void Rerolls12RedDiceWith4Rerolls3Plus()
        {
            var pool = new DicePool
            {
                NumberOfDice = 6,
                HitOn = 3,
                DiceColour = DiceColour.red,
                ReRolls = 2
            };
            TestAgainstSimulator(pool);
        }

        [TestMethod]
        public void Rerolls12RedDiceWith4Rerolls5Plus()
        {
            var pool = new DicePool
            {
                NumberOfDice = 6,
                HitOn = 5,
                DiceColour = DiceColour.red,
                ReRolls = 2
            };
            TestAgainstSimulator(pool);
        }

        public void TestAgainstSimulator(DicePool pool)
        {
            var service = new DiceCalculatorService(_diceFactory);
            var resultsFromService = service.ResultsOfDicePool(pool);
            var averageOfResultsFromService = HelperFunctions.CalculateAverage(resultsFromService);
            
            var resultsFromSimulator = GenerateTestDataUsingSimulator(pool);
            var averageOfResultsFromSimulator = HelperFunctions.CalculateAverage(resultsFromSimulator);

            Assert.IsTrue(CompareResults(
                averageOfResultsFromService,
                averageOfResultsFromSimulator,
                2));

            foreach (var outcomeFromService in resultsFromService
                .Where(res => res.Value > (decimal) 0.001))
            {
                var matchingOutcomeFromSimulator =
                    resultsFromSimulator
                        .Single(o => o.Key == outcomeFromService.Key);

                Assert.IsTrue(CompareResults(
                    outcomeFromService.Value,
                    matchingOutcomeFromSimulator.Value,
                    2));
            }
        }
        
        public Dictionary<int, decimal> GenerateTestDataUsingSimulator(DicePool pool)
        {
            var trialsPerThread = 550000;
            var numberOfThreads = 12;
            
            var trialResults = new List<Dictionary<int, int>>();
            var watch = new Stopwatch();
            watch.Start();
            
            // Switch to regular for if debugging.
            //for(var i = 1; i <= numberOfTrials; i++)
            Parallel.For(0, numberOfThreads, i =>
            {
                var simulator = new DiceSimulator(_diceFactory);
                trialResults.Add(simulator.GenerateResults(pool, trialsPerThread));
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
                var probability = outcome.Value / (decimal) (trialsPerThread * numberOfThreads);
                results.Add(outcome.Key, probability);
            }
            HelperFunctions.CheckProbability(results.Values);
            watch.Stop();
            return results;
        }

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
    }
}

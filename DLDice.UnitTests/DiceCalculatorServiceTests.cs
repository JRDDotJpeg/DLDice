using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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

        private static bool CompareResults(decimal resultA, decimal resultB)
        {
            resultA = Math.Round(resultA, decimals: 5);
            resultB = Math.Round(resultB, decimals: 5);
            return resultA == resultB;

        }

        [TestMethod]
        public void ResultsOfNDice_10Black4Plus()
        {
            var tenBlack = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = diceColour.black };
            
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
            var tenBlue = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = diceColour.blue };
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
            var tenRed = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = diceColour.red };
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
            var tenBlack3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = diceColour.black };
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
            var tenBlue3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = diceColour.blue };
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
            var tenRed3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = diceColour.red };
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
            var pool = new DicePool { NumberOfDice = 10, HitOn = 0, DiceColour = diceColour.red };
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
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.black, ReRolls = thisManyDice};
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
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.blue, ReRolls = thisManyDice };
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
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.red, ReRolls = thisManyDice };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(pool);

            var average = HelperFunctions.CalculateAverage(res);
            var expectedAverage = thisManyDice * TestDataGenerator.AverageOfRedRerollableDice();
            Assert.IsTrue(CompareResults(average, expectedAverage));
        }


        [TestMethod]
        public void Rerolls12BlackDiceWith4Rerolls()
        {
            // If the number of rerolls is small compared to the total number of dice
            // Then almost always, all of the rerolls will be used
            // these are black dice so their average increases by 0.25 if the dice has a reroll
            // A non rerollable dice has an average of 0.5.
            // so each reroll is the same as adding half a dice to the pool
            // So the results will be very similar to rolling the number of dice + (the number of rerolls)/2 with 0 rerolls.
            var service = new DiceCalculatorService();
            var thisManyDice = 3;
            var rerolls = 2; // must be even

            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.black, ReRolls = rerolls };
            var res = service.ResultsOfDicePool(pool);
            var averageWithRerolls = HelperFunctions.CalculateAverage(res);

            var noRerollsButExtraDicePool = new DicePool { NumberOfDice = (int)(thisManyDice + (rerolls/2.0)), HitOn = 4, DiceColour = diceColour.black, ReRolls = 0 };
            var noRerollsRes = service.ResultsOfDicePool(noRerollsButExtraDicePool);
            var averageNoRerolls = HelperFunctions.CalculateAverage(noRerollsRes);
            
            Assert.IsTrue(CompareResults(averageWithRerolls, averageNoRerolls));
        }

        [TestMethod]
        public void Rerolls12BlueDiceWith4Rerolls()
        {
            // If the number of rerolls is small compared to the total number of dice
            // Then almost always, all of the rerolls will be used
            // these are blue dice so their average increases by 1/3 if the dice has a reroll
            // A non rerollable dice has an average of 2/3.
            // so each reroll is the same as adding half a dice to the pool
            // So the results will be very similar to rolling the number of dice + (the number of rerolls)/2 with 0 rerolls.
            var service = new DiceCalculatorService();
            var thisManyDice = 12;
            var rerolls = 4; // must be even

            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.blue, ReRolls = rerolls };
            var res = service.ResultsOfDicePool(pool);
            var averageWithRerolls = HelperFunctions.CalculateAverage(res);

            var noRerollsButExtraDicePool = new DicePool { NumberOfDice = (int)(thisManyDice + (rerolls / 2.0)), HitOn = 4, DiceColour = diceColour.blue, ReRolls = 0 };
            var noRerollsRes = service.ResultsOfDicePool(noRerollsButExtraDicePool);
            var averageNoRerolls = HelperFunctions.CalculateAverage(noRerollsRes);

            Assert.IsTrue(CompareResults(averageWithRerolls, averageNoRerolls));
        }


        [TestMethod]
        public void Rerolls9RedDiceWith2Rerolls()
        {
            // If the number of rerolls is small compared to the total number of dice
            // Then almost always, all of the rerolls will be used
            // these are red dice so their average increases by 0.4 if the dice has a reroll
            // A non rerollable dice has an average of 0.8.
            // so each reroll is the same as adding half a dice to the pool
            // So the results will be very similar to rolling the number of dice + (the number of rerolls)/2 with 0 rerolls.
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var service = new DiceCalculatorService();
            var thisManyDice = 2;
            var rerolls = 1; // must be even

            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.red, ReRolls = rerolls };
            var res = service.ResultsOfDicePool(pool);
            var averageWithRerolls = HelperFunctions.CalculateAverage(res);
            watch.Stop();

            var noRerollsButExtraDicePool = new DicePool { NumberOfDice = (int)(thisManyDice + (rerolls / 2.0)), HitOn = 4, DiceColour = diceColour.red, ReRolls = 0 };
            var noRerollsRes = service.ResultsOfDicePool(noRerollsButExtraDicePool);
            var averageNoRerolls = HelperFunctions.CalculateAverage(noRerollsRes);

            Assert.IsTrue(CompareResults(averageWithRerolls, averageNoRerolls));
            
        }
    }
}

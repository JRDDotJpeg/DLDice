using System;
using System.Collections.Generic;
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

        [TestMethod]
        public void CheckBlackDiceRerollsWork()
        {
            var thisManyDice = 12;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.black, ReRolls = 1};
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(pool);

            var average = HelperFunctions.CalculateAverage(res);
            var expectedAverage = thisManyDice * TestDataGenerator.AverageOfBlackRerollableDice();
        }

        [TestMethod]
        public void CheckBlueDiceRerollsWork()
        {
            var thisManyDice = 12;
            var pool = new DicePool { NumberOfDice = thisManyDice, HitOn = 4, DiceColour = diceColour.blue, ReRolls = 1 };
            var service = new DiceCalculatorService();
            var res = service.ResultsOfDicePool(pool);

            var average = HelperFunctions.CalculateAverage(res);
            var expectedAverage = thisManyDice * TestDataGenerator.AverageOfBlueRerollableDice();
        }

    }
}

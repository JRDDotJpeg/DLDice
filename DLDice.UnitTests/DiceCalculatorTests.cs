﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DLDice;
using DLDice.API;
using DLDice.DTO;
using Newtonsoft.Json;


namespace DLDice.UnitTests
{
    [TestClass]
    public class DiceCalculatorTests
    {
        private static bool CompareResults(DiceResultsDTO resultsDTO, string expectedResultsJson)
        {
            var expectedResultsDto = JsonConvert.DeserializeObject<DiceResultsDTO>(expectedResultsJson);
            return resultsDTO.Results.All(entry =>
               Math.Round(resultsDTO.Results[entry.Key], decimals:5) == Math.Round(expectedResultsDto.Results[entry.Key],decimals: 5));
        }

        private DiceCalculator _calculator = null;
        private DiceCalculator DiceCalculator
        {
            get
            {
                if (_calculator is null)
                {
                    var tenBlack = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = diceColour.black };
                    var tenBlue = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = diceColour.blue };
                    var tenRed = new DicePool { NumberOfDice = 10, HitOn = 4, DiceColour = diceColour.red };
                    var tenBlack3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = diceColour.black };
                    var tenBlue3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = diceColour.blue };
                    var tenRed3Plus = new DicePool { NumberOfDice = 10, HitOn = 3, DiceColour = diceColour.red };

                    var zeroBlack = new DicePool { NumberOfDice = 0, HitOn = 0, DiceColour = diceColour.black };
                    var zeroBlue = new DicePool { NumberOfDice = 0, HitOn = 0, DiceColour = diceColour.blue };
                    var zeroRed = new DicePool { NumberOfDice = 0, HitOn = 0, DiceColour = diceColour.red };

                    var mockDiceCalculatorService = new Mock<IDiceCalculatorService>();
                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(tenBlack))
                        .Returns(JsonConvert.DeserializeObject<Dictionary<int, decimal>>(
                            ExpectedResultsFromDiceCalculatorService.TenBlackFourPlus));
                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(tenBlue))
                        .Returns(JsonConvert.DeserializeObject<Dictionary<int, decimal>>(
                            ExpectedResultsFromDiceCalculatorService.TenBlueFourPlus));
                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(tenRed))
                        .Returns(JsonConvert.DeserializeObject<Dictionary<int, decimal>>(
                            ExpectedResultsFromDiceCalculatorService.TenRedFourPlus));

                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(tenBlack3Plus))
                        .Returns(JsonConvert.DeserializeObject<Dictionary<int, decimal>>(
                            ExpectedResultsFromDiceCalculatorService.TenBlackThreePlus));
                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(tenBlue3Plus))
                        .Returns(JsonConvert.DeserializeObject<Dictionary<int, decimal>>(
                            ExpectedResultsFromDiceCalculatorService.TenBlueThreePlus));
                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(tenRed3Plus))
                        .Returns(JsonConvert.DeserializeObject<Dictionary<int, decimal>>(
                            ExpectedResultsFromDiceCalculatorService.TenRedThreePlus));

                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(zeroBlack))
                        .Returns(new Dictionary<int, decimal>());
                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(zeroBlue))
                        .Returns(new Dictionary<int, decimal>());
                    mockDiceCalculatorService.Setup(m => m.ResultsOfDicePool(zeroRed))
                        .Returns(new Dictionary<int, decimal>());


                    _calculator = new DiceCalculator(mockDiceCalculatorService.Object);
                }

                return _calculator;
            }
        }

        [TestMethod]
        public void CalculateResults_ExpectedBehaviour_10Black4Plus()
        {
            var dto = new DiceDTO
            {
               DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = diceColour.black
            });

            var res = DiceCalculator.CalculateResults(dto);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculator.TenBlackFourPlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void CalculateResults_ExpectedBehaviour_10Blue4Plus()
        {
            var dto = new DiceDTO
            {
                DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = diceColour.blue
            });

            var res = DiceCalculator.CalculateResults(dto);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculator.TenBlueFourPlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void CalculateResults_ExpectedBehaviour_10Red4Plus()
        {
            var dto = new DiceDTO
            {
                DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = diceColour.red
            });

            var res = DiceCalculator.CalculateResults(dto);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculator.TenRedFourPlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void CalculateResults_ExpectedBehaviour_10Black3Plus()
        {
            var dto = new DiceDTO
            {
                DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 3,
                DiceColour = diceColour.black
            });

            var res = DiceCalculator.CalculateResults(dto);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculator.TenBlackThreePlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void CalculateResults_ExpectedBehaviour_10Blue3Plus()
        {
            var dto = new DiceDTO
            {
                DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 3,
                DiceColour = diceColour.blue
            });

            var res = DiceCalculator.CalculateResults(dto);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculator.TenBlueThreePlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void CalculateResults_ExpectedBehaviour_10Red3Plus()
        {
            var dto = new DiceDTO
            {
                DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 3,
                DiceColour = diceColour.red
            });

            var res = DiceCalculator.CalculateResults(dto);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculator.TenRedThreePlus))
            {
                Assert.Fail("Results do not match");
            }
        }

        [TestMethod]
        public void CalculateResults_ExpectedBehaviour_10OfEach4Plus()
        {
            var dto = new DiceDTO
            {
                DicePools = new List<DicePool>()
            };

            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = diceColour.red
            });
            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = diceColour.blue
            });
            dto.DicePools.Add(new DicePool
            {
                NumberOfDice = 10,
                HitOn = 4,
                DiceColour = diceColour.black
            });

            var res = DiceCalculator.CalculateResults(dto);
            if (!CompareResults(res, ExpectedResultsFromDiceCalculator.TenOfEach4Plus))
            {
                Assert.Fail("Results do not match");
            }
        }
    }
}

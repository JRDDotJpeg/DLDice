using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLDice
{
    /// <summary>
    /// Class to generate results to test the DiceCalculator service with.
    /// Using this code to generate results in real time is not recommended.
    /// </summary>
    public class DiceSimulator
    {
        public DicePool DicePool { get; set; }

        public int NumberOfTrials { get; set; }

        private Results _results = new Results();
        public Dictionary<int, int> RawData => _results.RawData;

        private Random _random = new Random();

        public Dictionary<int, int> GenerateResults()
        {
            _results.ClearData(); // TODO this is a terrible bit of design
            var dice = new Dice(DicePool.HitOn, DicePool.DiceColour);
            for (var i = 0; i < NumberOfTrials; i++)
            {
                var rolledNumbers = new List<int>();
                for (var j = 0; j < DicePool.NumberOfDice; j++)
                {
                    rolledNumbers.Add(GetNextDiceRoll(dice.Sides.Count));
                }

                var rerollsUsed = 0;
                for (var r = 0; r < rolledNumbers.Count; r++)
                {
                    if (rerollsUsed == DicePool.ReRolls) break;
                    if (rolledNumbers[r] < DicePool.HitOn)
                    {
                        var newResult = GetNextDiceRoll(dice.Sides.Count);
                        rolledNumbers[r] = newResult;
                        rerollsUsed++;
                    }
                }

                var diceGeneratedByExplosions = new List<List<int>> {rolledNumbers};

                if (dice.Sides.Any(s => s.Explodes))
                {
                    for (var j = 0; j <= 9; j++) // Cap the number of explosions at 10.
                    {
                        if (diceGeneratedByExplosions[j] is null) break;
                        var generatedDice = (from rolledNumber in diceGeneratedByExplosions[j]
                            where dice.Sides[rolledNumber - 1].Explodes
                            select GetNextDiceRoll(dice.Sides.Count)).ToList();

                        if (generatedDice.Any())
                        {
                            diceGeneratedByExplosions.Add(generatedDice);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                var allResultsIncExplosions = new List<int>();
                foreach (var diceGeneratedByExplosion in diceGeneratedByExplosions)
                {
                    allResultsIncExplosions.AddRange(diceGeneratedByExplosion);
                }

                // TODO add dice factory

                var result = allResultsIncExplosions.Sum(rolledNumber => dice.Sides[rolledNumber - 1].Value);
                _results.RecordResult(result);
            }

            return RawData;
        }

        private int GetNextDiceRoll(int numberOfSides)
        {
            return _random.Next(1, numberOfSides + 1);
        }


        private class Results
        {
            public void RecordResult(int successes)
            {
                HelperFunctions.AddToDictionaryOrSumWithExisting(RawData, successes, 1);
            }

            public void ClearData()
            {
                RawData = new Dictionary<int, int>();
            }

            public Dictionary<int, int> RawData { get; private set; } = new Dictionary<int, int>();
        }
    }

    
    
}

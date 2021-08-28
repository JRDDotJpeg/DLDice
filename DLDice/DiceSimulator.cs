using System;
using System.Collections.Generic;
using System.Linq;

namespace DLDice
{
    /// <summary>
    /// Class to generate results to test the DiceCalculator service with.
    /// Using this code to generate results in real time is not recommended.
    /// </summary>
    internal class DiceSimulator
    {
        private readonly Random _random = new Random();

        private readonly IDiceFactory _diceFactory;

        internal DiceSimulator(IDiceFactory factory)
        {
            _diceFactory = factory;
        }

        public Dictionary<int, int> GenerateResults(DicePool pool, int numberOfTrials)
        {
            var results = new Results();
            var dice = _diceFactory.CreateDice(6, pool.HitOn, pool.DiceColour);
            for (var i = 0; i < numberOfTrials; i++)
            {
                results.RecordResult(RunTrial(pool, dice));
            }

            return results.Data;
        }

        private int RunTrial(DicePool pool, Dice dice)
        {
            var rolledNumbers = new List<int>();
            CreateInitialRolls(pool, dice, rolledNumbers);

            ApplyAnyRerolls(pool, dice, rolledNumbers);

            var allRollsIncExplosions = HandleAnyExplosions(dice, rolledNumbers);

            return allRollsIncExplosions.Sum(rolledNumber => dice.Sides[rolledNumber - 1].Value);
        }

        private List<int> HandleAnyExplosions(Dice dice, List<int> rolledNumbers)
        {
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

            return allResultsIncExplosions;
        }

        private void ApplyAnyRerolls(DicePool pool, Dice dice, List<int> rolledNumbers)
        {
            var rerollsUsed = 0;
            for (var r = 0; r < rolledNumbers.Count; r++)
            {
                if (rerollsUsed == pool.ReRolls) break;
                if (rolledNumbers[r] < pool.HitOn)
                {
                    var newResult = GetNextDiceRoll(dice.Sides.Count);
                    rolledNumbers[r] = newResult;
                    rerollsUsed++;
                }
            }
        }

        private void CreateInitialRolls(DicePool pool, Dice dice, List<int> rolledNumbers)
        {
            for (var j = 0; j < pool.NumberOfDice; j++)
            {
                rolledNumbers.Add(GetNextDiceRoll(dice.Sides.Count));
            }
        }

        private int GetNextDiceRoll(int numberOfSides)
        {
            return _random.Next(1, numberOfSides + 1);
        }


        private class Results
        {
            public void RecordResult(int successes)
            {
                HelperFunctions.AddToDictionaryOrSumWithExisting(Data, successes, 1);
            }

            public Dictionary<int, int> Data { get; } = new Dictionary<int, int>();
        }
    }

    
    
}

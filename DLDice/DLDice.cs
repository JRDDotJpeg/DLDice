using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DLDice.API;

namespace DLDice
{

    public interface IDLDice
    {
        IDiceCalculator Calculator { get; }
    }

    public class DLDice : IDLDice
    {
        public IDiceCalculator Calculator { get; }
        public DLDice()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DiceCalculator>().As<IDiceCalculator>();
            builder.RegisterType<DiceCalculatorService>().As<IDiceCalculatorService>();
            var container = builder.Build();
            Calculator = container.Resolve<IDiceCalculator>();
        }
    }
}

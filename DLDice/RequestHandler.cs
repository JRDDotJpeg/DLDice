using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DLDice.API;

namespace DLDice
{
    public class RequestHandler
    {
        public IDiceRoller Roller { get;  }
        public RequestHandler()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DiceRoller>().As<IDiceRoller>();
            var container = builder.Build();
            Roller = container.Resolve<IDiceRoller>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace NinjectIoC
{
    class Program
    {
        static void Main(string[] args)
        {
            var kernel = new StandardKernel();

            //Use InSingletonScope() to use the same CreditCard
            kernel.Bind<ICreditCard>().To<MasterCard>().InSingletonScope();
            
            //Set a new type to the kernel
            //kernel.Rebind<ICreditCard>().To<Visa>();


            var shopper = kernel.Get<Shopper>();
            shopper.Charge();
            Console.WriteLine(shopper.ChargesForCurrentCard);

            var shopper2 = kernel.Get<Shopper>();
            shopper2.Charge();
            Console.WriteLine(shopper2.ChargesForCurrentCard);

            Console.Read();
        }
    }

    public class Visa : ICreditCard
    {
        public int ChargeCount { get { return 0; } }

        public string Charge()
        {
            return "Charging the Visa!";
        }
    }

    public class MasterCard : ICreditCard
    {
        public int ChargeCount { get; set; }

        public string Charge()
        {
            ChargeCount++;
            return "Swiping the MasterCard";
        }
    }

    public class Resolver
    {
        private Dictionary<Type, Type> dependencyMap = new Dictionary<Type, Type>();

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public void Register<TFrom, TTo>()
        {
            dependencyMap.Add(typeof(TFrom), typeof(TTo));
        }

        private object Resolve(Type typeToResolve)
        {
            Type resolvedType = null;

            try
            {
                resolvedType = dependencyMap[typeToResolve];
            }
            catch
            {
                throw new Exception(string.Format("Could not resolve type {0}", typeToResolve.FullName));
            }

            var firstContructor = resolvedType.GetConstructors().First();
            var contructorParameters = firstContructor.GetParameters();

            if (contructorParameters.Count() == 0)
                return Activator.CreateInstance(resolvedType);

            IList<object> parameters = new List<object>();
            foreach (var parameterToResolve in contructorParameters)
            {
                parameters.Add(Resolve(parameterToResolve.ParameterType));
            }
            return firstContructor.Invoke(parameters.ToArray());
        }
    }    

    public interface ICreditCard
    {
        string Charge();
        int ChargeCount { get; }
    }

    public class Shopper
    {
        private readonly ICreditCard _creditCard;

        public Shopper(ICreditCard creditCard)
        {
            this._creditCard = creditCard;
        }

        public int ChargesForCurrentCard
        {
            get { return this._creditCard.ChargeCount; }
        }

        public void Charge()
        {
            var chargeMessage = _creditCard.Charge();
            Console.WriteLine(chargeMessage);
        }
    }
}

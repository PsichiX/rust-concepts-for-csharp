using System;

namespace Csharp
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("\n= ownership in a nutshell =");
            OwnershipLifetimesAndBorrowing.OwnershipInANutshell();

            Console.WriteLine("\n= passing object from scope to scope =");
            OwnershipLifetimesAndBorrowing.PassingObjectFromScopeToScope();

            Console.WriteLine("\n= borrowing =");
            OwnershipLifetimesAndBorrowing.Borrowing();

            Console.WriteLine("\n= premature data release =");
            OwnershipLifetimesAndBorrowing.PrematureDataRelease();

            Console.ReadKey(true);
        }
    }
}

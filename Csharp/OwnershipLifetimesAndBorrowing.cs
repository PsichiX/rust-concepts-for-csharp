using System;
using System.Collections.Generic;

namespace Csharp
{
    public static class OwnershipLifetimesAndBorrowing
    {
        class Bar { }

        public static void OwnershipInANutshell()
        {
            var bar = new Bar();
            Console.WriteLine(bar);
            var bar2 = bar;
            Console.WriteLine(bar);
        }

        static Bar MakeBar()
        {
            var bar = new Bar();
            Console.WriteLine(bar);
            return bar;
        }

        public static void PassingObjectFromScopeToScope()
        {
            var bar = MakeBar();
            Console.WriteLine(bar);
        }

        struct Bar2
        {
            public int answer;
        }

        static void ImmutableBorrow(in Bar2 bar)
        {
            Console.WriteLine(bar.answer);
            // compile-time error: cannot modify because `bar` is readonly.
            //bar.answer = 40;
        }

        static void MutableBorrow(ref Bar2 bar)
        {
            bar.answer = 40;
            Console.WriteLine(bar.answer);
        }

        public static void Borrowing()
        {
            var bar = new Bar2
            {
                answer = 42
            };
            ImmutableBorrow(bar);
            MutableBorrow(ref bar);
        }

        struct Bar3
        {
            public List<int> data;
        }

        static void DoSomethingCrashy(ref Bar3 bar, int state)
        {
            Console.WriteLine(bar.data[state]);
            if (state > 0)
            {
                bar.data = null;
            }
        }

        static void CannotDoSomethingCrashy(in Bar3 bar, int state)
        {
            Console.WriteLine(bar.data[state]);
            if (state > 0)
            {
                // compile-time error: cannot modify because `bar` is readonly.
                //bar.data = null;
            }
        }

        public static void PrematureDataRelease()
        {
            var bar = new Bar3
            {
                data = new List<int>
                {
                    1, 2, 3
                }
            };
            // run-time error: null pointer at third iteration
            // because of `Bar3` data mutation in second iteration.
            for (var i = 0; i < bar.data.Count; ++i)
            {
                Console.WriteLine(bar.data[i]);
                //DoSomethingCrashy(ref bar, i);
                CannotDoSomethingCrashy(bar, i);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using ExpressionSerialization;

namespace TestHarness
{
    static class Samples
    {
        static void Main(string[] args)
        {
			Console.WriteLine("***RUNNING UNIT TESTS***");
			UnitTests.Test(args);

            Console.WriteLine("***RUNNING WALKTHROUGN***");
            Walkthrough.BasicExpressionSerialization();
            Walkthrough.ComplexExpressionSerializationSamples();
            Walkthrough.DLinqQuerySerializationSamples();
            Walkthrough.AcrossTheWireSerializationSamples();

            Console.WriteLine("Press enter to run Unit Tests...");
            Console.Read();

            
        }
    }
}

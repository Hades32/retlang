using System;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    /*
	 * This demonstration imagines the following scenario:  A stream
	 * of quadratic equations is being received.  Each equation must
	 * be solved in turn and then its solution spat out.  We wish to
	 * take advantage of multi-core hardware that will be able to solve
	 * each independent quadratic equation rapidly and then combine
	 * the results back into one stream.
	 * 
	 * A contrived example, certainly, but not altogether different from
	 * many computing jobs that must process data packets from one stream
	 * and output them onto one other stream, but can efficiently do
	 * the actual calculations on the packets in parallel.
	 * 
	 * Our strategy will be to divide up the quadratics by their square
	 * term:  e.g. 3x^2 + 5X + 7 will be solved by the "3" solver.
	 * The constraint we set is that all the quadratics will have
	 * a square term with integer value between one and ten.  We will
	 * therefore create ten workers.
	 */

    [TestFixture]
    public class AlgebraDemonstration
    {
        // An immutable class that represents a quadratic equation
        // in the form of ax^2 + bx + c = 0.  This class will be
        // our inputs.  It's important that the classes we pass
        // between processes by immutable, or our framework cannot
        // guarantee thread safety.
        private class Quadratic
        {
            private readonly int _a;
            private readonly int _b;
            private readonly int _c;

            public Quadratic(int a, int b, int c)
            {
                _a = a;
                _b = b;
                _c = c;
            }

            public int A
            {
                get { return _a; }
            }

            public int B
            {
                get { return _b; }
            }

            public int C
            {
                get { return _c; }
            }
        }

        // An immutable class that represents the solutions to a quadratic
        // equation.
        private class QuadraticSolutions
        {
            private readonly double _solutionOne;
            private readonly double _solutionTwo;
            private readonly bool _complexSolutions;

            public QuadraticSolutions(double solutionOne, double solutionTwo, bool complexSolutions)
            {
                _solutionOne = solutionOne;
                _solutionTwo = solutionTwo;
                _complexSolutions = complexSolutions;
            }

            public string SolutionOne
            {
                get { return _solutionOne + ImaginarySuffix(); }
            }

            public string SolutionTwo
            {
                get { return _solutionTwo + ImaginarySuffix(); }
            }

            private string ImaginarySuffix()
            {
                return _complexSolutions ? "i" : "";
            }
        }

        // Immutable class representing a quadratic equation and its
        // two computed zeros.  This class will be output by the
        // solver threads.
        private class SolvedQuadratic
        {
            private readonly Quadratic quadratic;
            private readonly QuadraticSolutions solutions;

            public SolvedQuadratic(Quadratic quadratic, QuadraticSolutions solutions)
            {
                this.quadratic = quadratic;
                this.solutions = solutions;
            }

            public override string ToString()
            {
                return string.Format("The quadratic {0} * x^2 + {1} * x + {2} has zeroes at {3} and {4}.",
                                     quadratic.A, quadratic.B, quadratic.C, solutions.SolutionOne, solutions.SolutionTwo);
            }
        }

        // Here is a class that produces a stream of quadratics.  This
        // class simply randomly generates a fixed number of quadratics,
        // but one can imagine this class as representing a socket listener
        // that simply converts the packets received to quadratics and
        // publishes them out.
        private class QuadraticSource
        {
            // The class has its own thread to use for publishing.
            private readonly IProcessContext _processContext;
            private readonly int _numberToGenerate;
            private readonly Random _random;

            public QuadraticSource(IProcessContext processContext, int numberToGenerate, int seed)
            {
                _processContext = processContext;
                _numberToGenerate = numberToGenerate;
                _random = new Random(seed);
            }

            public void PublishQuadratics()
            {
                for (int idx = 0; idx < _numberToGenerate; idx++)
                {
                    Quadratic quadratic = Next();
                    // As agreed, we publish to a topic that is defined
                    // by the square term of the quadratic.
                    string topic = quadratic.A.ToString();
                    _processContext.Publish(topic, quadratic);
                }
                // Once all the quadratics have been published, stop.
                _processContext.Stop();
            }

            // This simply creates a pseudo-random quadratic.
            private Quadratic Next()
            {
                // Insure we have a quadratic.  No zero for the square parameter.
                int a = _random.Next(9) + 1;
                int b = -_random.Next(100);
                int c = _random.Next(10);

                return new Quadratic(a, b, c);
            }
        }

        // This is our solver class.  It is assigned its own context and
        // a topic to listen on.  When it receives a quadratic it publishes
        // its solution to the 'solved' topic.
        private class QuadraticSolver
        {
            private readonly IProcessContext _processContext;

            public QuadraticSolver(IProcessContext processContext, string squareTerm)
            {
                _processContext = processContext;
                _processContext.Subscribe<Quadratic>(new TopicEquals(squareTerm), ProcessReceivedQuadratic);
            }

            private void ProcessReceivedQuadratic(IMessageHeader header, Quadratic quadratic)
            {
                QuadraticSolutions solutions = Solve(quadratic);
                SolvedQuadratic solvedQuadratic = new SolvedQuadratic(quadratic, solutions);
                _processContext.Publish("solved", solvedQuadratic);
            }

            private QuadraticSolutions Solve(Quadratic quadratic)
            {
                int a = quadratic.A;
                int b = quadratic.B;
                int c = quadratic.C;
                bool imaginary = false;

                double discriminant = ((b*b) - (4*a*c));

                if (discriminant < 0)
                {
                    discriminant = -discriminant;
                    imaginary = true;
                }

                double tmp = Math.Sqrt(discriminant);

                double solutionOne = (-b + tmp)/(2*a);
                double solutionTwo = (-b - tmp)/(2*a);

                return new QuadraticSolutions(solutionOne, solutionTwo, imaginary);
            }
        }

        // Finally we have a sink for the solved processes.  This class
        // simply prints them out to the console, but one can imagine
        // the solved quadratics (or whatever) all streaming out across
        // the same socket.
        private class SolvedQuadraticSink
        {
            private readonly IProcessContext _processContext;
            private readonly int _numberToOutput;
            private int _solutionsReceived = 0;

            public SolvedQuadraticSink(IProcessContext processContext, int numberToOutput)
            {
                _processContext = processContext;
                _processContext.Subscribe<SolvedQuadratic>(new TopicEquals("solved"), PrintSolution);
                _numberToOutput = numberToOutput;
            }

            private void PrintSolution(IMessageHeader header, SolvedQuadratic solvedQuadratic)
            {
                _solutionsReceived++;
                Console.WriteLine(_solutionsReceived + ") " + solvedQuadratic);
                // Once we have received all the solved equations we are interested
                // in, we stop.
                if (_solutionsReceived == _numberToOutput)
                {
                    _processContext.Stop();
                }
            }
        }


        // Finally, our demonstration puts all the components together.
        [Test]
        public void DoDemonstration()
        {
            int numberOfQuadratics = 1000;

            // Create a factory to instantiate all our cooperating process contexts.
            ProcessContextFactory processContextFactory = new ProcessContextFactory();
            // We inform that factory that the processes will be communicating
            // by passing objects.
            processContextFactory.TransferEnvelopeFactory = new ObjectTransferEnvelopeFactory();
            processContextFactory.Start();

            // We create a source to generate the quadratics.
            IProcessContext sourceContext = processContextFactory.CreateAndStart();
            QuadraticSource source = new QuadraticSource(sourceContext, numberOfQuadratics, DateTime.Now.Millisecond);

            // We create and store a reference to 10 solvers,
            // one for each possible square term being published.
            IProcessContext[] solverContexts = new IProcessContext[10];
            for (int idx = 0; idx < 10; idx++)
            {
                solverContexts[idx] = processContextFactory.CreateAndStart();
                string topic = (idx + 1).ToString();
                new QuadraticSolver(solverContexts[idx], topic);
            }

            // Finally a sink to output our results.
            IProcessContext sinkContext = processContextFactory.CreateAndStart();
            new SolvedQuadraticSink(sinkContext, numberOfQuadratics);

            // This starts streaming the equations.
            source.PublishQuadratics();

            // We pause here to allow all the problems to be solved.
            sourceContext.Join();
            sinkContext.Join();

            // Once that is done, we stop all the solvers.
            foreach (IProcessContext processContext in solverContexts)
            {
                processContext.Stop();
                // for cleanliness sake, make sure these have truly stopped by joining.
                processContext.Join();
            }

            // Finally, we stop the factory itself.
            processContextFactory.Stop();
            processContextFactory.Join();

            Console.WriteLine("Demonstration complete.");
        }
    }
}
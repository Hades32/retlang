using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang.Channels;
using Retlang.Fibers;

namespace RetlangTests.Examples
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
    [Category("Demo")]
    [Ignore("Demo")]
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
            private readonly ThreadFiber _threadFiber;
            private readonly IChannel<Quadratic>[] _channels;
            private readonly int _numberToGenerate;
            private readonly Random _random;

            public QuadraticSource(ThreadFiber threadFiber, IChannel<Quadratic>[] channels, int numberToGenerate, int seed)
            {
                _threadFiber = threadFiber;
                _channels = channels;
                _numberToGenerate = numberToGenerate;
                _random = new Random(seed);
            }

            public void PublishQuadratics()
            {
                for (var idx = 0; idx < _numberToGenerate; idx++)
                {
                    var quadratic = Next();
                    // As agreed, we publish to a topic that is defined
                    // by the square term of the quadratic.
                    _channels[quadratic.A].Publish(quadratic);
                }
                // Once all the quadratics have been published, stop.
                _threadFiber.Dispose();
            }

            // This simply creates a pseudo-random quadratic.
            private Quadratic Next()
            {
                // Insure we have a quadratic.  No zero for the square parameter.
                var a = _random.Next(9) + 1;
                var b = -_random.Next(100);
                var c = _random.Next(10);

                return new Quadratic(a, b, c);
            }
        }

        // This is our solver class.  It is assigned its own fiber and
        // a channel to listen on.  When it receives a quadratic it publishes
        // its solution to the 'solved' channel.
        private class QuadraticSolver
        {
            private readonly IChannel<SolvedQuadratic> _solvedChannel;

            public QuadraticSolver(IFiber fiber, ISubscriber<Quadratic> channel, IChannel<SolvedQuadratic> solvedChannel)
            {
                _solvedChannel = solvedChannel;
                channel.Subscribe(fiber, ProcessReceivedQuadratic);
            }

            private void ProcessReceivedQuadratic(Quadratic quadratic)
            {
                var solutions = Solve(quadratic);
                var solvedQuadratic = new SolvedQuadratic(quadratic, solutions);
                _solvedChannel.Publish(solvedQuadratic);
            }

            private static QuadraticSolutions Solve(Quadratic quadratic)
            {
                var a = quadratic.A;
                var b = quadratic.B;
                var c = quadratic.C;
                var imaginary = false;

                double discriminant = ((b*b) - (4*a*c));

                if (discriminant < 0)
                {
                    discriminant = -discriminant;
                    imaginary = true;
                }

                var tmp = Math.Sqrt(discriminant);

                var solutionOne = (-b + tmp)/(2*a);
                var solutionTwo = (-b - tmp)/(2*a);

                return new QuadraticSolutions(solutionOne, solutionTwo, imaginary);
            }
        }

        // Finally we have a sink for the solved processes.  This class
        // simply prints them out to the console, but one can imagine
        // the solved quadratics (or whatever) all streaming out across
        // the same socket.
        private class SolvedQuadraticSink
        {
            private readonly ThreadFiber _fiber;
            private readonly int _numberToOutput;
            private int _solutionsReceived = 0;

            public SolvedQuadraticSink(ThreadFiber fiber, ISubscriber<SolvedQuadratic> solvedChannel,
                                       int numberToOutput)
            {
                solvedChannel.Subscribe(fiber, PrintSolution);
                _fiber = fiber;
                _numberToOutput = numberToOutput;
            }

            private void PrintSolution(SolvedQuadratic solvedQuadratic)
            {
                _solutionsReceived++;
                Console.WriteLine(_solutionsReceived + ") " + solvedQuadratic);
                // Once we have received all the solved equations we are interested
                // in, we stop.
                if (_solutionsReceived == _numberToOutput)
                {
                    _fiber.Dispose();
                }
            }
        }

        // Finally, our demonstration puts all the components together.
        [Test]
        public void DoDemonstration()
        {
            const int numberOfQuadratics = 10;


            // We create a source to generate the quadratics.
            var sourceFiber = new ThreadFiber("source");
            var sinkFiber = new ThreadFiber("sink");

          
                // We create and store a reference to 10 solvers,
                // one for each possible square term being published.
                var quadraticChannels = new IChannel<Quadratic>[10];

                // reference-preservation list to prevent GC'ing of solvers
                var solvers = new List<QuadraticSolver>();

                IChannel<SolvedQuadratic> solvedChannel = new Channel<SolvedQuadratic>();

                for (var idx = 0; idx < numberOfQuadratics; idx++)
                {
                    var fiber = new ThreadFiber("solver " + (idx + 1));
                    fiber.Start();
                    
                    quadraticChannels[idx] = new Channel<Quadratic>();
                    solvers.Add(new QuadraticSolver(fiber, quadraticChannels[idx], solvedChannel));
                }


                sourceFiber.Start();

                var source =
                    new QuadraticSource(sourceFiber, quadraticChannels, numberOfQuadratics, DateTime.Now.Millisecond);


                // Finally a sink to output our results.
                sinkFiber.Start();
                new SolvedQuadraticSink(sinkFiber, solvedChannel, numberOfQuadratics);

                // This starts streaming the equations.
                source.PublishQuadratics();

                // We pause here to allow all the problems to be solved.
                sourceFiber.Join();
                sinkFiber.Join();
            


            Console.WriteLine("Demonstration complete.");
        }
    }
}
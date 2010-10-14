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
            public readonly int A;
            public readonly int B;
            public readonly int C;

            public Quadratic(int a, int b, int c)
            {
                A = a;
                B = b;
                C = c;
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
            private readonly Quadratic _quadratic;
            private readonly QuadraticSolutions _solutions;

            public SolvedQuadratic(Quadratic quadratic, QuadraticSolutions solutions)
            {
                _quadratic = quadratic;
                _solutions = solutions;
            }

            public override string ToString()
            {
                return string.Format("The quadratic {0} * x^2 + {1} * x + {2} has zeroes at {3} and {4}.",
                    _quadratic.A, _quadratic.B, _quadratic.C, _solutions.SolutionOne, _solutions.SolutionTwo);
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
            private readonly IChannel<Quadratic>[] _channels;
            private readonly int _numberToGenerate;
            private readonly Random _random;

            public QuadraticSource(IChannel<Quadratic>[] channels, int numberToGenerate, int seed)
            {
                _channels = channels;
                _numberToGenerate = numberToGenerate;
                _random = new Random(seed);
            }

            public void PublishQuadratics()
            {
                for (var i = 0; i < _numberToGenerate; i++)
                {
                    var quadratic = Next();
                    // As agreed, we publish to a topic that is defined
                    // by the square term of the quadratic.
                    _channels[quadratic.A].Publish(quadratic);
                }
            }

            // This simply creates a pseudo-random quadratic.
            private Quadratic Next()
            {
                // Insure we have a quadratic.  No zero for the square parameter.
                return new Quadratic(_random.Next(9) + 1, -_random.Next(100), _random.Next(10));
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

                var discriminant = ((b * b) - (4 * a * c));

                if (discriminant < 0)
                {
                    discriminant = -discriminant;
                    imaginary = true;
                }

                var tmp = Math.Sqrt(discriminant);

                var solutionOne = (-b + tmp) / (2 * a);
                var solutionTwo = (-b - tmp) / (2 * a);

                return new QuadraticSolutions(solutionOne, solutionTwo, imaginary);
            }
        }

        // Finally we have a sink for the solved processes.  This class
        // simply prints them out to the console, but one can imagine
        // the solved quadratics (or whatever) all streaming out across
        // the same socket.
        private class SolvedQuadraticSink
        {
            private readonly IFiber _fiber;
            private readonly int _numberToOutput;
            private int _solutionsReceived;

            public SolvedQuadraticSink(IFiber fiber, ISubscriber<SolvedQuadratic> solvedChannel, int numberToOutput)
            {
                _fiber = fiber;
                _numberToOutput = numberToOutput;

                solvedChannel.Subscribe(fiber, PrintSolution);
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
            // We create a source to generate the quadratics.
            var sinkFiber = new ThreadFiber("sink");

            // We create and store a reference to 10 solvers,
            // one for each possible square term being published.
            var quadraticChannels = new IChannel<Quadratic>[10];

            // reference-preservation list to prevent GC'ing of solvers
            var solvers = new List<QuadraticSolver>();
            var solvedChannel = new Channel<SolvedQuadratic>();

            for (var i = 0; i < quadraticChannels.Length; i++)
            {
                var fiber = new ThreadFiber("solver " + (i + 1));
                fiber.Start();

                quadraticChannels[i] = new Channel<Quadratic>();
                solvers.Add(new QuadraticSolver(fiber, quadraticChannels[i], solvedChannel));
            }


            var source = new QuadraticSource(quadraticChannels, quadraticChannels.Length, DateTime.Now.Millisecond);

            // Finally a sink to output our results.
            sinkFiber.Start();
            new SolvedQuadraticSink(sinkFiber, solvedChannel, quadraticChannels.Length);

            // This starts streaming the equations.
            source.PublishQuadratics();

            // We pause here to allow all the problems to be solved.
            sinkFiber.Join();

            Console.WriteLine("Demonstration complete.");
        }
    }
}
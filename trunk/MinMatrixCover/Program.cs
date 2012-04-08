using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MinMatrixCover
{
    class Program
    {
        static void Main(string[] args)
        {
            PerfTest2();
            Console.ReadKey();
        }

        static float Solve(Matrix m, float prevN = 0)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var solution = m.Solve();
            timer.Stop();
            if (solution.Count < prevN)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                if (solution.Count == prevN)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
            Console.WriteLine("Solve Time ms: {0}", timer.ElapsedMilliseconds);
            Console.WriteLine("Solution N= {0}; Check={1} OFV={2}", solution.Count(), m.Check(solution), m.GetOFV(solution));

            //Console.WriteLine(m.ToString());
            return m.GetOFV(solution);

            //Console.WriteLine(m.Print(m.Solve()));
        }

        static float Solve2(Matrix m, float prevN = 0)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int state = 0;
            var solution = m.Solve2(10000, out state);
            timer.Stop();
            if (solution.Count < prevN)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                if (solution.Count == prevN)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
            if (0 == prevN)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine("Solve2 Time ms: {0}", timer.ElapsedMilliseconds);
            Console.WriteLine("Solution N= {0}; Check={1} OFV={2}", solution.Count(), m.Check(solution), m.GetOFV(solution));
            //Console.WriteLine(m.ToString());

            //Console.WriteLine(m.Print(m.Solve()));
            return m.GetOFV(solution);
        }

        static void GetStatistics(Matrix m, int n, float density)
        {
            int state = 0;
            int[] counter = new int[3];
            for (int i = 0; i < n; i++)
            {
                m.GenerateMatrix(density, i);
                m.Solve2(1000, out state);
                counter[state]++;
            }
            Console.WriteLine("Density={0}%, Convergense {1}%, Resolvent duplicate {2}%, j={3}", density*100, counter[2] * 100.0 / n, counter[1] * 100.0 / n, m.Width*density);
        }
        static void PerfTest()
        {
            float n;
            int w = 500;
            int h = 25;
            float density = 0.20f;
            string path = "scp41.txt";
            for (int i = 0; i < 1; i++)
            {
                Matrix m = new Matrix(w, h);
                Matrix mPav = new MatrixPav(w, h);
                Matrix mPavS2 = new MatrixPav(w, h);

                Matrix mPav2 = new MatrixAnt(w, h);
                //Matrix mPav2S2 = new MatrixAnt(500, 2000);

                m.ReadFromFile(path);
                mPav.ReadFromFile(path);
                mPav2.ReadFromFile(path);
                mPavS2.ReadFromFile(path);
                //m.GenerateMatrix(density, i);
                //mPav.GenerateMatrix(density, i);
                //mPavS2.GenerateMatrix(density, i);

                //mPav2.GenerateMatrix(density, i);

                ////mPav2S2.GenerateMatrix(0.003f, i);

                Console.WriteLine("Default:");
                n = Solve2(m);
                Console.WriteLine("Pav:");
                n = Solve(mPav, n);
                Console.WriteLine("Pav S2:");
                n = Solve2(mPavS2, n);
                Console.WriteLine("Pav2ant:");
                n = Solve(mPav2, n);
                //Console.WriteLine("Pav2S2:");
                //Solve2(mPav2S2, n);
                Console.WriteLine();
            }

        }

        static void PerfTest1()
        {
            float n;
            int w = 100;
            int h = 40;
            float density = 0.10f;
            string path = "scp41.txt";
            for (int i = 0; i < 10; i++)
            {
                Matrix m = new Matrix(w, h);
                Matrix mPav = new MatrixPav(w, h);
                Matrix mPavS2 = new MatrixPav(w, h);

                Matrix mPav2 = new MatrixAnt(w, h);
                //Matrix mPav2S2 = new MatrixAnt(500, 2000);


                m.GenerateMatrix(density, i);
                mPav.GenerateMatrix(density, i);
                mPavS2.GenerateMatrix(density, i);
                mPav2.GenerateMatrix(density, i);

                ////mPav2S2.GenerateMatrix(0.003f, i);

                Console.WriteLine("Default:");
                n = Solve2(m);
                Console.WriteLine("Pav:");
                n = Solve(mPav, n);
                Console.WriteLine("Pav S2:");
                n = Solve2(mPavS2, n);
                Console.WriteLine("Pav2ant:");
                n = Solve(mPav2, n);
                //Console.WriteLine("Pav2S2:");
                //Solve2(mPav2S2, n);
                Console.WriteLine();
            }

        }

        static void PerfTest2()
        {
            float n;
            int w = 100;
            int h = 40;
            for (float i = 0.01f; i < 0.2f; i += 0.01f)
            {
              
                Matrix m = new Matrix(w, h);
                GetStatistics(m, 100, i);
            }

        }
    }
}

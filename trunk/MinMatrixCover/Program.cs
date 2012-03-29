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
            //Matrix m0 = new Matrix(10, 10);
            //m0.GenerateMatrix(0.3f);
            //Console.WriteLine(m0.ToString());
            //Console.WriteLine(m0.Print(m0.Solve2(1)));
            ////Console.WriteLine(m0.Check(m0.Solve()));

            //Matrix m0p = new MatrixPav(10, 10);
            //m0p.GenerateMatrix(0.3f);
            //Console.WriteLine(m0p.ToString());
            //Console.WriteLine(m0p.Print(m0p.Solve()));
            ////Console.WriteLine(m0p.Check(m0p.Solve()));

            //Matrix m0pav2 = new MatrixPav2(10, 10);
            //m0pav2.GenerateMatrix(0.3f);
            //Console.WriteLine(m0pav2.ToString());
            //Console.WriteLine(m0pav2.Print(m0pav2.Solve()));


            //Matrix m = new MatrixPav(500, 20000);
            ////Matrix m = new Matrix(1000, 1000);

            ////Console.WriteLine(m.ToString());
            //Stopwatch timer = new Stopwatch();
            //timer.Start();
            //m.GenerateMatrix(0.03f);
            //timer.Stop();
            //Console.WriteLine("Time: {0}", timer.ElapsedMilliseconds);
            //Solve(m);

            //for (float d = 0; d < 1; d += 0.05f)
            //{
            //    Matrix m2 = new Matrix(600, 2000);
            //    m2.GenerateMatrix(d);
            //    Console.WriteLine("Density {0}:", d);
            //    Solve(m2);
            //}

            //Console.WriteLine(m.ToString());

            //Console.WriteLine(m.Print(m.Solve()));

            PerfTest();
            Console.ReadKey();
        }

        static int Solve(Matrix m, int prevN = 0)
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
            Console.WriteLine("Solution N= {0}; Check={1}", solution.Count(), m.Check(solution));
            //Console.WriteLine(m.ToString());
            return solution.Count();

            //Console.WriteLine(m.Print(m.Solve()));
        }

        static int Solve2(Matrix m, int prevN = 0)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var solution = m.Solve2(30);
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
            Console.WriteLine("Solution N= {0}; Check={1}", solution.Count(), m.Check(solution));
            //Console.WriteLine(m.ToString());

            //Console.WriteLine(m.Print(m.Solve()));
            return solution.Count;
        }

        static void PerfTest()
        {
            int n;
            int w = 500;
            int h = 2000;
            float density = 0.02f;
            for (int i = 0; i < 10; i++)
            {
                Matrix m = new Matrix(w, h);
                Matrix mPav = new MatrixPav(w, h);
                Matrix mPavS2 = new MatrixPav(w, h);

                Matrix mPav2 = new MatrixAnt(w, h);
                //Matrix mPav2S2 = new MatrixAnt(500, 2000);

                m.ReadFromFile("scp41.txt");
                mPav.ReadFromFile("scp41.txt");
                mPav2.ReadFromFile("scp41.txt");
                mPavS2.ReadFromFile("scp41.txt");
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
    }
}

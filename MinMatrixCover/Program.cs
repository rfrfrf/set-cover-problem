using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MinMatrixCover
{
    class Program
    {
        static void Main(string[] args)
        {
            Matrix mFile = new Matrix();
            mFile.ReadFromCsvFile("MatrixTest.csv");
            Console.WriteLine(mFile);
            Matrix m = new Matrix(30, 40);
            m.GenerateMatrix(0.1f);
            m.Output =  OutputType.ShowBetterSolution;
            var result = m.Solve2(10000);
            Console.WriteLine("Check: {0}", m.Check(result));

            //PerfTest4();
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
            int iterCount = 0;
            var solution = m.Solve2(10000, out state, out iterCount);
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

        static string GetStatistics(Matrix m, int n, float density)
        {
            int state = 0;
            int iterationCount = 0;
            int[] counter = new int[3];
            int sum = 0;
            Stopwatch stopwatch = new Stopwatch();

            for (int i = 0; i < n; i++)
            {
                m.GenerateMatrix(density, i);
                //Console.WriteLine("{0} iteration", i);
                //Console.WriteLine(m.ToString());
                stopwatch.Start();
                m.Solve2(10000, out state, out iterationCount);
                stopwatch.Stop();
                // if (state == 1)
                //    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                counter[state]++;
                sum += iterationCount;
            }

            Console.WriteLine("Density={0}%, Convergence {1}%, Resolvent duplicate {2}%, j={3}, iterationCount={4}, time = {5} ms", density * 100, counter[2] * 100.0 / n, counter[1] * 100.0 / n, m.Width * density, sum / n, stopwatch.ElapsedMilliseconds / n);
            //Console.WriteLine("Density={0}%, Iteration {1}", density * 100, sum/n);
            //Console.WriteLine("{0}; {1};", density * 100, sum / n);
            return string.Format("{0}x{1};{2};{3};{4}", m.Width, m.Height, density * 100, sum / n, stopwatch.ElapsedMilliseconds / n);

        }
        static void PerfTest()
        {
            float n;
            int w = 1000;
            int h = 200;
            float density = 0.20f;
            string path = "scp51.txt";
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
            //int w = 20;
            //int h = 15;
            TextWriter resultFile = new StreamWriter("resultFile.csv");
            resultFile.WriteLine("WxH; density; count; time(ms)");
            float[] densities = new float[] { 0.05f, 0.08f, 0.15f, 0.2f, 0.4f, 0.6f };
            string[] sizes = new string[] { "20x40", "20x80", "30x100", "40x100", "50x120", "60x150", "80x200" }; //"100x300"

            // string[] sizes = new string[] {"20x40", "40x100", "100x300", "400x800"};
            foreach (var size in sizes)
            {
                int w = int.Parse(size.Split('x')[0]);
                int h = int.Parse(size.Split('x')[1]);
                Console.WriteLine("Size: {0}", size);
                foreach (var density in densities)
                {
                    Console.WriteLine("    Density: {0}", density);
                    Matrix m = new Matrix(w, h);
                    resultFile.WriteLine(GetStatistics(m, 1, density));
                    resultFile.Flush();
                }
            }
            // Matrix m = new Matrix(w, h);
            //GetStatistics(m, 1000, 0.4f);
            resultFile.Close();
            Console.WriteLine("end");
        }

        static void PerfTest3()
        {
            float n;

            //Matrix m = new Matrix(10, 10);
            //m.GenerateMatrixForTest(0.2f, 4);
            //Console.WriteLine(m.ToString());

            //resultFile.WriteLine("WxH; density; count; time(ms)");
            float[] densities = new float[] { 0.05f, 0.08f, 0.15f, 0.2f, 0.4f, 0.6f };
            string[] sizes = new string[] { "20x40", "20x80", "30x100", "40x100", "50x120", "60x150", "80x200" }; //"100x300"

            // string[] sizes = new string[] {"20x40", "40x100", "100x300", "400x800"};
            foreach (var size in sizes)
            {
                int w = int.Parse(size.Split('x')[0]);
                int h = int.Parse(size.Split('x')[1]);
                Console.WriteLine("Size: {0}", size);
                int state;
                int iterCount;
                foreach (var density in densities)
                {
                    Console.WriteLine("    Density: {0}", density);
                    Matrix m = new Matrix(w, h);
                    //m.GenerateMatrixForTest(density, h/20);
                    m.GenerateMatrix(density);
                    var t = m.Solve2(10000, out state, out iterCount);
                    TextWriter resultFile = new StreamWriter(string.Format("resultDynamicFile_{0}_{1}.csv", size, density));
                    foreach (var res in m.ResultDynamic)
                    {
                        resultFile.WriteLine(string.Format("{0}; {1}", res.Key, res.Value));
                    }
                    // resultFile.WriteLine(GetStatistics(m, 1, density));
                    resultFile.Flush();
                    resultFile.Close();

                }
            }
            // Matrix m = new Matrix(w, h);
            //GetStatistics(m, 1000, 0.4f);
            Console.WriteLine("end");
        }

        static void PerfTest4()
        {
            float n;

            //Matrix m = new Matrix(10, 10);
            //m.GenerateMatrixForTest(0.2f, 4);
            //Console.WriteLine(m.ToString());

            //resultFile.WriteLine("WxH; density; count; time(ms)");
            float[] densities = new float[] { 0.05f };
            string[] sizes = new string[] { "80x300" }; //"100x300"

            // string[] sizes = new string[] {"20x40", "40x100", "100x300", "400x800"};
            foreach (var size in sizes)
            {
                int w = int.Parse(size.Split('x')[0]);
                int h = int.Parse(size.Split('x')[1]);
                Console.WriteLine("Size: {0}", size);
                int state;
                int iterCount;
                foreach (var density in densities)
                {
                    TextWriter resultFile = new StreamWriter(string.Format("resultDynamicFile_{0}_{1}.csv", size, density));
                    for (int i = 1; i < 2; i++)
                    {
                        Console.WriteLine("    Density: {0}", density);
                        Matrix m = new Matrix(w, h);
                        //Matrix m = new MatrixPav(w, h);

                        //m.GenerateMatrixForTest(density, h/20);
                        //m.GenerateMatrix(density);
                        m.GenerateMatrixForTest2(density, 15, i);
                        var t = m.Solve2(1000000, out state, out iterCount);
                        foreach (var res in m.ResultDynamic)
                        {
                            resultFile.WriteLine(string.Format("{0}; {1}", res.Key, res.Value));

                        }
                        resultFile.Flush();
                        resultFile.WriteLine();
                    }

                    // resultFile.WriteLine(GetStatistics(m, 1, density));

                    resultFile.Close();

                }
            }
            // Matrix m = new Matrix(w, h);
            //GetStatistics(m, 1000, 0.4f);
            Console.WriteLine("end");
        }
    }
}

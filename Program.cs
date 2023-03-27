using GameXYZ.Games;
using GameXYZ.output;
using SimulationLibrary.Components;
using SimulationLibrary.Operations;
using SimulationLibrary.Output;
using SimulationLibrary.Utils.Output;
using SimulationLibrary.ProgressVisualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using File = System.IO.File;

namespace GameXYZ
{
    internal class Program
    {
        public const string XMLfilesPath = @"..\..\..\input\XMLs\";

        private static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("----------------------------------------GameXYZ----------------------------------------");
            Console.WriteLine("Enter no of games to be simulated in millions");
            ulong iterations = ulong.Parse(Console.ReadLine()) * 1000000;

            Console.WriteLine();
            Console.WriteLine("Running " + iterations + " spins");
            Console.WriteLine();

            XMLData baseGameData = new(
                XMLfilesPath + "configuration.xml",
                XMLfilesPath + "3x5_slot.xml",
                XMLfilesPath + "3x5_reelstrips.xml",
                XMLfilesPath + "3x5_weighting.xml"
                );

            XMLData hsData = new(
                XMLfilesPath + "configuration.xml",
                XMLfilesPath + "h&s_slot.xml",
                XMLfilesPath + "h&s_reelstrips.xml",
                XMLfilesPath + "H&S_weighting.xml"
                );

            List<Thread> threads = new();

            GameXYZMetricsData metrics = new(baseGameData, hsData);

#if DEBUG
            foreach (int iterationsPerThread in GeneralFunctions.DistributeIntoGroups(iterations, 1))
#else
            foreach (int iterationsPerThread in GeneralFunctions.DistributeIntoGroups(iterations, Environment.ProcessorCount))
#endif
            {
                HoldNSpin holdNspinRound = new(hsData, metrics, baseGameData);
                BaseGame baseGame = new(holdNspinRound, baseGameData, metrics);

                Thread t = new(() => BaseGame.SimulateGames(iterationsPerThread, baseGame));
                threads.Add(t);
                t.Start();
            }

#if !DEBUG
            //ProgressbarCode
            Progress progress = new(metrics, iterations);
            Thread progressThread = new(progress.StartProgressBar);
            progressThread.Start();
            progressThread.Join();
#endif

            threads.ForEach(t => t.Join());

            //write metrics to Excel
            string excelTemplatePath = @"..\..\..\input\Output Template.xlsx";
            string excelOutputPath = @"..\..\..\output\excel\GameXYZ Output_" + string.Format("{0:MM-dd-HH-mm-ss}", DateTime.Now) + ".xlsx";
            //string excelOutputPathMath = @"..\..\..\MathTuning\Game Output_DO_NOT_CHANGE_NAME.xlsx";

            ExcelWriter excelWriter = new(excelTemplatePath, excelOutputPath);
            //ExcelWriter excelWriter1 = new(metrics, excelTemplatePath, excelOutputPathMath);
            excelWriter.WriteMetricsToExcel();
            // excelWriter1.WriteMetricsToExcel();
            stopwatch.Stop();
            double RTP = (metrics.fullGameCoinOut.Value) / ((double)baseGameData.getCostPerSpin() * iterations) * 100;

            string statsString = string.Format("{0, -14}{1, -14}{2, -20}{3, -10}{4, -10}",
                DateTime.Now.ToString("dd/MM/yyyy"),
                iterations,
                stopwatch.Elapsed,
                string.Format("{0:0.0000}", RTP),
                Environment.ProcessorCount) + Environment.NewLine;

            File.AppendAllText(@"..\..\..\output\SimulationHistory.txt", statsString);
            Console.WriteLine();
            Console.WriteLine("Base RTP: " + RTP + "%");
            Console.WriteLine("Time Elapsed: " + stopwatch.Elapsed);
            OutputLibrary.OpenFile(excelOutputPath);
        }
    }
}
using SimulationLibrary.Components;
using SimulationLibrary.Output;
using System.Collections.Generic;
using System.Linq;

namespace GameXYZ.output
{
    internal class GameXYZMetricsData : MetricsData
    {
        public static readonly string SummarySheetName = "Summary";
        public static readonly string BaseGameSheetName = "Base Game";
        public static readonly string FreeGameSheetName = "Free Game";
        public static readonly string HoldNSpinSheetName = "Hold & Spin";

        public static int costPerSpin;
        public static int noOfRows;
        public static int noOfCols;
        public static int lineCount;

        public Dictionary<string, Metric> dictionaryOfMetrics = new Dictionary<string, Metric>();
        public Metric fullGameCoinOut;

        public Metric bgCoinout;
        public Metric bgLinesCoinout;
        public Metric bgScatterCoinout;
        public Metric bgHits;
        public Metric bgWinHits;

        public Metric bgScatterHits;

        public Metric fgCoinout;
        public Metric fgLinesCoinout;
        public Metric fgScatterCoinout;
        public Metric fgHits;
        public Metric fgWinHits;
        public Metric fgLineWinHits;
        public Metric fgLineHits;
        public Metric fgTotalSpins;

        public Metric hsCoinout;
        public Metric hsCreditsCoinout;
        public Metric hsBlackoutGrandCoinout;
        public Metric hsJackpotCoinout;
        public Metric hsHits;

        public Metric gameName;
        public Metric gameType;

        public Metric hsTotalSpinsPlayed;
        public Metric hsTotalCorPayOfInterface;
        public Metric hsTotalCorCountInterface;

        public Metric hsGrandJPcoinout;

        public Metric hsCorsAtTriggerInterface;

        public Metric hsInterfaceCreditsCoinout;

        public Metric hsBlackoutHits;

        public Metric[] hsEntryCorCountTriggers;
        public Metric[] hsExitCorProbability;

        public Metric[,] baseGameCombinationsHits;
        public Metric[,] baseGameCombinationsCoinOut;

        public Metric[,] freeGameCombinationsHits;
        public Metric[,] freeGameCombinationsCoinOut;


        public Metric[] totalPrizeDistributionHits;
        public Metric[] totalPrizeDistributionCoinOut;

        public Metric[] FGprizeDistributionCoinOut;
        public Metric[] FGprizeDistributionHits;

        public Metric[] basePrizeDistributionHits;
        public Metric[] basePrizeDistributionCoinOut;

        private int[] buckets = { 0, 1, 2, 3, 4, 5, 10, 15, 20, 30, 50, 100, 200 };

        public Metric[] hsPrizeDistributionHits;
        public Metric[] hsPrizeDistributionCoinOut;



        public Dictionary<string, Metric> hsJPdictCorCoinoutInterface;
        public Dictionary<string, Metric> hsJPdictHitsInterface;

        public Metric hsTotalCorCollectedDuringHS;
        public Metric hsCorsAddedPerSpin;
        public Metric hsCorsCollectedAtTrigger;
        public Metric hsTotalCorValueAtTrigger;

        public GameXYZMetricsData(XMLData baseGameData, XMLData freeGameData)
        {
            costPerSpin = baseGameData.getCostPerSpin();
            noOfRows = baseGameData.getNoOfRows();
            noOfCols = baseGameData.getNoOfReels();
            lineCount = baseGameData.GetLineCount();
            //print text data
            OutputLibrary.printData(SummarySheetName, "H4", baseGameData.getAllCombos());
            OutputLibrary.printData(SummarySheetName, "B1", baseGameData.GetGameName());
            OutputLibrary.printData(SummarySheetName, "B3", costPerSpin.ToString());
            OutputLibrary.printData(SummarySheetName, "B4", lineCount.ToString());

            baseGameCombinationsHits = OutputLibrary.createSymbolComboMetrics(baseGameData, BaseGameSheetName, "I4");
            baseGameCombinationsCoinOut = OutputLibrary.createSymbolComboMetrics(baseGameData, BaseGameSheetName, "J4");

            noOfBets = OutputLibrary.CreateMetric(SummarySheetName, "B7");
            OutputLibrary.printData(SummarySheetName, "U3", noOfRows.ToString());
            OutputLibrary.printData(SummarySheetName, "U4", noOfCols.ToString());
            fullGameCoinOut = OutputLibrary.CreateMetric(SummarySheetName, "B9");

            bgCoinout = OutputLibrary.CreateMetric(BaseGameSheetName, "T3");
            bgLinesCoinout = OutputLibrary.CreateMetric(BaseGameSheetName, "T4");
            bgScatterCoinout = OutputLibrary.CreateMetric(BaseGameSheetName, "T5");
            bgHits = OutputLibrary.CreateMetric(BaseGameSheetName, "T7");
            bgWinHits = OutputLibrary.CreateMetric(BaseGameSheetName, "T8");

            fgCoinout = OutputLibrary.CreateMetric(FreeGameSheetName, "X7");
            fgLinesCoinout = OutputLibrary.CreateMetric(FreeGameSheetName, "X8");
            fgScatterCoinout = OutputLibrary.CreateMetric(FreeGameSheetName, "X9");
            fgTotalSpins = OutputLibrary.CreateMetric(FreeGameSheetName, "X4");
            fgHits = OutputLibrary.CreateMetric(FreeGameSheetName, "X5");
            fgLineHits = OutputLibrary.CreateMetric(FreeGameSheetName, "X11");
            fgLineWinHits = OutputLibrary.CreateMetric(FreeGameSheetName, "X12");

            hsCoinout = OutputLibrary.CreateMetric(HoldNSpinSheetName, "X38");

            hsHits = OutputLibrary.CreateMetric(HoldNSpinSheetName, "B2");

            hsTotalSpinsPlayed = OutputLibrary.CreateMetric(HoldNSpinSheetName, "X3");
            hsTotalCorCountInterface = OutputLibrary.CreateMetric(HoldNSpinSheetName, "X4");
            //hsGrandJPcoinout = OutputLibrary.CreateMetric(HoldNSpinSheetName, "X17");
            hsBlackoutGrandCoinout = OutputLibrary.CreateMetric(HoldNSpinSheetName, "X18");

            //hsCorsAtTriggerInterface = OutputLibrary.CreateMetric(HoldNSpinSheetName, "Y9");
            hsInterfaceCreditsCoinout = OutputLibrary.CreateMetric(HoldNSpinSheetName, "X16");

            hsJPdictCorCoinoutInterface = OutputLibrary.CreateMetricsDict(baseGameData.getJackpotNames(), HoldNSpinSheetName, "X9");
            hsJPdictHitsInterface = OutputLibrary.CreateMetricsDict(baseGameData.getJackpotNames(), HoldNSpinSheetName, "Y9");

            hsEntryCorCountTriggers = OutputLibrary.createColumnMetricArray(HoldNSpinSheetName, "X22", 15);
            hsExitCorProbability = OutputLibrary.createColumnMetricArray(HoldNSpinSheetName, "AA22", 15);

            hsBlackoutHits = OutputLibrary.CreateMetric(HoldNSpinSheetName, "Y18");

            basePrizeDistributionHits = OutputLibrary.createColumnMetricArray(BaseGameSheetName, "O4", buckets.Length);
            FGprizeDistributionHits = OutputLibrary.createColumnMetricArray(FreeGameSheetName, "T4", buckets.Length);
            hsPrizeDistributionHits = OutputLibrary.createColumnMetricArray(HoldNSpinSheetName, "O4", buckets.Length);
            totalPrizeDistributionHits = OutputLibrary.createColumnMetricArray(SummarySheetName, "N4", buckets.Length);

            //OutputLibrary.printData(BaseGameSheetName, "N4", buckets);
            //hsCorsAddedPerSpin = OutputLibrary.CreateMetric(HoldNSpinSheetName, "AR4");
            //hsCorsCollectedAtTrigger = OutputLibrary.CreateMetric(HoldNSpinSheetName, "AU1");
            //hsTotalCorValueAtTrigger = OutputLibrary.CreateMetric(HoldNSpinSheetName, "AR1");
        }

        public void populateTotalGameDistributions(int totalPay)
        {
            OutputLibrary.AddToDistribution(totalPay, costPerSpin, totalPrizeDistributionHits,  buckets);
        }

        public void populateFreeGameDistributions(int freeSpinPay)
        {
            OutputLibrary.AddToDistribution(freeSpinPay, costPerSpin, FGprizeDistributionHits,  buckets);
        }

        public void populateBaseGameDistributions(int baseGamePay)
        {
            OutputLibrary.AddToDistribution(baseGamePay, costPerSpin, basePrizeDistributionHits, buckets);
        }

        public void populateHSGameDistributions(int hsPay)
        {
            OutputLibrary.AddToDistribution(hsPay, costPerSpin, hsPrizeDistributionHits, buckets);
        }

        //public void writeData(string sheetName, string cellID, string data)
        //{
        //    OutputLibrary.printData(sheetName, cellID, data);
        //}
    }
}
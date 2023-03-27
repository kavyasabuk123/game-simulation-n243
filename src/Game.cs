using GameXYZ.output;
using SimulationLibrary.Components;
using SimulationLibrary.Output;
using System;

namespace GameXYZ.Games
{
    internal abstract class Game
    {
        public static int costPerSpin;
        protected readonly int[,] symbolDistributionPerReel;
        protected readonly Random rand = new(Guid.NewGuid().GetHashCode());

        protected int noOfRows;
        protected int noOfReels;

        protected int totalPay = 0;
        protected int hsPay = 0;

        protected GameXYZMetricsData metrics;
        protected XMLData gameData;

        protected Metric[,] gameCombinationsHits;
        protected Metric[,] gameCombinationsCoinOut;

        public PlayWindow currentWindow;

        protected int[] reelStops = { 0, 0, 0, 0, 0 };

        protected Game(XMLData gameData, GameXYZMetricsData metrics)
        {
            this.metrics = metrics;
            this.gameData = gameData;

            JackpotBonuses.jackpots = gameData.getJackpotConfiguration();
            JackpotBonuses.costPerSpin = gameData.getCostPerSpin();

            noOfRows = gameData.getNoOfRows();
            noOfReels = gameData.getNoOfReels();
            currentWindow = new(gameData.getNoOfRows(), gameData.getNoOfReels());

            symbolDistributionPerReel = new int[gameData.getNoOfUniqueSymbols() + 1, gameData.getNoOfReels()];
        }

        public abstract int Spin();
    }
}
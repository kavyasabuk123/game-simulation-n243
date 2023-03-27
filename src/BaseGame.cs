using GameXYZ.output;
using SimulationLibrary.Components;
using SimulationLibrary.Operations;
using System.Collections.Generic;

namespace GameXYZ.Games
{
    internal class BaseGame : Game
    {
        #region Constants

        private readonly HoldNSpin _featureRound;
        private readonly int[] lineSymbolsToEvaluate;

        private const string wildSymbol = "W";
        protected int wildSymbolId;

        private const string stackSymbol = "SS1";
        protected int stackSymbolID;

        private const string stackSymbol2 = "SS2";
        protected int stackSymbol2ID;

        private const string noneSymbol = "Blank";
        protected int noneSymbolID = 0;

        private const string corSymbol = "COR";
        protected int corSymbolIDofBG;

        private const string SCATSymbol = "SCAT";
        protected int SCATSymbolID;

        protected int hsTriggerCondition = 6;
        protected int freeGameTrigCondition = 3;

        private int corCountOfInterface = 0;
        private int scatCount;
        private int scatPay;

        #endregion Constants

        public BaseGame(HoldNSpin freeGameRound, XMLData baseGameData, GameXYZMetricsData metrics) :
            base(baseGameData, metrics)
        {
            _featureRound = freeGameRound;

            gameCombinationsHits = metrics.baseGameCombinationsHits;
            gameCombinationsCoinOut = metrics.baseGameCombinationsCoinOut;

            costPerSpin = baseGameData.getCostPerSpin();

            lineSymbolsToEvaluate = new int[gameData.getNoOfReels()];
            wildSymbolId = gameData.getSymbolID(wildSymbol);
            stackSymbolID = gameData.getSymbolID(stackSymbol);
            stackSymbol2ID = gameData.getSymbolID(stackSymbol2);
            SCATSymbolID = gameData.getSymbolID(SCATSymbol);
            corSymbolIDofBG = gameData.getSymbolID(corSymbol);
        }

        public override int Spin()
        {
            JackpotBonuses.IncrementAllJackpots();
            string reelSetName = "3x5_BaseReels_" + gameData.pullFromStringTable("3x5_ReelSet", rand);

            GeneralFunctions.LandReels(
                gameData.getAllReelStrips(),
                reelSetName,
                reelStops,
                currentWindow,
                rand);

#if DEBUG
            //PrintLibrary.Print(currentWindow, "Interface Before StackReplacement", gameData.getIDToSymbolMap());
#endif

            StackReplacement(reelSetName);

#if DEBUG
            //PrintLibrary.Print(currentWindow, "Interface After StackReplacement", gameData.getIDToSymbolMap());
#endif

            int linesPay = EvaluateLinePayForWindow(currentWindow);

#if DEBUG
            //PrintLibrary.Print(currentWindow, "Final Interface ", gameData.getIDToSymbolMap());
            //Console.WriteLine("Max: " + highestCorValue);
            //Console.WriteLine("Interface cor count: " + corCountOfInterface);
#endif
            #region Scatter And Free Games

            scatCount = currentWindow.getSymbolCount(SCATSymbolID);
            scatPay = GetScatterPay(scatCount);
            InitiateFreeGamesIfConditionMet(scatCount);

            #endregion

            MetricsForBaseGame(linesPay, scatPay);

            corCountOfInterface = currentWindow.getSymbolCount(corSymbolIDofBG);

            #region HoldNSpin

            if (corCountOfInterface >= hsTriggerCondition)
            {
                MetricsForEntryCorProbability(corCountOfInterface);

#if DEBUG
                //PrintLibrary.Print(persistenceWindow, "PW At Trigger", gameData.getIDToSymbolMap());
                //PrintLibrary.Print(currentWindow, "CW At Trigger", gameData.getIDToSymbolMap());
#endif

                MetricsForHoldNSpinInterface();

#if DEBUG
                //PrintLibrary.Print(persistenceWindow, "Persistence Row", gameData.getIDToSymbolMap());
                //PrintLibrary.Print(currentWindow, "Interface", gameData.getIDToSymbolMap());
#endif

                _featureRound.CopyCorsToHoldNSpinWindow(currentWindow, corSymbolIDofBG);
                _featureRound.HoldSpin(corCountOfInterface, reelSetName);
            }

            #endregion HoldNSpin

            #region Metrics

            metrics.noOfBets.Increment();
            metrics.fullGameCoinOut.Add(linesPay+scatPay);
            metrics.populateTotalGameDistributions(totalPay);

            #endregion Metrics

            ResetEverything();
            return 0;
        }

        public void StackReplacement(string reelSetName)
        {
            string stackReplacementSymbol = gameData.pullFromStringTable("3x5_SS1_Base_" + reelSetName.Substring(reelSetName.Length - 4) + "_Replacement", rand);
            string stackReplacement2Symbol = gameData.pullFromStringTable("3x5_SS2_Base_" + reelSetName.Substring(reelSetName.Length - 4) + "_Replacement", rand);

            for (int i = 0; i < noOfRows; i++)
            {
                for (int j = 0; j < noOfReels; j++)
                {
                    if (currentWindow.symbols[i, j] == stackSymbolID)
                    {
                        currentWindow.symbols[i, j] = gameData.getSymbolID(stackReplacementSymbol);
                    }
                    else if (currentWindow.symbols[i, j] == stackSymbol2ID)
                    {
                        currentWindow.symbols[i, j] = gameData.getSymbolID(stackReplacement2Symbol);
                    }
                }
            }
        }

        public void StackFGReplacement(string reelSetName)
        {
            string stackReplacementSymbol = gameData.pullFromStringTable("3x5_SS1_FG_Replacement", rand);
            string stack2ReplacementSymbol = gameData.pullFromStringTable("3x5_SS1_FG_Replacement", rand);

            for (int i = 0; i < noOfRows; i++)
            {
                for (int j = 0; j < noOfReels; j++)
                {
                    if (currentWindow.symbols[i, j] == stackSymbolID)
                    {
                        currentWindow.symbols[i, j] = gameData.getSymbolID(stackReplacementSymbol);
                    }
                    else if (currentWindow.symbols[i, j] == stackSymbol2ID)
                    {
                        currentWindow.symbols[i, j] = gameData.getSymbolID(stack2ReplacementSymbol);
                    }
                }
            }
        }

        public void MetricsForHoldNSpinInterface()
        {
            for (int i = 0; i < noOfRows; i++)
            {
                for (int j = 0; j < noOfReels; j++)
                {
                    if (currentWindow.symbolData[i, j] is string)
                    {
                        int jpPay = JackpotBonuses.AwardJackpot(currentWindow.symbolData[i, j]);
                        MetricsForHSJackpot(jpPay, i, j);
                        //metrics.hsJPdictCorCoinoutInterface[currentWindow.symbolData[i, j]].Add(jpPay);
                        //metrics.hsCoinout.Add(jpPay);
                        //metrics.hsTotalCorPayOfInterface.Add(jpPay);
                        //metrics.hsTotalCorCountInterface.Increment();
                        //metrics.hsJPdictHitsInterface[currentWindow.symbolData[i,j]].Increment();
                        //metrics.hsTotalCorValueAtTrigger.Add(jpPay);
                        //metrics.fullGameCoinOut.Add(jpPay);
                    }
                    else if (currentWindow.symbolData[i, j] is double)
                    {
                        MetricsForHSCredits(i, j);
                    }
                }
            }
        }

        public void MetricsForEntryCorProbability(int corCountOfInterface)
        {
            metrics.hsEntryCorCountTriggers[corCountOfInterface - hsTriggerCondition].Increment();
            metrics.hsHits.Increment();
        }

        public void MetricsForBaseGame(int linesPay, int scatPay)
        {
            metrics.bgLinesCoinout.Add(linesPay);
            metrics.bgScatterCoinout.Add(scatPay);
            metrics.bgCoinout.Add(linesPay + scatPay);
            totalPay += linesPay + scatPay;

            if (linesPay > 0) metrics.bgHits.Increment();
            if (linesPay > gameData.getCostPerSpin()) metrics.bgWinHits.Increment();

            metrics.populateBaseGameDistributions(linesPay + scatPay);
        }

        public void MetricsForHSJackpot(int jpPay, int rowIndex, int colIndex)
        {
            metrics.hsJPdictCorCoinoutInterface[currentWindow.symbolData[rowIndex, colIndex]].Add(jpPay);
            metrics.hsCoinout.Add(jpPay);
            metrics.hsTotalCorPayOfInterface.Add(jpPay);
            metrics.hsTotalCorCountInterface.Increment();
            metrics.hsJPdictHitsInterface[currentWindow.symbolData[rowIndex, colIndex]].Increment();
            metrics.hsTotalCorValueAtTrigger.Add(jpPay);
            metrics.fullGameCoinOut.Add(jpPay);
            totalPay += jpPay;
            hsPay += jpPay;
        }

        public void MetricsForHSCredits(int rowIndex, int colIndex)
        {
            metrics.hsInterfaceCreditsCoinout.Add((int)(currentWindow.symbolData[rowIndex, colIndex] * gameData.getCostPerSpin()));
            metrics.hsCoinout.Add((int)(currentWindow.symbolData[rowIndex, colIndex] * gameData.getCostPerSpin()));
            metrics.hsTotalCorPayOfInterface.Add((int)(currentWindow.symbolData[rowIndex, colIndex] * gameData.getCostPerSpin()));
            metrics.hsTotalCorCountInterface.Increment();
            metrics.hsTotalCorValueAtTrigger.Add((int)(currentWindow.symbolData[rowIndex, colIndex] * gameData.getCostPerSpin()));
            metrics.fullGameCoinOut.Add((int)(currentWindow.symbolData[rowIndex, colIndex] * gameData.getCostPerSpin()));
            totalPay += (int)(currentWindow.symbolData[rowIndex, colIndex] * gameData.getCostPerSpin());
            hsPay += (int)(currentWindow.symbolData[rowIndex, colIndex] * gameData.getCostPerSpin());
        }

        public dynamic DetermineCorValueForInterface()
        {
            string corValue = gameData.pullFromStringTable("COR_3X5", rand);

            if (corValue.StartsWith("M"))
            {
                string jackpotName = corValue;

                return jackpotName;
            }
            else
            {
                return double.Parse(corValue);
            }
        }

        public void ResetEverything()
        {
            currentWindow.Clear();
            totalPay = 0;
            hsPay = 0;
            corCountOfInterface = 0;
        }

        public static void SimulateGames(int noOfGames, BaseGame game)
        {
            for (int i = 0; i < noOfGames; i++)
            {
                game.Spin();
            }
        }

        protected int EvaluateLinePayForWindow(PlayWindow reelWindow)
        {
            int totalPay = 0;
            foreach (List<int> playLine in gameData.getPlayLines())
            {
                for (int i = 0; i < lineSymbolsToEvaluate.Length; i++)
                {
                    //TODO remove this and directly pass line positions
                    lineSymbolsToEvaluate[i] = reelWindow.symbols[playLine[i] + 1, i];
                }
                totalPay += GetPayForLine();
            }
            return totalPay;
        }

        private int GetPayForLine()
        {
            //First non wild symbol, if none exist it is 5 OAK wild case

            int firstNonWildSymbol = wildSymbolId;
            int wildOAK = gameData.getNoOfReels();

            //
            int OAK = 0;

            //reel position of the line
            int reel;

            //Wild only win
            for (reel = 0; reel < lineSymbolsToEvaluate.Length; reel++)
            {
                if (!(lineSymbolsToEvaluate[reel] == wildSymbolId))
                {
                    firstNonWildSymbol = lineSymbolsToEvaluate[reel];
                    wildOAK = reel;
                    OAK = reel;
                    break;
                }
            }
            int wildOnlyWin = gameData.getComboPay(wildSymbolId, wildOAK);

            //Extended win
            for (; reel < lineSymbolsToEvaluate.Length; ++reel)
            {
                if (lineSymbolsToEvaluate[reel] == firstNonWildSymbol || lineSymbolsToEvaluate[reel] == wildSymbolId)
                {
                    ++OAK;
                }
                else
                {
                    break;
                }
            }
            int extendedWin = gameData.getComboPay(firstNonWildSymbol, OAK);

            int awardedComboSymbol;
            int awardedOAK;
            int awardedWin;
            if (extendedWin > wildOnlyWin)
            {
                awardedComboSymbol = firstNonWildSymbol;
                awardedOAK = OAK;
                awardedWin = extendedWin;
            }
            else
            {
                awardedComboSymbol = wildSymbolId;
                awardedOAK = wildOAK;
                awardedWin = wildOnlyWin;
            }

            //Metrics
            if (awardedWin > 0)
            {
                gameCombinationsCoinOut[awardedComboSymbol, awardedOAK].Add(awardedWin);
                gameCombinationsHits[awardedComboSymbol, awardedOAK].Increment();
            }
            return awardedWin;
        }

        private void InitiateFreeGamesIfConditionMet(int scatCount)
        {
            if (scatCount >= freeGameTrigCondition)
            {
                int numFGs = gameData.getSpinsAwarded(scatCount);
                PlayFreeGamesAndGetPay(numFGs);
                metrics.fgHits.Increment();
            }
        }

        private int PlayFreeGamesAndGetPay(int nFreeGames)
        {
            int freeGamePay = 0;

            for (int i = 0; i < nFreeGames; ++i)
            {
                freeGamePay += FGSpin();
            }

            metrics.fullGameCoinOut.Add(freeGamePay);
            metrics.populateFreeGameDistributions(freeGamePay);
            totalPay += freeGamePay;

            return freeGamePay;
        }

        private int FGSpin()
        {
            string fGReelSetName = "3x5_FGReels";

            GeneralFunctions.LandReels(gameData.getAllReelStrips(), fGReelSetName, reelStops, currentWindow, rand);
            StackFGReplacement(fGReelSetName);

            int linesPay = EvaluateLinePayForWindow(currentWindow);
            int scatCount = currentWindow.getSymbolCount(SCATSymbolID);
            int scatPay = GetScatterPay(scatCount);

            MetricsForFG(linesPay, scatPay);    

            return (linesPay + scatPay);
        }


        public void MetricsForFG(int linesPay, int scatPay)
        {
            metrics.fgLinesCoinout.Add(linesPay);
            metrics.fgScatterCoinout.Add(scatPay);
            metrics.fgCoinout.Add(linesPay + scatPay);
            metrics.fgTotalSpins.Increment();
            if (linesPay > 0) metrics.fgLineHits.Increment();
            if (linesPay > gameData.getCostPerSpin())         metrics.fgLineWinHits.Increment();
        }
        private int GetScatterPay(int scatCount)
        {
            int scatPay = 0;

            if (scatCount >= freeGameTrigCondition)
            {
                scatPay = gameData.getScatComboPay(SCATSymbolID, scatCount);
            }

            return scatPay * gameData.getCostPerSpin();
        }
    }
}
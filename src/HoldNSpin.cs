using GameXYZ.output;
using SimulationLibrary.Components;

namespace GameXYZ.Games
{
    internal class HoldNSpin : Game
    {
        public PlayWindow holdNSpinWindow;

        private XMLData baseGameData;

        private const string corSymbol = "COR";
        protected int corIDofHS;

        private const string blankSymbol = "Blank";
        protected int blankIDofHS;

        public HoldNSpin(XMLData featureRoundData, GameXYZMetricsData metrics, XMLData baseGameData) : base(featureRoundData, metrics)
        {
            this.baseGameData = baseGameData;
            noOfRows = baseGameData.getNoOfRows();
            noOfReels = baseGameData.getNoOfReels();

            corIDofHS = gameData.getSymbolID(corSymbol);
            blankIDofHS = gameData.getSymbolID(blankSymbol);

            holdNSpinWindow = new(noOfRows, noOfReels);
        }

        public void HoldSpin(int corCountInterface, string reelSetName)
        {
#if DEBUG
            //PrintLibrary.Print(holdNSpinWindow, "Triggering Window", gameData.getIDToSymbolMap());
#endif

            #region Metrics

            // metrics.hsCorsAtTriggerInterface.Add(corCountInterface);

            #endregion Metrics

            int totalCorCountHS = corCountInterface;

            int hsSpinsLeft = 3;
            int totalCells = noOfRows * noOfReels;
            int blankCells = 0;

            while (hsSpinsLeft > 0)
            {
                metrics.hsTotalSpinsPlayed.Increment();

                hsSpinsLeft--;
                blankCells = totalCells - totalCorCountHS;
                string blankReelStrip = "HS_Normal_Locked_" + totalCorCountHS + " Locked";

                for (int i = 0; i < noOfRows; i++)
                {
                    for (int j = 0; j < noOfReels; j++)
                    {
                        if (holdNSpinWindow.symbols[i, j] == blankIDofHS)
                        {
                            int landedSymbolID = gameData.landSingleReel(blankReelStrip, rand);

                            if (landedSymbolID == corIDofHS)
                            {
                                hsSpinsLeft = 3;
                                totalCorCountHS++;
                                holdNSpinWindow.symbols[i, j] = corIDofHS;                                    //resetspins and updAte window. Do function
                                //metrics.hsTotalCorCollectedDuringHS.Increment();

                                corCountInterface++;
                                holdNSpinWindow.symbolData[i, j] = DetermineHSCorValue();

                                #region Metamorphic and Metrics

                                if (holdNSpinWindow.symbolData[i, j] is string)
                                {
                                    double jpValue = JackpotBonuses.GetJPvalue(holdNSpinWindow.symbolData[i, j]);
                                    int jpPay = JackpotBonuses.AwardJackpot(holdNSpinWindow.symbolData[i, j]);

                                    #region Metrics

                                    metrics.hsJPdictCorCoinoutInterface[holdNSpinWindow.symbolData[i, j]].Add(jpPay);
                                    metrics.hsJPdictHitsInterface[holdNSpinWindow.symbolData[i, j]].Increment();
                                    metrics.hsCoinout.Add(jpPay);
                                    //metrics.hsTotalCorPayOfInterface.Add(jpPay);
                                    metrics.fullGameCoinOut.Add(jpPay);
                                    metrics.hsTotalCorCountInterface.Increment();
                                    totalPay += jpPay;
                                    hsPay += jpPay;

                                    #endregion Metrics
                                }
                                else if (holdNSpinWindow.symbolData[i, j] is double)
                                {
                                    #region Metrics

                                    metrics.hsInterfaceCreditsCoinout.Add((int)(holdNSpinWindow.symbolData[i, j] * gameData.getCostPerSpin()));
                                    metrics.hsCoinout.Add((int)(holdNSpinWindow.symbolData[i, j] * gameData.getCostPerSpin()));
                                    //metrics.hsTotalCorPayOfInterface.Add((int)(holdNSpinWindow.symbolData[i, j] * gameData.getCostPerSpin()));
                                    metrics.fullGameCoinOut.Add((int)(holdNSpinWindow.symbolData[i, j] * gameData.getCostPerSpin()));
                                    metrics.hsTotalCorCountInterface.Increment();
                                    totalPay += (int)(holdNSpinWindow.symbolData[i, j] * gameData.getCostPerSpin());
                                    hsPay += (int)(holdNSpinWindow.symbolData[i, j] * gameData.getCostPerSpin());

                                    #endregion Metrics
                                }

                                #endregion Metamorphic and Metrics

#if DEBUG
                                //PrintLibrary.Print(holdNSpinWindow, "HoldNSpin", gameData.getIDToSymbolMap());
                                //Console.WriteLine("Highest COR: " + highestCorValue);
#endif
                            }
                        }
                    }
                }

#if DEBUG
                //PrintLibrary.Print(holdNSpinWindow, "Window after 1 Spin", gameData.getIDToSymbolMap());
                //Console.WriteLine("Highest COR: " + highestCorValue);
#endif

                SetSpinsToZeroAfterBlackout(ref hsSpinsLeft, totalCells, totalCorCountHS);
                //if (totalCells == totalCorCountHS)
                //{
                //    hsSpinsLeft = 0;
                //}
            }
#if DEBUG
            //PrintLibrary.Print(holdNSpinWindow, "Final Window", gameData.getIDToSymbolMap());
#endif

            #region Blackout

            if (totalCells == totalCorCountHS)
            {
                int grandJPaward = JackpotBonuses.AwardJackpot("Grand");

                #region Metrics

                metrics.hsCoinout.Add(grandJPaward);
                //metrics.hsGrandJPcoinout.Add(grandJPaward);
                metrics.fullGameCoinOut.Add(grandJPaward);
                metrics.hsBlackoutHits.Increment();
                metrics.hsBlackoutGrandCoinout.Add(grandJPaward);
                totalPay += grandJPaward;
                hsPay += grandJPaward;

                #endregion Metrics
            }

            #endregion Blackout

            metrics.hsExitCorProbability[totalCorCountHS - 6].Increment();
            metrics.populateHSGameDistributions(hsPay);

            holdNSpinWindow.Clear();
        }

        public void CopyCorsToHoldNSpinWindow(PlayWindow currentWindow, int corSymbolIDofBG)
        {
            #region Copying CORs from Interface

            for (int i = 0; i < currentWindow.symbols.GetLength(0); i++)
            {
                for (int j = 0; j < currentWindow.symbols.GetLength(1); j++)
                {
                    if (currentWindow.symbols[i, j] == corSymbolIDofBG)
                    {
                        holdNSpinWindow.symbols[i, j] = corIDofHS;
                        holdNSpinWindow.symbolData[i, j] = currentWindow.symbolData[i, j];
                    }
                    else
                    {
                        holdNSpinWindow.symbols[i, j] = blankIDofHS;
                    }
                }
            }

            #endregion Copying CORs from Interface
        }

        public void SetSpinsToZeroAfterBlackout(ref int spinsLeft, int totalCells, int totalCorCountHS)
        {
            if (totalCells == totalCorCountHS)
            {
                spinsLeft = 0;
            }
        }

        public dynamic DetermineHSCorValue()
        {
            string corValue = gameData.pullFromStringTable("COR_3x5", rand);

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

        public override int Spin()
        {
            return 0;
        }
    }
}
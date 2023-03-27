Enclosed project contains the complete math simulation for N206. 
1. The entry point for the code is in Program.cs. 
2. Once run, it asks for the number of simulations to run. 
3. Enter an appropriate value and the code outputs file into /Metrics/Output/. 
4. Release mode runs the program in multiple threads, Debug mode runs it as single threaded.


Few pointers:
1. BaseGame.cs and FreeGame.cs logic share several logical similarities, and common aspects are factored into a parent Game.cs.
2. JackpotBonuses.cs is thread safe and has everything related to JP bonus round, incrementing and resetting JP values.
3. ExcelWriter.cs takes in an excel output template and populate all the required metrics into a new template copy.
4. MetricsData.cs contains all the required metrics. To add a new metric, simply add a new class variable and instantiate it using the relevant Excel Template sheet name and cell ID. Then, use that class variable in relevant parts of the code. Use OutputLibrary functions to create Metrics.
5. SimulationHistory.txt stores few of the important metrics of all the simulations that are run till now.
6. Progress.cs prints progress bar on console.
8. Program.cs contains the entry point to the code, and logic to create several threads that can run on multicore CPU. 
9. This Project uses another Library(SimulationLibrary) written to work cohesively with this.

SimulationLibrary
1. This project contains several helper methods and classes that can be used for any slot game.
2. JackpotPrize.cs is thread safe and stores one jackpot value and has methods to increment and reset.
3. Metric stores one value and has thread safe methods to increment and add.
4. ProgressBar.cs creates and prints progress bar periodically. It has methods to input the % to show.
5. TupleComparer.cs compares two tuples <string, int> using item2 to be used in binary search.
6. WeightTable.cs stores one table, its weights and its cumulative weights.
7. PrintLibrary.cs contains all print utility functions.
8. SimLibrary.cs contains all simulation helper functions.
9. XMLData.cs is the main storage class for XML data.
10. OutputLibrary.cs contains functions relevent to writing to an Excel template.
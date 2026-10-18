using System.Globalization;
using Microsoft.ML;
using QueueManagementSystem.Models;
using QueueManagementSystem.Services;

namespace QueueManagementSystem.Ml
{
    public static class ModelTrainer
    {
        private const int FirstTestDay = 5;

        public static int Run(string projectDir)
        {
            string csvPath = Path.Combine(projectDir, "MLData", "queue_data.csv");
            string outPath = Path.Combine(projectDir, "MLModels", "wait_model.zip");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Training data not found: {csvPath}");
                Console.WriteLine("Run this command from the project folder.");
                return 1;
            }

            var inv = CultureInfo.InvariantCulture;

            var rows = File.ReadLines(csvPath)
                .Skip(1)
                .Where(l => l.Length > 0)
                .Select(line =>
                {
                    var c = line.Split(',');
                    var input = FeatureBuilder.Create(
                        serviceType: c[2],
                        dayOfWeek: int.Parse(c[4], inv),
                        hour: double.Parse(c[5], inv),
                        queueLength: double.Parse(c[6], inv),
                        busyCounters: double.Parse(c[7], inv),
                        activeCounters: double.Parse(c[8], inv),
                        avgServiceRecent: double.Parse(c[9], inv),
                        arrivalsLast15: double.Parse(c[10], inv));

                    input.WaitMinutes = float.Parse(c[11], inv);
                    return (Day: int.Parse(c[1], inv), Input: input);
                }).ToList();

            var train = rows.Where(r => r.Day < FirstTestDay).Select(r => r.Input).ToList();
            var test = rows.Where(r => r.Day >= FirstTestDay).Select(r => r.Input).ToList();

            Console.WriteLine($"Rows: {rows.Count:N0} | train {train.Count:N0} | held-out {test.Count:N0}");

            var ml = new MLContext(seed: 42);
            var trainData = ml.Data.LoadFromEnumerable(train);
            var testData = ml.Data.LoadFromEnumerable(test);

            string[] numeric =
            {
                nameof(WaitModelInput.DayOfWeek),
                nameof(WaitModelInput.Hour),
                nameof(WaitModelInput.QueueLength),
                nameof(WaitModelInput.BusyCounters),
                nameof(WaitModelInput.ActiveCounters),
                nameof(WaitModelInput.AvgServiceRecent),
                nameof(WaitModelInput.ArrivalsLast15),
                nameof(WaitModelInput.QueuePerCounter),
                nameof(WaitModelInput.WorkloadEstimate),
                nameof(WaitModelInput.UtilisationNow)
            };

            var pipeline = ml.Transforms.Categorical.OneHotEncoding("ServiceTypeEncoded", nameof(WaitModelInput.ServiceType))
                .Append(ml.Transforms.Concatenate("Features", new[] { "ServiceTypeEncoded" }.Concat(numeric).ToArray()))
                .Append(ml.Regression.Trainers.FastForest(
                    labelColumnName: nameof(WaitModelInput.WaitMinutes),
                    featureColumnName: "Features",
                    numberOfLeaves: 64,
                    numberOfTrees: 200,
                    minimumExampleCountPerLeaf: 10));

            Console.WriteLine("Training the Random Forest (this can take a few minutes)...");
            var model = pipeline.Fit(trainData);

            var metrics = ml.Regression.Evaluate(model.Transform(testData), labelColumnName: nameof(WaitModelInput.WaitMinutes));

            Console.WriteLine($"Random Forest  MAE {metrics.MeanAbsoluteError:F2} min | RMSE {metrics.RootMeanSquaredError:F2} | R2 {metrics.RSquared:F3}");

            if (test.Count > 0)
            {
                double formulaMae = test.Average(t => Math.Abs(t.WorkloadEstimate - t.WaitMinutes));
                Console.WriteLine($"Formula only   MAE {formulaMae:F2} min");
            }
            else
            {
                Console.WriteLine("No held-out data for formula comparison.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            ml.Model.Save(model, trainData.Schema, outPath);
            Console.WriteLine($"Saved {outPath}");
            Console.WriteLine("Restart the application so it loads the new model.");
            return 0;
        }
    }
}

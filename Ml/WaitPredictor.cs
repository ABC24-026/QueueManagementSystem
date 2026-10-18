using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using QueueManagementSystem.Models;

namespace QueueManagementSystem.Ml
{
    public interface IWaitPredictor
    {
        bool IsLoaded { get; }
        string? LoadError { get; }
        float? Predict(WaitModelInput input);
    }

    public class MlWaitPredictor : IWaitPredictor
    {
        private readonly object _lock = new();
        private PredictionEngine<WaitModelInput, WaitModelOutput>? _engine;

        public bool IsLoaded => _engine != null;
        public string? LoadError { get; private set; }

        public MlWaitPredictor(IWebHostEnvironment env, ILogger<MlWaitPredictor> log)
        {
            string path = Path.Combine(env.ContentRootPath, "MLModels", "wait_model.zip");

            if (!File.Exists(path))
            {
                LoadError = "MLModels/wait_model.zip not found. Train it with: dotnet run -- --train";
                log.LogWarning("{Message} Using the rule-based formula.", LoadError);
                return;
            }

            try
            {
                var ml = new MLContext(seed: 42);
                ml.ComponentCatalog.RegisterAssembly(typeof(FastForestRegressionTrainer).Assembly);

                using var stream = File.OpenRead(path);
                ITransformer model = ml.Model.Load(stream, out _);
                _engine = ml.Model.CreatePredictionEngine<WaitModelInput, WaitModelOutput>(model);

                log.LogInformation("Waiting-time model loaded from {Path}", path);
            }
            catch (Exception ex)
            {
                LoadError = ex.Message;
                log.LogError(ex, "Could not load the waiting-time model. Using the rule-based formula.");
            }
        }

        public float? Predict(WaitModelInput input)
        {
            if (_engine == null) return null;

            lock (_lock)
            {
                return _engine.Predict(input).Score;
            }
        }
    }
}
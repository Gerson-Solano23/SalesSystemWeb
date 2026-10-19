using System;
using System.Collections.Generic;
using System.Linq;

namespace SalesSystem.BLL.Forecasting
{
    /// <summary>
    /// Holt's linear trend method (double exponential smoothing), the same model the original
    /// Python prototype used (ExponentialSmoothing with an additive trend). The smoothing factors
    /// are chosen by grid search minimizing the one-step-ahead squared error, so no model file
    /// has to be trained, stored or kept in sync with the data.
    /// </summary>
    public static class HoltForecaster
    {
        public const int MinimumPoints = 3;
        private const double Z95 = 1.96;

        public record Result(double Alpha, double Beta, IReadOnlyList<(double Value, double Lower, double Upper)> Forecast);

        public static Result Forecast(IReadOnlyList<double> series, int horizon)
        {
            if (series.Count < MinimumPoints)
            {
                throw new ArgumentException($"At least {MinimumPoints} points are needed", nameof(series));
            }
            if (horizon < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(horizon));
            }

            double bestSse = double.MaxValue, bestAlpha = 0.5, bestBeta = 0.1;
            for (int a = 1; a <= 19; a++)
            {
                for (int b = 1; b <= 19; b++)
                {
                    double alpha = a * 0.05, beta = b * 0.05;
                    double sse = Fit(series, alpha, beta).Sse;
                    if (sse < bestSse)
                    {
                        (bestSse, bestAlpha, bestBeta) = (sse, alpha, beta);
                    }
                }
            }

            var (level, trend, finalSse) = Fit(series, bestAlpha, bestBeta);
            // One-step errors start at the second point; the interval widens with the horizon.
            double sigma = Math.Sqrt(finalSse / Math.Max(1, series.Count - 1));

            var forecast = Enumerable.Range(1, horizon).Select(h =>
            {
                double value = Math.Max(0, level + h * trend);
                double margin = Z95 * sigma * Math.Sqrt(h);
                return (value, Math.Max(0, value - margin), value + margin);
            }).ToList();

            return new Result(bestAlpha, bestBeta, forecast);
        }

        private static (double Level, double Trend, double Sse) Fit(IReadOnlyList<double> y, double alpha, double beta)
        {
            double level = y[0];
            double trend = y[1] - y[0];
            double sse = 0;

            for (int t = 1; t < y.Count; t++)
            {
                double predicted = level + trend;
                double error = y[t] - predicted;
                sse += error * error;

                double previousLevel = level;
                level = alpha * y[t] + (1 - alpha) * (level + trend);
                trend = beta * (level - previousLevel) + (1 - beta) * trend;
            }

            return (level, trend, sse);
        }
    }
}

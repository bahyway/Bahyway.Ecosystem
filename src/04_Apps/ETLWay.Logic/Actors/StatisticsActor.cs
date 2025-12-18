using Akka.Actor;
using BahyWay.SharedKernel.Application.Events;
using BahyWay.SharedKernel.Interfaces;
using System.Collections.Generic;

namespace ETLWay.Logic.Actors
{
    public class StatisticsActor : ReceiveActor
    {
        private readonly IMessageBus _bus;

        // In-Memory Database for Scores:
        // Key = SourceSystem (e.g. "Najaf"), Value = (Success, Fail)
        private readonly Dictionary<string, (int success, int fail)> _stats = new();

        public StatisticsActor(IMessageBus bus)
        {
            _bus = bus;

            // Listen for Particle Events
            ReceiveAsync<FlowParticleEvent>(async evt =>
            {
                // We only care if the job is FINISHED (Success or Error)
                if (evt.Status == "Moving" || string.IsNullOrEmpty(evt.SourceSystem))
                    return;

                // 1. Update Counts
                UpdateStats(evt.SourceSystem, evt.Status);

                // 2. Calculate New Score
                var (score, status) = CalculateScore(evt.SourceSystem);

                // 3. Broadcast to KGEditor Dashboard
                var scoreEvent = new ScoreUpdateEvent
                {
                    SourceSystem = evt.SourceSystem,
                    QualityScore = score,
                    SuccessCount = _stats[evt.SourceSystem].success,
                    FailureCount = _stats[evt.SourceSystem].fail,
                    OverallStatus = status
                };

                // Send to Redis Pub/Sub (Topic: "Dashboard.Scores")
                await _bus.PublishParticleAsync("Dashboard.Scores", scoreEvent);
            });
        }

        private void UpdateStats(string source, string status)
        {
            if (!_stats.ContainsKey(source))
            {
                _stats[source] = (0, 0);
            }

            var current = _stats[source];

            if (status == "Success")
                _stats[source] = (current.success + 1, current.fail);
            else if (status == "Error")
                _stats[source] = (current.success, current.fail + 1);
        }

        private (double score, string status) CalculateScore(string source)
        {
            var stats = _stats[source];
            int total = stats.success + stats.fail;
            if (total == 0) return (100.0, "Excellent");

            double score = ((double)stats.success / total) * 100.0;

            string status = score switch
            {
                >= 90 => "Excellent", // Green
                >= 70 => "Warning",   // Yellow
                _ => "Critical"       // Red
            };

            return (score, status);
        }
    }
}
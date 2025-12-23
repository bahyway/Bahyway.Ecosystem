using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using BahyWay.SharedKernel.Application.Events;
using BahyWay.SharedKernel.Infrastructure.Messaging;
using BahyWay.SharedKernel.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bahyway.KGEditor.UI.ViewModels.Monitoring
{
    public partial class PipelineMonitorViewModel : ObservableObject
    {
        private readonly IMessageBus _bus;

        public ObservableCollection<StationNode> Stations { get; } = new();
        public ObservableCollection<ParticleModel> Particles { get; } = new();
        public ObservableCollection<JobModel> Jobs { get; } = new();

        // Popup State - EXPLICITLY FALSE
        [ObservableProperty] private bool _isDetailsVisible = false;
        [ObservableProperty] private JobModel _selectedJob;

        public PipelineMonitorViewModel()
        {
            SetupMap();
            _bus = new RedisMessageBus("localhost:6379");
            InitializeAsync();
        }

        private void SetupMap()
        {
            Stations.Add(new StationNode("LandingZone", 50, 150, "📂"));
            Stations.Add(new StationNode("Extraction", 250, 150, "⚙️"));
            Stations.Add(new StationNode("Validation", 450, 150, "🛡️"));
            Stations.Add(new StationNode("StagingDB", 650, 150, "🛢️"));
            Stations.Add(new StationNode("ErrorQueue", 450, 300, "☠️"));
        }

        private async void InitializeAsync()
        {
            try
            {
                await _bus.SubscribeAsync<FlowParticleEvent>("Flow.Particles", OnEventReceived);
                await _bus.SubscribeAsync<ScoreUpdateEvent>("Dashboard.Scores", OnScoreReceived);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private void OnEventReceived(FlowParticleEvent evt)
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateParticle(evt);
                UpdateJobGrid(evt);
            });
        }

        //private void OnScoreReceived(ScoreUpdateEvent evt)
        //{
        //    Dispatcher.UIThread.Post(() =>
        //    {
        //        var job = Jobs.FirstOrDefault(j => j.Id == evt.ExecutionId);

        //        // If job row doesn't exist yet, create it
        //        if (job == null)
        //        {
        //            job = new JobModel
        //            {
        //                Id = evt.ExecutionId,
        //                FileName = "Unknown File",
        //                Source = evt.SourceSystem,
        //                Type = "Unknown",
        //                StartTime = DateTime.Now.ToString("HH:mm:ss")
        //            };
        //            Jobs.Insert(0, job);
        //        }

        //        // Update Status & Score
        //        job.Score = evt.QualityScore;
        //        job.Status = $"{evt.OverallStatus} ({evt.QualityScore}%)";
        //        if (evt.QualityScore < 50) job.StatusColor = "#FF0000";

        //        // --- THE FIX ---
        //        // Force update the text. If empty, show specific debug message.
        //        if (string.IsNullOrWhiteSpace(evt.Message))
        //        {
        //            job.ErrorDetails = $"[System] No error message provided. (Score: {evt.QualityScore}%)";
        //        }
        //        else
        //        {
        //            job.ErrorDetails = evt.Message;
        //        }
        //        // ----------------
        //    });
        //}
        private void OnScoreReceived(ScoreUpdateEvent evt)
        {
            Dispatcher.UIThread.Post(() =>
            {
                // DEBUG: Print to Visual Studio "Output" window
                System.Diagnostics.Debug.WriteLine($"[UI MATCHING] Incoming ID: {evt.ExecutionId} | Message: {evt.Message}");
                System.Diagnostics.Debug.WriteLine($"[UI MATCHING] Existing Jobs: {string.Join(", ", Jobs.Select(j => j.Id))}");

                // Try to find the job
                var job = Jobs.FirstOrDefault(j => j.Id == evt.ExecutionId);

                if (job == null)
                {
                    System.Diagnostics.Debug.WriteLine("[UI MATCHING] ❌ NO MATCH FOUND! Creating new Orphan row.");
                    job = new JobModel
                    {
                        Id = evt.ExecutionId,
                        FileName = "Orphaned Job", // This will verify if IDs are mismatched
                        Source = evt.SourceSystem,
                        Type = "Error",
                        StartTime = DateTime.Now.ToString("HH:mm:ss")
                    };
                    Jobs.Insert(0, job);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[UI MATCHING] ✅ MATCH FOUND!");
                }

                // Update Data
                job.Score = evt.QualityScore;
                job.Status = $"{evt.OverallStatus} ({evt.QualityScore}%)";
                job.ErrorDetails = evt.Message; // Assign the message

                if (evt.QualityScore < 50) job.StatusColor = "#FF0000";
            });
        }
        private void UpdateParticle(FlowParticleEvent evt)
        {
            var particle = Particles.FirstOrDefault(p => p.Id == evt.ExecutionId);
            var targetNode = Stations.FirstOrDefault(s => s.Name == evt.ToNode);

            if (particle == null)
            {
                var startNode = Stations.FirstOrDefault(s => s.Name == evt.FromNode) ?? Stations[0];
                particle = new ParticleModel
                {
                    Id = evt.ExecutionId,
                    X = startNode.X,
                    Y = startNode.Y,
                    Label = evt.FileName,
                    Color = evt.ColorHex
                };
                Particles.Add(particle);
            }

            if (targetNode != null)
            {
                particle.X = targetNode.X;
                particle.Y = targetNode.Y;
                particle.Status = evt.Status;
                particle.Color = evt.ColorHex;
            }
        }

        private void UpdateJobGrid(FlowParticleEvent evt)
        {
            var job = Jobs.FirstOrDefault(j => j.Id == evt.ExecutionId);
            if (job == null)
            {
                job = new JobModel
                {
                    Id = evt.ExecutionId,
                    FileName = evt.FileName,
                    Source = evt.SourceSystem ?? "Unknown",
                    Type = evt.FileType ?? "File"
                };
                Jobs.Insert(0, job);
            }

            switch (evt.ToNode)
            {
                case "LandingZone": job.LandingStatus = evt.Status; break;
                case "Extraction": job.LandingStatus = "Success"; job.ExtractionStatus = evt.Status; break;
                case "Validation": job.ExtractionStatus = "Success"; job.SchemaCheckStatus = evt.Status; break;
                case "StagingDB": job.SchemaCheckStatus = "Success"; job.LoadStatus = evt.Status; break;
                case "ErrorQueue":
                    if (evt.FromNode == "Extraction") job.ExtractionStatus = "Error";
                    else if (evt.FromNode == "Validation") job.SchemaCheckStatus = "Error";
                    break;
            }
            job.RefreshColors();
        }

        [RelayCommand]
        public void ViewDetails(JobModel job)
        {
            SelectedJob = job;
            IsDetailsVisible = true;
        }

        [RelayCommand]
        public void CloseDetails()
        {
            IsDetailsVisible = false;
        }
    }

    public class StationNode
    {
        public string Name { get; }
        public double X { get; }
        public double Y { get; }
        public string Icon { get; }
        public StationNode(string name, double x, double y, string icon) { Name = name; X = x; Y = y; Icon = icon; }
    }

    public partial class ParticleModel : ObservableObject
    {
        public string Id { get; set; }
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private string _color;
        [ObservableProperty] private string _label;
        [ObservableProperty] private string _status;
    }

    public partial class JobModel : ObservableObject
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }

        // THIS WAS MISSING -> Fixes CS0117
        public string StartTime { get; set; }

        [ObservableProperty] private double _score;
        [ObservableProperty] private string _status;
        [ObservableProperty] private string _statusColor = "#FFFF00";

        // This holds the error message text
        [ObservableProperty] private string _errorDetails = "Waiting for details...";

        [ObservableProperty] private string _landingStatus = "Pending";
        [ObservableProperty] private string _extractionStatus = "Pending";
        [ObservableProperty] private string _schemaCheckStatus = "Pending";
        [ObservableProperty] private string _loadStatus = "Pending";

        public string LandingColor => GetColor(_landingStatus);
        public string ExtractionColor => GetColor(_extractionStatus);
        public string SchemaColor => GetColor(_schemaCheckStatus);
        public string LoadColor => GetColor(_loadStatus);

        public void RefreshColors()
        {
            OnPropertyChanged(nameof(LandingColor));
            OnPropertyChanged(nameof(ExtractionColor));
            OnPropertyChanged(nameof(SchemaColor));
            OnPropertyChanged(nameof(LoadColor));
        }

        private string GetColor(string status) => status switch
        {
            "Success" => "#00FF00",
            "Error" => "#FF0000",
            "Moving" => "#FFFF00",
            "Processing" => "#FFFF00",
            "Warning" => "#FFA500",
            _ => "#444444"
        };
    }
}
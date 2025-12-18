using Avalonia;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bahyway.KGEditor.UI.Services
{
    public class Particle
    {
        public string Id { get; set; }
        public Point CurrentPosition { get; set; }
        public Point TargetPosition { get; set; }
        public double Progress { get; set; } // 0.0 to 1.0
        public string Color { get; set; }
    }

    public class ParticleEngine
    {
        private List<Particle> _particles = new List<Particle>();
        private DispatcherTimer _timer;

        public event Action OnFrameUpdate; // Tell UI to redraw

        public ParticleEngine()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
            _timer.Tick += (s, e) => UpdatePhysics();
            _timer.Start();
        }

        public void SpawnParticle(Point start, Point end, string color)
        {
            _particles.Add(new Particle
            {
                Id = Guid.NewGuid().ToString(),
                CurrentPosition = start,
                TargetPosition = end,
                Progress = 0,
                Color = color
            });
        }

        private void UpdatePhysics()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Progress += 0.02; // Speed

                // Linear Interpolation (Lerp)
                double x = p.CurrentPosition.X + (p.TargetPosition.X - p.CurrentPosition.X) * p.Progress;
                double y = p.CurrentPosition.Y + (p.TargetPosition.Y - p.CurrentPosition.Y) * p.Progress;

                // Update rendering position (You would bind this to the UI)
                // ...

                if (p.Progress >= 1.0) _particles.RemoveAt(i); // Arrived
            }
            OnFrameUpdate?.Invoke();
        }
    }
}
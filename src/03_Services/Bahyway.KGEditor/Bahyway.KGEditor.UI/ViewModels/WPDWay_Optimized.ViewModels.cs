using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using System;

namespace WPDWay_Optimized.UI.ViewModels
{
    public partial class WPDWay_OptimizedNodeViewModel : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private bool _isVisible = true;
        public virtual string Color => "#808080";
        public virtual string Shape => "CIRCLE";
    }

    public partial class Hub_pipe_segmentViewModel : WPDWay_OptimizedNodeViewModel
    {
        public override string Color => "#3498db";
        public override string Shape => "RECTANGLE";
        [ObservableProperty] private long _color_id;
        [ObservableProperty] private Guid _segment_uuid;
        [ObservableProperty] private string _material;
        [ObservableProperty] private int _diameter_mm;
    }

}

using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using System;

namespace Najaf_Cemetery_Master.UI.ViewModels
{
    public partial class Najaf_Cemetery_MasterNodeViewModel : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private bool _isVisible = true;
        public virtual string Color => "#808080";
        public virtual string Shape => "CIRCLE";
    }

    public partial class Hub_deceased_recordViewModel : Najaf_Cemetery_MasterNodeViewModel
    {
        [ObservableProperty] private long _tribal_color_id;
        [ObservableProperty] private string _civil_id;
    }

}

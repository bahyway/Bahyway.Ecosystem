using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;

namespace Bahyway.KGEditor.UI.ViewModels
{
    public partial class NodeViewModel : ObservableObject
    {
        [ObservableProperty] private string _id;
        [ObservableProperty] private string _name;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private IBrush _color;

        public NodeViewModel(string id, string name, double x, double y, string colorHex)
        {
            _id = id; _name = name; _x = x; _y = y;
            _color = Brush.Parse(colorHex);
        }
    }
}

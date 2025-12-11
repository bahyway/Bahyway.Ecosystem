using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;

namespace Bahyway.KGEditor.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<NodeViewModel> Nodes { get; } = new();

        [ObservableProperty] private string _consoleLog = "System Ready...";

        // --- SELECTION STATE ---
        [ObservableProperty] private NodeViewModel? _selectedNode;

        public MainWindowViewModel()
        {
            // Initial Data
            Nodes.Add(new NodeViewModel("1", "Sensor 03", 400, 300, "#FF0000"));
            Nodes.Add(new NodeViewModel("2", "Valve A", 200, 150, "#00FF00"));
        }

        // Called when user clicks a node on the Canvas
        public void SelectNode(string id)
        {
            SelectedNode = Nodes.FirstOrDefault(n => n.Id == id);
            if (SelectedNode != null)
            {
                ConsoleLog = $"[SELECTED] {SelectedNode.Name} ({SelectedNode.Id})";
            }
        }
    }
}
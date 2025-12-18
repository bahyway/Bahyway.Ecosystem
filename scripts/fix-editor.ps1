# Define Paths
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$viewPath = "$uiPath/Views"

# 1. Fix EditorView.axaml (The UI Layout)
$xamlContent = @'
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="Bahyway.KGEditor.UI.Views.EditorView">
  <Grid Background="#1e1e1e">
      <TextBlock Text="Editor View" HorizontalAlignment="Center" VerticalAlignment="Center" Foreground="Gray"/>
  </Grid>
</UserControl>
'@
Set-Content -Path "$viewPath/EditorView.axaml" -Value $xamlContent

# 2. Fix EditorView.axaml.cs (The Code Behind)
$csContent = @'
using Avalonia.Controls;

namespace Bahyway.KGEditor.UI.Views
{
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
        }
    }
}
'@
Set-Content -Path "$viewPath/EditorView.axaml.cs" -Value $csContent

Write-Host "✅ Fixed EditorView.axaml. Ready to build!" -ForegroundColor Green
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Bahyway.ShoWay.UI.Views.Monitoring
{
    public partial class PipelineMonitorView : UserControl
    {
        private bool _isDragging = false;
        private Point _startPoint;
        private TranslateTransform? _transform;

        public PipelineMonitorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnPopupPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                _isDragging = true;
                _startPoint = e.GetPosition(this);
                if (border.RenderTransform is TranslateTransform tt) _transform = tt;
            }
        }

        private void OnPopupPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _transform != null)
            {
                var currentPoint = e.GetPosition(this);
                var delta = currentPoint - _startPoint;
                _transform.X += delta.X;
                _transform.Y += delta.Y;
                _startPoint = currentPoint;
            }
        }

        private void OnPopupPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
        }
    }
}
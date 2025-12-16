using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SnakeMarsTheme.ViewModels;

namespace SnakeMarsTheme.Views;

public partial class WizardView : UserControl
{
    private int _currentStep = 1;
    private WizardViewModel ViewModel => (WizardViewModel)DataContext;
    
    public WizardView()
    {
        InitializeComponent();
        UpdateStepVisibility();
    }
    
    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep > 1)
        {
            _currentStep--;
            ViewModel.CurrentStep = _currentStep;
            UpdateStepVisibility();
        }
    }
    
    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        // Validate current step
        if (_currentStep == 1 && string.IsNullOrWhiteSpace(ViewModel.ThemeName))
        {
            MessageBox.Show("Ingresa un nombre para el tema.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        if (_currentStep < 4)
        {
            _currentStep++;
            ViewModel.CurrentStep = _currentStep;
            UpdateStepVisibility();
            
            if (_currentStep == 4)
            {
                ViewModel.GoNextCommand.Execute(null); // Update summary
            }
        }
        else
        {
            // Save theme using ViewModel
            ViewModel.GoNextCommand.Execute(null);
        }
    }
    
    private void UpdateStepVisibility()
    {
        // Hide all panels
        Step1Panel.Visibility = Visibility.Collapsed;
        Step2Panel.Visibility = Visibility.Collapsed;
        Step3Panel.Visibility = Visibility.Collapsed;
        Step4Panel.Visibility = Visibility.Collapsed;
        
        // Show current panel
        switch (_currentStep)
        {
            case 1: Step1Panel.Visibility = Visibility.Visible; break;
            case 2: Step2Panel.Visibility = Visibility.Visible; break;
            case 3: Step3Panel.Visibility = Visibility.Visible; break;
            case 4: Step4Panel.Visibility = Visibility.Visible; break;
        }
        
        // Colors for step indicators
        var cyan = (SolidColorBrush)FindResource("AccentCyanBrush");
        var gray = (SolidColorBrush)FindResource("TextSecondaryBrush");
        var transparent = new SolidColorBrush(Colors.Transparent);
        var black = new SolidColorBrush(Colors.Black);
        
        // Update step borders and labels
        Step1Border.Background = _currentStep == 1 ? cyan : transparent;
        Step1Label.Foreground = _currentStep == 1 ? black : (_currentStep > 1 ? cyan : gray);
        Step1Label.FontWeight = _currentStep == 1 ? FontWeights.Bold : FontWeights.Normal;
        
        Step2Border.Background = _currentStep == 2 ? cyan : transparent;
        Step2Label.Foreground = _currentStep == 2 ? black : (_currentStep > 2 ? cyan : gray);
        Step2Label.FontWeight = _currentStep == 2 ? FontWeights.Bold : FontWeights.Normal;
        
        Step3Border.Background = _currentStep == 3 ? cyan : transparent;
        Step3Label.Foreground = _currentStep == 3 ? black : (_currentStep > 3 ? cyan : gray);
        Step3Label.FontWeight = _currentStep == 3 ? FontWeights.Bold : FontWeights.Normal;
        
        Step4Border.Background = _currentStep == 4 ? cyan : transparent;
        Step4Label.Foreground = _currentStep == 4 ? black : gray;
        Step4Label.FontWeight = _currentStep == 4 ? FontWeights.Bold : FontWeights.Normal;
        
        // Update buttons
        BtnBack.Visibility = _currentStep > 1 ? Visibility.Visible : Visibility.Hidden;
        BtnNext.Content = _currentStep == 4 ? "✓ Crear Tema" : "Siguiente ▶";
    }
    
    // =============================================================
    // DRAG & DROP (from ThemeEditorView)
    // =============================================================
    
    private bool _isDragging;
    private bool _hasMoved;
    private Point _dragStart;
    private object? _draggedWidget;
    private Canvas? _activeCanvas;  // Dynamic canvas for current drag
    
    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Deselect when clicking empty canvas
        if (e.OriginalSource == WizardCanvas)
        {
            // No selection in Wizard, just release
        }
    }
    
    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging && _hasMoved)
        {
            // Refresh preview after drag
            ViewModel.UpdateCanvasPreview();
        }
        _isDragging = false;
        _hasMoved = false;
        _draggedWidget = null;
        _activeCanvas?.ReleaseMouseCapture();
        _activeCanvas = null;
    }
    
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _draggedWidget == null || _activeCanvas == null) return;
        
        var pos = e.GetPosition(_activeCanvas);
        var deltaX = (int)(pos.X - _dragStart.X);
        var deltaY = (int)(pos.Y - _dragStart.Y);
        
        if (Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1)
        {
            _hasMoved = true;
        }
        
        // Update widget position using reflection (supports any widget type)
        var widgetType = _draggedWidget.GetType();
        var xProp = widgetType.GetProperty("X");
        var yProp = widgetType.GetProperty("Y");
        
        if (xProp != null && yProp != null)
        {
            var currentX = (int)(xProp.GetValue(_draggedWidget) ?? 0);
            var currentY = (int)(yProp.GetValue(_draggedWidget) ?? 0);
            
            var newX = Math.Max(0, Math.Min(currentX + deltaX, ViewModel.CanvasWidth));
            var newY = Math.Max(0, Math.Min(currentY + deltaY, ViewModel.CanvasHeight));
            
            xProp.SetValue(_draggedWidget, newX);
            yProp.SetValue(_draggedWidget, newY);
        }
        
        _dragStart = pos;
    }
    
    private void Widget_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext != null)
        {
            // Select this widget in property panel
            var widgetItem = border.DataContext as WidgetItem;
            if (widgetItem != null && ViewModel != null)
            {
                ViewModel.SelectedThemeWidget = widgetItem;
            }
            
            // Find parent Canvas dynamically (works for both layouts)
            _activeCanvas = FindParentCanvas(border);
            if (_activeCanvas == null) return;
            
            _isDragging = true;
            _hasMoved = false;
            _draggedWidget = border.DataContext;
            _dragStart = e.GetPosition(_activeCanvas);
            _activeCanvas.CaptureMouse();
            
            e.Handled = true;
        }
    }
    
    // Helper to find parent Canvas in visual tree
    private Canvas? FindParentCanvas(DependencyObject child)
    {
        var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is Canvas canvas)
                return canvas;
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
    
    private void Video_MediaEnded(object sender, RoutedEventArgs e)
    {
        if (sender is MediaElement media)
        {
            media.Position = TimeSpan.Zero;
            media.Play();
        }
    }
    
    // =============================================================
    // ZOOM FUNCTIONALITY  
    // =============================================================
    
    private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Zoom con scroll del ratón
        var delta = e.Delta > 0 ? 0.1 : -0.1;
        var newScale = ZoomTransform.ScaleX + delta;
        
        // Limitar zoom entre 0.5x y 3.0x
        newScale = Math.Max(0.5, Math.Min(3.0, newScale));
        
        // Apply to both transforms (only one will be visible at a time)
        ZoomTransform.ScaleX = newScale;
        ZoomTransform.ScaleY = newScale;
        
        if (ZoomTransformHorizontal != null)
        {
            ZoomTransformHorizontal.ScaleX = newScale;
            ZoomTransformHorizontal.ScaleY = newScale;
        }
        
        e.Handled = true;
    }
    
    // SelectedWidgets_SelectionChanged REMOVED - no longer needed
    // XAML binds directly to SelectedThemeWidget, PropertyChanged auto-updates canvas
}

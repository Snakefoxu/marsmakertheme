using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using SnakeMarsTheme.ViewModels;

namespace SnakeMarsTheme.Views;

/// <summary>
/// Theme Editor View - Clean reconstruction following WizardView patterns
/// </summary>
public partial class ThemeEditorView : UserControl
{
    // ═══════════════════════════════════════════════════════════════
    // DRAG & DROP STATE
    // ═══════════════════════════════════════════════════════════════
    private bool _isDragging;
    private bool _hasMoved;
    private Point _dragStart;
    private PlacedWidgetItem? _draggedWidget;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════
    public ThemeEditorView()
    {
        InitializeComponent();
        
        // Keyboard shortcuts
        Loaded += (s, e) =>
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.KeyDown += Window_KeyDown;
            }
            Focusable = true;
            Focus();
        };
        
        // Video control subscription
        DataContextChanged += OnDataContextChanged;
    }

    private ThemeEditorViewModel ViewModel => (ThemeEditorViewModel)DataContext;

    // ═══════════════════════════════════════════════════════════════
    // DRAG & DROP HANDLERS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Widget click - starts drag operation
    /// </summary>
    private void Widget_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.DataContext is not PlacedWidgetItem widget) return;
        
        // Multi-select with Ctrl
        bool addToSelection = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        ViewModel.SelectWidget(widget, addToSelection);
        
        // Save for undo
        ViewModel.SaveStateForUndo();
        
        // Start drag - ALWAYS use ThemeCanvas (parent with MouseMove/MouseUp handlers)
        _isDragging = true;
        _hasMoved = false;
        _draggedWidget = widget;
        _dragStart = e.GetPosition(ThemeCanvas);
        ThemeCanvas.CaptureMouse();
        
        e.Handled = true;
    }
    
    /// <summary>
    /// Mouse move - updates widget position during drag
    /// </summary>
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _draggedWidget == null) return;
        
        var pos = e.GetPosition(ThemeCanvas);
        var deltaX = (int)(pos.X - _dragStart.X);
        var deltaY = (int)(pos.Y - _dragStart.Y);
        
        // Threshold to start actual movement
        if (Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1)
        {
            _hasMoved = true;
            
            // Move selected widgets - NO SNAP during drag for smooth movement
            if (ViewModel.SelectedWidgets.Count > 1 && _draggedWidget.IsSelected)
            {
                foreach (var w in ViewModel.SelectedWidgets)
                {
                    w.X = Math.Max(0, w.X + deltaX);
                    w.Y = Math.Max(0, w.Y + deltaY);
                }
            }
            else
            {
                _draggedWidget.X = Math.Max(0, _draggedWidget.X + deltaX);
                _draggedWidget.Y = Math.Max(0, _draggedWidget.Y + deltaY);
            }
            
            _dragStart = pos;
        }
    }
    
    /// <summary>
    /// Mouse up - ends drag operation and applies snap if enabled
    /// </summary>
    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging && _hasMoved)
        {
            // Apply snap to grid at END of drag (not during)
            if (ViewModel.SnapToGrid)
            {
                foreach (var w in ViewModel.SelectedWidgets)
                {
                    w.X = ViewModel.SnapToGridValue(w.X);
                    w.Y = ViewModel.SnapToGridValue(w.Y);
                }
            }
            
            ViewModel.UpdatePreview();
        }
        
        _isDragging = false;
        _hasMoved = false;
        _draggedWidget = null;
        ThemeCanvas.ReleaseMouseCapture();
    }
    
    /// <summary>
    /// Canvas click - deselects when clicking empty area
    /// </summary>
    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource == ThemeCanvas)
        {
            ViewModel.DeselectAll();
        }
    }
    

    // ═══════════════════════════════════════════════════════════════
    // KEYBOARD SHORTCUTS
    // ═══════════════════════════════════════════════════════════════
    
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (!IsVisible) return;
        
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            switch (e.Key)
            {
                case Key.C:
                    ViewModel.CopyWidgetCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.V:
                    ViewModel.PasteWidgetCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.D:
                    ViewModel.DuplicateWidgetCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Z:
                    ViewModel.UndoCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Y:
                    ViewModel.RedoCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.S:
                    ViewModel.QuickSaveProjectCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.O:
                    ViewModel.LoadProjectCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.N:
                    ViewModel.NewProjectCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.A:
                    ViewModel.SelectAllCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
        else if (e.Key == Key.Delete)
        {
            ViewModel.DeleteSelectedCommand.Execute(null);
            e.Handled = true;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // WIDGET LIBRARY - DOUBLE CLICK TO ADD
    // ═══════════════════════════════════════════════════════════════
    
    private void Widget_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        ViewModel.AddWidgetCommand.Execute(null);
    }

    // ═══════════════════════════════════════════════════════════════
    // VIDEO BACKGROUND SUPPORT
    // ═══════════════════════════════════════════════════════════════
    
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ThemeEditorViewModel oldVm)
            oldVm.PropertyChanged -= ViewModel_PropertyChanged;
        if (e.NewValue is ThemeEditorViewModel newVm)
            newVm.PropertyChanged += ViewModel_PropertyChanged;
    }
    
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeEditorViewModel.VideoSource))
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (ViewModel.VideoSource != null && ViewModel.IsVideoBackground)
                    {
                        BackgroundVideo.Source = ViewModel.VideoSource;
                        BackgroundVideo.Play();
                    }
                    else
                    {
                        BackgroundVideo.Stop();
                        BackgroundVideo.Source = null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Video error: {ex.Message}");
                }
            });
        }
    }
    
    private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
    {
        if (sender is MediaElement media)
        {
            media.Position = TimeSpan.Zero;
            media.Play();
        }
    }
    
    private void BackgroundVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Video failed: {e.ErrorException?.Message}");
        MessageBox.Show(
            $"No se pudo cargar el video:\n{e.ErrorException?.Message}",
            "Error de Video",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        
        if (DataContext is ThemeEditorViewModel vm)
        {
            vm.IsVideoBackground = false;
            vm.VideoSource = null;
        }
    }
    
    private void BackgroundVideo_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is MediaElement media && ViewModel.IsVideoBackground)
        {
            try { media.Play(); }
            catch { /* ignore */ }
        }
    }
}

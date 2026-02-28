using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using EZSee.ViewModels;

namespace EZSee
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel = null!;
        private bool _isDragging;
        private Point _lastMousePosition;
        private WindowStyle _savedWindowStyle;
        private WindowState _savedWindowState;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = _viewModel.GetSettings();
            if (settings.StartMaximized)
                WindowState = WindowState.Maximized;

            ApplyTheme(_viewModel.IsDarkMode);
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsFullScreen))
            {
                if (_viewModel.IsFullScreen)
                {
                    _savedWindowStyle = WindowStyle;
                    _savedWindowState = WindowState;
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                    ToolbarPanel.Visibility = Visibility.Collapsed;
                    StatusBarPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    WindowStyle = _savedWindowStyle;
                    WindowState = _savedWindowState;
                    ToolbarPanel.Visibility = Visibility.Visible;
                    StatusBarPanel.Visibility = Visibility.Visible;
                }
            }
            else if (e.PropertyName == nameof(MainViewModel.IsDarkMode))
            {
                ApplyTheme(_viewModel.IsDarkMode);
            }
        }

        private void ApplyTheme(bool isDark)
        {
            var themeUri = new Uri(isDark
                ? "Themes/DarkTheme.xaml"
                : "Themes/LightTheme.xaml", UriKind.Relative);

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary { Source = themeUri });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    _viewModel.ToggleViewMode();
                    e.Handled = true;
                    break;
                case Key.F:
                    _viewModel.ToggleFullScreen();
                    e.Handled = true;
                    break;
                case Key.Space:
                    _viewModel.ToggleSlideshow();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    if (_viewModel.IsFullScreen)
                    {
                        _viewModel.IsFullScreen = false;
                        e.Handled = true;
                    }
                    else if (_viewModel.IsSlideshowPlaying)
                    {
                        _viewModel.ToggleSlideshow();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_viewModel.IsFolderView && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _viewModel.ThumbnailSize += e.Delta > 0 ? 20 : -20;
                e.Handled = true;
                return;
            }

            if (_viewModel.IsSingleImageView)
            {
                double factor = e.Delta > 0 ? 1.15 : 1.0 / 1.15;
                _viewModel.ZoomBy(factor);
                e.Handled = true;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    var path = files[0];
                    if (Directory.Exists(path))
                        _ = _viewModel.LoadFolderAsync(path);
                    else if (Services.ImageService.IsSupported(path))
                        _ = _viewModel.LoadFileAsync(path);
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void ImageArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                    _viewModel.FitToWindow();
                else
                    _viewModel.ToggleFullScreen();
                e.Handled = true;
                return;
            }

            if (e.ClickCount == 1 && _viewModel.ZoomLevel > 1.0)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(this);
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void ImageArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void ImageArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(this);
                var delta = currentPos - _lastMousePosition;

                double speed = Keyboard.Modifiers == ModifierKeys.Shift ? 0.3 : 1.0;
                _viewModel.PanX += delta.X * speed;
                _viewModel.PanY += delta.Y * speed;

                _lastMousePosition = currentPos;
            }
        }

        private void Thumbnail_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ThumbnailItem item)
            {
                _viewModel.OpenThumbnail(item.FilePath);
            }
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_viewModel.CurrentFilePath))
                Clipboard.SetText(_viewModel.CurrentFilePath);
        }

        private void RevealInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_viewModel.CurrentFilePath) && File.Exists(_viewModel.CurrentFilePath))
            {
                Process.Start("explorer.exe", $"/select,\"{_viewModel.CurrentFilePath}\"");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.SaveSettings(ActualWidth, ActualHeight, WindowState == WindowState.Maximized);
            _viewModel.Dispose();
        }
    }
}
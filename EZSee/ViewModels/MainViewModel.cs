using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EZSee.Models;
using EZSee.Services;

namespace EZSee.ViewModels
{
    public class ThumbnailItem : ViewModelBase
    {
        private BitmapImage? _thumbnail;
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);

        public BitmapImage? Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }
    }

    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly ThumbnailCacheService _thumbnailCache;
        private readonly DispatcherTimer _slideshowTimer;
        private CancellationTokenSource? _thumbnailCts;

        private ViewMode _currentViewMode = ViewMode.SingleImage;
        private BitmapImage? _currentImage;
        private ImageInfo? _currentImageInfo;
        private string? _currentFilePath;
        private string? _currentFolderPath;
        private int _currentIndex = -1;
        private double _zoomLevel = 1.0;
        private double _panX;
        private double _panY;
        private bool _isFullScreen;
        private bool _showInfoOverlay;
        private bool _isSlideshowPlaying;
        private int _thumbnailSize = 150;
        private string _statusText = "Ready";
        private bool _isDarkMode = true;
        private double _slideshowInterval = 3.0;
        private ViewerSettings _settings;

        private System.Collections.Generic.List<string> _imageFiles = new();
        public ObservableCollection<ThumbnailItem> Thumbnails { get; } = new();

        public MainViewModel()
        {
            _settings = ViewerSettings.Load();
            _isDarkMode = _settings.DarkMode;
            _thumbnailSize = _settings.ThumbnailSize;
            _slideshowInterval = _settings.SlideshowIntervalSeconds;

            _thumbnailCache = new ThumbnailCacheService(_settings.DecodeThreadCount);

            _slideshowTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_slideshowInterval)
            };
            _slideshowTimer.Tick += (_, _) => NextImage();

            OpenCommand = new RelayCommand(OpenFileOrFolder);
            NextImageCommand = new RelayCommand(NextImage, () => CanNavigate);
            PreviousImageCommand = new RelayCommand(PreviousImage, () => CanNavigate);
            FirstImageCommand = new RelayCommand(FirstImage, () => CanNavigate);
            LastImageCommand = new RelayCommand(LastImage, () => CanNavigate);
            ToggleViewModeCommand = new RelayCommand(ToggleViewMode);
            ToggleFullScreenCommand = new RelayCommand(ToggleFullScreen);
            ToggleInfoOverlayCommand = new RelayCommand(() => ShowInfoOverlay = !ShowInfoOverlay);
            ToggleSlideshowCommand = new RelayCommand(ToggleSlideshow);
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            FitToWindowCommand = new RelayCommand(FitToWindow);
            ActualSizeCommand = new RelayCommand(ActualSize);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ExitFullScreenCommand = new RelayCommand(ExitFullScreen);
            OpenThumbnailCommand = new RelayCommand(OpenThumbnail);

            if (!string.IsNullOrEmpty(_settings.LastOpenedPath) && File.Exists(_settings.LastOpenedPath))
            {
                _ = LoadFileAsync(_settings.LastOpenedPath);
            }
        }

        // Properties
        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    OnPropertyChanged(nameof(IsSingleImageView));
                    OnPropertyChanged(nameof(IsFolderView));
                }
            }
        }

        public bool IsSingleImageView => CurrentViewMode == ViewMode.SingleImage;
        public bool IsFolderView => CurrentViewMode == ViewMode.FolderView;

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public ImageInfo? CurrentImageInfo
        {
            get => _currentImageInfo;
            set => SetProperty(ref _currentImageInfo, value);
        }

        public string? CurrentFilePath
        {
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        public string? CurrentFolderPath
        {
            get => _currentFolderPath;
            set => SetProperty(ref _currentFolderPath, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, Math.Clamp(value, 0.1, 20.0));
        }

        public double PanX
        {
            get => _panX;
            set => SetProperty(ref _panX, value);
        }

        public double PanY
        {
            get => _panY;
            set => SetProperty(ref _panY, value);
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set => SetProperty(ref _isFullScreen, value);
        }

        public bool ShowInfoOverlay
        {
            get => _showInfoOverlay;
            set => SetProperty(ref _showInfoOverlay, value);
        }

        public bool IsSlideshowPlaying
        {
            get => _isSlideshowPlaying;
            set => SetProperty(ref _isSlideshowPlaying, value);
        }

        public int ThumbnailSize
        {
            get => _thumbnailSize;
            set
            {
                if (SetProperty(ref _thumbnailSize, Math.Clamp(value, 60, 400)))
                {
                    _settings.ThumbnailSize = _thumbnailSize;
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetProperty(ref _isDarkMode, value))
                {
                    _settings.DarkMode = value;
                }
            }
        }

        public double SlideshowInterval
        {
            get => _slideshowInterval;
            set
            {
                if (SetProperty(ref _slideshowInterval, Math.Clamp(value, 0.5, 60.0)))
                {
                    _slideshowTimer.Interval = TimeSpan.FromSeconds(_slideshowInterval);
                    _settings.SlideshowIntervalSeconds = _slideshowInterval;
                }
            }
        }

        public bool CanNavigate => _imageFiles.Count > 0;

        public string NavigationText =>
            _imageFiles.Count > 0 ? $"{_currentIndex + 1} / {_imageFiles.Count}" : string.Empty;

        // Commands
        public ICommand OpenCommand { get; }
        public ICommand NextImageCommand { get; }
        public ICommand PreviousImageCommand { get; }
        public ICommand FirstImageCommand { get; }
        public ICommand LastImageCommand { get; }
        public ICommand ToggleViewModeCommand { get; }
        public ICommand ToggleFullScreenCommand { get; }
        public ICommand ToggleInfoOverlayCommand { get; }
        public ICommand ToggleSlideshowCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand FitToWindowCommand { get; }
        public ICommand ActualSizeCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ExitFullScreenCommand { get; }
        public ICommand OpenThumbnailCommand { get; }

        // Methods
        public async void OpenFileOrFolder()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tiff;*.tif;*.webp;*.heif;*.heic;*.ico|All files|*.*",
                Title = "Open Image"
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadFileAsync(dialog.FileName);
            }
        }

        public async Task LoadFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var folder = Path.GetDirectoryName(filePath);
            if (folder != null && folder != _currentFolderPath)
            {
                CurrentFolderPath = folder;
                _imageFiles = ImageService.GetImagesInFolder(folder);
            }

            _currentIndex = _imageFiles.IndexOf(filePath);
            if (_currentIndex < 0 && _imageFiles.Count > 0)
                _currentIndex = 0;

            await LoadCurrentImageAsync();

            _settings.LastOpenedPath = filePath;
        }

        public async Task LoadFolderAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            CurrentFolderPath = folderPath;
            _imageFiles = ImageService.GetImagesInFolder(folderPath);

            if (_imageFiles.Count > 0)
            {
                _currentIndex = 0;
                await LoadCurrentImageAsync();
            }
            else
            {
                CurrentImage = null;
                CurrentImageInfo = null;
                CurrentFilePath = null;
                StatusText = "No images found in folder";
            }

            OnPropertyChanged(nameof(NavigationText));
        }

        private async Task LoadCurrentImageAsync()
        {
            if (_currentIndex < 0 || _currentIndex >= _imageFiles.Count)
                return;

            var filePath = _imageFiles[_currentIndex];
            CurrentFilePath = filePath;
            StatusText = $"Loading {Path.GetFileName(filePath)}...";

            CurrentImage = await Task.Run(() => ImageService.LoadImage(filePath));
            CurrentImageInfo = await Task.Run(() => ImageService.GetImageInfo(filePath));

            FitToWindow();

            StatusText = Path.GetFileName(filePath);
            OnPropertyChanged(nameof(NavigationText));

            _ = PrefetchAdjacentAsync();
        }

        private async Task PrefetchAdjacentAsync()
        {
            var prefetchCount = _settings.PrefetchCount;
            for (int offset = 1; offset <= prefetchCount; offset++)
            {
                var nextIdx = _currentIndex + offset;
                var prevIdx = _currentIndex - offset;

                if (nextIdx < _imageFiles.Count)
                    await Task.Run(() => ImageService.LoadImage(_imageFiles[nextIdx]));
                if (prevIdx >= 0)
                    await Task.Run(() => ImageService.LoadImage(_imageFiles[prevIdx]));
            }
        }

        public void NextImage()
        {
            if (_imageFiles.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _imageFiles.Count;
            _ = LoadCurrentImageAsync();
        }

        public void PreviousImage()
        {
            if (_imageFiles.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _imageFiles.Count) % _imageFiles.Count;
            _ = LoadCurrentImageAsync();
        }

        public void FirstImage()
        {
            if (_imageFiles.Count == 0) return;
            _currentIndex = 0;
            _ = LoadCurrentImageAsync();
        }

        public void LastImage()
        {
            if (_imageFiles.Count == 0) return;
            _currentIndex = _imageFiles.Count - 1;
            _ = LoadCurrentImageAsync();
        }

        public void ToggleViewMode()
        {
            if (CurrentViewMode == ViewMode.SingleImage)
            {
                CurrentViewMode = ViewMode.FolderView;
                _ = LoadThumbnailsAsync();
            }
            else
            {
                CurrentViewMode = ViewMode.SingleImage;
            }
        }

        public async Task LoadThumbnailsAsync()
        {
            _thumbnailCts?.Cancel();
            _thumbnailCts = new CancellationTokenSource();
            var token = _thumbnailCts.Token;

            Thumbnails.Clear();

            foreach (var file in _imageFiles)
            {
                if (token.IsCancellationRequested) break;
                var item = new ThumbnailItem { FilePath = file };
                Thumbnails.Add(item);
            }

            foreach (var item in Thumbnails)
            {
                if (token.IsCancellationRequested) break;
                try
                {
                    item.Thumbnail = await _thumbnailCache.GetOrLoadThumbnailAsync(
                        item.FilePath, ThumbnailSize, token);
                }
                catch (OperationCanceledException) { break; }
            }
        }

        public void OpenThumbnail(object? parameter)
        {
            if (parameter is string filePath)
            {
                var idx = _imageFiles.IndexOf(filePath);
                if (idx >= 0)
                {
                    _currentIndex = idx;
                    _ = LoadCurrentImageAsync();
                    CurrentViewMode = ViewMode.SingleImage;
                }
            }
        }

        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        private void ExitFullScreen()
        {
            if (IsFullScreen)
                IsFullScreen = false;
        }

        public void ToggleSlideshow()
        {
            IsSlideshowPlaying = !IsSlideshowPlaying;
            if (IsSlideshowPlaying)
                _slideshowTimer.Start();
            else
                _slideshowTimer.Stop();
        }

        public void ZoomIn()
        {
            ZoomLevel *= 1.2;
        }

        public void ZoomOut()
        {
            ZoomLevel /= 1.2;
        }

        public void ZoomBy(double factor)
        {
            ZoomLevel *= factor;
        }

        public void FitToWindow()
        {
            ZoomLevel = 1.0;
            PanX = 0;
            PanY = 0;
        }

        public void ActualSize()
        {
            ZoomLevel = 1.0;
            PanX = 0;
            PanY = 0;
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        public void SaveSettings(double windowWidth, double windowHeight, bool isMaximized)
        {
            _settings.WindowWidth = windowWidth;
            _settings.WindowHeight = windowHeight;
            _settings.StartMaximized = isMaximized;
            _settings.Save();
        }

        public ViewerSettings GetSettings() => _settings;

        public void Dispose()
        {
            _slideshowTimer.Stop();
            _thumbnailCts?.Cancel();
            _thumbnailCts?.Dispose();
            _thumbnailCache.Dispose();
            _settings.Save();
        }
    }
}

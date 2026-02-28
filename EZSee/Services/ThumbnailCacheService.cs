using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace EZSee.Services
{
    public class ThumbnailCacheService : IDisposable
    {
        private readonly ConcurrentDictionary<string, BitmapImage?> _cache = new();
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public ThumbnailCacheService(int maxConcurrentDecodes = 2)
        {
            _semaphore = new SemaphoreSlim(maxConcurrentDecodes);
        }

        public BitmapImage? GetCached(string filePath)
        {
            return _cache.TryGetValue(filePath, out var image) ? image : null;
        }

        public async Task<BitmapImage?> GetOrLoadThumbnailAsync(string filePath, int thumbnailSize, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(filePath, out var cached))
                return cached;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_cache.TryGetValue(filePath, out cached))
                    return cached;

                var thumbnail = await Task.Run(() => ImageService.LoadImage(filePath, thumbnailSize), cancellationToken);
                _cache.TryAdd(filePath, thumbnail);
                return thumbnail;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Dispose();
                _cache.Clear();
                _disposed = true;
            }
        }
    }
}

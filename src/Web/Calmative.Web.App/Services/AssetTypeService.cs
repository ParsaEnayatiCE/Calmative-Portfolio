using Calmative.Web.App.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Calmative.Web.App.Services
{
    public class AssetTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    public class AssetTypesResponse
    {
        public List<AssetTypeItem> BuiltInTypes { get; set; } = new();
        public List<AssetTypeItem> CustomTypes { get; set; } = new();
    }

    public interface IAssetTypeService
    {
        Task<string> GetAssetTypeDisplayName(AssetType type);
        Task RefreshAssetTypes();
        Task<Dictionary<int, string>> GetAllAssetTypeDisplayNames();
    }

    public class AssetTypeService : IAssetTypeService
    {
        private readonly IApiService _apiService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AssetTypeService> _logger;
        private readonly ConcurrentDictionary<int, string> _assetTypeNames = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private const string CacheKey = "AssetTypes";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
        private bool _isInitialized = false;

        public AssetTypeService(IApiService apiService, IMemoryCache memoryCache, ILogger<AssetTypeService> logger)
        {
            _apiService = apiService;
            _memoryCache = memoryCache;
            _logger = logger;
            
            // Start loading asset types in the background
            Task.Run(async () => await LoadAssetTypesFromApi()).ConfigureAwait(false);
        }

        public async Task<string> GetAssetTypeDisplayName(AssetType type)
        {
            int typeId = (int)type;
            
            _logger.LogDebug("Getting display name for asset type: {TypeId}", typeId);
            
            // Handle built-in types directly for better performance
            if (typeId <= 11)
            {
                return GetBuiltInAssetTypeDisplayName(type);
            }
            
            // For custom types, ensure types are loaded
            await EnsureAssetTypesLoaded();
            
            // Check if we have the type in our dictionary
            if (_assetTypeNames.TryGetValue(typeId, out string? name) && !string.IsNullOrEmpty(name))
            {
                _logger.LogDebug("Found display name for type {TypeId}: {DisplayName}", typeId, name);
                return name;
            }
            
            // If still not found, try to load directly from API
            try 
            {
                _logger.LogInformation("Asset type {TypeId} not found in cache, refreshing types", typeId);
                await RefreshAssetTypes();
                
                if (_assetTypeNames.TryGetValue(typeId, out name) && !string.IsNullOrEmpty(name))
                {
                    _logger.LogDebug("Found display name after refresh for type {TypeId}: {DisplayName}", typeId, name);
                    return name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing asset types for type {TypeId}", typeId);
            }
            
            // If still not found, return a generic name
            _logger.LogWarning("Could not find display name for asset type {TypeId}, using generic name", typeId);
            return $"سفارشی ({typeId - 1000})";
        }

        public async Task<Dictionary<int, string>> GetAllAssetTypeDisplayNames()
        {
            await EnsureAssetTypesLoaded();
            return new Dictionary<int, string>(_assetTypeNames);
        }

        public async Task RefreshAssetTypes()
        {
            await LoadAssetTypesFromApi(forceRefresh: true);
        }

        private async Task EnsureAssetTypesLoaded()
        {
            if (!_isInitialized || !_memoryCache.TryGetValue(CacheKey, out _))
            {
                await LoadAssetTypesFromApi();
            }
        }

        private async Task LoadAssetTypesFromApi(bool forceRefresh = false)
        {
            // Use semaphore to prevent multiple concurrent API calls
            await _semaphore.WaitAsync();
            try
            {
                // Double-check to avoid race conditions
                if (forceRefresh || !_isInitialized || !_memoryCache.TryGetValue(CacheKey, out _))
                {
                    _logger.LogInformation("Loading asset types from API");
                    
                    try
                    {
                        var assetTypesResponse = await _apiService.GetAsync<AssetTypesResponse>("asset/types");
                        
                        // Clear existing types if refreshing
                        if (forceRefresh)
                        {
                            _assetTypeNames.Clear();
                        }
                        
                        // Add built-in types
                        foreach (var type in assetTypesResponse.BuiltInTypes)
                        {
                            _assetTypeNames[type.Id] = type.DisplayName;
                            _logger.LogDebug("Added built-in type: {Id} - {Name}", type.Id, type.DisplayName);
                        }
                        
                        // Add custom types
                        foreach (var type in assetTypesResponse.CustomTypes.Where(t => t.IsActive))
                        {
                            // Custom types have IDs offset by 1000
                            int customTypeId = type.Id + 1000;
                            _assetTypeNames[customTypeId] = type.DisplayName;
                            _logger.LogDebug("Added custom type: {Id} - {Name}", customTypeId, type.DisplayName);
                        }
                        
                        _logger.LogInformation("Loaded {Count} asset types", _assetTypeNames.Count);
                        
                        // Cache the result
                        _memoryCache.Set(CacheKey, true, CacheDuration);
                        _isInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading asset types from API");
                        
                        // Add built-in types as fallback
                        foreach (AssetType type in Enum.GetValues(typeof(AssetType)))
                        {
                            if ((int)type <= 11)
                            {
                                _assetTypeNames[(int)type] = GetBuiltInAssetTypeDisplayName(type);
                            }
                        }
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static string GetBuiltInAssetTypeDisplayName(AssetType type)
        {
            return type switch
            {
                AssetType.Currency => "ارز",
                AssetType.Gold => "طلا",
                AssetType.Silver => "نقره",
                AssetType.Crypto => "رمزارز",
                AssetType.PreciousMetals => "فلزات گرانبها",
                AssetType.Car => "ماشین",
                AssetType.RealEstate => "املاک",
                AssetType.Stock => "سهام",
                AssetType.Bond => "اوراق قرضه",
                AssetType.ETF => "صندوق‌های قابل معامله",
                AssetType.Custom => "سفارشی",
                AssetType.Unknown => "نامشخص",
                _ => "نامشخص"
            };
        }
    }
} 
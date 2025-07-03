using Calmative.Admin.Web.Models;
using Calmative.Admin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Calmative.Admin.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AssetTypesController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AssetTypesController> _logger;

        public AssetTypesController(IApiService apiService, ILogger<AssetTypesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiService.GetAsync<JsonDocument>("/api/admin/asset-types", GetAdminToken());
                
                var model = new AssetTypesListViewModel
                {
                    BuiltInTypes = new List<AssetTypeViewModel>(),
                    CustomTypes = new List<AssetTypeViewModel>()
                };
                
                if (response != null)
                {
                    var root = response.RootElement;
                    
                    if (root.TryGetProperty("builtInTypes", out var builtInTypes))
                    {
                        model.BuiltInTypes = ConvertToAssetTypeViewModels(builtInTypes, true);
                    }
                    
                    if (root.TryGetProperty("customTypes", out var customTypes))
                    {
                        model.CustomTypes = ConvertToAssetTypeViewModels(customTypes, false);
                    }
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset types");
                TempData["ErrorMessage"] = "خطا در دریافت انواع دارایی";
                return View(new AssetTypesListViewModel());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateAssetTypeViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssetTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.PostAsync("/api/admin/asset-types/custom", model, GetAdminToken());
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "نوع دارایی با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    ModelState.AddModelError("", errorResponse?.Message ?? "خطا در ایجاد نوع دارایی");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset type");
                ModelState.AddModelError("", "خطا در ایجاد نوع دارایی");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var assetType = await _apiService.GetAsync<CustomAssetTypeDto>($"/api/admin/asset-types/custom/{id}", GetAdminToken());
                
                if (assetType == null)
                {
                    TempData["ErrorMessage"] = "نوع دارایی یافت نشد";
                    return RedirectToAction(nameof(Index));
                }
                
                var model = new UpdateAssetTypeViewModel
                {
                    Id = assetType.Id,
                    Name = assetType.Name,
                    DisplayName = assetType.DisplayName,
                    Description = assetType.Description,
                    IsActive = assetType.IsActive
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving asset type with ID {AssetTypeId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت اطلاعات نوع دارایی";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateAssetTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.PutAsync($"/api/admin/asset-types/custom/{id}", model, GetAdminToken());
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "نوع دارایی با موفقیت بروزرسانی شد";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    ModelState.AddModelError("", errorResponse?.Message ?? "خطا در بروزرسانی نوع دارایی");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset type with ID {AssetTypeId}", id);
                ModelState.AddModelError("", "خطا در بروزرسانی نوع دارایی");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"/api/admin/asset-types/custom/{id}", GetAdminToken());
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "نوع دارایی با موفقیت حذف شد";
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    TempData["ErrorMessage"] = errorResponse?.Message ?? "خطا در حذف نوع دارایی";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset type with ID {AssetTypeId}", id);
                TempData["ErrorMessage"] = "خطا در حذف نوع دارایی";
                return RedirectToAction(nameof(Index));
            }
        }

        private string GetAdminToken()
        {
            // In a real implementation, you would have an admin token
            // For this demo, we're returning an empty string
            return "";
        }

        private List<AssetTypeViewModel> ConvertToAssetTypeViewModels(JsonElement items, bool isBuiltIn)
        {
            var result = new List<AssetTypeViewModel>();
            
            if (items.ValueKind != JsonValueKind.Array)
            {
                return result;
            }
            
            foreach (var item in items.EnumerateArray())
            {
                var assetType = new AssetTypeViewModel
                {
                    IsBuiltIn = isBuiltIn,
                    IsActive = isBuiltIn ? true : false
                };
                
                if (item.TryGetProperty("id", out var id))
                {
                    assetType.Id = id.GetInt32();
                }
                
                if (item.TryGetProperty("name", out var name))
                {
                    assetType.Name = name.GetString() ?? string.Empty;
                }
                
                if (item.TryGetProperty("displayName", out var displayName))
                {
                    assetType.DisplayName = displayName.GetString() ?? string.Empty;
                }
                
                if (!isBuiltIn)
                {
                    if (item.TryGetProperty("description", out var description))
                    {
                        assetType.Description = description.GetString();
                    }
                    
                    if (item.TryGetProperty("isActive", out var isActive))
                    {
                        assetType.IsActive = isActive.GetBoolean();
                    }
                    
                    if (item.TryGetProperty("createdAt", out var createdAt) && createdAt.ValueKind != JsonValueKind.Null)
                    {
                        assetType.CreatedAt = createdAt.GetDateTime();
                    }
                    
                    if (item.TryGetProperty("updatedAt", out var updatedAt) && updatedAt.ValueKind != JsonValueKind.Null)
                    {
                        assetType.UpdatedAt = updatedAt.GetDateTime();
                    }
                }
                
                result.Add(assetType);
            }
            
            return result;
        }
    }

    public class CustomAssetTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
} 
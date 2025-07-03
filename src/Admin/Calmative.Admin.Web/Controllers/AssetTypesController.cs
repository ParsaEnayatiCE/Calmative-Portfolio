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
                var response = await _apiService.GetAsync<dynamic>("/api/admin/asset-types", GetAdminToken());
                
                var model = new AssetTypesListViewModel
                {
                    BuiltInTypes = ConvertToAssetTypeViewModels(response.BuiltInTypes, true),
                    CustomTypes = ConvertToAssetTypeViewModels(response.CustomTypes, false)
                };
                
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

        private List<AssetTypeViewModel> ConvertToAssetTypeViewModels(dynamic items, bool isBuiltIn)
        {
            var result = new List<AssetTypeViewModel>();
            
            foreach (var item in items)
            {
                result.Add(new AssetTypeViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    Description = isBuiltIn ? null : item.Description,
                    IsBuiltIn = isBuiltIn,
                    IsActive = isBuiltIn ? true : item.IsActive,
                    CreatedAt = isBuiltIn ? null : item.CreatedAt,
                    UpdatedAt = isBuiltIn ? null : item.UpdatedAt
                });
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
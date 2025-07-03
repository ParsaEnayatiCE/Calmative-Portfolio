using Calmative.Web.App.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace Calmative.Web.App.Models
{
    public class AssetTypeModelBinder : IModelBinder
    {
        private readonly ILogger<AssetTypeModelBinder> _logger;

        public AssetTypeModelBinder(ILogger<AssetTypeModelBinder> logger)
        {
            _logger = logger;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                _logger.LogWarning("No value found for {ModelName}", modelName);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                _logger.LogWarning("Empty value for {ModelName}", modelName);
                return Task.CompletedTask;
            }

            _logger.LogInformation("Binding AssetType value: {Value}", value);

            // Try to parse as integer first
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue))
            {
                _logger.LogInformation("Successfully parsed as integer: {IntValue}", intValue);

                // Handle built-in types
                if (intValue <= 11)
                {
                    bindingContext.Result = ModelBindingResult.Success((AssetType)intValue);
                    return Task.CompletedTask;
                }
                
                // Handle custom types
                if (intValue >= 1000)
                {
                    bindingContext.Result = ModelBindingResult.Success((AssetType)intValue);
                    return Task.CompletedTask;
                }
            }

            // Try to parse as enum name
            if (Enum.TryParse<AssetType>(value, true, out var enumValue))
            {
                _logger.LogInformation("Successfully parsed as enum name: {EnumValue}", enumValue);
                bindingContext.Result = ModelBindingResult.Success(enumValue);
                return Task.CompletedTask;
            }

            _logger.LogWarning("Failed to bind AssetType value: {Value}", value);
            bindingContext.ModelState.TryAddModelError(modelName, $"مقدار '{value}' برای نوع دارایی نامعتبر است");
            return Task.CompletedTask;
        }
    }

    public class AssetTypeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(AssetType))
            {
                return new AssetTypeModelBinder(
                    context.Services.GetRequiredService<ILogger<AssetTypeModelBinder>>());
            }

            return null;
        }
    }
} 
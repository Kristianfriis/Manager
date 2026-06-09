using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Manager.Client.Services;

public class LocalStorageService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task EnsureModuleLoaded()
    {
        if (_module == null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/localStorageInterop.js");
        }
    }

    // Accepts strings or objects, but only serializes if it's NOT a string
    public async Task SetItemAsync<T>(string key, T value)
    {        
        await EnsureModuleLoaded();

        string jsonString;
        if (value is string stringValue)
        {
            jsonString = stringValue; // Skip serialization for raw strings
        }
        else
        {
            jsonString = JsonSerializer.Serialize(value);
        }

        await _module!.InvokeVoidAsync("setItem", key, jsonString);
    }

    // Automatically detects if T is a string and skips deserialization
    public async Task<T?> GetItemAsync<T>(string key)
    {
        await EnsureModuleLoaded();
        var rawValue = await _module!.InvokeAsync<string?>("getItem", key);

        if (string.IsNullOrEmpty(rawValue))
        {
            return default;
        }

        // If the caller requested a string type, return the raw value directly
        if (typeof(T) == typeof(string))
        {
            return (T)(object)rawValue;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(rawValue);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        await EnsureModuleLoaded();
        await _module!.InvokeVoidAsync("removeItem", key);
    }

    public async Task ClearAsync()
    {
        await EnsureModuleLoaded();
        await _module!.InvokeVoidAsync("clear");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
        }
    }
}
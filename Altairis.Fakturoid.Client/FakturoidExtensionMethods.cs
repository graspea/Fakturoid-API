using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altairis.Fakturoid.Client;

internal static class FakturoidExtensionMethods {
    private static readonly JsonSerializerOptions JSON_SETTINGS = new() {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static Task<HttpResponseMessage> FakturoidPostAsJsonAsync<T>(this HttpClient client, string requestUri, T value) {
        var json = JsonSerializer.Serialize(value, JSON_SETTINGS);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return client.PostAsync(requestUri, content);
    }

    public static Task<HttpResponseMessage> FakturoidPatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value) {
        var json = JsonSerializer.Serialize(value, JSON_SETTINGS);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
        return client.SendAsync(request);
    }

    public static async Task<T> FakturoidReadAsAsync<T>(this HttpContent content) {
        var json = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JSON_SETTINGS);
    }
    public static void EnsureFakturoidSuccess(this HttpResponseMessage r) {
        if (r == null) throw new ArgumentNullException(nameof(r));
        if (r.IsSuccessStatusCode) return;

        throw new FakturoidException(r);
    }

}

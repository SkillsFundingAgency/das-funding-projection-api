using System.Text.Json;
using System.Text.Json.Serialization;

namespace FundingProjection.Api.Testing;

public static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    extension(HttpResponseMessage message)
    {
        public HttpResponseAssertions Is => new(message);

        public async Task<TEntity?> ReadContentAsAsync<TEntity>()
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(message.Content);

            string json = await message.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TEntity>(json, JsonOptions);
        }
    }
}
using System.Net.Http.Json;
using WorkTogetherly.Shared.Models;

namespace WorkTogetherly.Shared.Services
{
    public class AmenityService
    {
        private readonly HttpClient _authClient;

        public AmenityService(IHttpClientFactory httpClientFactory)
        {
            _authClient = httpClientFactory.CreateClient("Auth");
        }

        public async Task<List<MaterialDto>> GetAllMaterialsAsync()
        {
            var result = await _authClient.GetFromJsonAsync<List<MaterialDto>>("api/materials");
            return result ?? [];
        }

        public async Task<List<RuleDto>> GetAllRulesAsync()
        {
            var result = await _authClient.GetFromJsonAsync<List<RuleDto>>("api/rules");
            return result ?? [];
        }
    }
}

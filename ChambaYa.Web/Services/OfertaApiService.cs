using System.Net.Http.Json;
using System.Text.Json;
using ChambaYa.Web.Models;

namespace ChambaYa.Web.Services
{
    public class OfertaApiService
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public OfertaApiService(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ChambaYaApi");
        }

        public async Task<List<OfertaModel>> ListarAsync()
        {
            var respuesta = await _http.GetFromJsonAsync<ApiResponse<List<OfertaModel>>>("Ofertas", _jsonOptions);
            return respuesta?.Data ?? new List<OfertaModel>();
        }

        public async Task<(bool Success, string RawJson, int StatusCode)> CrearAsync(OfertaModel oferta)
        {
            var response = await _http.PostAsJsonAsync("CrearOferta", oferta);
            var rawJson = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, rawJson, (int)response.StatusCode);
        }
    }
}
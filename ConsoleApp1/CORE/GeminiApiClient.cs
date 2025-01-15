using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventScraperBackend.Models;
using System.Text.RegularExpressions;

namespace EventScraperBackend
{
    public class ApiConection
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        public ApiConection()
        {
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("La variable de entorno 'GEMINI_API_KEY' no está configurada.");
            }
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public async Task<string> SendPromptWithEventAsync(string prompt, EventData eventData)
        {

            if (string.IsNullOrEmpty(eventData.Name) || string.IsNullOrEmpty(eventData.Description))
            {
                throw new ArgumentException("El nombre del evento y la descripción son obligatorios para la consulta a la API");
            }
            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                      parts = new []
                       {
                           new {
                            text = $"{prompt} Nombre: {eventData.Name}. Descripcion: {eventData.Description}. Fecha: {eventData.Date}. Lugar: {eventData.Place}."
                            }
                        }
                    }
                }
            };

            string jsonRequest = JsonSerializer.Serialize(requestData);

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}");
            request.Content = content;
            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument jsonResponse = JsonDocument.Parse(responseBody);


                if (jsonResponse.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {

                    var firstCandidate = candidates[0];


                    if (firstCandidate.TryGetProperty("content", out var contentElement) && contentElement.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            return text.GetString();
                        }
                    }
                    return null;

                }
                return null;

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error en la solicitud a la API: {ex.Message}");
                return null;

            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Error en la solicitud a la API por timeout: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error al parsear la respuesta JSON: {ex.Message}");
                return null;
            }

        }
        public async Task<string> SendPromptWithHtmlAsync(string prompt, string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                throw new ArgumentException("El html es obligatorio para la consulta a la API");
            }
            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                      parts = new []
                       {
                           new {
                            text = $"{prompt}  HTML: {html}"
                            }
                        }
                    }
                }
            };

            string jsonRequest = JsonSerializer.Serialize(requestData);

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}");
            request.Content = content;
            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();


                string responseBody = await response.Content.ReadAsStringAsync();

                // Limpiar la respuesta con regex
                string cleanedResponse = Regex.Replace(responseBody, @"^```json\s*|```$", "", RegexOptions.Singleline);
                if (string.IsNullOrEmpty(cleanedResponse))
                {
                    Console.WriteLine("La respuesta de la API está vacía.");
                    return @"{
                                      ""containers"": [],
                                      ""categories"": []
                                       }";
                }
                if (cleanedResponse.StartsWith("{"))
                {
                    return cleanedResponse;
                }
                else
                {
                    Console.WriteLine("La respuesta de la API no es un JSON válido. Retornando JSON vacio");
                    return @"{
                                 ""containers"": [],
                                 ""categories"": []
                                    }";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error en la solicitud a la API: {ex.Message}");
                return null;

            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Error en la solicitud a la API por timeout: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error al parsear la respuesta JSON: {ex.Message}");
                return @"{
                                  ""containers"": [],
                                   ""categories"": []
                                    }";
            }

        }
    }
}
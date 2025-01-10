using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventScraperBackend.Models;

namespace EventScraperBackend.Core
{
    public class ApiConection
    {
        private readonly string _apiKey = "AIzaSyBNLqtLDCK-oiVXMC9122qS10iHdFho_7A";
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=GEMINI_API_KEY";
        public async Task<string> SendPromptWithEventAsync(string prompt, EventData eventData)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                throw new ArgumentException("El prompt no puede estar vacío.", nameof(prompt));
            }
            if (eventData == null)
            {
                throw new ArgumentException("El EventData no puede ser nulo.", nameof(eventData));
            }


            string finalPrompt = $"{prompt} " +
                                  $"Nombre: {eventData.Name}, " +
                                  $"URL: {eventData.Url}, " +
                                  $"Imagen: {eventData.ImageUrl}, " +
                                 $"Imagen detalle: {eventData.ImageUrlDetail}, " +
                                 $"Descripcion: {eventData.Description}, " +
                                 $"Fecha: {eventData.Date}, " +
                                  $"Lugar: {eventData.Place}, " +
                                  $"Direccion: {eventData.FinallocationDiv}, " +
                                  $"Url Compra: {eventData.BuyUrl} ";
            using var client = new HttpClient();
            // Reemplaza GEMINI_API_KEY por la clave de la API directamente en la URL
            string finalUrl = _apiUrl.Replace("GEMINI_API_KEY", _apiKey);

            var requestData = new
            {
                contents = new[]
                 {
                 new {
                       parts = new[]
                       {
                         new { text = finalPrompt }
                        }
                    }
                }
            };
            // Serializa el objeto a JSON
            var jsonRequest = JsonSerializer.Serialize(requestData);

            // Crea el contenido de la petición HTTP
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await client.PostAsync(finalUrl, content);
                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado no es exitoso (200-299)

                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error en la petición a la API: {e.Message}");
                return null; // O maneja el error como prefieras
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error al procesar el JSON: {e.Message}");
                return null;
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using EventScraperBackend.Models;
using System.Collections.Generic;
using EventScraperBackend.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.Json;
using System.Text.Json.Nodes;
using SeleniumExtras.WaitHelpers;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using HtmlAgilityPack;
using System.Xml.Serialization;

namespace EventScraperBackend
{
    public class EventProcessor
    {
        private readonly ApiConection _apiConection;
        private readonly EventDataExtractor _dataExtractor;
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private List<Tuple<string, string>> _buyLinks;
        private List<string> _websourceLinks;
        //private Func<string> _getHtml;

        public EventProcessor(ApiConection apiConection, IWebDriver driver, WebDriverWait wait/*, Func<string> getHtml*/)
        {
            _apiConection = apiConection;
            _dataExtractor = new EventDataExtractor();
            _driver = driver;
            _wait = wait;
            _buyLinks = new List<Tuple<string, string>>();
            _websourceLinks = new List<string>();
            //_getHtml = getHtml;
        }

        public async void ProcessHtml(string html)
        {
            string prompt = @"
               Analiza el siguiente HTML de una página web de eventos y extrae la información relevante.
                Identifica los contenedores principales que agrupan la información de cada evento.
                 Para cada contenedor, extrae la información del evento, incluyendo:
                    - nombre del evento.
                    - direccion la imagen del evento (generalmente terminada en .png).
                    - el dia y horario del evento.
                    - el link para navegar hacia mas informacion.
                    - categoria de elvento si figura (por ej. musica, teatro,cine, etc).
                    - link de compra si figura.
                    - contenedor del menu de las categorias si figura.
                Devuelve un JSON con la siguiente estructura:
                {
                  ""containers"": [
                        {
                           ""container_selector"": ""selector_del_contenedor"",
                          ""nombre_evento"": ""nombre_del_evento"",
                           ""image_url_png"": ""direccion_png"",
                           ""dia_horario"": ""dia_y_horario_del_evento"",
                           ""link_navegacion"": ""linga_de_navegacion_mas_informacion"",
                           ""categoria"": ""categoria_del_even0"",
                            ""link_compra"": ""link_de_compra"",
                            ""menu_categorias"": ""menu_de_categorias"",
                            ""contedor_menu_categorias"": ""selector_del_contenedor_menu_categorias""
                        }
                     ]
                  ""categories"": [
                        {
                            ""menu_categorias"": ""menu_de_categorias""
                            ""contedor_menu_categorias"": ""selector_del_contenedor_menu_categorias""
                            ""category_selector"": ""selector_de_categoria"",
                            ""category_url_attribute"": ""atributo_url_categoria""
                        }
                    ]
                 }
                ";

            try
            {
                string response = await _apiConection.SendPromptWithHtmlAsync(prompt, html);

                if (!string.IsNullOrEmpty(response))
                {
                    ProcessApiResponse(response, html);
                }
                else
                {
                    Console.WriteLine("No se obtuvo respuesta de la API");
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrio un error (general): {ex.Message}");
            }
        }
        private void ProcessApiResponse(string apiResponse, string html)
        {
            try
            {
                ResponseJson responseJson = JsonSerializer.Deserialize<ResponseJson>(apiResponse);

                if (responseJson?.Candidates != null && responseJson.Candidates.Count > 0)
                {
                    var firstCandidate = responseJson.Candidates[0];
                    if (firstCandidate?.Content?.Parts != null && firstCandidate.Content.Parts.Count > 0)
                    {
                        var firstPart = firstCandidate.Content.Parts[0];
                        if (firstPart?.Text != null)
                        {
                            string cleanedResponse = Regex.Replace(firstPart.Text, @"^```json\s*|```$", "", RegexOptions.Singleline);
                            string jsonText = cleanedResponse;
                            try
                            {
                                var jsonContent = JsonSerializer.Deserialize<ApiResponseData>(jsonText);
                                // Procesar Contenedores de Eventos
                                if (jsonContent?.Containers != null && jsonContent.Containers.Count > 0)
                                {
                                    foreach (var containerElement in jsonContent.Containers)
                                    {
                                        string containerSelector = containerElement.container_selector;
                                        string nombreEvento = containerElement.nombre_evento;
                                        string imageUrlPng = containerElement.image_url_png;
                                        string diaHorario = containerElement.dia_horario;
                                        string linkNavegacion = containerElement.link_navegacion;
                                        string categoria = containerElement.categoria;
                                        string linkCompra = containerElement.link_compra;
                                        string menuCategorias = containerElement.menu_categorias;
                                        string contedorMenuCategorias = containerElement.contedor_menu_categorias;
                                        Console.WriteLine($"Selector de contenedores: {containerSelector}");
                                        if (containerSelector.StartsWith(".") || containerSelector.StartsWith("#") || !string.IsNullOrEmpty(containerSelector))
                                        {
                                            List<HtmlNode> contenedoresIniciales = ElementExtractor.FindElementsFromHtml(html, containerSelector);
                                            Console.WriteLine($"Total de contenedores encontrados: {contenedoresIniciales?.Count}");
                                            if (contenedoresIniciales != null)
                                            {
                                                foreach (var contenedorInicial in contenedoresIniciales)
                                                {
                                                    try
                                                    {
                                                        HtmlNode boton = ElementExtractor.FindElement(html, linkNavegacion);
                                                        string url = ElementExtractor.ExtractAttribute(boton, "href");

                                                        HtmlNode buyButton = ElementExtractor.FindElement(html, linkCompra);
                                                        string buyUrl = ElementExtractor.ExtractAttribute(buyButton, "href");
                                                        HtmlNode imageElement = ElementExtractor.FindElement(html, imageUrlPng);
                                                        string eventName = ElementExtractor.ExtractAttribute(imageElement, "alt");
                                                        if (buyUrl != null && buyUrl.StartsWith("https://www.ticketek.com.ar/websource"))
                                                        {
                                                            _websourceLinks.Add(buyUrl);
                                                        }
                                                        else if (buyUrl != null)
                                                        {
                                                            _buyLinks.Add(new Tuple<string, string>(eventName, buyUrl));
                                                        }


                                                        EventData eventData = _dataExtractor.ExtractData(contenedorInicial, _driver, _wait, url, imageUrlPng, imageUrlPng, nombreEvento, linkCompra, linkNavegacion, contenedorInicial.InnerHtml);
                                                        if (eventData != null)
                                                        {
                                                            ProcessEvent(eventData);
                                                            Console.WriteLine($"Elementos extraídos: Nombre: '{eventData.Name}', Imagen: '{eventData.ImageUrl}', URL: '{eventData.Url}', Imagen Detalle: '{eventData.ImageUrlDetail}', Descripcion: '{eventData.Description}', Fecha: '{eventData.Date}', Lugar: '{eventData.Place}', Direccion: '{eventData.FinallocationDiv}', URL Compra: '{eventData.BuyUrl}'");
                                                        }


                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.WriteLine($"Error en iteración (general): {ex.Message}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("El selector de contenedores no es valido");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No se encontraron contenedores en la respuesta de la API.");
                                }
                                if (jsonContent.Categories != null && jsonContent.Categories.Count > 0)
                                {
                                    foreach (var categoryElement in jsonContent.Categories)
                                    {
                                        string categorySelector = categoryElement.category_selector;
                                        string categoryUrlAttribute = categoryElement.category_url_attribute;
                                        List<HtmlNode> categoryElements = ElementExtractor.FindElementsFromHtml(html, categorySelector);
                                        if (categoryElements != null)
                                        {
                                            foreach (var categoryElem in categoryElements)
                                            {
                                                try
                                                {
                                                    string categoryUrl = ElementExtractor.ExtractAttribute(categoryElem, categoryUrlAttribute);
                                                    Console.WriteLine($"URL de la categoria encontrada: {categoryUrl}");

                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"Error al extraer la URL de la categoria: {ex.Message}");
                                                }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No se encontraron categorías en la respuesta de la API.");
                                }
                            }
                            catch (JsonException ex)
                            {
                                Console.WriteLine($"Error al parsear la respuesta JSON: {ex.Message}");
                                return;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ocurrio un error (general): {ex.Message}");
                                return;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No se encontraron candidatos en la respuesta de la API.");
                    }
                    Console.WriteLine("URLs de compra encontradas:");
                    foreach (var buyLink in _buyLinks)
                    {
                        Console.WriteLine($"Evento: {buyLink.Item1}, URL de compra: {buyLink.Item2}");
                    }
                    Console.WriteLine("URLs websource encontradas:");
                    foreach (var websourceLink in _websourceLinks)
                    {
                        Console.WriteLine($"{websourceLink}");
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error al parsear la respuesta JSON: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrio un error (general): {ex.Message}");
                return;
            }
        }
        public async void ProcessEvent(EventData eventData)
        {
            string prompt = "Analiza la siguiente información del evento y clasificalo en una categoría (Música, Familia, Teatro, Deportes, Especiales). Si la informacion de fecha, horario y precio son multiples, devuelve todos los valores disponibles";
            try
            {
                string response = await _apiConection.SendPromptWithEventAsync(prompt, eventData);
                if (response != null)
                {
                    eventData.Category = response;
                    Console.WriteLine("Respuesta de la API:");
                    Console.WriteLine(response);
                }
                else
                {
                    Console.WriteLine("No se obtuvo respuesta de la API");
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
    public class ResponseJson
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }
    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
    public class ApiResponseData
    {
        [JsonPropertyName("containers")]
        public List<Container> Containers { get; set; }

        [JsonPropertyName("categories")]
        public List<Category> Categories { get; set; }
    }
    public class Container
    {
        [JsonPropertyName("container_selector")]
        public string container_selector { get; set; }
        [JsonPropertyName("nombre_evento")]
        public string nombre_evento { get; set; }
        [JsonPropertyName("image_url_png")]
        public string image_url_png { get; set; }
        [JsonPropertyName("dia_horario")]
        public string dia_horario { get; set; }
        [JsonPropertyName("link_navegacion")]
        public string link_navegacion { get; set; }
        [JsonPropertyName("categoria")]
        public string categoria { get; set; }
        [JsonPropertyName("link_compra")]
        public string link_compra { get; set; }
        [JsonPropertyName("menu_categorias")]
        public string menu_categorias { get; set; }
        [JsonPropertyName("contedor_menu_categorias")]
        public string contedor_menu_categorias { get; set; }
    }
    public class Category
    {
        [JsonPropertyName("category_selector")]
        public string category_selector { get; set; }

        [JsonPropertyName("category_url_attribute")]
        public string category_url_attribute { get; set; }
    }
}
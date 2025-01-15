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
                    - El selector CSS para la imagen del evento dentro del contenedor.
                    - El atributo de la etiqueta img donde se encuentra la URL de la imagen.
                    - El atributo de la etiqueta img donde se encuentra el nombre del evento.
                    - El selector CSS para el link que contiene la URL del detalle del evento.
                    - El atributo de la etiqueta a que tiene la URL del detalle del evento.
                    - El selector CSS para el link que contiene la URL de compra del evento.
                    - El atributo de la etiqueta a que tiene la URL de compra del evento.
                Devuelve un JSON con la siguiente estructura:
                {
                  ""containers"": [
                        {
                           ""container_selector"": ""selector_del_contenedor"",
                          ""image_selector"": ""selector_de_la_imagen"",
                           ""image_url_attribute"": ""atributo_de_la_url_de_la_imagen"",
                           ""image_name_attribute"": ""atributo_del_nombre_de_la_imagen"",
                           ""link_selector"": ""selector_del_link"",
                           ""link_url_attribute"": ""atributo_del_link"",
                          
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
                                        string imageSelector = containerElement.image_selector;
                                        string imageUrlAttribute = containerElement.image_url_attribute;
                                        string imageNameAttribute = containerElement.image_name_attribute;
                                        string linkSelector = containerElement.link_selector;
                                        string linkUrlAttribute = containerElement.link_url_attribute;
                                        string buyLinkSelector = containerElement.buy_link_selector;
                                        string buyLinkUrlAttribute = containerElement.buy_link_url_attribute;
                                        // Extraer el elemento más interno si existe


                                        // Remover el punto inicial del selector, si existe.
                                        if (containerSelector.StartsWith("."))
                                        {
                                            containerSelector = containerSelector.Substring(1);
                                        }
                                        if (imageSelector.StartsWith("."))
                                        {
                                            imageSelector = imageSelector.Substring(1);
                                        }
                                        if (linkSelector != null)
                                        {
                                            if (linkSelector.StartsWith("."))
                                            {
                                                linkSelector = linkSelector.Substring(1);
                                            }
                                        }

                                        if (buyLinkSelector != null)
                                        {
                                            if (buyLinkSelector.StartsWith("."))
                                            {
                                                buyLinkSelector = buyLinkSelector.Substring(1);
                                            }
                                        }

                                        // Nueva lógica para eliminar "div.", "row."
                                        if (containerSelector.StartsWith("div."))
                                        {
                                            containerSelector = containerSelector.Substring(4);
                                        }
                                        if (containerSelector.StartsWith("row."))
                                        {
                                            containerSelector = containerSelector.Substring(4);
                                        }
                                        if (imageSelector.StartsWith("div."))
                                        {
                                            imageSelector = imageSelector.Substring(4);
                                        }
                                        if (imageSelector.StartsWith("row."))
                                        {
                                            imageSelector = imageSelector.Substring(4);
                                        }
                                        if (linkSelector != null)
                                        {
                                            if (linkSelector.StartsWith("div."))
                                            {
                                                linkSelector = linkSelector.Substring(4);
                                            }
                                            if (linkSelector.StartsWith("row."))
                                            {
                                                linkSelector = linkSelector.Substring(4);
                                            }
                                        }
                                        if (buyLinkSelector != null)
                                        {
                                            if (buyLinkSelector.StartsWith("div."))
                                            {
                                                buyLinkSelector = buyLinkSelector.Substring(4);
                                            }
                                            if (buyLinkSelector.StartsWith("row."))
                                            {
                                                buyLinkSelector = buyLinkSelector.Substring(4);
                                            }
                                        }

                                        // sacar punto y convertiro en espacio
                                        if (containerSelector.Contains("."))
                                        {
                                            containerSelector = containerSelector.Replace(".", " ");
                                        }
                                        if (imageSelector.Contains("."))
                                        {
                                            imageSelector = imageSelector.Replace(".", " ");
                                        }
                                        if (linkSelector != null && linkSelector.Contains("."))
                                        {
                                            linkSelector = linkSelector.Replace(".", " ");
                                        }
                                        if (buyLinkSelector != null && buyLinkSelector.Contains("."))
                                        {
                                            buyLinkSelector = buyLinkSelector.Replace(".", " ");
                                        }
                                        // Nueva lógica para tomar solo la primera parte del selector si contiene espacios
                                        if (containerSelector.Contains(" "))
                                        {
                                            containerSelector = containerSelector.Split(' ')[0];
                                        }

                                        Console.WriteLine($"Selector de contenedores: {containerSelector}");
                                        if (containerSelector.StartsWith(".") || containerSelector.StartsWith("#") || !string.IsNullOrEmpty(containerSelector))
                                        {
                                            List<HtmlNode> contenedoresIniciales = ElementExtractor.FindElementsFromHtml(html, containerSelector);
                                            Console.WriteLine($"Total de contenedores encontrados: {contenedoresIniciales?.Count}");
                                            if (contenedoresIniciales != null)
                                            {
                                                foreach (var contenedorInicial in contenedoresIniciales)
                                                {
                                                    string url = null;
                                                    string buyUrl = null;
                                                    string eventName = null;
                                                    HtmlNode imageElement = null;
                                                    try
                                                    {
                                                        if (linkSelector != null)
                                                        {
                                                            HtmlNode boton = ElementExtractor.FindElement(html, linkSelector);
                                                            url = ElementExtractor.ExtractAttribute(boton, linkUrlAttribute);
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("No se encontró un selector para el link del evento.");
                                                        }

                                                        if (buyLinkSelector != null)
                                                        {
                                                            HtmlNode buyButton = ElementExtractor.FindElement(html, buyLinkSelector);
                                                            buyUrl = ElementExtractor.ExtractAttribute(buyButton, buyLinkUrlAttribute);
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("No se encontró un selector para el link de compra del evento.");
                                                        }

                                                        if (imageSelector != null)
                                                        {
                                                            imageElement = ElementExtractor.FindElement(html, imageSelector);
                                                            eventName = ElementExtractor.ExtractAttribute(imageElement, imageNameAttribute);
                                                        }


                                                        EventData eventData = _dataExtractor.ExtractData(contenedorInicial, _driver, _wait, url, imageSelector, imageUrlAttribute, imageNameAttribute, linkSelector, linkUrlAttribute, contenedorInicial.InnerHtml);
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
                                }
                            }
                            catch (Exception ex)

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

        [JsonPropertyName("image_selector")]
        public string image_selector { get; set; }

        [JsonPropertyName("image_url_attribute")]
        public string image_url_attribute { get; set; }

        [JsonPropertyName("image_name_attribute")]
        public string image_name_attribute { get; set; }

        [JsonPropertyName("link_selector")]
        public string link_selector { get; set; }

        [JsonPropertyName("link_url_attribute")]
        public string link_url_attribute { get; set; }
        [JsonPropertyName("buy_link_selector")]
        public string buy_link_selector { get; set; }

        [JsonPropertyName("buy_link_url_attribute")]
        public string buy_link_url_attribute { get; set; }
    }
    public class Category
    {
        [JsonPropertyName("category_selector")]
        public string category_selector { get; set; }

        [JsonPropertyName("category_url_attribute")]
        public string category_url_attribute { get; set; }
    }
}
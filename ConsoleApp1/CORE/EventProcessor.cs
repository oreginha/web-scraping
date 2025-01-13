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

        public EventProcessor(ApiConection apiConection, IWebDriver driver, WebDriverWait wait)
        {
            _apiConection = apiConection;
            _dataExtractor = new EventDataExtractor();
            _driver = driver;
            _wait = wait;
            _buyLinks = new List<Tuple<string, string>>();
            _websourceLinks = new List<string>();
        }

        public async void ProcessHtml(string html)
        {
            string prompt = @"
               Analiza el siguiente HTML de una página web de eventos y extrae la información relevante.

                1.  **Eventos Principales:**
                    *   Identifica los contenedores principales que agrupan la información de cada evento individual.
                    *   Para cada contenedor, extrae la siguiente información del evento:
                        *   El selector CSS del contenedor.
                        *   El selector CSS para la imagen del evento dentro del contenedor.
                        *   El atributo de la etiqueta `img` donde se encuentra la URL de la imagen.
                        *   El atributo de la etiqueta `img` donde se encuentra el nombre del evento.
                        *   El selector CSS para el link que contiene la URL del detalle del evento.
                        *   El atributo de la etiqueta `a` que tiene la URL del detalle del evento.
                        *   El selector CSS para el link que contiene la URL de compra del evento.
                        *   El atributo de la etiqueta `a` que tiene la URL de compra del evento.

                2.  **Categorías de Eventos:**
                    *   Identifica el menú o la sección que contiene las categorías de eventos (por ejemplo, ""Música"", ""Teatro"", ""Deportes"", etc.).
                    *   Para cada categoría, extrae:
                        *   El selector CSS del elemento que contiene la categoría.
                         *  El atributo de la etiqueta `a` que tiene la URL de la categoría.

                Devuelve un JSON con la siguiente estructura:

                ```json
                {
                  ""containers"": [
                    {
                      ""container_selector"": ""selector_del_contenedor"",
                      ""image_selector"": ""selector_de_la_imagen"",
                      ""image_url_attribute"": ""atributo_de_la_url_de_la_imagen"",
                      ""image_name_attribute"": ""atributo_del_nombre_de_la_imagen"",
                      ""link_selector"": ""selector_del_link"",
                      ""link_url_attribute"": ""atributo_del_link"",
                      ""buy_link_selector"": ""selector_del_link_de_compra"",
                       ""buy_link_url_attribute"": ""atributo_del_link_de_compra""
                    }
                  ],
                  ""categories"": [
                    {
                       ""category_selector"": ""selector_de_la_categoria"",
                       ""category_url_attribute"": ""atributo_de_la_url_de_la_categoria""
                     }
                   ]
                }
                ```
                ";

            try
            {
                string response = await _apiConection.SendPromptWithHtmlAsync(prompt, html);

                if (!string.IsNullOrEmpty(response))
                {
                    ProcessApiResponse(response);
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
        private void ProcessApiResponse(string apiResponse)
        {
            try
            {
                JsonDocument jsonResponse = JsonDocument.Parse(apiResponse);
                // Procesar Contenedores de Eventos
                if (jsonResponse.RootElement.TryGetProperty("containers", out var containers) && containers.GetArrayLength() > 0)
                {
                    foreach (JsonElement containerElement in containers.EnumerateArray())
                    {
                        string containerSelector = containerElement.GetProperty("container_selector").GetString();
                        string imageSelector = containerElement.GetProperty("image_selector").GetString();
                        string imageUrlAttribute = containerElement.GetProperty("image_url_attribute").GetString();
                        string imageNameAttribute = containerElement.GetProperty("image_name_attribute").GetString();
                        string linkSelector = containerElement.GetProperty("link_selector").GetString();
                        string linkUrlAttribute = containerElement.GetProperty("link_url_attribute").GetString();
                        string buyLinkSelector = containerElement.GetProperty("buy_link_selector").GetString();
                        string buyLinkUrlAttribute = containerElement.GetProperty("buy_link_url_attribute").GetString();

                        IReadOnlyCollection<IWebElement> contenedoresIniciales = _wait.Until(ExpectedConditions
                              .PresenceOfAllElementsLocatedBy(By.CssSelector(containerSelector)));
                        Console.WriteLine($"Total de contenedores encontrados: {contenedoresIniciales.Count}");
                        foreach (var contenedorInicial in contenedoresIniciales)
                        {
                            IWebElement container = contenedorInicial;
                            try
                            {
                                IWebElement boton = container.FindElement(By.CssSelector(linkSelector));
                                string url = ElementExtractor.ExtractAttribute(boton, linkUrlAttribute);

                                IWebElement buyButton = ElementExtractor.FindElementWithoutWait(container, By.CssSelector(buyLinkSelector));
                                string buyUrl = ElementExtractor.ExtractAttribute(buyButton, buyLinkUrlAttribute);

                                IWebElement imageElement = ElementExtractor.FindElement(container, By.CssSelector(imageSelector), _wait);
                                string eventName = ElementExtractor.ExtractAttribute(imageElement, imageNameAttribute);

                                if (buyUrl.StartsWith("https://www.ticketek.com.ar/websource"))
                                {
                                    _websourceLinks.Add(buyUrl);
                                }
                                else
                                {
                                    _buyLinks.Add(new Tuple<string, string>(eventName, buyUrl));
                                }


                                EventData eventData = _dataExtractor.ExtractData(container, _driver, _wait, url, imageSelector, imageUrlAttribute, imageNameAttribute, linkSelector, linkUrlAttribute);
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

                // Procesar Categorias
                if (jsonResponse.RootElement.TryGetProperty("categories", out var categories) && categories.GetArrayLength() > 0)
                {

                    foreach (JsonElement categoryElement in categories.EnumerateArray())
                    {
                        string categorySelector = categoryElement.GetProperty("category_selector").GetString();
                        string categoryUrlAttribute = categoryElement.GetProperty("category_url_attribute").GetString();

                        IReadOnlyCollection<IWebElement> categoryElements = _wait.Until(ExpectedConditions
                         .PresenceOfAllElementsLocatedBy(By.CssSelector(categorySelector)));

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
            catch (JsonException ex)
            {
                Console.WriteLine($"Error al parsear la respuesta JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrio un error (general): {ex.Message}");
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
}
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using EventScraperBackend.Models;
using EventScraperBackend.Core;

namespace EventScraperBackend
{
    public class TicketekScraper : BaseScraper
    {
        private EventDataExtractor dataExtractor;
          public TicketekScraper(string driverPath, string websiteUrl, int timeoutSeconds)
              : base(driverPath, websiteUrl, timeoutSeconds)
           {
                 dataExtractor = new EventDataExtractor();
            }
           public void ScrapeEvents()
           {
                 try {

                    // Buscar un elemento
                        IReadOnlyCollection<IWebElement> contenedoresIniciales = wait.Until(ExpectedConditions
                            .PresenceOfAllElementsLocatedBy(By.CssSelector("div.tkt-artist-list-image-item.relative.col-xs-10")));

                    Console.WriteLine($"Total de contenedores encontrados: {contenedoresIniciales.Count}");

                        foreach (var contenedorInicial in contenedoresIniciales)
                        {
                            IWebElement container = contenedorInicial;
                                try
                                {
                                    var boton = container.FindElement(By.CssSelector("a"));
                                    var url = boton.GetAttribute("href");

                                      EventData eventData;
                                       if (url != null){
                                          eventData =  dataExtractor.ExtractData(container, driver, wait, url);
                                            if(eventData != null) {
                                           Console.WriteLine($"Elementos extraídos: Nombre: '{eventData.Name}', Imagen: '{eventData.ImageUrl}', URL: '{eventData.Url}', Imagen Detalle: '{eventData.ImageUrlDetail}', Descripcion: '{eventData.Description}', Fecha: '{eventData.Date}', Lugar: '{eventData.Place}', Direccion: '{eventData.FinallocationDiv}', URL Compra: '{eventData.BuyUrl}'");
                                          }
                                        }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error en iteración (general): {ex.Message}");
                                }
                            }

                    }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ocurrio un error (general): {ex.Message}");
                }
               finally
               {
                 Dispose();
                }
           }
    }
}
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using EventScraperBackend.Models;
using EventScraperBackend.Core;

namespace EventScraperBackend
{
    public class TicketekScraper : BaseScraper
    {
        private EventDataExtractor _dataExtractor;
        private EventProcessor _eventProcessor;

        public TicketekScraper(string driverPath, string websiteUrl, int timeoutSeconds, EventProcessor eventProcessor)
             : base(driverPath, websiteUrl, timeoutSeconds)
        {
            _dataExtractor = new EventDataExtractor();
            _eventProcessor = eventProcessor;
        }
        private void WaitForPageLoad()
        {
            WebDriverWait waitPageLoad = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
            waitPageLoad.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }

        public void ScrapeEvents()
        {
            try
            {
                WaitForPageLoad(); // Esperar a que la página esté completamente cargada.
                string html = driver.PageSource;
                if (!string.IsNullOrEmpty(html))
                {
                    _eventProcessor.ProcessHtml(html);
                    Console.WriteLine("HTML de la página extraído correctamente.");

                }
                else
                {
                    Console.WriteLine("Error al extraer el HTML de la página.");
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
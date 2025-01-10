using System;
using System.Threading.Tasks;
using EventScraperBackend;
using EventScraperBackend.Core;
using EventScraperBackend.Models;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string driverPath = "C:\\SeleniumDrivers\\chromedriver";
            string websiteUrl = "https://www.ticketek.com.ar/eventos";
            int timeoutSeconds = 20;
            var ticketekScraper = new TicketekScraper(driverPath, websiteUrl, timeoutSeconds);
            var apiConection = new ApiConection();

            List<EventData> eventList = ticketekScraper.ScrapeEvents();

            if (eventList != null)
            {
                foreach (EventData eventData in eventList)
                {
                    string prompt = "Analiza la siguiente información del evento y clasificalo en una categoría (Música, Familia, Teatro, Deportes, Especiales). Si la informacion de fecha, horario y precio son multiples, devuelve todos los valores disponibles";

                    try
                    {
                        string response = await apiConection.SendPromptWithEventAsync(prompt, eventData);
                        if (response != null)
                        {
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
            else
            {
                Console.WriteLine("No se obtuvieron eventos de la página.");
            }


        }
    }
}
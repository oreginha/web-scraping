using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;
using EventScraperBackend;
using EventScraperBackend.Core;
using OpenQA.Selenium.Support.UI;

namespace ConsoleApp1
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            string driverPath = "C:\\SeleniumDrivers\\chromedriver";
            string websiteUrl = "https://www.ticketek.com.ar/musica";
            int randomPort = new Random().Next(49152, 65535);

            // Configuración del driver
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument($"--remote-debugging-port={randomPort}"); // Usar el puerto aleatorio

            var apiConection = new ApiConection();

            using (IWebDriver driver = new ChromeDriver(driverPath, options))
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
                // Navegar a la URL
                driver.Navigate().GoToUrl(websiteUrl);

                // Espera explícita para que la página se cargue completamente
                ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState === 'complete'");
                Console.WriteLine("Página cargada correctamente. Presiona cualquier tecla para extraer el HTML y procesarlo, o ESC para salir.");

                var eventProcessor = new EventProcessor(apiConection, driver, wait);
                bool exit = false;
                while (!exit)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);


                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        exit = true;
                        Console.WriteLine("Programa finalizado.");
                    }
                    else
                    {

                        string html = driver.PageSource;
                       List<string> cleanHtml = HtmlCleaner.CleanHtml(html); // Limpiar el HTML con la clase HtmlCleaner
                        Console.WriteLine("HTML extraído y enviado al EventProcessor.");
                        eventProcessor.ProcessHtml(cleanHtml[0]);
                        await Task.Delay(2000);  // Espera de 2 segundos entre peticiones a la API.
                        Console.WriteLine("Presiona cualquier tecla para extraer el html o ESC para salir");

                    }
                }

            }

        }
    }
}
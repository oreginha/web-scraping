using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace EventScraperBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Configuracion
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--remote-debugging-port=9222");


                // Inicialización
                IWebDriver driver = new ChromeDriver("C:\\SeleniumDrivers\\chromedriver");
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Navigate().GoToUrl("https://www.ticketek.com.ar/musica");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                // Buscar un elemento
                var contenedores = driver.FindElements(By.CssSelector("div.tkt-artist-list-image-item.relative.col-xs-10.col-sm-33.col-md-10"));

                Console.WriteLine(contenedores.Count);

                foreach (var container in contenedores)
                {
                    try
                    {
                        // Imprimir el HTML del contenedor para inspección
                        var containerHTML = container.GetAttribute("innerHTML");
                        Console.WriteLine("HTML contenedor: " + containerHTML);

                        // Esperar a que el elemento sea visible y luego extraer el texto
                        var nameElement = container.FindElement(By.CssSelector("div.info-container.absolute span.text-uppercase.info-title"));
                        Console.WriteLine("Elemento encontrado: " + nameElement.ToString());
                        var name = nameElement.Text;
                        Console.WriteLine(name);

                        // Extraer la URL de la imagen
                        var imageElement = container.FindElement(By.CssSelector("img.img-responsive.tkt-img-info.col-xs-10.no-padding"));
                        var imageUrl = imageElement.GetAttribute("src");
                        Console.WriteLine(imageUrl);

                    }
                    catch (NoSuchElementException e)
                    {
                        Console.WriteLine($"No se pudo encontrar el elemento: {e.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ocurrio un error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrio un error: {ex.Message}");
            }
        }
    }
}
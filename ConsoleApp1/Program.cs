using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Linq;

namespace EventScraperBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            // Cerrar consolas existentes
         
            IWebDriver driver = null;
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
                driver = new ChromeDriver("C:\\SeleniumDrivers\\chromedriver");

                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Navigate().GoToUrl("https://www.ticketek.com.ar/musica");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                // Buscar un elemento
                var contenedores = driver.FindElements(By.CssSelector("div.tkt-artist-list-image-item.relative.col-xs-10.col-sm-33.col-md-10"));

                Console.WriteLine(contenedores.Count);

                foreach (var container in contenedores)
                {
                    try
                    {
                        // Imprimir el HTML del contenedor para inspección
                        //var containerHTML = container.GetAttribute("innerHTML");
                        //Console.WriteLine("HTML contenedor: " + containerHTML);


                        // Extraer la URL de la imagen y el nombre
                        var imageElement = container.FindElement(By.CssSelector("img.img-responsive.tkt-img-info.col-xs-10.no-padding"));
                        var imageUrl = imageElement.GetAttribute("src");
                        var name = imageElement.GetAttribute("alt");
                        Console.WriteLine(imageUrl);
                        Console.WriteLine(name);

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
            finally
            {
                if (driver != null)
                {
                    driver.Quit();
                }

            }
        }
    }
}

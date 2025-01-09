using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Linq;
using System.Text;
using System.Collections.Generic;

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
                IReadOnlyCollection<IWebElement> contenedoresIniciales = wait.Until(ExpectedConditions
                    .PresenceOfAllElementsLocatedBy(By.CssSelector("div.tkt-artist-list-image-item.relative.col-xs-10")));

                Console.WriteLine($"Total de contenedores encontrados: {contenedoresIniciales.Count}");

                foreach (var contenedorInicial in contenedoresIniciales)
                {
                    IWebElement container = contenedorInicial;
                    string imageUrl = null;
                    string name = null;
                    string url = null;
                    string imageUrlDetail = null;
                    string description = null;
                    string date = null;
                    string place = null;
                    string finallocationDiv = null;
                    string buyUrl = null;
                    try
                    {
                        IWebElement imageElement = null;
                        try
                        {
                            imageElement = container.FindElement(By.CssSelector("img.img-responsive.tkt-img-info.col-xs-10"));

                            // Extraer la URL de la imagen y el nombre
                            imageUrl = imageElement.GetAttribute("src");
                            name = imageElement.GetAttribute("alt");

                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener la imagen y el nombre: {ex.Message}");
                        }


                        IWebElement boton = null;
                        try
                        {
                            boton = container.FindElement(By.CssSelector("a"));
                            url = boton.GetAttribute("href");
                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener la URL: {ex.Message}");
                        }

                        // Abre una nueva pestaña
                        string originalWindow = driver.CurrentWindowHandle;
                        driver.SwitchTo().NewWindow(WindowType.Tab);

                        // Navega a la página de detalles en la nueva pestaña
                        if (url != null)
                        {
                            driver.Navigate().GoToUrl(url);
                        }
                        else
                        {
                            Console.WriteLine("No se puede navegar a la URL de la pagina de detalle porque no se pudo obtener");
                            driver.Close();
                            driver.SwitchTo().Window(originalWindow);
                            continue;
                        }
                        // Esperar a que cargue la página
                        try
                        {
                            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("body.os-windows.browser-chrome.device-unknown")));
                        }
                        catch (WebDriverTimeoutException ex)
                        {
                            Console.WriteLine($"Error al esperar la pagina de detalle: {ex.Message}");
                            driver.Close();
                            driver.SwitchTo().Window(originalWindow);
                            continue;

                        }
                        IWebElement imageElementDetailObject = null;
                        try
                        {
                            // Extraer la URL de la imagen grande
                            imageElementDetailObject = driver.FindElement(By.CssSelector("img.img-responsive"));
                            imageUrlDetail = imageElementDetailObject.GetAttribute("src");
                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener la imagen de detalle: {ex.Message}");
                        }

                        IWebElement descriptionElementObject = null;
                        try
                        {
                            // Extraer la descripción
                            descriptionElementObject = driver.FindElement(By.CssSelector("p"));
                            description = descriptionElementObject.Text;

                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener la descripcion: {ex.Message}");
                        }
                        IWebElement dateElementObject = null;
                        try
                        {
                            // Extraer la fecha
                            dateElementObject = driver.FindElement(By.CssSelector("strong"));
                            date = dateElementObject.Text;
                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener la fecha: {ex.Message}");
                        }

                        IWebElement placeElementObject = null;
                        try
                        {
                            // Extraer el lugar
                            placeElementObject = driver.FindElement(By.CssSelector("h4.text-uppercase"));
                            place = placeElementObject.Text;
                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener el lugar: {ex.Message}");
                        }
                        IReadOnlyCollection<IWebElement> locationDivs = null;
                        try
                        {
                            //Extraer direccion
                            locationDivs = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div.text-black")));
                            StringBuilder concatenatedText = new StringBuilder();
                            foreach (IWebElement locationDiv in locationDivs)
                            {
                                concatenatedText.Append(locationDiv.Text);
                                concatenatedText.Append(" "); // Add a space between each div for readability
                            }
                            finallocationDiv = concatenatedText.ToString().Trim();
                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener la direccion: {ex.Message}");
                        }

                        IWebElement buyElementObject = null;
                        try
                        {
                            // Extraer el link de compra
                            buyElementObject = driver.FindElement(By.CssSelector("a.btn.btn-default.show-btn.show-buy.animate"));
                            buyUrl = buyElementObject.GetAttribute("href");

                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine($"Error al obtener la url de compra: {ex.Message}");
                        }
                        //Console.WriteLine($"Elementos extraídos: Nombre: '{name}', Imagen: '{imageUrl}', URL: '{url}', Imagen Detalle: '{imageUrlDetail}', Descripcion: '{description}', Fecha: '{date}', Lugar: '{place}', Direccion: '{finallocationDiv}', URL Compra: '{buyUrl}'");

                        // Cerrar la pestaña actual
                        driver.Close();
                        driver.SwitchTo().Window(originalWindow);

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
                if (driver != null)
                {
                    driver.Quit();
                }
            }
        }
    }
}
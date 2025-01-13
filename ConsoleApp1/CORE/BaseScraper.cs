using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace EventScraperBackend.Core
{
    public abstract class BaseScraper
    {
          public IWebDriver driver;
        public WebDriverWait wait;
        public BaseScraper(string driverPath, string websiteUrl, int timeoutSeconds)
        {
                 // Configuracion
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--remote-debugging-port=9222");


                 driver = new ChromeDriver(driverPath);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Navigate().GoToUrl(websiteUrl);
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        }

      protected virtual void Dispose()
        {
           if (driver != null)
                {
                  driver.Quit();
               }
        }
    }
}
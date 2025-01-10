using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using EventScraperBackend.Models;
using EventScraperBackend.Core;
using System.Text;

namespace EventScraperBackend.Core
{
    public class EventDataExtractor
    {
        public EventData ExtractData(IWebElement container, IWebDriver driver, WebDriverWait wait, string url)
        {
            EventData eventData = new EventData();

            try
            {
                IWebElement imageElement = ElementExtractor.FindElement(container, By.CssSelector("img.img-responsive.tkt-img-info.col-xs-10"), wait);
                eventData.ImageUrl = ElementExtractor.ExtractAttribute(imageElement, "src");
                eventData.Name = ElementExtractor.ExtractAttribute(imageElement, "alt");


                IWebElement boton = ElementExtractor.FindElement(container, By.CssSelector("a"), wait);
                eventData.Url = ElementExtractor.ExtractAttribute(boton, "href");
                // Abre una nueva pestaña
                string originalWindow = driver.CurrentWindowHandle;
                NavigationHelper.OpenNewTabAndNavigate(driver, url);
                bool footerExists = false;
                try
                {
                    var footer = driver.FindElement(By.CssSelector("tkt-image-show no-padding.text-uppercase.col-xs-10 bg-grey"));
                }
                catch (Exception ex)
                {


                    footerExists = true;

                }


                if (footerExists)
                {
                    eventData = ProcessEventWithHrefs(driver, wait, url, originalWindow);
                }
                else
                {
                    eventData = ProcessEventWithoutHrefs(driver, wait, url, originalWindow);
                }

                NavigationHelper.CloseCurrentTabAndSwitchBack(driver, originalWindow);

            }
            finally
            {
            }
            return eventData;
        }
        private static EventData ProcessEventWithHrefs(IWebDriver driver, WebDriverWait wait, string url, string originalWindow)
        {
            EventData eventData = new EventData();

            IReadOnlyCollection<IWebElement> hrefs = ElementExtractor.FindElements(By.CssSelector("#footer a[href]"), wait);
            if (hrefs == null)
            {
                NavigationHelper.CloseCurrentTabAndSwitchBack(driver, originalWindow);
                return null;
            }
            else
            {

                foreach (var hrefElement in hrefs)
                {
                    try
                    {
                        string href = ElementExtractor.ExtractAttribute(hrefElement, "href");
                        // Abre una nueva pestaña
                        string innerTabOriginalWindow = driver.CurrentWindowHandle;
                        NavigationHelper.OpenNewTabAndNavigate(driver, href);
                        try
                        {
                            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("body.os-windows.browser-chrome.device-unknown")));
                            IWebElement imageElementDetailObject = ElementExtractor.FindElement(null, By.CssSelector("img.img-responsive.col-sm-10"), wait);
                            eventData.ImageUrlDetail = ElementExtractor.ExtractAttribute(imageElementDetailObject, "src");
                            IWebElement descriptionElementObject = ElementExtractor.FindElement(null, By.CssSelector("p"), wait);
                            eventData.Description = ElementExtractor.ExtractText(descriptionElementObject);
                            IWebElement dateElementObject = ElementExtractor.FindElement(null, By.CssSelector("strong"), wait);
                            eventData.Date = ElementExtractor.ExtractText(dateElementObject);
                            IWebElement placeElementObject = ElementExtractor.FindElement(null, By.CssSelector("h4.text-uppercase"), wait);
                            eventData.Place = ElementExtractor.ExtractText(placeElementObject);
                            IReadOnlyCollection<IWebElement> locationDivs = ElementExtractor.FindElements(By.CssSelector("div.text-black"), wait);
                            StringBuilder concatenatedText = new StringBuilder();
                            foreach (IWebElement locationDiv in locationDivs)
                            {
                                concatenatedText.Append(locationDiv.Text);
                                concatenatedText.Append(" "); // Add a space between each div for readability
                            }
                            eventData.FinallocationDiv = concatenatedText.ToString().Trim();
                            IWebElement buyElementObject = ElementExtractor.FindElement(null, By.CssSelector("a.btn.btn-default.show-btn.show-buy.animate"), wait);
                            eventData.BuyUrl = ElementExtractor.ExtractAttribute(buyElementObject, "href");


                            NavigationHelper.CloseCurrentTabAndSwitchBack(driver, innerTabOriginalWindow);
                            return eventData;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error en iteración (href): {ex.Message}");
                            NavigationHelper.CloseCurrentTabAndSwitchBack(driver, innerTabOriginalWindow);
                            return null;
                        }
                        finally
                        {
                            NavigationHelper.CloseCurrentTabAndSwitchBack(driver, innerTabOriginalWindow);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error en iteración (href): {ex.Message}");
                        NavigationHelper.CloseCurrentTabAndSwitchBack(driver, originalWindow);
                        return null;
                    }

                }
                return eventData;

            }
        }
        private EventData ProcessEventWithoutHrefs(IWebDriver driver, WebDriverWait wait, string url, string originalWindow)
        {
            EventData eventData = new EventData();
            try
            {
                // Esperar a que cargue la página
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("body.os-windows.browser-chrome.device-unknown")));
                IWebElement imageElementDetailObject = ElementExtractor.FindElement(null, By.CssSelector("img.img-responsive.col-sm-10"), wait);
                eventData.ImageUrlDetail = ElementExtractor.ExtractAttribute(imageElementDetailObject, "src");
                IWebElement descriptionElementObject = ElementExtractor.FindElement(null, By.CssSelector("p"), wait);
                eventData.Description = ElementExtractor.ExtractText(descriptionElementObject);
                IWebElement dateElementObject = ElementExtractor.FindElement(null, By.CssSelector("strong"), wait);
                eventData.Date = ElementExtractor.ExtractText(dateElementObject);
                IWebElement placeElementObject = ElementExtractor.FindElement(null, By.CssSelector("h4.text-uppercase"), wait);
                eventData.Place = ElementExtractor.ExtractText(placeElementObject);

                IReadOnlyCollection<IWebElement> locationDivs = ElementExtractor.FindElements(By.CssSelector("div.text-black"), wait);
                StringBuilder concatenatedText = new StringBuilder();
                foreach (IWebElement locationDiv in locationDivs)
                {
                    concatenatedText.Append(locationDiv.Text);
                    concatenatedText.Append(" "); // Add a space between each div for readability
                }
                eventData.FinallocationDiv = concatenatedText.ToString().Trim();

                IWebElement buyElementObject = ElementExtractor.FindElementWithoutWait(null, By.CssSelector("a.btn.btn-default.show-btn.show-buy.animate"));
                eventData.BuyUrl = ElementExtractor.ExtractAttribute(buyElementObject, "href");
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Error al esperar la pagina de detalle: {ex.Message}");
                NavigationHelper.CloseCurrentTabAndSwitchBack(driver, originalWindow);
                return null;
            }
            NavigationHelper.CloseCurrentTabAndSwitchBack(driver, originalWindow);
            return eventData;
        }

        private void OpenNewTabAndNavigate(IWebDriver driver, string url)
        {
            string originalWindow = driver.CurrentWindowHandle;
            driver.SwitchTo().NewWindow(WindowType.Tab);
            driver.Navigate().GoToUrl(url);
        }
        private void CloseCurrentTabAndSwitchBack(IWebDriver driver, string originalWindow)
        {
            driver.Close();
            driver.SwitchTo().Window(originalWindow);
        }
    }
}
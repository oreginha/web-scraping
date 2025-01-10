using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace EventScraperBackend.Core
{
    public static class ElementExtractor
    {

        public static string ExtractAttribute(IWebElement element, string attributeName)
        {
            if (element == null) { return null; }
            try
            {
                return element.GetAttribute(attributeName);
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Error al obtener el atributo '{attributeName}': {ex.Message}");
                return null;
            }

        }
        public static string ExtractText(IWebElement element)
        {
            if (element == null) { return null; }
            try
            {
                return element.Text;
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Error al obtener el texto: {ex.Message}");
                return null;
            }
        }
        public static IWebElement FindElement(IWebElement container, By by, WebDriverWait wait)
        {
            if (container == null) { return null; }

            try
            {
                return wait.Until(ExpectedConditions.ElementIsVisible(by));
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Error al obtener el elemento: {ex.Message}");
                return null;
            }
        }
        public static IWebElement FindElementWithoutWait(IWebElement container, By by)
        {
            if (container == null) { return null; }
            try
            {
                return container.FindElement(by);
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Error al obtener el elemento sin espera: {ex.Message}");
                return null;
            }

        }
        public static IReadOnlyCollection<IWebElement> FindElements(By by, WebDriverWait wait)
        {
            try
            {
                return wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(by));
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine($"Error al obtener los elementos: {ex.Message}");
                return null;
            }
        }

    }
}
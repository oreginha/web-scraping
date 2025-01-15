using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // Add this using directive

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
        public static string ExtractAttribute(HtmlNode element, string attributeName)
        {

            if (element == null) { return null; }
            try
            {
            if (attributeName.StartsWith(".")) { attributeName = attributeName.Substring(1); }
                return element.GetAttributeValue(attributeName, null);
            }
            catch (Exception ex)
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
        public static string ExtractText(HtmlNode element)
        {
            if (element == null) { return null; }
            try
            {
                return element.InnerText;
            }
            catch (Exception ex)
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
        public static HtmlNode FindElement(string html, string cssSelector)
        {

            if (cssSelector.Contains(" "))
            {
                cssSelector = cssSelector.Split(' ')[0];
            }
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(cssSelector))
            {
                return null;
            }
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            try
            {
                var respuesta = doc.DocumentNode.QuerySelector(cssSelector);
                return respuesta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el elemento (html) : {ex.Message}");
                return null;
            }
        }

        public static string FindElementByAtribute(string html, string atribute)
        {

            if (atribute.Contains(" "))
            {
                atribute = atribute.Split(' ')[0];
            }
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(atribute))
            {
                return null;
            }
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            try
            {
                var nodo = doc.DocumentNode.OuterHtml;
                var respuesta = nodo.Substring(nodo.IndexOf(atribute));

                return respuesta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el elemento (html) : {ex.Message}");
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

        public static HtmlNode FindElementNode(string html, string cssSelector)
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(cssSelector))
            {
                return null;
            }
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            try
            {
                return doc.DocumentNode.SelectSingleNode(cssSelector);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el elemento (html) : {ex.Message}");
                return null;
            }
        }

        public static List<HtmlNode> FindElementsFromHtml(string html, string cssSelector)
        {
            if (cssSelector.StartsWith("row"))
            {
                cssSelector = cssSelector.Substring(3);
            }
            if (cssSelector.StartsWith("."))
            {
                cssSelector = cssSelector.Substring(1);
            }
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(cssSelector))
            {
                return new List<HtmlNode>();
            }
            // Validacion del selector
            HashSet<string> InvalidSelectors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ":nth-child", ":nth-of-type"
            };

            foreach (var selector in InvalidSelectors)
            {
                if (cssSelector.Contains(selector))
                {
                    Console.WriteLine($"Selector {cssSelector} no es valido");
                    return new List<HtmlNode>();
                }
            }
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            try
            {
               
                
                    return doc.DocumentNode.QuerySelectorAll( "div."+ cssSelector).ToList();
                
              
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener los elementos desde html : {ex.Message}");
                return new List<HtmlNode>();
            }
        }
    }
}

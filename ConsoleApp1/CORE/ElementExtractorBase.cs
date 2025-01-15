namespace EventScraperBackend.Core
{
    public static class ElementExtractorBase
    {
        public static string ExtractText(HtmlAgilityPack.HtmlNode element)
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
    }
}
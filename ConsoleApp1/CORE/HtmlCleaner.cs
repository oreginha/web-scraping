using System.Text.RegularExpressions;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace EventScraperBackend.Core
{
    public static class HtmlCleaner
    {
        public static string CleanHtml(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // Seleccionar solo los elementos dentro de la sección principal
            var mainSection = doc.DocumentNode.SelectSingleNode("//main[@class='row']");

            if (mainSection == null)
            {
                return string.Empty;
            }
            var allNodes = mainSection.Descendants().ToList(); // Materializar la lista de descendientes

            // Eliminar atributos
            foreach (var node in allNodes)
            {
                if (node.HasAttributes)
                {
                    node.Attributes.RemoveAll();
                }
            }
            // Eliminar scripts, estilos, metas, links, etc.
            var nodesToRemove = allNodes
                           .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "link" || n.Name == "meta" || n.Name == "noscript" || (n.NodeType == HtmlNodeType.Comment))
                            .ToList(); // Materializar la lista de nodos a remover
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
            return mainSection.InnerHtml;
        }
    }
}
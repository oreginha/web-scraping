using System.Text.RegularExpressions;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace EventScraperBackend.Core
{
    public static class HtmlCleaner
    {
        private static readonly HashSet<string> AttributesToKeep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "src", "alt", "href", "title", "class"
        };
        private static readonly HashSet<string> DataAttributesToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
             "data-tkt-menu-item", "data-type", "data-icon",  "data-swap-target", "data-swap-linked", "data-swap-group"
        };
        private static readonly HashSet<string> ClassesToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ng-hide","ng-show"
        };


        public static List<string> CleanHtml(string html)
        {
            var section1 = "";
            var section2 = "";
            List<string> result = new List<string>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // Seleccionar solo los elementos dentro de la sección principal
            var mainSection = doc.DocumentNode.SelectSingleNode("//main[@class='row']");
            var menuSection = doc.DocumentNode.SelectSingleNode("//div[@id='main-nav']");

            if (mainSection == null && menuSection == null)
            {
                return result;
            }

            //string combinedHtml = "";
            if (mainSection != null)
            {
                var allNodesMain = mainSection.Descendants().ToList();
                foreach (var node in allNodesMain)
                {
                    if (node.HasAttributes)
                    {
                        var attributesToRemove = node.Attributes
                                .Where(attr => !AttributesToKeep.Contains(attr.Name))
                                .ToList();
                        foreach (var attr in attributesToRemove)
                        {
                            node.Attributes.Remove(attr);
                        }
                        foreach (var attrName in DataAttributesToRemove)
                        {
                            node.Attributes.Remove(attrName);
                        }
                        foreach (var attr in node.Attributes.Where(attr => ClassesToRemove.Contains(attr.Value)).ToList())
                        {
                            node.Attributes.Remove(attr);
                        }
                        foreach (var attr in node.Attributes.Where(attr => attr.Name.StartsWith("ng-")).ToList())
                        {
                            node.Attributes.Remove(attr);
                        }


                    }
                    if (node.HasClass("modal") || node.XPath.Contains("modal"))
                    {
                        node.Remove();
                    }
                }

                var nodesToRemoveMain = allNodesMain
                            .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "link" || n.Name == "meta" || n.Name == "noscript" || (n.NodeType == HtmlNodeType.Comment))
                           .ToList();
                foreach (var node in nodesToRemoveMain)
                {
                    node.Remove();
                }
               section1= mainSection.InnerHtml;

            }
            if (menuSection != null)
            {
                var allNodesMenu = menuSection.Descendants().ToList();
                foreach (var node in allNodesMenu)
                {
                    if (node.HasAttributes)
                    {
                        var attributesToRemove = node.Attributes
                              .Where(attr => !AttributesToKeep.Contains(attr.Name))
                               .ToList();
                        foreach (var attr in attributesToRemove)
                        {
                            node.Attributes.Remove(attr);
                        }
                        foreach (var attrName in DataAttributesToRemove)
                        {
                            node.Attributes.Remove(attrName);
                        }
                        foreach (var attr in node.Attributes.Where(attr => ClassesToRemove.Contains(attr.Value)).ToList())
                        {
                            node.Attributes.Remove(attr);
                        }
                        foreach (var attr in node.Attributes.Where(attr => attr.Name.StartsWith("ng-")).ToList())
                        {
                            node.Attributes.Remove(attr);
                        }
                    }
                    if (node.HasClass("modal") || node.XPath.Contains("modal"))
                    {
                        node.Remove();
                    }
                }
                var nodesToRemoveMenu = allNodesMenu
                             .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "link" || n.Name == "meta" || n.Name == "noscript" || (n.NodeType == HtmlNodeType.Comment))
                              .ToList();
                foreach (var node in nodesToRemoveMenu)
                {
                    node.Remove();
                }

                section1 = menuSection.InnerHtml;

            }

            //Eliminar lineas repetidas
            string[] lines1 = section1.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string[] lines2 = section2.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            List<string> lines = new List<string>();
            lines.AddRange(lines1);
            lines.AddRange(lines2);
            return lines;

        }
    }
}
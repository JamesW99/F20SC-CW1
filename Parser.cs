using System;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace MyBrowser
{
    class Parser
    {
        HtmlDocument Doc;
        Uri BaseUri;

        public Parser(Uri BaseUri, string HTML)
        {
            Doc = new HtmlDocument();
            Doc.LoadHtml(HTML);
            this.BaseUri = BaseUri;

            // List<Uri> Resources = new List<Uri>();
            // findResources(Resources);
            // foreach (Uri Uri in Resources)
            // {
            //     Console.WriteLine(Uri);
            // }
        }

        public string getTitle()
        {
            HtmlNode TitleNode = Doc.DocumentNode.SelectSingleNode("//title");
            return TitleNode.GetDirectInnerText().Trim();
        }

        bool getUriForNode(HtmlNode Node, string Attribute, out Uri OutUri)
        {
            try
            {
                string Value = Node.Attributes[Attribute].Value;
                if (Uri.TryCreate(BaseUri, Value, out OutUri))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                OutUri = new Uri("");
            }
            return false;
        }

        public void findResources(List<Uri> Result, string XPath, string Attribute)
        {
            foreach (HtmlNode Node in Doc.DocumentNode.SelectNodes(XPath))
            {
                Uri Out;
                if (getUriForNode(Node, Attribute, out Out))
                {
                    Result.Add(Out);
                }
            }
        }

        public void findResources(List<Uri> Result)
        {
            findResources(Result, "//a", "href");
            findResources(Result, "//img", "src");
            findResources(Result, "//link", "href");
            findResources(Result, "//script", "src");
        }

        public List<KeyValuePair<String, Uri>> findLinks()
        {
            List<KeyValuePair<String, Uri>> Result = new List<KeyValuePair<string, Uri>>();
            foreach (HtmlNode Node in Doc.DocumentNode.SelectNodes("//a"))
            {
                Uri Out;
                if (!getUriForNode(Node, "href", out Out))
                    continue;

                StringBuilder Writer = new StringBuilder();
                summarize(Node, Writer);
                string Title = Writer.ToString().Replace("\n", " ").Replace("  ", " ").Trim();
                Result.Add(new KeyValuePair<string, Uri>(Title, Out));
            }
            return Result;
        }

        void summarize(HtmlNode Node, StringBuilder Writer)
        {
            if (Node.NodeType == HtmlNodeType.Text)
            {
                string ParentTag = Node.ParentNode.Name.ToLower().Trim();
                if (ParentTag != "script" && ParentTag != "svg" && ParentTag != "style")
                {
                    string ToWrite = Node.InnerText.Trim();
                    if (ToWrite.Length != 0)
                    {
                        Writer.Append(ToWrite);
                        Writer.Append(" ");
                    }
                }

                if (ParentTag == "p" || ParentTag == "div" || ParentTag == "ul" || ParentTag == "li")
                {
                    if (Writer.Length == 0 || Writer[Writer.Length - 1] != '\n')
                        Writer.Append('\n');
                }
            }

            foreach (HtmlNode Child in Node.ChildNodes)
            {
                summarize(Child, Writer);
            }
        }

        public string getTextSummary()
        {
            StringBuilder Writer = new StringBuilder();
            summarize(Doc.DocumentNode, Writer);
            return Writer.ToString();
        }
    }
}
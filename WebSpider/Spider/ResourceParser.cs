namespace myLegis.Spider
{
    #region Using directives.
    // ----------------------------------------------------------------------

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    using Sgml;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Linq;

    // ----------------------------------------------------------------------
    #endregion

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Parses a single HTML resource for links.
    /// </summary>
    internal class ResourceParser
    {
        #region Public methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="uriInfo">The URI info.</param>
        /// <param name="textContent">Content of the text.</param>
        public ResourceParser(
            SpiderSettings settings,
            UriResourceInformation uriInfo,
            string textContent)
        {
            _settings = settings;
            _uriInfo = uriInfo;
            _textContent = textContent;
        }

        /// <summary>
        /// Get all links from the text content.
        /// </summary>
        /// <returns></returns>
        public List<UriResourceInformation> ExtractLinks()
        {
            if (string.IsNullOrEmpty(_textContent))
            {
                return new List<UriResourceInformation>();
            }
            else
            {
                //Xml reader for html contentn
                XmlReader xml = GetDocReader(_textContent, _uriInfo.BaseUri);

                //Parse out all the links.
                List<UriResourceInformation> result = DoExtractLinks(xml, _uriInfo);

                int index = 0;
                // Write out the extracted links.
                foreach (UriResourceInformation information in result)
                {
                    Console.WriteLine(
                        string.Format(
                            @"Extracted {0} {1}/{2}: Found '{3}' in document at URI '{4}'.",
                            information.LinkType,
                            index + 1,
                            result.Count,
                            information.AbsoluteUri.AbsoluteUri,
                            _uriInfo.AbsoluteUri.AbsoluteUri));

                    index++;
                }

                return result;

            }
        }

        /// <summary>
        /// Get all links from the text content.
        /// </summary>
        /// <returns></returns>
        public List<iCollector> ExtractCollectorRequest(List<iCollector> req)
        {
            if (string.IsNullOrEmpty(_textContent))
            {
                return new List<iCollector>();
            }
            else
            {

                //Xml reader for html contentn
                XmlReader xml = GetDocReader(_textContent, _uriInfo.BaseUri);

                //Parse out all the links.
                List<iCollector> result = DoExtractCollector(xml, _uriInfo, req);

                return result;

            }
        }

        /// <summary>
        /// Do the Collector extraction.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="_uriInfo"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private List<iCollector> DoExtractCollector(XmlReader xml, UriResourceInformation _uriInfo, List<iCollector> req)
        {

            //Process the iCollector Collect method, on all requests.
            foreach (iCollector col in req)
                col.Collect(xml, _uriInfo);

            return req.ToList();
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Does the extract links.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <param name="uriInfo">The URI info.</param>
        /// <returns></returns>
        private List<UriResourceInformation> DoExtractLinks(
            XmlReader xml,
            UriResourceInformation uriInfo)
        {

            //Resulting resource list.
            List<UriResourceInformation> links = new List<UriResourceInformation>();
            //Loop through the HTML doc as Xml.
            while (xml.Read())
            {

                //Do something based on the element type.
                switch (xml.NodeType)
                {
                    //Grab inside comments, too.
                    case XmlNodeType.Comment:
                        XmlReader childXml =
                            GetDocReader(xml.Value, uriInfo.BaseUri);

                        //Grab links inside the comments
                        List<UriResourceInformation> childLinks =
                            DoExtractLinks(childXml, uriInfo);
                        links.AddRange(childLinks);
                        break;

                    // An HTML node element.
                    case XmlNodeType.Element:

                        //Temp link attributes holder.
                        string[] linkAttributeNames;
                        //Link types.
                        UriType linkType;

                        // If this is a link element(A, FORM, APPLET, REL), proceed to store the URLs to modify.
                        if (IsLinkElement(
                            xml.Name,
                            out linkAttributeNames,
                            out linkType))
                        {

                            //Loop through all the elements in the element.
                            while (xml.MoveToNextAttribute())
                            {
                                //Loop through each attribute of this (A, FORM, APPLET, REL) element.
                                foreach (string a in linkAttributeNames)
                                {
                                    //If the resource attribute matches, then add it.
                                    if (string.Compare(a, xml.Name, true) == 0)
                                    {

                                        string url = xml.Value;

                                        if (xml.Value.Contains("get_bill_text.asp"))
                                        {
                                            string sdfa = "Stop";
                                        }

                                        if (xml.Value.Contains("get_fulltext.asp"))
                                        {
                                            string adf = @"Stop";
                                        }

                                        //Save the Resource information
                                        UriResourceInformation ui = null;

                                        //Flag resource as a form.
                                        if (xml.Name == @"action")
                                            linkType = UriType.Form;

                                        //Create link
                                        ui = new UriResourceInformation(
                                            _settings.Options,
                                            url,
                                            new Uri(url, UriKind.RelativeOrAbsolute),
                                            uriInfo.BaseUriWithFolder,
                                            linkType,
                                            uriInfo.AbsoluteUri,
                                            uriInfo.Index);

                                        //Is in same domain
                                        bool isOnSameSite =
                                            ui.IsOnSameSite(uriInfo.BaseUri);

                                        //Stay on Site, and is processable.
                                        if ((isOnSameSite ||
                                            !_settings.Options.StayOnSite) &&
                                            ui.IsProcessableUri)
                                        {

                                            //Check to see if the link points to current session of legis
                                            if (ui.OriginalUrl.Contains(String.Format("session={0}",
                                                _settings.Options.TargetSession)))
                                            {
                                                //Add the resource.
                                                links.Add(ui);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Also, look for style attributes.
                            //while (xml.MoveToNextAttribute())
                            //{
                            //    links.AddRange(
                            //        ExtractStyleUrls(
                            //        uriInfo.BaseUriWithFolder,
                            //        xml.Name,
                            //        xml.Value));
                            //}
                        }
                        break;
                }
            }

            if (links.ToArray().Length > 0)
            {
                string stp = @"stop";
            }

            return links;
        }

        /// <summary>
        /// Checks whether the given name is a HTML element (=tag) with
        /// a contained link. If true, linkAttributeNames contains a list
        /// of all attributes that are links.
        /// </summary>
        /// <returns>Returns true, if it is a link element,
        /// false otherwise.</returns>
        private static bool IsLinkElement(
            string name,
            out string[] linkAttributeNames,
            out UriType linkType)
        {
            foreach (LinkElement e in LinkElement.LinkElements)
            {
                if (string.Compare(name, e.Name, true) == 0)
                {
                    linkAttributeNames = e.Attributes;
                    linkType = e.LinkType;
                    return true;
                }
            }

            linkAttributeNames = null;
            linkType = UriType.Resource;
            return false;
        }

        /// <summary>
        /// Detects URLs in styles.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        //private List<UriResourceInformation> ExtractStyleUrls(
        //    Uri baseUri,
        //    string attributeName,
        //    string attributeValue)
        //{
        //    List<UriResourceInformation> result =
        //        new List<UriResourceInformation>();

        //    if (string.Compare(attributeName, @"style", true) == 0)
        //    {
        //        if (attributeValue != null &&
        //            attributeValue.Trim().Length > 0)
        //        {
        //            MatchCollection matchs = Regex.Matches(
        //                attributeValue,
        //                @"url\s*\(\s*([^\)\s]+)\s*\)",
        //                RegexOptions.Singleline | RegexOptions.IgnoreCase);

        //            if (matchs.Count > 0)
        //            {
        //                foreach (Match match in matchs)
        //                {
        //                    if (match != null && match.Success)
        //                    {
        //                        string url = match.Groups[1].Value;

        //                        UriResourceInformation ui =
        //                            new UriResourceInformation(
        //                            _settings.Options,
        //                            url,
        //                            new Uri(url, UriKind.RelativeOrAbsolute),
        //                            baseUri,
        //                            UriType.Resource,
        //                            _uriInfo.AbsoluteUri,
        //                            );

        //                        bool isOnSameSite =
        //                            ui.IsOnSameSite(baseUri);

        //                        if ((isOnSameSite ||
        //                            !_settings.Options.StayOnSite) &&
        //                            ui.IsProcessableUri)
        //                        {
        //                            result.Add(ui);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        /// <summary>
        /// Gets the doc reader.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        private static XmlReader GetDocReader(
            string html,
            Uri baseUri)
        {

            SgmlReader r = new SgmlReader();

            if (baseUri != null &&
                !string.IsNullOrEmpty(baseUri.ToString()))
                r.SetBaseUri(baseUri.ToString());
            r.DocType = @"HTML";
            r.WhitespaceHandling = WhitespaceHandling.All;
            r.CaseFolding = CaseFolding.None;
            StringReader sr = new StringReader(html);
            r.InputStream = sr;
            r.Read();

            return r;

        }

        // ------------------------------------------------------------------
        #endregion

        #region Private variables.
        // ------------------------------------------------------------------

        private readonly SpiderSettings _settings;
        private readonly UriResourceInformation _uriInfo;
        private readonly string _textContent;

        // ------------------------------------------------------------------
        #endregion

    }

    /////////////////////////////////////////////////////////////////////////
}
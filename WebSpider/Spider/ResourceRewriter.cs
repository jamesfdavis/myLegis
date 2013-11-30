namespace myLegis.Spider
{
    #region Using directives.
    // ----------------------------------------------------------------------

    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    // ----------------------------------------------------------------------
    #endregion

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Replace URIs inside a given HTML document that was previously 
    /// downloaded with the local URIs.
    /// </summary>
    internal sealed class ResourceRewriter
    {
        #region Public methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public ResourceRewriter(
            SpiderSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Replace URIs inside a given HTML document that was previously 
        /// downloaded with the local URIs.
        /// </summary>
        /// <returns>Returns the content text with the replaced links.</returns>
        public string ReplaceLinks(
            string textContent,
            UriResourceInformation uriInfo)
        {
            ResourceParser parser = new ResourceParser(
                _settings,
                uriInfo,
                textContent);

            List<UriResourceInformation> linkInfos =
                parser.ExtractLinks();

            // For remembering duplicates.
            Dictionary<string, string> replacedLinks =
                new Dictionary<string, string>();

            foreach (UriResourceInformation linkInfo in linkInfos)
            {
                if (linkInfo.WantFollowUri || linkInfo.IsResourceUri)
                {
                    DownloadedResourceInformation dlInfo =
                        new DownloadedResourceInformation(
                        linkInfo,
                        _settings.Options.DestinationFolderPath);

                    if (!string.IsNullOrEmpty(linkInfo.OriginalUrl))
                    {

                        string textContentBefore = textContent;

                        string link = Regex.Escape(linkInfo.OriginalUrl);

                        textContent = Regex.Replace(
                            textContent,
                            string.Format(@"""{0}""", link),
                            string.Format(@"""{0}""", dlInfo.LocalFileName),
                            RegexOptions.IgnoreCase | RegexOptions.Multiline);

                        textContent = Regex.Replace(
                            textContent,
                            string.Format(@"'{0}'", link),
                            string.Format(@"'{0}'", dlInfo.LocalFileName),
                            RegexOptions.IgnoreCase | RegexOptions.Multiline);

                        // For style-"url(...)"-links.
                        textContent = Regex.Replace(
                            textContent,
                            string.Format(@"\(\s*{0}\s*\)", link),
                            string.Format(@"({0})", dlInfo.LocalFileName),
                            RegexOptions.IgnoreCase | RegexOptions.Multiline);

                        // Some checking.
                        // 2007-07-27, Uwe Keim.
                        if (linkInfo.OriginalUrl != dlInfo.LocalFileName.Name &&
                            textContentBefore == textContent &&
                            !replacedLinks.ContainsKey(linkInfo.AbsoluteUri.AbsolutePath))
                        {
                            throw new ApplicationException(
                                string.Format(
                                    @"Failed to replace URI '{0}' with URI '{1}' in HTML text '{2}'.",
                                    linkInfo.OriginalUrl,
                                    dlInfo.LocalFileName,
                                    textContent));
                        }
                        else
                        {
                            // Remember.
                            replacedLinks[linkInfo.AbsoluteUri.AbsolutePath] =
                                linkInfo.AbsoluteUri.AbsolutePath;
                        }
                    }
                    //					*/
                }
            }

            // --

            return textContent;
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private variables.
        // ------------------------------------------------------------------

        private readonly SpiderSettings _settings;

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////
}
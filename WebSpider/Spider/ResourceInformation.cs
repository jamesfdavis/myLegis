namespace myLegis.Spider
{
    #region Using directives.
    // ----------------------------------------------------------------------

    using System;
    using System.Text.RegularExpressions;
    using System.Diagnostics;
    using System.Text;

    // ----------------------------------------------------------------------
    #endregion

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Information about a downloaded resource.
    /// </summary>
    [Serializable]
    [DebuggerDisplay(@"URI = {_absoluteUri}")]
    public class UriResourceInformation
    {
        #region Public methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="UriResourceInformation"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="originalUrl">The original URL.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="linkType">Type of the link.</param>
        public UriResourceInformation(
            WebSiteDownloaderOptions options,
            string originalUrl,
            Uri uri,
            Uri baseUri,
            UriType linkType,
            Uri parentUri,
            Int32 index)
        {
            _options = options;
            _originalUrl = originalUrl;
            _baseUri = baseUri;
            _parentUri = parentUri;
            _index = index;

            uri = new Uri(CleanupUrl(uri.OriginalString), UriKind.RelativeOrAbsolute);

            if (Uri.IsWellFormedUriString(uri.OriginalString, UriKind.Absolute))
            {
                _absoluteUri = uri;
                _relativeUri = null;
            }
            else if (Uri.IsWellFormedUriString(uri.OriginalString, UriKind.Relative))
            {
                _absoluteUri = MakeAbsoluteUri(baseUri, uri);
                _relativeUri = uri;
            }
            else
            {
                if (originalUrl.StartsWith(@"#"))
                {
                    _absoluteUri = null;
                    _relativeUri = new Uri(originalUrl, UriKind.RelativeOrAbsolute);
                }
                else
                {
                    _absoluteUri = MakeAbsoluteUri(baseUri, uri);
                    _relativeUri = uri;
                }
            }

            _linkType = linkType;
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="UriResourceInformation"/> class.
        /// </summary>
        /// <param name="copyFrom">The copy from.</param>
        public UriResourceInformation(
            UriResourceInformation copyFrom)
        {
            _options = copyFrom._options;
            _originalUrl = copyFrom._originalUrl;
            _relativeUri = copyFrom._relativeUri;
            _baseUri = copyFrom._baseUri;
            _absoluteUri = copyFrom._absoluteUri;
            _linkType = copyFrom._linkType;
            _parentUri = copyFrom._parentUri;
        }

        /// <summary>
        /// Determines whether [is on same site] [the specified URI].
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// 	<c>true</c> if [is on same site] [the specified URI]; otherwise, 
        /// <c>false</c>.
        /// </returns>
        public bool IsOnSameSite(
            Uri uri)
        {
            if (string.Compare(_absoluteUri.Host, uri.Host, true) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // ------------------------------------------------------------------
        #endregion

        #region Public properties.
        // ------------------------------------------------------------------

        /// <summary>
        /// Gets the original URL.
        /// </summary>
        /// <value>The original URL.</value>
        public string OriginalUrl
        {
            get
            {
                return _originalUrl;
            }
        }

        /// <summary>
        /// Gets the relative URI.
        /// </summary>
        /// <value>The relative URI.</value>
        public Uri RelativeUri
        {
            get
            {
                return _relativeUri;
            }
        }

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <value>The base URI.</value>
        public Uri BaseUri
        {
            get
            {
                return _baseUri;
            }
        }

        /// <summary>
        /// Gets the base URI with folder.
        /// </summary>
        /// <value>The base URI with folder.</value>
        public Uri BaseUriWithFolder
        {
            get
            {
                if (_absoluteUri.AbsoluteUri.Contains(@"/"))
                {

                    StringBuilder sb = new StringBuilder();

                    if (_absoluteUri.AbsoluteUri.ToString().Contains("?"))
                        sb.Append(_absoluteUri.AbsoluteUri.Substring(0, _absoluteUri.AbsoluteUri.LastIndexOf('?')));

                    sb.Remove(sb.ToString().LastIndexOf("/"), (sb.Length - sb.ToString().LastIndexOf("/")));

                    //string full =
                    //    _absoluteUri.AbsoluteUri.Substring(0,
                    //    _absoluteUri.AbsoluteUri.LastIndexOf('/'));

                    return new Uri(sb.ToString(), UriKind.RelativeOrAbsolute);
                }
                else
                {
                    return _baseUri;
                }
            }
        }

        /// <summary>
        /// Gets the absolute URI.
        /// </summary>
        /// <value>The absolute URI.</value>
        public Uri AbsoluteUri
        {
            get
            {
                return _absoluteUri;
            }
        }

        /// <summary>
        /// Gets the parent absolute URI.
        /// </summary>
        /// <value>The absolute URI.</value>
        public Uri Parent
        {
            get
            {
                return _parentUri;
            }
        }

        /// <summary>
        /// Gets the index of the URI.
        /// </summary>
        /// <value>The absolute URI.</value>
        public Int32 Index
        {
            get
            {
                return _index;
            }
        }

        /// <summary>
        /// Gets the type of the link.
        /// </summary>
        /// <value>The type of the link.</value>
        public UriType LinkType
        {
            get
            {
                if (_calculatedLinkType == null)
                {
                    _calculatedLinkType =
                        CheckVerifyLinkType(_absoluteUri, _linkType);
                }

                return _calculatedLinkType.Value;
            }
        }

        /// <summary>
        /// Decides whether to follow an URI.
        /// </summary>
        /// <value><c>true</c> if [want follow URI]; otherwise, <c>false</c>.</value>
        public bool WantFollowUri
        {
            get
            {
                return DoWantFollowUri(
                    _absoluteUri,
                    _linkType);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is resource URI.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is resource URI; otherwise, <c>false</c>.
        /// </value>
        public bool IsResourceUri
        {
            get
            {
                return DoIsResourceUri(_absoluteUri);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is processable URI.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is processable URI; otherwise, <c>false</c>.
        /// </value>
        public bool IsProcessableUri
        {
            get
            {
                return
                    !_originalUrl.StartsWith(@"#") &&
                    DoIsProcessableUri(_absoluteUri, _linkType);
            }
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Checks the type of the verify link.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="linkType">Type of the link.</param>
        /// <returns></returns>
        private UriType CheckVerifyLinkType(
            Uri absoluteUri,
            UriType linkType)
        {
            if (linkType == UriType.Resource)
            {
                // The original.
                return linkType;
            }
            else if (linkType == UriType.Form) //Return the form type JD - 2/3/13
                return linkType;
            else
            {
                if (DoIsProcessableUri(absoluteUri, linkType))
                {
                    // Ensure PDFs don't get parsed.
                    string head = ResourceDownloader.DownloadHead(
                        absoluteUri,
                        _options);

                    if (string.IsNullOrEmpty(head))
                    {
                        return UriType.Resource;
                    }
                    else
                    {
                        head = head.ToLowerInvariant();

                        if (head.Contains(@"pdf") ||
                            head.Contains(@"application") ||
                            head.Contains(@"image"))
                        {
                            return UriType.Resource;
                        }
                        else
                        {
                            Debug.Assert(
                                head.Contains(@"text"),
                                @"no text document type but marked as content.");

                            // The original.
                            return linkType;
                        }
                    }
                }
                else
                {
                    return UriType.Resource;
                }
            }
        }

        /// <summary>
        /// Makes the absolute URI.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        private static Uri MakeAbsoluteUri(
            Uri baseUri,
            Uri uri)
        {
            if (Uri.IsWellFormedUriString(
                uri.OriginalString,
                UriKind.Absolute))
            {
                return uri;
            }
            else
            {
                if (baseUri == null)
                {
                    return uri;
                }
                else
                {

                    string[] up = uri.OriginalString.Split('?');

                    UriBuilder ub = new UriBuilder(baseUri.AbsoluteUri);

                    ub.Path = CombineVPath(ub.Path, up[0]);
                    if (up.Length > 1)
                        ub.Query = up[1];

                    return ub.Uri;

                }
            }
        }

        /// <summary>
        /// Combines the virtual path.
        /// </summary>
        /// <param name="s1">The s1.</param>
        /// <param name="s2">The s2.</param>
        /// <returns></returns>
        private static string CombineVPath(
            string s1,
            string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                return s2;
            }
            else if (string.IsNullOrEmpty(s2))
            {
                return s1;
            }
            else
            {
                if (s2.IndexOf('/') == 0)
                {
                    return s2;
                }
                else
                {
                    s1 = s1.TrimEnd('/');
                    s2 = s2.TrimStart('/');

                    return s1 + @"/" + s2;
                }

            }
        }

        /// <summary>
        /// Cleanups the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static string CleanupUrl(
            string url)
        {
            // Remove accidentially contained ASP-tags.
            url = Regex.Replace(
                url,
                @"<%.*?%>",
                string.Empty,
                RegexOptions.Singleline);

            // Remove anchors.
            url = Regex.Replace(
                url,
                @"#.*?$",
                string.Empty,
                RegexOptions.Singleline);

            return url;
        }

        /// <summary>
        /// Decides whether to follow an URI.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="linkType">Type of the link.</param>
        /// <returns></returns>
        private static bool DoWantFollowUri(
            Uri absoluteUri,
            UriType linkType)
        {
            if (absoluteUri == null ||
                linkType == UriType.Resource)
            {
                return false;
            }
            else
            {
                if (absoluteUri.Scheme == Uri.UriSchemeHttp ||
                    absoluteUri.Scheme == Uri.UriSchemeHttps ||
                    absoluteUri.Scheme == Uri.UriSchemeFile)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Does the is resource URI.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <returns></returns>
        private static bool DoIsResourceUri(
            Uri absoluteUri)
        {
            return
                absoluteUri != null &&
                (
                absoluteUri.Scheme == Uri.UriSchemeHttp ||
                absoluteUri.Scheme == Uri.UriSchemeHttps ||
                absoluteUri.Scheme == Uri.UriSchemeFile);
        }

        /// <summary>
        /// Does the is processable URI.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="linkType">Type of the link.</param>
        /// <returns></returns>
        private static bool DoIsProcessableUri(
            Uri absoluteUri,
            UriType linkType)
        {
            return
                absoluteUri != null &&
                (DoWantFollowUri(absoluteUri, linkType) ||
                DoIsResourceUri(absoluteUri));
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private variables.
        // ------------------------------------------------------------------

        private readonly WebSiteDownloaderOptions _options;
        private readonly string _originalUrl;

        private readonly Uri _relativeUri = null;
        private readonly Uri _baseUri = null;
        private Uri _parentUri = null;
        private Int32 _index;
        private readonly Uri _absoluteUri = null;

        private readonly UriType _linkType = UriType.Content;
        private UriType? _calculatedLinkType = null;

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////
}
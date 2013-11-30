namespace myLegis.Spider
{
    #region Using directives.
    // ----------------------------------------------------------------------

    using System;
    using System.Net;
    using System.IO;
    using System.Configuration;
    using System.Collections.Generic;

    // ----------------------------------------------------------------------
    #endregion

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Configure options for the downloader.
    /// </summary>
    [Serializable]
    public class WebSiteDownloaderOptions
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="WebSiteDownloaderOptions"/> class.
        /// </summary>
        public WebSiteDownloaderOptions()
        {

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[@"maximumLinkDepth"]))
                _maximumLinkDepth = Convert.ToInt32(ConfigurationManager.AppSettings[@"maximumLinkDepth"]);

            //Placeholder containers
            this.GitCollectionRequest = new List<iCollector>();
            this.UriFollower = new List<iFollower>();

        }

        #region Public properties.
        // ------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the download URI.
        /// </summary>
        /// <value>The download URI.</value>
        public Uri DownloadUri
        {
            get
            {
                return _downloadUri;
            }
            set
            {
                _downloadUri = value;
            }
        }

        public int TargetSession { get; set; }

        public List<iCollector> GitCollectionRequest { get; set; }
        public List<iFollower> UriFollower { get; set; }
        
        /// <summary>
        /// Gets or sets the destination folder path.
        /// </summary>
        /// <value>The destination folder path.</value>
        public DirectoryInfo DestinationFolderPath
        {
            get
            {
                if (_destinationFolderPath != null &&
                    !_destinationFolderPath.Exists)
                {
                    _destinationFolderPath.Create();
                }

                return _destinationFolderPath;
            }
            set
            {
                _destinationFolderPath = value;
            }
        }

        /// <summary>
        /// State file name.
        /// </summary>
        public String DestinationFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [stay on site].
        /// </summary>
        /// <value><c>true</c> if [stay on site]; otherwise, <c>false</c>.</value>
        public bool StayOnSite
        {
            get
            {
                return _stayOnSite;
            }
            set
            {
                _stayOnSite = value;
            }
        }

        /// <summary>
        /// Gets or sets the link depth.
        /// </summary>
        /// <value>The link depth.</value>
        public int MaximumLinkDepth
        {
            get
            {
                return _maximumLinkDepth;
            }
            set
            {
                _maximumLinkDepth = value;
            }
        }

        /// <summary>
        /// Whether/how to use the proxy server.
        /// </summary>
        /// <value>The proxy usage.</value>
        public DownloadProxyUsage ProxyUsage
        {
            get
            {
                string proxyUsage =
                    ConfigurationManager.AppSettings[@"downloadProxyUsage"];

                if (string.IsNullOrEmpty(proxyUsage))
                {
                    return DownloadProxyUsage.Default;
                }
                else
                {
                    return (DownloadProxyUsage)Enum.Parse(
                        typeof(DownloadProxyUsage),
                        proxyUsage,
                        true);
                }
            }
        }

        /// <summary>
        /// Returns non-NULL if a proxy is required.
        /// </summary>
        /// <value>The proxy.</value>
        public IWebProxy Proxy
        {
            get
            {
                if (!_proxySet)
                {
                    if (ProxyUsage == DownloadProxyUsage.UseProxy)
                    {
                        WebProxy proxy = new WebProxy();

                        proxy.Address =
                            new Uri(
                            ConfigurationManager.AppSettings[@"downloadProxyAddress"]);
                        proxy.BypassProxyOnLocal =
                            ConvertToBoolean(
                            ConfigurationManager.AppSettings[@"downloadProxyBypassProxyOnLocal"],
                            true);
                        proxy.UseDefaultCredentials =
                            ConvertToBoolean(
                            ConfigurationManager.AppSettings[@"downloadProxyUseDefaultCredentials"],
                            true);

                        if (!proxy.UseDefaultCredentials)
                        {
                            NetworkCredential credentials = new NetworkCredential();

                            credentials.Domain =
                                ConvertToString(
                                ConfigurationManager.AppSettings[@"downloadProxyCredentialsDomain"],
                                string.Empty);
                            credentials.Password =
                                ConvertToString(
                                ConfigurationManager.AppSettings[@"downloadProxyCredentialsPassword"],
                                string.Empty);
                            credentials.UserName =
                                ConvertToString(
                                ConfigurationManager.AppSettings[@"downloadProxyCredentialsUserName"],
                                string.Empty);

                            proxy.Credentials = credentials;
                        }

                        _proxy = proxy;
                    }
                    else
                    {
                        _proxy = null;
                    }

                    _proxySet = true;
                }

                return _proxy;
            }
            set
            {
                _proxy = value;
                _proxySet = true;
            }
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="fallbackTo">The fallback to.</param>
        /// <returns></returns>
        private static string ConvertToString(
            object o,
            string fallbackTo)
        {
            if (o == null)
            {
                return fallbackTo;
            }
            else
            {
                return o.ToString();
            }
        }

        /// <summary>
        /// Converts to boolean.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fallbackTo">if set to <c>true</c> [fallback to].</param>
        /// <returns></returns>
        private static bool ConvertToBoolean(
            object text,
            bool fallbackTo)
        {
            if (text != null && IsBoolean(text))
            {
                return Convert.ToBoolean(text);
            }
            else
            {
                return fallbackTo;
            }
        }

        /// <summary>
        /// Checks whether a string contains a valid boolean.
        /// </summary>
        /// <param name="o">The string to check.</param>
        /// <returns>
        /// Returns TRUE if is a boolean, FALSE if not.
        /// </returns>
        public static bool IsBoolean(
            object o)
        {
            try
            {
                if (o == null || o.ToString().Trim().Length <= 0 ||
                    (
                    o.ToString().Trim().ToLower() != bool.TrueString.ToLower() &&
                    o.ToString().Trim().ToLower() != bool.FalseString.ToLower()
                    ))
                {
                    return false;
                }
                else
                {
                    bool.Parse(o.ToString());
                }
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private variables.
        // ------------------------------------------------------------------

        private Uri _downloadUri;
        private DirectoryInfo _destinationFolderPath;
        private bool _stayOnSite = true;
        private int _maximumLinkDepth;

        [NonSerialized]
        private IWebProxy _proxy;
        [NonSerialized]
        private bool _proxySet;

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////
}
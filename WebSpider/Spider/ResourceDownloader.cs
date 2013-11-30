namespace myLegis.Spider
{
    #region Using directives.
    // ----------------------------------------------------------------------

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Cache;
    using System.Text;
    using System.Net;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Diagnostics;

    // ----------------------------------------------------------------------
    #endregion

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Downloading of a single resource like e.g. a HTML page or an image
    /// or a video.
    /// </summary>
    internal sealed class ResourceDownloader
    {
        #region Public methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Downloads the head.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static string DownloadHead(
            Uri absoluteUri,
            WebSiteDownloaderOptions options)
        {
            try
            {
                if (_headPool.ContainsKey(absoluteUri))
                {
                    return _headPool[absoluteUri];
                }
                else
                {
                    Debug.WriteLine(
                        string.Format(
                        @"Reading HEAD from URL '{0}'.",
                        absoluteUri));

                    HttpWebRequest req =
                        (HttpWebRequest)WebRequest.Create(absoluteUri);
                    req.Method = @"HEAD";
                    ApplyProxy(req, options);

                    RequestCachePolicy cp = new RequestCachePolicy(
                        RequestCacheLevel.BypassCache);
                    req.CachePolicy = cp;

                    using (HttpWebResponse resp =
                        (HttpWebResponse)req.GetResponse())
                    {
                        _headPool[absoluteUri] = resp.ContentType;
                        return resp.ContentType;
                    }
                }
            }
            catch (WebException x)
            {
                if (x.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse resp =
                        (HttpWebResponse)x.Response;

                    if (resp.StatusCode == HttpStatusCode.NotFound ||
                        resp.StatusCode == HttpStatusCode.InternalServerError ||
                        resp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Trace.WriteLine(
                            string.Format(
                            @"Ignoring web exception: '{0}'.",
                            x.Message));
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Download a binary content.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="binaryContent">Content of the binary.</param>
        /// <param name="options">The options.</param>
        public static void DownloadBinary(
            Uri absoluteUri,
            out byte[] binaryContent,
            WebSiteDownloaderOptions options)
        {
            Debug.WriteLine(
                string.Format(
                @"Reading content from URL '{0}'.",
                absoluteUri));

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(absoluteUri);
                ApplyProxy(req, options);

                RequestCachePolicy cp = new RequestCachePolicy(
                    RequestCacheLevel.BypassCache);
                req.CachePolicy = cp;

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (Stream stream = resp.GetResponseStream())
                using (MemoryStream mem = new MemoryStream())
                {
                    int blockSize = 16384;
                    byte[] blockBuffer = new byte[blockSize];
                    int read;

                    while ((read = stream.Read(blockBuffer, 0, blockSize)) > 0)
                    {
                        mem.Write(blockBuffer, 0, read);
                    }

                    mem.Seek(0, SeekOrigin.Begin);

                    binaryContent = mem.GetBuffer();
                }
            }
            catch (WebException x)
            {
                if (x.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse resp =
                        (HttpWebResponse)x.Response;

                    if (resp.StatusCode == HttpStatusCode.NotFound ||
                        resp.StatusCode == HttpStatusCode.InternalServerError ||
                        resp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Trace.WriteLine(
                            string.Format(
                            @"Ignoring web exception: '{0}'.",
                            x.Message));
                        binaryContent = null;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Download a FORM post response content.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="binaryContent">Content of the binary.</param>
        /// <param name="options">The options.</param>
        public static void DownloadPost(
            Uri absoluteUri,
            out byte[] binaryContent,
            WebSiteDownloaderOptions options)
        {
            Debug.WriteLine(
                string.Format(
                @"Reading content from FORM '{0}'.",
                absoluteUri));

            try
            {
                using (WebClient wc = new WebClient())
                {
                    binaryContent = wc.UploadData(absoluteUri, new byte[0]);
                }

                //HttpWebRequest req = (HttpWebRequest)WebRequest.Create(absoluteUri);

                //req.Method = @"POST";
                //req.ContentType = "application/x-www-form-urlencoded";
                //req.ContentLength = 1;

                ////Apply proxy settings.
                //ApplyProxy(req, options);

                //RequestCachePolicy cp = new RequestCachePolicy(
                //    RequestCacheLevel.BypassCache);
                //req.CachePolicy = cp;

                //using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                //using (Stream stream = resp.GetResponseStream())
                //using (MemoryStream mem = new MemoryStream())
                //{
                //    int blockSize = 16384;
                //    byte[] blockBuffer = new byte[blockSize];
                //    int read;

                //    while ((read = stream.Read(blockBuffer, 0, blockSize)) > 0)
                //    {
                //        mem.Write(blockBuffer, 0, read);
                //    }

                //    mem.Seek(0, SeekOrigin.Begin);

                //    binaryContent = mem.GetBuffer();
                //}
            }
            catch (WebException x)
            {
                if (x.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse resp =
                        (HttpWebResponse)x.Response;

                    if (resp.StatusCode == HttpStatusCode.NotFound ||
                        resp.StatusCode == HttpStatusCode.InternalServerError ||
                        resp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine(
                            string.Format(
                            @"Ignoring web exception: '{0}'.",
                            x.Message));
                        binaryContent = null;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Download a HTML page. Returns both the binary content,
        /// as well as the textual representation of the HTML page.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="textContent">Content of the text.</param>
        /// <param name="encodingName">Name of the encoding.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="binaryContent">Content of the binary.</param>
        /// <param name="options">The options.</param>
        public static void DownloadHtml(
            Uri absoluteUri,
            out string textContent,
            out string encodingName,
            out Encoding encoding,
            out byte[] binaryContent,
            WebSiteDownloaderOptions options)
        {
            DownloadBinary(absoluteUri, out binaryContent, options);

            encodingName = DetectEncodingName(binaryContent);

            Debug.WriteLine(
                string.Format(
                @"Detected encoding '{0}' for remote HTML document from URL '{1}'.",
                encodingName,
                absoluteUri));

            if (binaryContent != null && binaryContent.Length > 0)
            {
                encoding = GetEncodingByName(encodingName);
                textContent = encoding.GetString(binaryContent);
            }
            else
            {
                // Default.
                encoding = Encoding.Default;
                textContent = null;
            }
        }

        /// <summary>
        /// Download the form response page. Returns both the binary content,
        /// as well as the textual representation of the response page.
        /// </summary>
        /// <param name="absoluteUri">The absolute URI.</param>
        /// <param name="textContent">Content of the text.</param>
        /// <param name="encodingName">Name of the encoding.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="binaryContent">Content of the binary.</param>
        /// <param name="options">The options.</param>
        public static void DownloadForm(
            Uri absoluteUri,
            out string textContent,
            out string encodingName,
            out Encoding encoding,
            out byte[] binaryContent,
            WebSiteDownloaderOptions options)
        {

            DownloadPost(absoluteUri, out binaryContent, options);
            encodingName = DetectEncodingName(binaryContent);

            Debug.WriteLine(
                string.Format(
                @"Detected encoding '{0}' for remote HTML document from URL '{1}'.",
                encodingName,
                absoluteUri));

            if (binaryContent != null && binaryContent.Length > 0)
            {

                encoding = GetEncodingByName(encodingName);
                textContent = encoding.GetString(binaryContent);

            }
            else
            {
                // Default.
                encoding = Encoding.Default;
                textContent = null;
            }
        }
        // ------------------------------------------------------------------
        #endregion

        #region Private helper.
        // ------------------------------------------------------------------

        /// <summary>
        /// Helper function for safely converting a response stream encoding
        /// to a supported Encoding class.
        /// </summary>
        /// <param name="encodingName">Name of the encoding.</param>
        /// <returns></returns>
        private static Encoding GetEncodingByName(
            string encodingName)
        {
            Encoding encoding = Encoding.Default;

            if (encodingName != null && encodingName.Length > 0)
            {
                try
                {
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch (NotSupportedException x)
                {
                    encoding = Encoding.Default;

                    Trace.WriteLine(
                        string.Format(
                        @"Unsupported encoding: '{0}'. Returning default encoding '{1}'. Exception '{2}'.",
                        encodingName,
                        encoding,
                        x));

                    encoding = Encoding.Default;
                }
            }

            return encoding;
        }

        /// <summary>
        /// Tries to detect the name of the encoding, contained within the
        /// HTML content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private static string DetectEncodingName(
            byte[] content)
        {
            if (content == null || content.Length <= 0)
            {
                return null;
            }
            else
            {
                // Decode with default encoding to detect the .
                string html = Encoding.Default.GetString(content);

                // Find.
                Match match = Regex.Match(
                    html,
                    _htmlContentEncodingPattern,
                    RegexOptions.Singleline |
                    RegexOptions.IgnoreCase);

                if (match == null || !match.Success || match.Groups.Count < 2)
                {
                    return null;
                }
                else
                {
                    return match.Groups[1].Value;
                }
            }
        }

        /// <summary>
        /// If a proxy is required, apply it to the request.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="options">The options.</param>
        private static void ApplyProxy(
            WebRequest req,
            WebSiteDownloaderOptions options)
        {
            switch (options.ProxyUsage)
            {
                default:
                case DownloadProxyUsage.Default:
                    req.Proxy = WebRequest.DefaultWebProxy;
                    break;

                case DownloadProxyUsage.NoProxy:
                    req.Proxy = null;
                    break;

                case DownloadProxyUsage.UseProxy:
                    Debug.Assert(options.Proxy != null);
                    req.Proxy = options.Proxy;
                    break;
            }
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private variables.
        // ------------------------------------------------------------------

        /// <summary>
        /// Caching already downloaded HEADs.
        /// </summary>
        private static readonly Dictionary<Uri, string> _headPool =
            new Dictionary<Uri, string>();

        /// <summary>
        /// &lt;meta http-equiv="Content-Type" content="text/html; charset=utf-8"&gt;.
        /// </summary>
        private static readonly string _htmlContentEncodingPattern =
            "<meta\\s+http-equiv\\s*=\\s*[\"'\\s]?Content-Type\\b.*?charset\\s*=\\s*([^\"'\\s>]*)";

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////
}
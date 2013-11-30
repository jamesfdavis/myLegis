using System.Linq;


namespace myLegis.Spider
{
    #region Using directives.
    // ----------------------------------------------------------------------

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;

    // ----------------------------------------------------------------------
    #endregion

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Downloading a complete website.
    /// </summary>
    public class WebSiteDownloader :
        IDisposable
    {

        /// <summary>
        /// Gets a list list of all Resources (A, FORM, IMG) that 
        /// the spider found.
        /// </summary>
        public List<DownloadedResourceInformation> Resources
        {
            get
            {
                return _settings.Resources;
            }
        }

        public List<iCollector> Parsings
        {
            get
            {
                return _settings.Parsings;
            }
        }

        #region Public methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSiteDownloader"/> 
        /// class.
        /// </summary>
        /// <param name="options">The options.</param>
        public WebSiteDownloader(
            WebSiteDownloaderOptions options)
        {

            Console.WriteLine(
                string.Format(
                    @"Constructing WebSiteDownloader for URI '{0}', destination folder path '{1}'.",
                    options.DownloadUri,
                    options.DestinationFolderPath));

            _settings = SpiderSettings.Restore(options.DestinationFolderPath, options.DestinationFileName);
            _settings.Options = options;

        }

        /// <summary>
        /// Performs the complete downloading (synchronously). 
        /// Does return only when completely finished or when an exception
        /// occured.
        /// </summary>
        public void Process(List<iCollector> git, List<iFollower> folo)
        {

            string baseUrl = _settings.Options.DownloadUri.OriginalString.TrimEnd('/').Split('?')[0];

            if (_settings.Options.DownloadUri.AbsolutePath.IndexOf('/') >= 0 &&
                _settings.Options.DownloadUri.AbsolutePath.Length > 1)
                baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf('/'));

            // The URI that is configured to be the start URI.
            Uri baseUri = new Uri(baseUrl, UriKind.Absolute);

            // Set the initial seed.
            DownloadedResourceInformation seedInfo =
                new DownloadedResourceInformation(
                    _settings.Options,
                    @"/",
                    _settings.Options.DownloadUri,
                    baseUri,
                    _settings.Options.DestinationFolderPath,
                    _settings.Options.DestinationFolderPath,
                    UriType.Content,
                    _settings.Options.DownloadUri,
                    0); //Index zero.

            // Add the first one as the seed.
            if (!_settings.HasContinueDownloadedResourceInfos)
                _settings.AddContinueDownloadedResourceInfos(seedInfo);

            // 2007-07-27, Uwe Keim:
            // Doing a multiple looping, to avoid stack overflows.
            // Since a download-"tree" (i.e. the hierachy of all downloadable
            // pages) can get _very_ deep, process one part at a time only.
            // The state is already persisted, so we need to set up again at
            // the previous position.
            int index = 0;
            while (_settings.HasContinueDownloadedResourceInfos)
            {
                // Fetch one.
                DownloadedResourceInformation processInfo =
                    _settings.PopContinueDownloadedResourceInfos();

                Console.WriteLine(
                    string.Format(
                        @"{0}. loop: Starting processing URLs from '{1}'.",
                        index + 1,
                        processInfo.AbsoluteUri.AbsoluteUri));

                // Process the URI, add any continue URIs to start
                // again, later.
                ProcessUrl(processInfo, 0, git, folo);

                index++;
            }

            Console.WriteLine(
                string.Format(
                    @"{0}. loop: Finished processing URLs from seed '{1}'.",
                    index + 1,
                    _settings.Options.DownloadUri));
        }

        /// <summary>
        /// Performs the complete downloading (asynchronously). 
        /// Return immediately. Calls the ProcessCompleted event
        /// upon completion.
        /// </summary>
        public void ProcessAsync()
        {
            processAsyncBackgroundWorker = new BackgroundWorker();

            processAsyncBackgroundWorker.WorkerSupportsCancellation = true;

            processAsyncBackgroundWorker.DoWork +=
                processAsyncBackgroundWorker_DoWork;
            processAsyncBackgroundWorker.RunWorkerCompleted +=
                processAsyncBackgroundWorker_RunWorkerCompleted;

            // Start.
            processAsyncBackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Cancels a currently running asynchron processing.
        /// </summary>
        public void CancelProcessAsync()
        {
            if (processAsyncBackgroundWorker != null)
            {
                processAsyncBackgroundWorker.CancelAsync();
            }
        }

        // ------------------------------------------------------------------
        #endregion

        #region Public events.
        // ------------------------------------------------------------------

        public class ProcessingUrlEventArgs :
            EventArgs
        {
            #region Public methods.

            /// <summary>
            /// Constructor.
            /// </summary>
            internal ProcessingUrlEventArgs(
                DownloadedResourceInformation uriInfo,
                int depth)
            {
                this.uriInfo = uriInfo;
                this.depth = depth;
            }

            #endregion

            #region Public properties.

            /// <summary>
            /// 
            /// </summary>
            public int Depth
            {
                get
                {
                    return depth;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public DownloadedResourceInformation UriInfo
            {
                get
                {
                    return uriInfo;
                }
            }

            #endregion

            #region Private variables.

            private readonly DownloadedResourceInformation uriInfo;
            private readonly int depth;

            #endregion
        }

        public delegate void ProcessingUrlEventHandler(
            object sender,
            ProcessingUrlEventArgs e);

        /// <summary>
        /// Called when processing an URL.
        /// </summary>
        public event ProcessingUrlEventHandler ProcessingUrl;

        // ------------------------------------------------------------------
        #endregion

        #region Asynchron processing.
        // ------------------------------------------------------------------

        private BackgroundWorker processAsyncBackgroundWorker = null;

        void processAsyncBackgroundWorker_DoWork(
            object sender,
            DoWorkEventArgs e)
        {
            try
            {
                Process(_settings.Options.GitCollectionRequest, _settings.Options.UriFollower);
            }
            catch (StopProcessingException)
            {
                // Do nothing, just end.
            }
        }

        void processAsyncBackgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            if (ProcessCompleted != null)
            {
                ProcessCompleted(this, e);
            }
        }

        public delegate void ProcessCompletedEventHandler(
            object sender,
            RunWorkerCompletedEventArgs e);

        /// <summary>
        /// Called when the asynchron processing is completed.
        /// </summary>
        public event ProcessCompletedEventHandler ProcessCompleted;

        /// <summary>
        /// 
        /// </summary>
        private class StopProcessingException :
            Exception
        {
        }

        // ------------------------------------------------------------------
        #endregion

        #region IDisposable members.
        // ------------------------------------------------------------------

        ~WebSiteDownloader()
        {
            ////settings.Persist();
        }

        public void Dispose()
        {
            ////settings.Persist();
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Process one single URI with a document behind (i.e. no
        /// resource URI).
        /// </summary>
        /// <param name="uriInfo">The URI info.</param>
        /// <param name="depth">The depth.</param>
        private void ProcessUrl(
            DownloadedResourceInformation uriInfo,
            int depth, List<iCollector> git, List<iFollower> folo)
        {
            Console.WriteLine(
                string.Format(
                    @"Processing URI '{0}', with depth {1}.",
                    uriInfo.AbsoluteUri.AbsoluteUri,
                    depth));

            bool blnFollow = true;
            //Check to see if the uriInfo is followable by an internal list
            if (depth > 0)
                blnFollow = ((from f in folo.ToList()
                              let matches = f.pattern.Matches(uriInfo.AbsoluteUri.AbsoluteUri.ToString())
                              where matches.Count > 0 && f.depth == depth
                              select f).Count()) > 0 ? true : false;  //return true if follow exists.

            if (_settings.Options.MaximumLinkDepth > -1 &&
                depth > _settings.Options.MaximumLinkDepth)
            {
                Console.WriteLine(
                    string.Format(
                        @"Depth {1} exceeds maximum configured depth. Ending recursion " +
                            @"at URI '{0}'.",
                        uriInfo.AbsoluteUri.AbsoluteUri,
                        depth));
            }
            else if (!blnFollow)
            {
                Console.WriteLine(
                    string.Format(
                        @"Follower {1} exceeds maximum configured depth. Not following " +
                            @" URI '{0}'.",
                        uriInfo.AbsoluteUri.AbsoluteUri,
                        depth));

                //Fake our way into it.
                //_settings.AddDownloadedResourceInfo(uriInfo);


            }
            else if (depth > _maxDepth)
            {
                Console.WriteLine(
                    string.Format(
                        @"Depth {1} exceeds maximum allowed recursion depth. " +
                            @"Ending recursion at URI '{0}' to possible continue later.",
                        uriInfo.AbsoluteUri.AbsoluteUri,
                        depth));

                // Add myself to start there later.
                // But only if not yet process, otherwise we would never finish.
                if (_settings.HasDownloadedUri(uriInfo))
                {
                    Console.WriteLine(
                        string.Format(
                            @"URI '{0}' was already downloaded. NOT continuing later.",
                            uriInfo.AbsoluteUri.AbsoluteUri));
                }
                else
                {
                    _settings.AddDownloadedResourceInfo(uriInfo);

                    // Finished the function.

                    Console.WriteLine(
                        string.Format(
                            @"Added URI '{0}' to continue later.",
                            uriInfo.AbsoluteUri.AbsoluteUri));
                }
            }
            else
            {
                // If we are in asynchron mode, periodically check for stops.
                if (processAsyncBackgroundWorker != null)
                {
                    if (processAsyncBackgroundWorker.CancellationPending)
                    {
                        throw new StopProcessingException();
                    }
                }

                // --

                // Notify event sinks about this URL.
                if (ProcessingUrl != null)
                {
                    ProcessingUrlEventArgs e = new ProcessingUrlEventArgs(
                        uriInfo,
                        depth);

                    ProcessingUrl(this, e);
                }

                // --

                if (uriInfo.IsProcessableUri)
                {

                    if (_settings.HasDownloadedUri(uriInfo))
                    {
                        Console.WriteLine(
                            string.Format(
                                @"URI '{0}' was already downloaded. Skipping.",
                                uriInfo.AbsoluteUri.AbsoluteUri));
                    }
                    else
                    {


                        Console.WriteLine(
                            string.Format(
                                @"URI '{0}' was not already downloaded. Processing.",
                                uriInfo.AbsoluteUri.AbsoluteUri));

                        //Switch case variables.
                        string textContent;
                        string encodingName;
                        Encoding encoding;
                        byte[] binaryContent;

                        //Local storage.
                        ResourceStorer storer = new ResourceStorer(_settings);
                        ResourceParser parser = null;
                        List<UriResourceInformation> linkInfos = null;
                        List<iCollector> req = null;

                        switch (uriInfo.LinkType)
                        {
                            case UriType.Content:

                                Console.WriteLine(string.Format(@"Processing content URI '{0}', with depth {1}.",
                                                                uriInfo.AbsoluteUri.AbsoluteUri, depth));

                                //Grab the page content.
                                ResourceDownloader.DownloadHtml(
                                    uriInfo.AbsoluteUri,
                                    out textContent,
                                    out encodingName,
                                    out encoding,
                                    out binaryContent,
                                    _settings.Options);

                                //Fire-up resource parser (A, FORMS, IMG) parser.
                                parser = new ResourceParser(
                                    _settings,
                                    uriInfo,
                                    textContent);

                                //Grab all the Git collector requests, that match the Uri.
                                req = (from g in _settings.Options.GitCollectionRequest
                                       where g.pageType == UriType.Content
                                       && uriInfo.AbsoluteUri.AbsoluteUri.ToString().Contains(g.pageName)
                                       && !(from o in _settings.Parsings
                                            where o.pageType == g.pageType
                                            select o.source.AbsoluteUri.AbsoluteUri)
                                           .Contains(uriInfo.AbsoluteUri.AbsoluteUri)
                                       select g.Clone()).ToList();

                                //Have valid requests?
                                if (req.Count() > 0)
                                {
                                    //Persist the collector results.
                                    _settings.PersistCollectorResultInfo(parser.ExtractCollectorRequest(req));
                                }

                                //Process link extraction.
                                linkInfos = parser.ExtractLinks();

                                // Add before parsing childs.
                                _settings.AddDownloadedResourceInfo(uriInfo);

                                foreach (UriResourceInformation linkInfo in linkInfos)
                                {
                                    DownloadedResourceInformation dlInfo =
                                        new DownloadedResourceInformation(
                                            linkInfo,
                                            uriInfo.LocalFolderPath,
                                            uriInfo.LocalBaseFolderPath,
                                            linkInfo.Parent,
                                            depth + 1);

                                    // Recurse.
                                    ProcessUrl(dlInfo, depth + 1, git, folo);

                                    // Do not return or break immediately if too deep, 
                                    // because this would omit certain pages at this
                                    // recursion level.
                                }

                                // Persist after completely parsed childs.
                                _settings.PersistDownloadedResourceInfo(uriInfo);
                                break;
                            case UriType.Resource:

                                //Console.WriteLine(
                                //string.Format(
                                //    @"Processing resource URI '{0}', with depth {1}.",
                                //    uriInfo.AbsoluteUri.AbsoluteUri,
                                //    depth));

                                //Scrape Resource (IMG, JS)
                                //ResourceDownloader.DownloadBinary(
                                //    uriInfo.AbsoluteUri,
                                //    out binaryContent,
                                //    _settings.Options);

                                //storer = new ResourceStorer(_settings);
                                //Save the resource (IMG, JS)
                                //storer.StoreBinary(
                                //    binaryContent,
                                //    uriInfo);

                                //Act like we did it.
                                _settings.AddDownloadedResourceInfo(uriInfo);
                                _settings.PersistDownloadedResourceInfo(uriInfo);

                                break;
                            case UriType.Form:
                                Console.WriteLine(
                                   string.Format(
                                       @"Processing Form POST to URI '{0}', with depth {1}.",
                                       uriInfo.AbsoluteUri.AbsoluteUri,
                                       depth));

                                //Grab the Form response content.
                                ResourceDownloader.DownloadForm(
                                    uriInfo.AbsoluteUri,
                                    out textContent,
                                    out encodingName,
                                    out encoding,
                                    out binaryContent,
                                    _settings.Options);

                                //Fire-up resource parser (A, FORMS, IMG) parser.
                                parser = new ResourceParser(
                                   _settings,
                                   uriInfo,
                                   textContent);

                                var w = _settings.Parsings;

                                //Grab all the Git collector requests, that match the Uri.
                                req = (from g in _settings.Options.GitCollectionRequest
                                       where g.pageType == UriType.Form
                                       && uriInfo.AbsoluteUri.AbsoluteUri.ToString().Contains(g.pageName)
                                       && !(from o in _settings.Parsings
                                            where o.pageType == g.pageType
                                            select o.source.AbsoluteUri.AbsoluteUri)
                                           .Contains(uriInfo.AbsoluteUri.AbsoluteUri)
                                       select g.Clone()).ToList();

                                //Have requests?
                                if (req.Count() > 0)
                                {
                                    //Persist the collector results.
                                    _settings.PersistCollectorResultInfo(parser.ExtractCollectorRequest(req));
                                }

                                //Process link extraction.
                                linkInfos = parser.ExtractLinks();

                                // Add before parsing childs.
                                _settings.AddDownloadedResourceInfo(uriInfo);

                                foreach (UriResourceInformation linkInfo in linkInfos)
                                {
                                    DownloadedResourceInformation dlInfo =
                                        new DownloadedResourceInformation(
                                            linkInfo,
                                            uriInfo.LocalFolderPath,
                                            uriInfo.LocalBaseFolderPath,
                                            linkInfo.Parent,
                                            depth + 1);

                                    // Recurse.
                                    ProcessUrl(dlInfo, depth + 1, git, folo);

                                    // Do not return or break immediately if too deep, 
                                    // because this would omit certain pages at this
                                    // recursion level.
                                }

                                // Persist after completely parsed childs.
                                _settings.PersistDownloadedResourceInfo(uriInfo);

                                break;
                            default:
                                break;
                        }

                        Console.WriteLine(
                            string.Format(
                                @"Finished processing URI '{0}'.",
                                uriInfo.AbsoluteUri.AbsoluteUri));

                    }
                }
                else
                {

                    Console.WriteLine(
                        string.Format(
                            @"URI '{0}' is not processable. Skipping.",
                            uriInfo.AbsoluteUri.AbsoluteUri));

                }
            }
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private variables.
        // ------------------------------------------------------------------

        private const int _maxDepth = 500;
        private readonly SpiderSettings _settings = new SpiderSettings();

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////
}
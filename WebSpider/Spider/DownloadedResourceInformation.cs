namespace myLegis.Spider
{
	#region Using directives.
	// ----------------------------------------------------------------------

	using System;
	using System.IO;
	using System.Diagnostics;

	// ----------------------------------------------------------------------
	#endregion

	/////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Information about a downloaded resource.
	/// Additionally with information about local resources.
	/// </summary>
	[Serializable]
	public sealed class DownloadedResourceInformation :
		UriResourceInformation,
		IEquatable<DownloadedResourceInformation>
	{
		#region Public methods.
		// ------------------------------------------------------------------

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="copyFrom">The copy from.</param>
		public DownloadedResourceInformation(
			DownloadedResourceInformation copyFrom )
			:
			base( copyFrom )
		{
			_localBaseFolderPath = copyFrom._localBaseFolderPath;
			_localFolderPath = copyFrom._localFolderPath;
			_localFilePath = copyFrom._localFilePath;
			_localFileName = copyFrom._localFileName;
            _parent = copyFrom._parent;
            _index = copyFrom._index;
			_addedByProcessID = copyFrom._addedByProcessID;
			_dateAdded = copyFrom._dateAdded;
		}

		/// <summary>
		/// Construcxctor.
		/// </summary>
		/// <param name="copyFrom">The copy from.</param>
		/// <param name="baseFolderPath">The base folder path.</param>
		public DownloadedResourceInformation(
			UriResourceInformation copyFrom,
			DirectoryInfo baseFolderPath )
			:
			base( copyFrom )
		{
			_localBaseFolderPath = baseFolderPath;

			_localFilePath = new FileInfo(
				Path.Combine(
					baseFolderPath.FullName,
					MakeLocalFileName(
						copyFrom.AbsoluteUri,
						copyFrom.BaseUri,
						copyFrom.LinkType ).Name ) );

			_localFileName =
				new FileInfo( _localFilePath.Name );
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="copyFrom">The copy from.</param>
		/// <param name="folderPath">The folder path.</param>
		/// <param name="baseFolderPath">The base folder path.</param>
		public DownloadedResourceInformation(
			UriResourceInformation copyFrom,
			DirectoryInfo folderPath,
			DirectoryInfo baseFolderPath,
            Uri parent,
            Int32 index)
			:
			base( copyFrom )
		{
			_localFolderPath = folderPath;
			_localBaseFolderPath = baseFolderPath;
            _index = index;
            _parent = parent;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="options">The options.</param>
		/// <param name="originalUrl">The original URL.</param>
		/// <param name="uri">The URI.</param>
		/// <param name="baseUri">The base URI.</param>
		/// <param name="folderPath">The folder path.</param>
		/// <param name="baseFolderPath">The base folder path.</param>
		/// <param name="linkType">Type of the link.</param>
		public DownloadedResourceInformation(
			WebSiteDownloaderOptions options,
			string originalUrl,
			Uri uri,
			Uri baseUri,
			DirectoryInfo folderPath,
			DirectoryInfo baseFolderPath,
			UriType linkType,
            Uri parent, 
            Int32 index)
			:
			base( options, originalUrl, uri, baseUri, linkType, parent, index)
		{
			_localFolderPath = folderPath;
			_localBaseFolderPath = baseFolderPath;
            _parent = parent;
            _index = index;
		}

		// ------------------------------------------------------------------
		#endregion

		#region Public properties.
		// ------------------------------------------------------------------

        //public Uri Parent { get { return _parent; } set { _parent = value; } }
        //public Int32 Index { get { return _index; } set { _index = value; } }


		/// <summary>
		/// Gets the local base folder path.
		/// </summary>
		/// <value>The local base folder path.</value>
		public DirectoryInfo LocalBaseFolderPath
		{
			get
			{
				return _localBaseFolderPath;
			}
		}

		/// <summary>
		/// Gets the local folder path.
		/// </summary>
		/// <value>The local folder path.</value>
		public DirectoryInfo LocalFolderPath
		{
			get
			{
				return _localFolderPath;
			}
		}

		/// <summary>
		/// Gets the local file path.
		/// </summary>
		/// <value>The local file path.</value>
		public FileInfo LocalFilePath
		{
			get
			{
				return _localFilePath;
			}
		}

		/// <summary>
		/// Gets the name of the local file.
		/// </summary>
		/// <value>The name of the local file.</value>
		public FileInfo LocalFileName
		{
			get
			{
				return _localFileName;
			}
		}

		/// <summary>
		/// Gets the added by process ID.
		/// </summary>
		/// <value>The added by process ID.</value>
		public int AddedByProcessID
		{
			get
			{
				return _addedByProcessID;
			}
		}

		/// <summary>
		/// Gets the date added.
		/// </summary>
		/// <value>The date added.</value>
		public DateTime DateAdded
		{
			get
			{
				return _dateAdded;
			}
		}

        /// <summary>
        /// Gets the index of the parsing.
        /// </summary>
        public Int32 Index
        {
            get
            {
                return _index;
            }
        }

        /// <summary>
        /// Gets the parent of the parsing.
        /// </summary>
        public Uri Parent
        {
            get
            {
                return _parent;
            }
        }

		// ------------------------------------------------------------------
		#endregion

		#region Private methods.
		// ------------------------------------------------------------------

		/// <summary>
		/// Makes the name of the local file.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="baseUri">The base URI.</param>
		/// <param name="linkType">Type of the link.</param>
		/// <returns></returns>
		private static FileInfo MakeLocalFileName(
			Uri uri,
			Uri baseUri,
			UriType linkType )
		{
			Debug.Assert(
				uri.IsAbsoluteUri,
				@"URI must be absolute but is not." );

			// Each URI is unique, use this fact.
			string unique =
				uri.AbsoluteUri.GetHashCode().ToString( @"X8" );

			FileInfo result;

			if ( linkType == UriType.Content )
			{
				if ( string.Compare(
					uri.AbsoluteUri.TrimEnd( '/' ),
					baseUri.AbsoluteUri.TrimEnd( '/' ),
					true ) == 0 )
				{
					result = new FileInfo( @"index.html" );
				}
				else
				{
					result = new FileInfo( unique +
						CorrectFileExtension(
							TryExtractFileExtension( uri ) ) );
				}
			}
			else
			{
				Debug.Assert( linkType == UriType.Resource || linkType == UriType.Form );

				result =
					new FileInfo( unique +
						CorrectFileExtension(
							TryExtractFileExtension( uri ) ) );
			}

			Console.WriteLine(
				string.Format(
					@"Making local file path '{0}' for URI '{1}'.",
					result.FullName,
					uri.AbsoluteUri ) );

			return result;
		}

		/// <summary>
		/// Corrects the file extension.
		/// </summary>
		/// <param name="extension">The extension.</param>
		/// <returns></returns>
		private static string CorrectFileExtension(
			string extension )
		{
			if ( string.IsNullOrEmpty( extension ) )
			{
				return extension;
			}
			else
			{
				extension = extension.TrimStart( '.' ).ToLowerInvariant();

				foreach ( string allowedExtension in _allowedExtensions )
				{
					if ( allowedExtension == extension )
					{
						return @"." + extension;
					}
				}

				return @".html";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static readonly string[] _allowedExtensions =
			new string[] 
			{ 
				@"jpg",
				@"jpeg",
				@"png",
				@"gif",
				@"html",
				@"css",
				@"pdf",
				@"txt",
			};

		/// <summary>
		/// Tries the extract file extension.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		private static string TryExtractFileExtension(
			Uri uri )
		{
			string ext = Path.GetExtension(
				uri.AbsoluteUri.Split( '?' )[0] );

			return ext;
		}

		/// <summary>
		/// Gets a value indicating whether [file exists].
		/// </summary>
		/// <value><c>true</c> if [file exists]; otherwise, <c>false</c>.</value>
		public bool FileExists
		{
			get
			{
				FileInfo fileName = MakeLocalFileName(
					AbsoluteUri,
					BaseUri,
					LinkType );

				FileInfo filePath = new FileInfo(
					Path.Combine(
						_localFolderPath.FullName,
						fileName.Name ) );

				return filePath.Exists;
			}
		}

		// ------------------------------------------------------------------
		#endregion

		#region Private variables.
		// ------------------------------------------------------------------

		private readonly DirectoryInfo _localBaseFolderPath;
		private readonly DirectoryInfo _localFolderPath;
		private readonly FileInfo _localFilePath;
		private readonly FileInfo _localFileName;
        private Uri _parent;
        private Int32 _index;

		private readonly int _addedByProcessID = Process.GetCurrentProcess().Id;
		private readonly DateTime _dateAdded = DateTime.Now;

		// ------------------------------------------------------------------
		#endregion

		#region IEquatable<DownloadedResourceInformation> members.
		// ------------------------------------------------------------------

		/// <summary>
		/// Indicates whether the current object is equal to another object of 
		/// the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the other parameter; 
		/// otherwise, false.
		/// </returns>
		public bool Equals(
			DownloadedResourceInformation other )
		{
			if ( AbsoluteUri == null && other.AbsoluteUri == null )
			{
				return true;
			}
			else if ( AbsoluteUri == null || other.AbsoluteUri == null )
			{
				return false;
			}
			else
			{
				int result =
					string.Compare(
						AbsoluteUri.AbsoluteUri,
						other.AbsoluteUri.AbsoluteUri,
						true );

				return result == 0;
			}
		}

		// ------------------------------------------------------------------
		#endregion
	}

	/////////////////////////////////////////////////////////////////////////
}
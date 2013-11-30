namespace myLegis.Spider
{
	#region Using directives.
	// ----------------------------------------------------------------------

	using System;
	using System.IO;
	using System.Diagnostics;
	using System.Text;

	// ----------------------------------------------------------------------
	#endregion

	/////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Persistantly store a resource that was previously downloaded from
	/// an URI to the local file system.
	/// </summary>
	internal sealed class ResourceStorer
	{
		#region Public methods.
		// ------------------------------------------------------------------

		/// <summary>
		/// Constructor.
		/// </summary>
		public ResourceStorer(
			SpiderSettings settings )
		{
			_settings = settings;
		}

		/// <summary>
		/// Stores a binary resource to the local file system.
		/// </summary>
		/// <returns>Return the info about the stored data.</returns>
		public DownloadedResourceInformation StoreBinary(
			byte[] binaryContent,
			UriResourceInformation uriInfo )
		{
			DownloadedResourceInformation result =
				new DownloadedResourceInformation(
				uriInfo,
				_settings.Options.DestinationFolderPath );

			try
			{
				if ( result.LocalFilePath.Exists )
				{
					result.LocalFilePath.Delete();
				}

				if ( binaryContent != null && binaryContent.Length > 0 )
				{
					Console.WriteLine(
						string.Format(
						@"Writing binary content to file '{0}'.",
						result.LocalFilePath ) );

					using ( FileStream s = result.LocalFilePath.OpenWrite() )
					{
						s.Write( binaryContent, 0, binaryContent.Length );
					}
				}
			}
			catch ( IOException x )
			{
				Console.WriteLine(
					string.Format(
					@"Ignoring IO exception while storing binary file: '{0}'.",
					x.Message ) );
			}
			catch ( UnauthorizedAccessException x )
			{
				Console.WriteLine(
					string.Format(
					@"Ignoring exception while storing binary file: '{0}'.",
					x.Message ) );
			}

			return result;
		}

		/// <summary>
		/// Stores a HTML resource to the local file system.
		/// Does no hyperlink replacement.
		/// </summary>
		/// <returns>Return the info about the stored data.</returns>
		public DownloadedResourceInformation StoreHtml(
			string textContent,
			Encoding encoding,
			UriResourceInformation uriInfo )
		{
			DownloadedResourceInformation result =
				new DownloadedResourceInformation(
				uriInfo,
				_settings.Options.DestinationFolderPath );

			try
			{
				if ( result.LocalFilePath.Exists )
				{
					result.LocalFilePath.Delete();
				}

				if ( !result.LocalFilePath.Directory.Exists )
				{
					result.LocalFilePath.Directory.Create();
				}

				Console.WriteLine(
					string.Format(
					@"Writing text content to file '{0}'.",
					result.LocalFilePath ) );

				using ( FileStream s = new FileStream(
					result.LocalFilePath.FullName,
					FileMode.Create,
					FileAccess.Write ) )
				using ( StreamWriter w = new StreamWriter( s, encoding ) )
				{
					w.Write( textContent );
				}
			}
			catch ( IOException x )
			{
				Console.WriteLine(
					string.Format(
					@"Ignoring IO exception while storing HTML file: '{0}'.",
					x.Message ) );
			}
			catch ( UnauthorizedAccessException x )
			{
				Console.WriteLine(
					string.Format(
					@"Ignoring exception while storing HTML file: '{0}'.",
					x.Message ) );
			}

			return result;
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
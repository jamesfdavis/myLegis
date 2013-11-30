namespace myLegis.Spider
{
	#region Using directives.
	// ----------------------------------------------------------------------
	// ----------------------------------------------------------------------
	#endregion

	/////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Information about HTML elements that are links.
	/// </summary>
	internal sealed class LinkElement
	{
		#region Private methods.
		// ------------------------------------------------------------------

		/// <summary>
		/// Constructor.
		/// </summary>
		private LinkElement(
			UriType linkType,
			string name,
			params string[] attributes )
		{
			LinkType = linkType;
			Name = name;
			Attributes = attributes;
		}

		// ------------------------------------------------------------------
		#endregion

		#region Public properties.
		// ------------------------------------------------------------------

		/// <summary>
		/// This list was taken from the Perl module 'HTML-Tagset-3.03\blib\lib\HTML\Tagset.pm',
		/// '%linkElements' hash.
		/// </summary>
		public static readonly LinkElement[] LinkElements =
			new LinkElement[] 
		{ 
			new LinkElement( UriType.Content, @"a", "href" ),
			new LinkElement( UriType.Resource, @"applet", "archive", "codebase", "code" ),
			new LinkElement( UriType.Content, @"area", "href" ),
			new LinkElement( UriType.Resource, @"base", "href" ),
			new LinkElement( UriType.Resource, @"bgsound", "src" ),
			new LinkElement( UriType.Resource, @"blockquote", "cite" ),
			new LinkElement( UriType.Resource, @"body", "background" ),
			new LinkElement( UriType.Resource, @"del", "cite" ),
			new LinkElement( UriType.Resource, @"embed", "pluginspage", "src" ),
			new LinkElement( UriType.Form, @"form", "action" ),
			new LinkElement( UriType.Content, @"frame", "src", "longdesc" ),
			new LinkElement( UriType.Content, @"iframe", "src", "longdesc" ),
			new LinkElement( UriType.Resource, @"ilayer", "background" ),
			new LinkElement( UriType.Resource, @"img", "src", "lowsrc", "longdesc", "usemap" ),
			new LinkElement( UriType.Resource, @"input", "src", "usemap" ),
			new LinkElement( UriType.Resource, @"ins", "cite" ),
			new LinkElement( UriType.Resource, @"isindex", "action" ),
			new LinkElement( UriType.Resource, @"head", "profile" ),
			new LinkElement( UriType.Resource, @"layer", "background", "src" ),
			new LinkElement( UriType.Content, @"link", "href" ),
			new LinkElement( UriType.Resource, @"object", "classid", "codebase", "data", "archive", "usemap" ),
			new LinkElement( UriType.Resource, @"q", "cite" ),
			new LinkElement( UriType.Resource, @"script", "src", "for" ),
			new LinkElement( UriType.Resource, @"table", "background" ),
			new LinkElement( UriType.Resource, @"td", "background" ),
			new LinkElement( UriType.Resource, @"th", "background" ),
			new LinkElement( UriType.Resource, @"tr", "background" ),
			new LinkElement( UriType.Resource, @"xmp", "href" ) 
		};

		/// <summary>
		/// The type of link.
		/// </summary>
		public readonly UriType LinkType;

		/// <summary>
		/// The name of the tag.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The attributes that contain the link.
		/// </summary>
		public readonly string[] Attributes;

		// ------------------------------------------------------------------
		#endregion
	}

	/////////////////////////////////////////////////////////////////////////
}
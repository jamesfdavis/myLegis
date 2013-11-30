namespace myLegis.Spider
{
	/////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// The type of an URI resource.
	/// </summary>
	public enum UriType
	{
		#region Enum members.
		// ------------------------------------------------------------------

		/// <summary>
		/// A HTML page with content to parse and to follow the links.
		/// </summary>
		Content,

		/// <summary>
		/// A resource link like images, videos, etc.
		/// </summary>
		Resource,

        /// <summary>
		/// A submittable form.
		/// </summary>
		Form

		// ------------------------------------------------------------------
		#endregion
	}

	/////////////////////////////////////////////////////////////////////////
}
namespace myLegis.Spider
{
	/////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// How to handle proxy.
	/// </summary>
	public enum DownloadProxyUsage
	{
		#region Enum members.
		// ------------------------------------------------------------------

		/// <summary>
		/// Explicitely use the provided proxy.
		/// </summary>
		UseProxy,

		/// <summary>
		/// Explicitely use no proxy.
		/// </summary>
		NoProxy,

		/// <summary>
		/// Use the system-default proxy.
		/// </summary>
		Default

		// ------------------------------------------------------------------
		#endregion
	}

	/////////////////////////////////////////////////////////////////////////
}
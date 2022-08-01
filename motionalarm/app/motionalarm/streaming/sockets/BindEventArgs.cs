namespace library
{
	/// <summary>
	/// A base class for binding event information used by communication constructs.
	/// </summary>
	public class BindEventArgs
	{
		/// <summary>
		/// Cannot instantiate
		/// </summary>
		protected BindEventArgs()
		{ additionalData = null; }

		/// <summary>
		/// Provides data in addition to the necessary binding information in the sub class.
		/// </summary>
		public object additionalData { get; set; }
	}
}

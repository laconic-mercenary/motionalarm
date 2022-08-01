
namespace library
{
	public sealed class RxSocketEventArgs : RxEventArgs
	{
		/// <summary>
		/// Gets or sets the data that was received.
		/// </summary>
		public byte[] data { get; set; }

		/// <summary>
		/// Gets or sets the length of the data that was received.
		/// </summary>
		public int dataLength { get; set; }
	}
}

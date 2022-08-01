namespace library
{
	/// <summary>
	/// Used to specify data to send over a socket.
	/// </summary>
	public sealed class TxSocketEventArgs : TxEventArgs
	{
		/// <summary>
		/// Gets or sets the data that is to be sent or was sent.
		/// </summary>
		public byte[] data { get; set; }
	}
}

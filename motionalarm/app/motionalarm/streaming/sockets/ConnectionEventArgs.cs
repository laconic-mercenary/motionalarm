
namespace library
{
	public sealed class ConnectionEventArgs
	{
		/// <summary>
		/// Gets or sets the remote address to connect to or connected to you.
		/// </summary>
		public System.Net.IPAddress remoteAddress { get; set; }

		/// <summary>
		/// Gets or sets the port number the connection will receive or was received on.
		/// </summary>
		public int portNumber { get; set; }

		/// <summary>
		/// Gets or sets additional data.
		/// </summary>
		public object additionalData { get; set; }
	}
}

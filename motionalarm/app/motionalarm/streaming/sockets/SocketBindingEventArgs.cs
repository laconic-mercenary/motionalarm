
namespace library
{
	public sealed class SocketBindingEventArgs : BindEventArgs
	{
		/// <summary>
		/// Gets or sets the local address to bind to.
		/// </summary>
		public System.Net.IPAddress localAddress { get; set; }

		/// <summary>
		/// Gets the port number to bind to.
		/// </summary>
		public int portNumber { get; set; }
	}
}

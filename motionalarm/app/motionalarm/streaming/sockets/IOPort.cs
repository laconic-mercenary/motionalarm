namespace library
{
	/// <summary>
	/// A generic I/O port that can manifest anything.
	/// </summary>
	public abstract class IOPort
	{
		/// <summary>
		/// Call this to officially get the port ready for communication.
		/// </summary>
		/// <param name="bindingInformation"></param>
		public abstract void bind(BindEventArgs bindingInformation);
		/// <summary>
		/// Call this to send data through the port.
		/// </summary>
		/// <param name="sendInformation"></param>
		/// <returns></returns>
		public abstract int send(TxEventArgs sendInformation);
		/// <summary>
		/// Call this to receive data - which can be either async or sync, if async
		/// the Rx and Tx events are provided below.
		/// </summary>
		public abstract void receive();
		/// <summary>
		/// Closes and cleans up the port.  Generally a call to bind() could place this 
		/// in a ready state again, if not instantiation.
		/// </summary>
		public abstract void close();
		
		public delegate void RxEvent(RxEventArgs e);
		public delegate void TxEvent(TxEventArgs e);
	}
}

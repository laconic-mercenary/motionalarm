
namespace app.motionalarm.streaming
{
    public class StreamPreferences
    {
        public StreamPreferences()
        {
            password = null;
            localConnectionPort = Globals.Streaming.DEFAULT_LOCAL_PORT;
        }

        public string password
        {
            get;
            set;
        }

        public int localConnectionPort
        {
            get;
            set;
        }
    }
}

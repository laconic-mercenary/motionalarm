namespace app.motionalarm.configuration
{
    static public class Config
    {
        public static AppConfiguration current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AppConfiguration();
                }
                return _current;
            }
        }
        private static AppConfiguration _current = null;
    }
}

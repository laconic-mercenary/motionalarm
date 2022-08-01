namespace app.motionalarm.logging
{
    public static class Logger
    {
        static Logger()
        {
            isEnabled = true;
        }

        public static void setDir(string dir)
        {
            try
            {
                if (dir == null)
                {
                    dir = string.Empty;
                }
                _path = dir + "\\" + _name;
            }
            catch
            {
                dir = string.Empty;
            }
        }

        public static void log(string info)
        {
            if (isEnabled == true)
            {
                try
                {
                    string content = System.DateTime.Now.ToString() + " - " + info;
                    if (System.IO.File.Exists(logPath))
                    {
                        System.IO.StreamWriter writer = System.IO.File.AppendText(logPath);
                        writer.WriteLine(content);
                        writer.Close();
                    }
                    else
                    {
                        System.IO.File.WriteAllText(logPath, content);
                    }
                }
                catch
                {
                    isEnabled = false;
                }
            }
        }

        public static string logPath
        {
            get
            {
                return _path;
            }
        }

        public static bool isEnabled { get; set; }
        private static string _path = string.Empty;
        private static string _name = "log.txt";
    }
}
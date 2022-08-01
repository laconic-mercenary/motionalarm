
namespace app.motionalarm.reporting
{
    using StrList = System.Collections.Generic.List<string>;
    using Dir = System.IO.Directory;
    using DirInfo = System.IO.DirectoryInfo;

    /// <summary>
    /// Provides a service to create html intrusion reports.
    /// </summary>
    public static class Reporting
    {
        /// <summary>
        /// Enabling this will allow for the creation of reports, else
        /// create will simple do nothing.
        /// </summary>
        public static bool isEnabled { get; set; }

        /// <summary>
        /// Dumps the report to the specified file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="imagePath"></param>
        public static void create(string path, string imagePath)
        {
            // 
            if (isEnabled == true)
            {
                IntrusionReportContents contents = new IntrusionReportContents();
                HtmlReport report = new HtmlReport();
                // title
                report.setTitle(contents.title);
                // header
                report.appendBreakPoint();
                report.appendBreakPoint();
                report.appendEmphasizedText(contents.softwareStamp);
                // disclaimer                
                report.appendBreakPoint();
                report.appendBreakPoint();
                report.appendCenteredText(contents.header);
                // content            
                report.appendBreakPoint();
                report.appendBreakPoint();
                report.appendParagraph("Intrusion Detected on");
                report.appendParagraph(contents.timeStamp.ToString());
                // image
                report.appendBreakPoint();
                report.appendImageLink(imagePath, 500, 400);
                report.appendBreakPoint();
                report.appendStrongText(contents.disclaimer);
                report.closeDocument();
                report.toFile(path);
            }
        }

        /// <summary>
        /// Gets a DateTime.Now string that is friendly to append to file names.
        /// </summary>
        /// <returns></returns>
        public static string getFileFriendlyDateString()
        {
            System.DateTime now = System.DateTime.Now;
            string dateTimeNowStr = string.Format(
                "{0},{1},{2}_{3}{4}",
                now.Date.Day, now.Date.Month, now.Year, now.Hour, now.Minute
            );
            return dateTimeNowStr;
        }

        /// <summary>
        /// Gets a list of the reports paths in a specified directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static string[] getAllReportNames(string directory)
        {
            StrList list = new StrList();
            try
            {
                string[] files = Dir.GetFiles(directory);
                foreach (string file in files)
                {
                    if (file.ToLower().Contains("report") && file.ToLower().Contains(".htm"))
                    {
                        list.Add(file);
                    }
                }
            }
            catch
            {
                // let it go
            }
            // check if there were any files...
            return list.ToArray();
        } // 
    }
}

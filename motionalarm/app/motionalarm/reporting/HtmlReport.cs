
namespace app.motionalarm.reporting
{
    /**
     *	Example Usage:	
            report.setTitle("Test Report Title");
            report.appendBulletedList(new string[] { "item1", "item2" });
            report.appendBreakPoint();
            report.appendOrderedList(new string[] { "item1", "item2" });
            report.appendBreakPoint();
            report.appendCenteredText("centered text");
            report.appendBreakPoint();
            report.appendMetaData("author", "mlcs");
            report.closeDocument();
            report.toFile("C:\\REPORT.htm");	 */

    using Err = System.Exception;

    /// <summary>
    /// Generates a formatted report using HTML providing image
    /// and paragraph capabilities.
    /// </summary>
    public class HtmlReport
    {

        /// <summary>
        /// Creats a blank html document.
        /// </summary>
        public HtmlReport()
        {
            setTitle("report");
        }

        /// <summary>
        /// This is optional.  Sets the DOCTYPE version which tells the web browser
        /// the version of the markup and how to parse the document.  
        /// (EX type: "-//W3C//DTD HTML 5.0//EN" "http://www.w3.org/TR/html4/strict.dtd" )
        /// (EX type: "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd" )
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dtdUrl"></param>
        public void setDocTypeDefinition(string type, string dtdUrl)
        {
            this.docType = string.Format(
                "<!DOCTYPE HTML PUBLIC \"{0}\"" + System.Environment.NewLine +
                "\"{1}\"", type, dtdUrl
            );
        }

        /// <summary>
        /// Sets the title of the HTML page, which will appear in the 
        /// browser when opened.
        /// </summary>
        /// <param name="title"></param>
        public void setTitle(string title)
        {
            if (string.IsNullOrEmpty(title) == false)
            {
                this.title = "<title>" + title + "</title>";
            }
        }

        /// <summary>
        /// This is optiona.  Adds meta data to the html document.  This is automatically 
        /// placed in the head tag so this can be called at anytime.  
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public void appendMetaData(string name, string content)
        {
            this.metaData += "<meta name=\"" + name + "\" content=\"" + content + "\" />";
        }

        #region << Append Methods >>

        /// <summary>
        /// Appends a list of bulleted items
        /// </summary>
        /// <param name="items"></param>
        public void appendBulletedList(string[] items)
        {
            this.buffer += "<menu>";
            foreach (string item in items)
            {
                this.buffer += "<li>" + item + "</li>";
            }
            this.buffer += "</menu>";
        }

        /// <summary>
        /// Appends a java applet to the document.  innerHtml is optional.  
        /// Example 'className' would be 'LinkedList.class' or 'Bubbles.class'
        /// </summary>
        /// <param name="className"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="innerHtml"></param>
        public void appendJavaApplet(string className, int width, int height, string innerHtml)
        {
            this.buffer += string.Format("<applet code=\"{0}\" width=\"{1}\" height=\"{2}\"> {3} </applet>",
                className, width, height, innerHtml
            );
        }

        /// <summary>
        /// Appends a Flash object (.SWF file).  Path can be relative.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="name"></param>
        /// <param name="pathToSWF"></param>
        public void appendFlashObject(int width, int height, string name, string pathToSWF)
        {
            this.buffer += string.Format(
                "<object width=\"{0}\" height=\"{1}\"> <param name=\"{2}\" value=\"{3}\"/> </object>",
                width, height, name, pathToSWF
            );
        }

        /// <summary>
        /// This will add text that is centered on the page.
        /// </summary>
        /// <param name="text"></param>
        public void appendCenteredText(string text)
        {
            appendNoAttributedTag("center", text);
        }

        /// <summary>
        /// Appends a numbered list to the document.  The items specified
        /// are automatically numbered going downward on the page.
        /// </summary>
        /// <param name="items"></param>
        public void appendOrderedList(string[] items)
        {
            this.buffer += "<ol>";
            foreach (string item in items)
            {
                this.buffer += "<li>" + item + "</li>";
            }
            this.buffer += "</ol>";
        }

        /// <summary>
        /// Appends a break tag to the current buffer.  This is 
        /// independent of the appendNewLine property.
        /// </summary>
        public void appendBreakPoint()
        {
            buffer += "<br/>";
        }

        /// <summary>
        /// Appends a table with the specified columns for headers and values.
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="values"></param>
        /// <param name="borderSize"></param>
        public void appendTable(string[] columnNames, string[][] values, uint borderSize)
        {
            buffer += string.Format(
                "<table border=\"{0}\">",
                borderSize
            );
            //
            // append the columns
            buffer += "<tr>";
            foreach (string column in columnNames)
            {
                buffer += string.Format("<th>{0}</th>", column);
            }
            //
            // put on the values			
            for (int row = 0; row < values.GetUpperBound(0); row++)
            {
                buffer += "<tr>";
                for (int col = 0; col < values[row].Length; col++)
                {
                    buffer += "<td>" + values[row][col] + "</td>";
                }
            }
        }

        /// <summary>
        /// Appends text in a small, italic font.
        /// </summary>
        /// <param name="text"></param>
        public void appendItalicText(string text)
        {
            appendNoAttributedTag("i", text);
        }

        /// <summary>
        /// Appends a button and allows for a javascript function to handle the event.
        /// Use the appendScript() method to append java script code, then call this.
        /// "[button type=\"button\" onclick=\"onClickEvent\"] buttonName [/button]"
        /// </summary>
        /// <param name="buttonName"></param>
        /// <param name="onClickEvent"></param>
        public void appendButtonWithEvent(string buttonName, string onClickEvent)
        {
            buffer += string.Format(
                "<button type=\"button\" onclick=\"{1}\">{0}</button>",
                buttonName, onClickEvent
            );
        }

        /// <summary>
        /// Appends a simple script tag with no events for attributes.  
        /// "script type=\"typeString\"] code [/script]"
        /// </summary>
        /// <param name="typeString"></param>
        /// <param name="code"></param>
        public void appendScript(string typeString, string code)
        {
            buffer += string.Format("<script type=\"{0}\">{1}</script>", typeString, code);
        }

        /// <summary>
        /// Strong text is basically bold text.
        /// </summary>
        /// <param name="text"></param>
        public void appendStrongText(string text)
        {
            appendNoAttributedTag("strong", text);
        }

        /// <summary>
        /// Appends formatted source code to the report.
        /// </summary>
        /// <param name="code"></param>
        public void appendCode(string code)
        {
            appendNoAttributedTag("code", code);
        }

        /// <summary>
        /// Appends some emphasized text to the document.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="appendNewLine"></param>
        public void appendEmphasizedText(string contents)
        {
            appendNoAttributedTag("em", contents);
        }

        /// <summary>
        /// Appends a paragraph on without formatting.  (Font tags are removed in HTML5).
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="appendNewLine"></param>
        public void appendParagraph(string contents)
        {
            appendNoAttributedTag("p", contents);
        }

        /// <summary>
        /// Appends a link to the buffer, path must be formatted like a web address
        /// or a local path.  innerText can be empty though.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="innerText"></param>
        public void appendHyperLink(string path, string innerText)
        {
            buffer += "<a href=\"" + path + "\">" + innerText + "</a>";
        }

        /// <summary>
        /// [img src="path" width="width" height="height" /]
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void appendImageLink(string sourcePath, uint width, uint height)
        {
            buffer += string.Format(
                "<img src=\"{0}\" width=\"{1}\" height=\"{2}\" alt=\"Cannot Display Image.\"/>",
                sourcePath, width, height
            );
        }

        #endregion << Append Methods >>

        /// <summary>
        /// Dumps the contents of the html file to an extern file.
        /// These should carry the .htm extension.
        /// </summary>
        /// <param name="path"></param>
        public void toFile(string path)
        {
            // automatically close the document
            closeDocument();
            // dumps the report
            System.IO.File.WriteAllText(path, buffer);
        }

        /// <summary>
        /// Appends the body and html closing tags on the document.
        /// </summary>
        public void closeDocument()
        {
            if (isClosed == false)
            {
                string preamble = "<html><head>" + this.metaData + this.title + "</head><body>";
                buffer = preamble + buffer + "</body></html>";
                isClosed = true;
            }
        }

        /// <summary>
        /// Appends to the buffer, a tag that doesn't required attributes and only
        /// has inner html to worry about.
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="innerHtml"></param>
        /// <param name="appendNewLine"></param>
        protected void appendNoAttributedTag(string tagName, string innerHtml)
        {
            buffer += string.Format(
                "<{0}>{1}</{0}>", tagName, innerHtml
            );
        }

        protected bool isClosed = false;
        protected string docType = string.Empty;
        protected string metaData = string.Empty;
        protected string title = string.Empty;
        protected string buffer = string.Empty;
    }
}

namespace app.motionalarm.email
{

    using System.Net;
    using System.Net.Mail;

    public sealed class GmailSender
    {

        /// <summary>
        /// Creates an email service with an existing account's credentials.
        /// </summary>
        public GmailSender()
        {
            // 
            senderServer = new SmtpClient("smtp.gmail.com");
            senderServer.Port = 587;
            senderServer.Credentials = new NetworkCredential("lambdaTestSender", sendReliable());
            senderServer.EnableSsl = true;
        }

        ~GmailSender()
        {
            senderServer.Dispose();
        }

        /// <summary>
        /// Sends a blocking email with the specified message.
        /// </summary>
        /// <param name="message"></param>
        public void send(MailMessage message)
        {
            senderServer.Send(message);
        }

        /// <summary>
        /// Sends a non-blocking email with the specified message.
        /// </summary>
        /// <param name="message"></param>
        public void sendAsync(MailMessage message)
        {
            senderServer.SendCompleted += new SendCompletedEventHandler(senderServer_SendCompleted);
            senderServer.SendAsync(message, senderServer as object);
        }

        // sneaky sneaky...
        string sendReliable()
        {
            string _pass = "1qa2ws3ed!QA@WS#ED";
            return _pass;
        }

        void senderServer_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // (e.UserState as SmtpClient)
            if (e.Error != null)
            {
                // handle error
                if (onAsyncSendError != null)
                {
                    onAsyncSendError(e.Error as object, null);
                }
            }
        }

        public string sendersEmail
        {
            get
            {
                return "lambdaTestSender@gmail.com";
            }
        }

        public event System.EventHandler onAsyncSendError = null;

        private SmtpClient senderServer = null;

    }
}
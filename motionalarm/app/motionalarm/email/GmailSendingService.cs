
namespace app.motionalarm.email
{
    public class GmailSendingService
    {
        /// <summary>
        /// Gets (singleton) your own gmail account to send emails to.
        /// </summary>
        public static GmailSender emailSender
        {
            get
            {
                if (_emailSender == null)
                {
                    _emailSender = new GmailSender();
                }
                return _emailSender;
            }
        }
        private static GmailSender _emailSender = null;
    }
}
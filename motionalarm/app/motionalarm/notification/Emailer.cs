//
// done
namespace app.motionalarm.notification
{
    using System.Net.Mail;

    public class Emailer : app.motionalarm.notification.INotifier
    {

        /// <summary>
        /// Creates an emailing service.  Specifying null for preferences
        /// automatically disables sending notifications.
        /// </summary>
        /// <param name="emailPreferences"></param>
        public Emailer()
        {
            this.isEnabled = false;
        }

        public void setPreferences(app.motionalarm.configuration.EmailPreferences emailPreferences)
        {
            if (emailPreferences != null)
            {
                this.isEnabled = true;
                this.preferences = emailPreferences;
            }
            else
            {
                this.isEnabled = false;
            }
        }

        public void notify()
        {
            if (isEnabled)
            {
                email.GmailSendingService.emailSender.send(getMailMessage());
            }
        }

        public void removeAttachments()
        {
            this.preferences.attachmentPaths.Clear();
        }

        public void notifyAsync()
        {
            if (isEnabled)
            {
                email.GmailSendingService.emailSender.sendAsync(getMailMessage());
            }
        }

        /// <summary>
        /// Gets or sets if the emailer should be enabled or not.
        /// </summary>
        public bool isEnabled { get; set; }

        private MailMessage getMailMessage()
        {
            // create the new default message
            MailMessage message = new System.Net.Mail.MailMessage();
            //
            // pass over the body
            message.Body = this.preferences.body;
            //
            // add the attachments
            if (this.preferences.attachmentPaths != null)
            {
                // add them 1 by 1
                foreach (string attachPath in this.preferences.attachmentPaths)
                {
                    message.Attachments.Add(new System.Net.Mail.Attachment(attachPath));
                }
            }
            //
            // pass over the 
            message.From = new MailAddress(email.GmailSendingService.emailSender.sendersEmail);
            //
            // pass over the subject
            message.Subject = this.preferences.subjectLine;

            foreach (string emailAddress in this.preferences.addresses)
            {
                message.To.Add(emailAddress);
            }
            // send back the message
            return message;
        }

        private app.motionalarm.configuration.EmailPreferences preferences = null;

    }
}

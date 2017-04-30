using System.Net.Mail;

namespace TrunkCommon
{
    public static class EMailHelper
    {
        /// <summary>
        /// Smtp认证方式
        /// </summary>
        public enum EmailCredential { DefaultCredentials, PassWordCredentials };

        /// <summary>
        /// 发送不需要验证的Email
        /// </summary>
        /// <param name="host">smtp地址</param>
        /// <param name="port">端口</param>
        /// <param name="mail">邮件体</param>
        public static void SendEmail(string host, int port, MailMessage mail, int timeout)
        {
            SendEmail(host, port, string.Empty, string.Empty, mail, EmailCredential.DefaultCredentials, timeout);
        }

        /// <summary>
        /// 发送带用户和验证的Email
        /// </summary>
        /// <param name="host">smtp地址</param>
        /// <param name="port">端口</param>
        /// <param name="username">用户名 </param>
        /// <param name="password">密码</param>
        /// <param name="mail">邮件体</param>
        public static void SendEmail(string host, int port, string username, string password, MailMessage mail, int timeout)
        {
            SendEmail(host, port, username, password, mail, EmailCredential.PassWordCredentials, timeout);
        }

        /// <summary>
        /// 发送不需要验证的Email
        /// </summary>
        /// <param name="host">smtp地址</param>
        /// <param name="port">端口</param>
        /// <param name="senderAddress">发信人地址</param>
        /// <param name="receiverAddress">收件人地址</param>
        /// <param name="emailSubject">邮件标题</param>
        /// <param name="emailBody">邮件内容</param>
        public static void SendEmail(string host, int port, string senderAddress, string receiverAddress, string emailSubject, string emailBody, int timeout)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(senderAddress);
            string[] receiverList = receiverAddress.Split(';');
            for (int i = 0; i < receiverList.Length; i++)
                mailMessage.To.Add(receiverList[i]);
            mailMessage.Subject = emailSubject;
            mailMessage.Body = emailBody;
            mailMessage.Priority = MailPriority.Normal;
            SendEmail(host, port, mailMessage, timeout);
        }

        /// <summary>
        /// 发送带用户和验证的Email
        /// </summary>
        /// <param name="host">smtp地址</param>
        /// <param name="port">端口</param>
        /// <param name="username">用户名 </param>
        /// <param name="password">密码</param>
        /// <param name="senderAddress">发信人地址</param>
        /// <param name="receiverAddress">收件人地址</param>
        /// <param name="emailSubject">邮件标题</param>
        /// <param name="emailBody">邮件内容</param>
        public static void SendEmail(string host, int port, string username, string password, string senderAddress, string receiverAddress,
                                     string emailSubject, string emailBody, int timeout)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(senderAddress);
            mailMessage.To.Add(receiverAddress);
            mailMessage.Subject = emailSubject;
            mailMessage.Body = emailBody;
            mailMessage.Priority = MailPriority.Normal;
            SendEmail(host, port, username, password, mailMessage, timeout);
        }

        /// <summary>
        ///  发送邮件
        /// </summary>
        /// <param name="host">smtp地址</param>
        /// <param name="port">端口</param>
        /// <param name="username">用户名 </param>
        /// <param name="password">密码</param>
        /// <param name="mail">邮件体</param>
        /// <param name="credential">认证方式</param>
        private static void SendEmail(string host, int port, string username, string password, MailMessage mail, EmailCredential credential, int timeout)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Timeout = timeout;
            if (host != null && host.Length > 0)
            {
                smtpClient.Host = host;
            }
            smtpClient.Port = port;

            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (credential == EmailCredential.PassWordCredentials)
                smtpClient.Credentials = new System.Net.NetworkCredential(username, password);
            else
                smtpClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            smtpClient.Send(mail);
        }
    }
}

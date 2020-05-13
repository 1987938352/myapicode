using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailPost
{
  public  class EmailSmtp
    {
        public static  void EmailPost(string email, string text)
        {
            using (MailMessage mailMessage = new MailMessage())
            using (SmtpClient smtpClient = new SmtpClient("smtp.qq.com"))
            {
                mailMessage.To.Add(email);
                mailMessage.Body = $"注册码为{text}";
                mailMessage.From = new MailAddress("554476199@qq.com");
                mailMessage.Subject = "来自Blog的注册码";
                smtpClient.Credentials = new System.Net.NetworkCredential("554476199@qq.com", "nnvgfjseacqzbfab");//如果启用了“客户端授权码”，要用授权码代替密码
                 smtpClient.Send(mailMessage);
            }
        }
    }
}

using StackExchange.Redis;
using System;
using System.Threading;

namespace EmailPost
{
    class Program
    {
        static void Main(string[] args)
        {

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
            var db = redis.GetDatabase();
            while (true)
            {
                if (db.KeyExists("Email"))
                {
                    string Email = db.ListRightPop("Email");
                    if (!string.IsNullOrEmpty( Email))
                    {
                        string key = $"Email{Email}";
                        if (db.KeyExists(key))
                        {
                          string myGuid=  db.HashGet(key, "Guid");
                           EmailSmtp.EmailPost(Email, myGuid);
                        }

                        continue;
                    } 
                }
                Console.WriteLine("睡两小时");  
                Thread.Sleep(2000);
            }
        }
    }
}

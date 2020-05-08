using Core.Entitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.DataBase
{
  public  class MyContextSend
    {
        public static async Task SeedAsync(MyContext myContext,ILoggerFactory loggerFactory,int retry = 0)
        {
            int retryForAvailability = retry;
            try
            {
                bool isAny =await myContext.Post.AnyAsync();
                if (!isAny)
                {
                    myContext.Post.AddRange(new List<Post> {
                    new Post
                    {
                        Title="post title 1",
                        Body="post body 1",
                        Author="Dave",
                        CastModified=DateTime.Now
                    },new Post
                    {
                        Title="post title 2",
                        Body="post body 2",
                        Author="Dave",
                        CastModified=DateTime.Now
                    },new Post
                    {
                        Title="post title 3",
                        Body="post body 3",
                        Author="Dave",
                        CastModified=DateTime.Now
                    },new Post
                    {
                        Title="post title 4",
                        Body="post body 4",
                        Author="Dave",
                        CastModified=DateTime.Now
                    },new Post
                    {
                        Title="post title 5",
                        Body="post body 5",
                        Author="Dave",
                        CastModified=DateTime.Now
                    },new Post
                    {
                        Title="post title 6",
                        Body="post body 6",
                        Author="Dave",
                        CastModified=DateTime.Now
                    },new Post
                    {
                        Title="post title 7",
                        Body="post body 7",
                        Author="Dave",
                        CastModified=DateTime.Now
                    },new Post
                    {
                        Title="post title 8",
                        Body="post body 8",
                        Author="Dave",
                        CastModified=DateTime.Now
                    }
                    });
                    await myContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability<10)
                {
                    retryForAvailability++;
                    var logger = loggerFactory.CreateLogger<MyContextSend>();
                    logger.LogError(ex.Message);
                    await SeedAsync(myContext, loggerFactory, retryForAvailability);
                }
                throw;
            }
        }
    }
}

using Core.Entitites;
using Core.InterFace;
using infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositories
{
    public class UserRepositories : IUserRepository
    {
        private readonly MyContext myContext;

        public UserRepositories(MyContext myContext)
        {
            this.myContext = myContext;
        }
        public async Task<User> Login(int Id, string pwd)
        {
            var user = await myContext.User.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
            {
                return null;
            }
            if (user.PassWord != pwd)
            {
                return null;
            }
            return user;

        }


    }
}

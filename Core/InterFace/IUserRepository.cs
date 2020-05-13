using Core.Entitites;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.InterFace
{
  public  interface IUserRepository
    {
        Task<User> Login(int Id, string pwd);
        User Register(string pwd);

    }
}

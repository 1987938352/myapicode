using Core.InterFace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.DataBase
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly MyContext myContext;
        public UnitOfWork(MyContext myContext)
        {
            this.myContext = myContext;
        }
        public async Task<bool> SaveAsync()
        {
            return await myContext.SaveChangesAsync() > 0;
        }
    }
}

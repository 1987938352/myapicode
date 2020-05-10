
using infrastructure.Extensions;
using Core.Entitites;
using Core.InterFace;
using infrastructure.DataBase;
using infrastructure.DTO;
using infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly MyContext myContext;
        private readonly IPropertyMappingContainer propertyMappingContainer;

        public PostRepository(MyContext myContext,IPropertyMappingContainer propertyMappingContainer)
        {
            this.myContext = myContext;
            this.propertyMappingContainer = propertyMappingContainer;
        }

        public void AddPost(Post post)
        {
            //return Task.Run(() =>
            //{
                myContext.Post.Add(post);
            //}); 
        }

        public async Task<PaginatedList <Post>> GetAllPostsAsync(PostParameters postParameters)
        {
            var query = myContext.Post.AsQueryable();

            if (!string.IsNullOrEmpty(postParameters.Title))
            {
                var title = postParameters.Title;//不知道为什么转大小写后会报错 所以没有ToUpperInvariant
                query = query.Where(x => x.Title.Contains(title));
            }
            query = query.ApplySort(postParameters.OrderBy, propertyMappingContainer.Resolve<PostDTO, Post>());
            //query = query.OrderBy(x => x.Id);

            var count = await query.CountAsync();
            var data =await query.Skip(postParameters.PageIndex * postParameters.PageSize).Take(postParameters.PageSize).ToListAsync();
           
            return  new PaginatedList<Post>(postParameters.PageIndex,postParameters.PageSize,count, data);
        }
        public async Task<bool> SaveAsync()
        {
            return await myContext.SaveChangesAsync() > 0;
        }
        public async Task<Post>GetPostByIdAsync(int id)
        {
            return await myContext.Post.FindAsync(id);
        }

        public void Delete(Post post)
        {
            myContext.Post.Remove(post);
        }

        public void Update(Post post)
        {
            myContext.Entry(post).State = EntityState.Modified;

        }

        public async Task<int> GetCount()
        {
            int count = await myContext.Post.CountAsync();
            return count;
        }

        public Task<List<Post>> GetAllPost()
        {
            var listPost = myContext.Post.ToListAsync();
            return listPost;
        }
    }
}

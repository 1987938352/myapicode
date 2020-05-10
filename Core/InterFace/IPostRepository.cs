using Core.Entitites;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.InterFace
{
  public  interface IPostRepository
    {
        Task<PaginatedList<Post>> GetAllPostsAsync(PostParameters postParameters);
        Task<List<Post>> GetAllPost();
        Task<int> GetCount();
        void AddPost(Post post);
        Task<bool> SaveAsync();
        Task<Post> GetPostByIdAsync(int id);

        void Delete(Post post);

        void Update(Post post);
    }
}

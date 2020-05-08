using System;
using System.Collections.Generic;
using System.Text;
using Core.Entitites;
using infrastructure.DTO;
using infrastructure.Services;

namespace infrastructure.Resources
{
    public class PostPropertyMapping : PropertyMapping<PostDTO, Post>
    {
        public PostPropertyMapping() : base(
            new Dictionary<string, List<MappedProperty>>
                (StringComparer.OrdinalIgnoreCase)
                {
                    [nameof(PostDTO.Title)] = new List<MappedProperty>
                    {
                        new MappedProperty{ Name = nameof(Post.Title), Revert = false}
                    },
                    [nameof(PostDTO.Body)] = new List<MappedProperty>
                    {
                        new MappedProperty{ Name = nameof(Post.Body), Revert = false}
                    },
                    [nameof(PostDTO.Author)] = new List<MappedProperty>
                    {
                        new MappedProperty{ Name = nameof(Post.Author), Revert = false}
                    }
                })
        {
        }
    }
}

using AutoMapper;
using Core.Entitites;
using infrastructure.DTO;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapicode.Extensions
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Post, PostDTO>();
            CreateMap<PostDTO, Post>();
            CreateMap<PostAddResource, Post>().ReverseMap(); ;
            CreateMap<PostUpdateResource, Post>().ReverseMap();
        }
    }
}

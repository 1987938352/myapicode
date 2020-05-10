using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Entitites;
using Core.InterFace;
using infrastructure.DTO;
using infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using myapicode.ES;
using Nest;

namespace myapicode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ESController : ControllerBase
    {
        private readonly IESSever eSSever;
        private readonly IPostRepository postRepository;
        private readonly IMapper mapper;

        public ESController(IESSever eSSever, IPostRepository postRepository, IMapper mapper)
        {
            this.eSSever = eSSever;
            this.postRepository = postRepository;
            this.mapper = mapper;
        } 
        [HttpGet]
        public async Task<int> Index()
        {
            List<Post> listPost =await postRepository.GetAllPost();
           return  eSSever.EsIndex(listPost);
        }
        [HttpPost]
        public IActionResult Search(string query)
        {
            List<Post> postList = eSSever.Search(query);
            var postDTO = mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(postList);
            var resultAll = new
            {
                value = postDTO,
                count = postList.Count()
            };
            return Ok(resultAll);
        }
        [HttpGet("{name}")]
       //该方法为测试方法
        public void Search2(string name)
        {
            bool eb = eSSever.ElasticLinqClient.IndexExists("mypost").Exists;
            if (eb)
            {
                var result2 = eSSever.ElasticLinqClient.Search<Post>(x=>x.Index("mypost").Type(TypeName.Create<Post>()).Query(op => op.Term(
                                          ss => ss.Field( qq
                                          => qq.Title).Value("*ajax*"))));
                var result = eSSever.ElasticLinqClient.Search<Post>(x =>
                 x.Index("mypost").Type(TypeName.Create<Post>())
                 .Query(q =>
                 q.Match(m => m.Field(
                     f => f.Title)
                 .Query("英")
                     )
                ).From(0).Size(10));
                var res = result.Documents.ToList();
                var a = 1;
            }
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string[] Chains { get; set; }
    }
}
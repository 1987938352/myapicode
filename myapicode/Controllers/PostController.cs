using AutoMapper;
using Core.Entitites;
using Core.InterFace;
using infrastructure.DTO;
using infrastructure.Extensions;
using infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace myapicode.Controllers
{
    [Route("api/posts")]

    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository postRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor accessor;
        private readonly LinkGenerator generator;
        private readonly ITypeHelperService typeHelperService;
        private readonly IPropertyMappingContainer propertyMappingContainer;
        private readonly ILogger logger;

        public PostController(
            IPostRepository postRepository,
            IUnitOfWork unitOfWork,
           ILoggerFactory loggerFactory,
           IConfiguration configuration,
           IMapper mapper,
            IHttpContextAccessor accessor,
             LinkGenerator generator,
             ITypeHelperService typeHelperService,
             IPropertyMappingContainer propertyMappingContainer)
        {
            this.postRepository = postRepository;
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.mapper = mapper;
            this.accessor = accessor;
            this.generator = generator;
            this.typeHelperService = typeHelperService;
            this.propertyMappingContainer = propertyMappingContainer;
            this.logger = loggerFactory.CreateLogger("Blog Api Controller.PostController");
        }

        [HttpGet(Name = "GetPosts")]
        public async Task<IActionResult> Get([FromQuery]PostParameters postParameters)
        {
          
            //throw new Exception("Error!!");
            //var v = configuration["key1"];
            //logger.LogError("Get ALl Post...");
            if (!typeHelperService.TypeHasProperties<PostDTO>(postParameters.Fields))
            {
                return BadRequest("Fields not exists");//400错误
            }
            if (!propertyMappingContainer.ValidateMappingExistsFor<PostDTO, Post>(postParameters.OrderBy))
            {
                return BadRequest("Fields not exists");
            }
            var postList = await postRepository.GetAllPostsAsync(postParameters);
            var postDTO = mapper.Map<IEnumerable<Post>, IEnumerable<PostDTO>>(postList);

            var result = postDTO.ToDynamicIEnumerable(postParameters.Fields);

            var shapeWithLinks = result.Select(x =>
            {
                var dict = x as IDictionary<string, object>;
                var dictlinks = CreateLinksForPost((int)dict["Id"], postParameters.Fields);
                dict.Add("links", dictlinks);
                return dict;
            });

            var links = CreateLinksForPosts(postParameters, postList.HasPrevious, postList.HasNext);
            
            var count= await postRepository.GetCount();
            var resultAll = new
            {
                value = shapeWithLinks,
                links,
                count= count
            };

            //var previousPageLink = postList.HasPrevious ?
            //    CreatePostUri(postParameters, PaginationResourceUriType.PreviousPage) : null;
            //var nextPageLink = postList.HasNext ?
            //     CreatePostUri(postParameters, PaginationResourceUriType.NextPage) : null;

            var meta = new
            {
                Pagesize = postList.PageSize,
                PageIndex = postList.PageIndex,     
                TotalItemsCount = postList.TotalItemsCount,
                PageCount = postList.PageCount
                //,
                //previousPageLink,
                //nextPageLink
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {//将meta匿名类属性 大写转小写
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            })); ;
            return Ok(resultAll);

        }
        [HttpGet("{id}", Name = "GetPost")]
        public async Task<IActionResult> Get(int id, string fields = null)
        {
            if (!typeHelperService.TypeHasProperties<PostDTO>(fields))
            {
                return BadRequest("Fields not exists");//400错误
            }

            var post = await postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var postResource = mapper.Map<Post, PostDTO>(post);
            var shapedPostResource = postResource.ToDynamic(fields);

            var links = CreateLinksForPost(id, fields);
            var result = shapedPostResource as IDictionary<string, object>;
            result.Add("links", links);
            return Ok(result);
        }
        [Authorize(Roles = "admin")]
        [HttpPost(Name = "CreatePost")]
        public async Task<IActionResult> Post([FromBody] PostAddResource postAddResource)
        {
            if (postAddResource == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var newPost = mapper.Map<PostAddResource, Post>(postAddResource);
            newPost.Author = "admin";
            newPost.CastModified = DateTime.Now;

            postRepository.AddPost(newPost);
            if (!await unitOfWork.SaveAsync())
            {
                throw new Exception("Save Failed");
            }
            var resultResource = mapper.Map<Post, PostDTO>(newPost);

            var links = CreateLinksForPost(newPost.Id);
            var linkPostResourse = resultResource.ToDynamic() as IDictionary<string, object>;
            linkPostResourse.Add("links", links);

            return CreatedAtRoute("GetPost", new { id = linkPostResourse["Id"] }, linkPostResourse);
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}", Name = "DeletePost")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            postRepository.Delete(post);
            if (!await unitOfWork.SaveAsync())
            {
                throw new Exception($"Delete post {id} failed when saving");
            }
            return NoContent();//204
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}",Name ="UploadPost")]
       public async Task<IActionResult> UploadPost(int id,[FromBody]PostUpdateResource postUpdate)
        {
            if (postUpdate == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var post = await postRepository.GetPostByIdAsync(id);
            if (post==null)
            {
                return NotFound();
            }
            post.CastModified = DateTime.Now;
            mapper.Map(postUpdate, post);
            if (!await unitOfWork.SaveAsync())
            {
                throw new Exception($"Updating post {id} failed when saving");
            }
            return NoContent();//204
        }

        //public async Task<IActionResult>PartiallyUpdateCityForCountry(int id, [FromBody]JsonPatchDocument<PostUpdateResource> patchDoc)
        //{
        //    if (patchDoc==null)
        //    {
        //        return BadRequest();
        //    }
        //    var post = await postRepository.GetPostByIdAsync(id);
        //    if (post==null)
        //    {
        //        return NotFound();
        //    }
        //    var postToPatch = mapper.Map<PostUpdateResource>(post);
        //    patchDoc.ApplyTo(postToPatch,ModelState);

        //    TryValidateModel(postToPatch);
        //    if (!ModelState.IsValid)
        //    {
        //        return UnprocessableEntity(ModelState);

        //    }
        //    mapper.Map(postToPatch, post);
        //    post.CastModified = DateTime.Now;
        //    postRepository.Update(post);
        //    if (!await unitOfWork.SaveAsync())
        //    {
        //        throw new Exception($"Delete post {id} failed when saving");
        //    }
        //    return NoContent();
        //}
        private string CreatePostUri(PostParameters parameters, PaginationResourceUriType uriType)
        {
            switch (uriType)
            {
                case PaginationResourceUriType.PreviousPage:
                    var previousParameters = new
                    {
                        pageIndex = parameters.PageIndex - 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return generator.GetUriByRouteValues(accessor.HttpContext,"GetPosts", previousParameters);
                     
                    
                case PaginationResourceUriType.NextPage:
                    var nextParameters = new
                    {
                        pageIndex = parameters.PageIndex + 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return generator.GetUriByRouteValues(accessor.HttpContext, "GetPosts", nextParameters);
                     
                default:
                    var currentParameters = new
                    {
                        pageIndex = parameters.PageIndex,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return  generator.GetUriByRouteValues(accessor.HttpContext, "GetPosts", currentParameters);
            }
        }

        private IEnumerable<LinkResource> CreateLinksForPost(int id, string fields = null)
        {
            var links = new List<LinkResource>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                
                  links.Add(
                    new LinkResource(
                       generator.GetUriByRouteValues(accessor.HttpContext, "GetPost", new { id }), "self", "GET"));
               
            }
            else
            {
                links.Add(
                    new LinkResource(
                        generator.GetUriByRouteValues(accessor.HttpContext, "GetPost", new { id, fields }), "self", "GET"));
            }

            links.Add(
                new LinkResource(
                    generator.GetUriByRouteValues(accessor.HttpContext, "DeletePost", new { id }), "delete_post", "DELETE"));

            return links;
        }

        private IEnumerable<LinkResource> CreateLinksForPosts(PostParameters postResourceParameters,
            bool hasPrevious, bool hasNext)
        {
            var links = new List<LinkResource>
            {
                new LinkResource(
                    CreatePostUri(postResourceParameters, PaginationResourceUriType.CurrentPage),
                    "self", "GET")
            };

            if (hasPrevious)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.PreviousPage),
                        "previous_page", "GET"));
            }

            if (hasNext)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.NextPage),
                        "next_page", "GET"));
            }

            return links;
        }

    }
}
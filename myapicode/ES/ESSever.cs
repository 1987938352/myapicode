using Core.Entitites;
using Elasticsearch.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapicode.ES
{
    public class ESSever:IESSever
    {
        public IElasticClient ElasticLinqClient { get; set; }
        
        public ESSever(IConfiguration configuration)
        {

            var uris = configuration["EsUrl"];
            List<string> list = new List<string>();
            list.Add(uris);
            var list2 = list.Select(u => new Uri(u));
            var connectionPool = new StaticConnectionPool(list2);
            var settings = new ConnectionSettings(connectionPool).RequestTimeout(TimeSpan.FromSeconds(30));

            this.ElasticLinqClient = new ElasticClient(settings);
        }
        public int EsIndex(List<Post> lists)
        {
            Result result = new Result();
            int n = 0;
            lists.ForEach(l =>
            {
                result = this.ElasticLinqClient.Index(l, t =>
                   t.Index("mypost")
                   .Type(TypeName.Create<Post>())
                   .Id(l.Id)).Result;
                if (result == Result.Created || result == Result.Updated)
                {
                    n++;
                }
            });
            return n;
        }

        public List<Post> Search(string query)
        {
            bool eb = this.ElasticLinqClient.IndexExists("mypost").Exists;
            List<Post> result = new List<Post>();
            if (eb)
            {
                var  reSer = this.ElasticLinqClient.Search<Post>(x =>
                 x.Index("mypost").Type(TypeName.Create<Post>())
                 .Query(q =>
                 q.Match(m => m.Field(
                     f => f.Title)
                 .Query(query)
                     )
                ).From(0).Size(9));
                result = reSer.Documents.ToList();
            }
            return result;
        }

    }
}

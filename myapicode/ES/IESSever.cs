using Core.Entitites;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myapicode.ES
{
    public interface IESSever
    {
        IElasticClient ElasticLinqClient { get; set; }
        int EsIndex(List<Post> lists);
        List<Post> Search(string query);
    }
}

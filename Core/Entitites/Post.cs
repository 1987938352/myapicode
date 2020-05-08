using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Core.Entitites
{
  public  class Post:Entity
    {
        
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime CastModified { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace infrastructure.DTO
{
   public class PostDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime CastModified { get; set; }
    }
}

using Core.InterFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entitites
{
  public abstract  class Entity:IEntity
    {
      public  int Id { get; set; }
    }
}

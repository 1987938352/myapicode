using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace infrastructure.DTO
{
  public  class PostAddDTOValidatator:AbstractValidator<PostAddResource>
    {
        public PostAddDTOValidatator()
        {
            RuleFor(x => x.Title)
                 .NotNull() 
                 .WithName("标题")
                 .WithMessage("{PropertyName}是必填的")
                 .MaximumLength(50)
                 .WithMessage("{PropertyName}的最大长度是{MaxLength}");

            RuleFor(x => x.Body)
                .NotNull()
                .WithName("正文")
                .WithMessage("{{PropertyName}是必填的")
                .MinimumLength(10)
                .WithMessage("{PropertyName}的最小长度是{MinLength}");
            //{PropertyName}可以代替 Name里的字符

        }
    }
}

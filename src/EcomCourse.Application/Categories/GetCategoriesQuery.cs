using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace EcomCourse.Application.Categories
{
    public sealed record GetCategoriesQuery: IRequest<IReadOnlyList<CategoryDto>>;   
}

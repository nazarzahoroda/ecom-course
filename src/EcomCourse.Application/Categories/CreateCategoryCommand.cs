using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Domain.Common;
using MediatR;

namespace EcomCourse.Application.Categories
{
    public sealed record CreateCategoryCommand(
    string Name) : IRequest<Result<Guid>>;
}

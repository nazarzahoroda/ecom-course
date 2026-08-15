using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using MediatR;

namespace EcomCourse.UnitTests
{
    public sealed record CreateFooCommand(string Name) : IRequest<string>;

    public sealed class CreateFooCommandHandler : IRequestHandler<CreateFooCommand, string>
    {
        public Task<string> Handle(CreateFooCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Foo created: {request.Name}");
        }
    }

    public sealed class CreateFooCommandValidator : AbstractValidator<CreateFooCommand>
    {
        public CreateFooCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .MinimumLength(3);
        }
    }
}

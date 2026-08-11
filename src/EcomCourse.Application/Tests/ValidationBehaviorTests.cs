using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EcomCourse.Application.Tests
{
    public sealed class ValidationBehaviorTests
    {
        [Fact]
        public async Task ValidationBehavior_ShouldInterceptInvalidRequest()
        {
            var services = new ServiceCollection();

            services.AddApplication();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<ISender>();

            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => mediator.Send(new TestRequest(string.Empty)));

            Assert.Contains(
                exception.Errors,
                error => error.PropertyName == nameof(TestRequest.Value));
        }

        private sealed record TestRequest(string Value) : IRequest<string>;

        private sealed class TestRequestValidator : AbstractValidator<TestRequest>
        {
            public TestRequestValidator()
            {
                RuleFor(x => x.Value)
                    .NotEmpty();
            }
        }
        private sealed class TestRequestHandler : IRequestHandler<TestRequest, string>
        {
            public Task<string> Handle(
                TestRequest request,
                CancellationToken cancellationToken)
            {
                throw new InvalidOperationException(
                    "Handler should not be called for an invalid request.");
            }
        }
    }
}

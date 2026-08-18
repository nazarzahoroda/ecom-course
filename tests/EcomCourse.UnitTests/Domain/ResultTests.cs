using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using EcomCourse.Domain.Common;

namespace EcomCourse.UnitTests.Domain
{
    public class ResultTests
    {
        [Fact]
        public void SuccessShouldCreateSuccessResultWhenNoErrorProvided()
        {
            var result = Result.Success();

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(DomainError.None, result.Error);
        }

        [Fact]
        public void FailureShouldCreateFailureResultWithError()
        {
            var customError = new DomainError("Test.Error", "Опис тестової помилки.");

            var result = Result.Failure(customError);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(customError, result.Error);
        }

        [Fact]
        public void GenericSuccessShouldContainValueWhenOperationSucceeds()
        {
            var expectedValue = "Hello, ECommerce!";

            var result = Result.Success(expectedValue);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedValue, result.Value);
            Assert.Equal(DomainError.None, result.Error);
        }

        [Fact]
        public void GenericFailureShouldContainErrorAndNullValueWhenOperationFails()
        {
            var error = DomainError.NullValue;

            var result = Result.Failure<string>(error);

            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Equal(error, result.Error);
        }
    }
}

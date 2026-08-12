using System;

public interface IResult
{
    bool IsSuccess { get; }
    string Error { get; }
}

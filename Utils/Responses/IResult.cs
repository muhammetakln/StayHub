namespace Utils.Responses
{
    public interface IResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public string? Details { get; }
        public int StatusCode { get; }

    }
    public interface IResult<T> : IResult
    {
        T? Data { get; }
    }
}
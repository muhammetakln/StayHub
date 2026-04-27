using System;

namespace Utils.Responses
{
    public class Result : IResult
    {
        public bool IsSuccess { get; private set; }

        
        public string Message { get; private set; }

       
        public string? Details { get; private set; }

       
        public int StatusCode { get; private set; }


        private Result(bool isSuccess, string message, string? details = null, int statusCode = 400)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            Details = details;
            StatusCode = statusCode;
        }

       
        public static IResult Success(string message = "İşlem başarılı", int statusCode = 200)
            => new Result(true, message, null, statusCode);

        
        public static IResult Success(string message, string details, int statusCode = 200)
            => new Result(true, message, details, statusCode);

        
        public static IResult Failure(string message = "İşlem başarısız", string? details = null, int statusCode = 400)
            => new Result(false, message, details, statusCode);

        
        public static IResult NotFound(string message = "Kayıt bulunamadı")
            => new Result(false, message, null, 404);

       
        public static IResult Unauthorized(string message = "Yetkilendirme başarısız")
            => new Result(false, message, null, 401);

        
        public static IResult Forbidden(string message = "Erişim reddedildi")
            => new Result(false, message, null, 403);

        public static IResult ServerError(string message = "Sunucu hatası", string? details = null)
            => new Result(false, message, details, 500);


        public override string ToString()
            => $"[{StatusCode}] {(IsSuccess ? "✓" : "✗")} {Message}" +
               (string.IsNullOrEmpty(Details) ? string.Empty : $"\nDetails: {Details}");

        public override bool Equals(object? obj)
        {
            if (obj is not Result other)
                return false;

            return IsSuccess == other.IsSuccess &&
                   Message == other.Message &&
                   Details == other.Details &&
                   StatusCode == other.StatusCode;
        }

        public override int GetHashCode()
            => HashCode.Combine(IsSuccess, Message, Details, StatusCode);
    }

   
    public class Result<T> : IResult<T>
    {
       
        public bool IsSuccess { get; private set; }

       
        public string Message { get; private set; }

       
        public string? Details { get; private set; }

       
        public int StatusCode { get; private set; }

       
        public T? Data { get; private set; }


        private Result(bool isSuccess, T? data, string message, string? details = null, int statusCode = 400)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message ?? string.Empty;
            Details = details;
            StatusCode = statusCode;
        }

        public static IResult<T> Success(T? data, string message = "İşlem başarılı", int statusCode = 200)
            => new Result<T>(true, data, message, null, statusCode);

      
        public static IResult<T> Success(T? data, string message, string details, int statusCode = 200)
            => new Result<T>(true, data, message, details, statusCode);

        
        public static IResult<T> Failure(string message = "İşlem başarısız", string? details = null, int statusCode = 400)
            => new Result<T>(false, default, message, details, statusCode);

        
        public static IResult<T> NotFound(string message = "Kayıt bulunamadı")
            => new Result<T>(false, default, message, null, 404);

        public static IResult<T> Unauthorized(string message = "Yetkilendirme başarısız")
            => new Result<T>(false, default, message, null, 401);

       
        public static IResult<T> Forbidden(string message = "Erişim reddedildi")
            => new Result<T>(false, default, message, null, 403);

        public static IResult<T> ServerError(string message = "Sunucu hatası", string? details = null)
            => new Result<T>(false, default, message, details, 500);


        public override string ToString()
            => $"[{StatusCode}] {(IsSuccess ? "✓" : "✗")} {Message}" +
               (Data != null ? $"\nData: {Data}" : string.Empty) +
               (string.IsNullOrEmpty(Details) ? string.Empty : $"\nDetails: {Details}");

        public override bool Equals(object? obj)
        {
            if (obj is not Result<T> other)
                return false;

            return IsSuccess == other.IsSuccess &&
                   Message == other.Message &&
                   Details == other.Details &&
                   StatusCode == other.StatusCode &&
                   (Data?.Equals(other.Data) ?? other.Data == null);
        }

        public override int GetHashCode()
            => HashCode.Combine(IsSuccess, Message, Details, StatusCode, Data);
    }

    
}
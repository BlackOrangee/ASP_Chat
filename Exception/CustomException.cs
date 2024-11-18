namespace ASP_Chat.Exception
{
    public class CustomException : System.Exception
    {
        public ExceptionCodes Code { get; set; }

        public enum ExceptionCodes {
            UserNotFound = 1,
            UserAlreadyExists = 2,
            InvalidCredentials = 3
        }

        public StatusCodes StatusCode { get; set; }
        
        public enum StatusCodes {
            BadRequest = 400,
            Unauthorized = 401,
            Forbidden = 403,
            NotFound = 404,
            InternalServerError = 500
        }

        public CustomException(string message, ExceptionCodes code, StatusCodes statusCode) : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}

namespace ASP_Chat.Exception
{
    public class CustomException : System.Exception
    {
        public ExceptionCodes Code { get; set; }
        public StatusCodes StatusCode { get; set; }

        public enum ExceptionCodes {
            SecretKeyNotSet = 0,
            UserNotFound,
            UsersNotFound,
            UserAlreadyExists,
            InvalidCredentials,
            ChatTypeNotFound,
            GroupNameIsEmpty,
            ChannelNameIsEmpty,
            ChannelTagIsEmpty,
        }
        
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

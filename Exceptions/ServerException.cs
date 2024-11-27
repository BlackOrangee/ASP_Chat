namespace ASP_Chat.Exceptions
{
    public class ServerException : System.Exception
    {
        public ExceptionCodes Code { get; set; }
        public StatusCodes StatusCode { get; set; }

        public ServerException(string message, ExceptionCodes code, StatusCodes statusCode) : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}

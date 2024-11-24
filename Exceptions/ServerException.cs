namespace ASP_Chat.Exceptions
{
    public class ServerException : System.Exception
    {
        public ExceptionCodes Code { get; set; }
        public StatusCodes StatusCode { get; set; }

        public enum ExceptionCodes {
            SecretKeyNotSet = 0,
            EmptyCredentials,
            InvalidCredentials,
            InvalidToken,
            UserNotFound,
            UserNotAdmin,
            UserNotInChat,
            UserAlreadyExists,
            UserAlreadyModerator,
            UsersNotFound,
            ChatNotFound,
            ChatTypeNotFound,
            ChatCanNotHaveUsers,
            ChatCanNotHaveModerators,
            ChatCanNotBeUpdated,
            GroupNameIsEmpty,
            ChannelNameIsEmpty,
            ChannelTagIsEmpty,
            MessageNotFound,
            MessageIsEmpty,
            NoPermissionToSendMessage,
            NoPermissionToEditMessage,
            NoPermissionToDeleteMessage,
        }
        
        public enum StatusCodes {
            BadRequest = 400,
            Unauthorized = 401,
            Forbidden = 403,
            NotFound = 404,
            InternalServerError = 500
        }

        public ServerException(string message, ExceptionCodes code, StatusCodes statusCode) : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}

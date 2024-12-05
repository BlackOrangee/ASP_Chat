namespace ASP_Chat.Exceptions
{
    public class ServerExceptionFactory
    {
        public static ServerException SecretKeyNotSet()
        {
            return new ServerException("JWT secret key is not set",
                ExceptionCodes.SecretKeyNotSet,
                StatusCodes.InternalServerError);
        }

        public static ServerException InvalidCredentials()
        {
            return new ServerException("Invalid credentials",
                ExceptionCodes.InvalidCredentials,
                StatusCodes.Unauthorized);
        }

        public static ServerException InvalidToken()
        {
            return new ServerException("Invalid token",
                ExceptionCodes.InvalidToken,
                StatusCodes.Unauthorized);
        }

        public static ServerException UserNotFound()
        {
            return new ServerException("User not found",
                ExceptionCodes.UserNotFound,
                StatusCodes.NotFound);
        }

        public static ServerException UserNotAdmin()
        {
            return new ServerException("User is not admin",
                ExceptionCodes.UserNotAdmin,
                StatusCodes.Forbidden);
        }

        public static ServerException UserNotInChat()
        {
            return new ServerException("User is not in chat",
                ExceptionCodes.UserNotInChat,
                StatusCodes.Forbidden);
        }

        public static ServerException UserCanNotLeaveChatAsAdmin()
        {
            return new ServerException("User can not leave chat as admin",
                ExceptionCodes.UserCanNotLeaveChatAsAdmin,
                StatusCodes.Forbidden);
        }

        public static ServerException UserAlreadyExists()
        {
            return new ServerException("User already exists",
                ExceptionCodes.UserAlreadyExists,
                StatusCodes.BadRequest);
        }

        public static ServerException UserAlreadyInChat()
        {
            return new ServerException("User already in chat",
                ExceptionCodes.UserAlreadyInChat,
                StatusCodes.BadRequest);
        }

        public static ServerException UserAlreadyModerator()
        {
            return new ServerException("User already moderator",
                ExceptionCodes.UserAlreadyModerator,
                StatusCodes.BadRequest);
        }

        public static ServerException UsersNotFound()
        {
            return new ServerException("Users not found",
                ExceptionCodes.UsersNotFound,
                StatusCodes.NotFound);
        }

        public static ServerException ChatNotFound()
        {
            return new ServerException("Chat not found",
                ExceptionCodes.ChatNotFound,
                StatusCodes.NotFound);
        }

        public static ServerException ChatAlreadyExists()
        {
            return new ServerException("Chat already exists",
                ExceptionCodes.ChatAlreadyExists,
                StatusCodes.BadRequest);
        }

        public static ServerException InvalidP2PChatUsersCount()
        {
            return new ServerException("Invalid P2P chat users count",
                ExceptionCodes.InvalidP2PChatUsersCount,
                StatusCodes.BadRequest);
        }

        public static ServerException ChatNotPublic()
        {
            return new ServerException("Chat is not public",
                ExceptionCodes.ChatNotPublic,
                StatusCodes.Forbidden);
        }

        public static ServerException ChatTypeNotFound()
        {
            return new ServerException("Chat type not found",
                ExceptionCodes.ChatTypeNotFound,
                StatusCodes.NotFound);
        }

        public static ServerException ChatCanNotHaveUsers()
        {
            return new ServerException("Chat can not have users",
                ExceptionCodes.ChatCanNotHaveUsers,
                StatusCodes.BadRequest);
        }

        public static ServerException ChatCanNotHaveModerators()
        {
            return new ServerException("Chat can not have moderators",
                ExceptionCodes.ChatCanNotHaveModerators,
                StatusCodes.BadRequest);
        }

        public static ServerException ChatCanNotBeUpdated()
        {
            return new ServerException("Chat can not be updated",
                ExceptionCodes.ChatCanNotBeUpdated,
                StatusCodes.BadRequest);
        }

        public static ServerException MessageNotFound()
        {
            return new ServerException("Message not found",
                ExceptionCodes.MessageNotFound,
                StatusCodes.NotFound);
        }

        public static ServerException NoPermissionToSendMessage()
        {
            return new ServerException("No permission to send message",
                ExceptionCodes.NoPermissionToSendMessage,
                StatusCodes.Forbidden);
        }

        public static ServerException NoPermissionToEditMessage()
        {
            return new ServerException("No permission to edit message",
                ExceptionCodes.NoPermissionToEditMessage,
                StatusCodes.Forbidden);
        }

        public static ServerException NoPermissionToDeleteMessage()
        {
            return new ServerException("No permission to delete message",
                ExceptionCodes.NoPermissionToDeleteMessage,
                StatusCodes.Forbidden);
        }

        public static ServerException UniqueNameIsTaken(string uniqueName)
        {
            return new ServerException($"UniqueName {uniqueName} is taken",
                ExceptionCodes.UniqueNameIsTaken,
                StatusCodes.BadRequest);
        }
    }
}

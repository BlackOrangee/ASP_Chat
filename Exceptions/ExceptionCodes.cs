namespace ASP_Chat.Exceptions
{
    public enum ExceptionCodes
    {
        InternalServerError = 0,
        ValidationError,
        SecretKeyNotSet,
        InvalidCredentials,
        InvalidToken,
        InvalidP2PChatUsersCount,
        UniqueNameIsTaken,
        UserNotFound,
        UserNotAdmin,
        UserNotInChat,
        UserCanNotLeaveChatAsAdmin,
        UserAlreadyExists,
        UserAlreadyInChat,
        UserAlreadyModerator,
        UsersNotFound,
        ChatNotFound,
        ChatNotPublic,
        ChatAlreadyExists,
        ChatTypeNotFound,
        ChatCanNotHaveUsers,
        ChatCanNotHaveModerators,
        ChatCanNotBeUpdated,
        MessageNotFound,
        MediaNotFound,
        NoPermissionToSendMessage,
        NoPermissionToEditMessage,
        NoPermissionToDeleteMessage,
        NoPermissionToGetMediaLink,
    }
}

namespace ASP_Chat.Exceptions
{
    public enum ExceptionCodes
    {
        SecretKeyNotSet = 0,
        EmptyCredentials,
        InvalidCredentials,
        InvalidToken,
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
}

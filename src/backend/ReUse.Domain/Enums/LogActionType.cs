namespace ReUse.Domain.Enums;

public enum LogActionType
{
    Login,
    Logout,
    LoginFailed,
    PasswordReset,
    TokenRefresh,

    UserCreated,
    UserUpdated,
    UserDeactivated,
    UserReactivated,
    UserDeleted,
    RoleAssigned,
    RoleRevoked,

    ProductApproved,
    ProductRejected,
    ProductDeleted,
    CommentDeleted,
    FeedbackDeleted,
    CategoryCreated,
    CategoryUpdated,
    CategoryDeleted,

    ReportCreated,
    ReportReviewed,

    SettingUpdated,
    DataExported,

    Other
}
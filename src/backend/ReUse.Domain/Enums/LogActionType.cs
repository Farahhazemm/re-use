namespace ReUse.Domain.Enums;

public enum LogActionType
{
    // Authentication
    Login,
    Logout,
    LoginFailed,
    PasswordReset,
    TokenRefresh,

    // User Management
    UserCreated,
    UserUpdated,
    UserDeactivated,
    UserReactivated,
    UserDeleted,
    RoleAssigned,
    RoleRevoked,

    // Content Moderation
    ProductApproved,
    ProductRejected,
    ProductDeleted,
    CommentDeleted,
    FeedbackDeleted,
    CategoryCreated,
    CategoryUpdated,
    CategoryDeleted,

    // System / Configuration
    SettingUpdated,
    DataExported,

    // General / fallback
    Other
}
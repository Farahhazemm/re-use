namespace ReUse.Domain.Enums;

public enum LogActionType
{
    // Authentication
    Login,
    Logout,
    LoginFailed,
    PasswordChanged,
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

    // Content / Product Moderation
    ProductApproved,
    ProductRejected,
    ProductDeleted,
    ProductRestored,
    PremiumGranted,
    PremiumRemoved,
    CommentDeleted,
    FeedbackDeleted,
    CategoryCreated,
    CategoryUpdated,
    CategoryDeleted,

    // Reports
    ReportCreated,
    ReportReviewed,

    // Security
    UnauthorizedAccess,
    PermissionDenied,

    // Payments
    PaymentSuccess,
    PaymentFailed,

    // System / Infrastructure
    SettingUpdated,
    DataExported,
    InfrastructureFailure,
    UnhandledException,

    Other
}
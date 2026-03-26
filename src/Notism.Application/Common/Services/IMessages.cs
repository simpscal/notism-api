namespace Notism.Application.Common.Services;

public interface IMessages
{
    // Auth Messages
    string UserAlreadyExists { get; }
    string ErrorCreatingUser { get; }
    string InvalidCredentials { get; }
    string InvalidOrExpiredResetToken { get; }
    string UserNotFound { get; }
    string PasswordResetEmailSent { get; }
    string PasswordResetSuccess { get; }
    string RequestPasswordResetFailed { get; }
    string RefreshTokenNotFound { get; }

    // Food/Category Messages
    string CategoryNotFound { get; }
    string CategoryAlreadyExists { get; }
    string FoodNotFound { get; }
    string FoodNotAvailable { get; }
    string DiscountPriceMustBeLess { get; }

    // Cart Messages
    string InsufficientStock { get; }
    string CartItemNotFound { get; }
    string CartItemNotBelongToUser { get; }

    // Order Messages
    string OrderNotFound { get; }
    string NoCartItemsFound { get; }
    string CartItemsNotFound { get; }

    // User/Admin Messages
    string CannotDeleteOwnAccount { get; }
    string CannotDeleteAdmin { get; }
    string CannotUpdateOwnRole { get; }

    // Shared Messages
    string FailedToDeleteFile { get; }
    string UnexpectedError { get; }
    string LoggedOutSuccessfully { get; }

    // Validation Messages
    string PasswordRequired { get; }
    string PasswordMinLength { get; }
    string PasswordSpecialChar { get; }
    string PasswordUppercase { get; }
    string PasswordNumber { get; }
    string PasswordsDoNotMatch { get; }
    string FieldRequired { get; }
    string InvalidValue { get; }
    string SkipMustBeNonNegative { get; }
    string TakeMustBeBetween { get; }
    string UserIdRequired { get; }
    string FoodIdRequired { get; }
    string QuantityMustBeGreaterThanZero { get; }
}

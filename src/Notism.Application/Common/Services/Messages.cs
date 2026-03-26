using Microsoft.Extensions.Localization;

namespace Notism.Application.Common.Services;

public class Messages : IMessages
{
    private readonly IStringLocalizer<Messages> _localizer;

    public Messages(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
    }

    // Auth Messages
    public string UserAlreadyExists => _localizer["UserAlreadyExists"];
    public string ErrorCreatingUser => _localizer["ErrorCreatingUser"];
    public string InvalidCredentials => _localizer["InvalidCredentials"];
    public string InvalidOrExpiredResetToken => _localizer["InvalidOrExpiredResetToken"];
    public string UserNotFound => _localizer["UserNotFound"];
    public string PasswordResetEmailSent => _localizer["PasswordResetEmailSent"];
    public string PasswordResetSuccess => _localizer["PasswordResetSuccess"];
    public string RequestPasswordResetFailed => _localizer["RequestPasswordResetFailed"];
    public string RefreshTokenNotFound => _localizer["RefreshTokenNotFound"];

    // Food/Category Messages
    public string CategoryNotFound => _localizer["CategoryNotFound"];
    public string CategoryAlreadyExists => _localizer["CategoryAlreadyExists"];
    public string FoodNotFound => _localizer["FoodNotFound"];
    public string FoodNotAvailable => _localizer["FoodNotAvailable"];
    public string DiscountPriceMustBeLess => _localizer["DiscountPriceMustBeLess"];

    // Cart Messages
    public string InsufficientStock => _localizer["InsufficientStock"];
    public string CartItemNotFound => _localizer["CartItemNotFound"];
    public string CartItemNotBelongToUser => _localizer["CartItemNotBelongToUser"];

    // Order Messages
    public string OrderNotFound => _localizer["OrderNotFound"];
    public string NoCartItemsFound => _localizer["NoCartItemsFound"];
    public string CartItemsNotFound => _localizer["CartItemsNotFound"];

    // User/Admin Messages
    public string CannotDeleteOwnAccount => _localizer["CannotDeleteOwnAccount"];
    public string CannotDeleteAdmin => _localizer["CannotDeleteAdmin"];
    public string CannotUpdateOwnRole => _localizer["CannotUpdateOwnRole"];

    // Shared Messages
    public string FailedToDeleteFile => _localizer["FailedToDeleteFile"];
    public string UnexpectedError => _localizer["UnexpectedError"];
    public string LoggedOutSuccessfully => _localizer["LoggedOutSuccessfully"];

    // Validation Messages
    public string PasswordRequired => _localizer["PasswordRequired"];
    public string PasswordMinLength => _localizer["PasswordMinLength"];
    public string PasswordSpecialChar => _localizer["PasswordSpecialChar"];
    public string PasswordUppercase => _localizer["PasswordUppercase"];
    public string PasswordNumber => _localizer["PasswordNumber"];
    public string PasswordsDoNotMatch => _localizer["PasswordsDoNotMatch"];
    public string FieldRequired => _localizer["FieldRequired"];
    public string InvalidValue => _localizer["InvalidValue"];
    public string SkipMustBeNonNegative => _localizer["SkipMustBeNonNegative"];
    public string TakeMustBeBetween => _localizer["TakeMustBeBetween"];
    public string UserIdRequired => _localizer["UserId Required"];
    public string FoodIdRequired => _localizer["FoodId Required"];
    public string QuantityMustBeGreaterThanZero => _localizer["QuantityMustBeGreaterThanZero"];
}

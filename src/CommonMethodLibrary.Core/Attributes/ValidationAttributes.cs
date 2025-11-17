using System.ComponentModel.DataAnnotations;
using CommonMethodLibrary.Core.Helpers;

namespace CommonMethodLibrary.Core.Attributes;

/// <summary>
/// 手机号验证特性
/// </summary>
public class PhoneNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var phoneNumber = value.ToString()!;
        if (!ValidationHelper.IsValidPhoneNumber(phoneNumber))
        {
            return new ValidationResult(ErrorMessage ?? "手机号格式不正确");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// 邮箱验证特性
/// </summary>
public class EmailAddressExAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var email = value.ToString()!;
        if (!ValidationHelper.IsValidEmail(email))
        {
            return new ValidationResult(ErrorMessage ?? "邮箱格式不正确");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// 身份证号验证特性
/// </summary>
public class IdCardAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var idCard = value.ToString()!;
        if (!ValidationHelper.IsValidIdCard(idCard))
        {
            return new ValidationResult(ErrorMessage ?? "身份证号格式不正确");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// URL验证特性
/// </summary>
public class UrlAddressAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var url = value.ToString()!;
        if (!ValidationHelper.IsValidUrl(url))
        {
            return new ValidationResult(ErrorMessage ?? "URL格式不正确");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// 强密码验证特性
/// </summary>
public class StrongPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var password = value.ToString()!;
        if (!ValidationHelper.IsStrongPassword(password))
        {
            return new ValidationResult(ErrorMessage ?? "密码强度不够：至少8位，包含大小写字母、数字和特殊字符");
        }

        return ValidationResult.Success;
    }
}

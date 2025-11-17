using System.ComponentModel.DataAnnotations;
using CommonMethodLibrary.Core.Attributes;

namespace CommonMethodLibrary.WebAPI.Models.DTOs;

/// <summary>
/// 创建用户DTO
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddressEx(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    [PhoneNumber(ErrorMessage = "手机号格式不正确")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [StrongPassword(ErrorMessage = "密码必须至少8位，包含大小写字母、数字和特殊字符")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [StringLength(50)]
    public string? RealName { get; set; }

    /// <summary>
    /// 年龄
    /// </summary>
    [Range(1, 150, ErrorMessage = "年龄必须在1-150之间")]
    public int? Age { get; set; }
}

using System.ComponentModel.DataAnnotations;
using CommonMethodLibrary.Core.Attributes;

namespace CommonMethodLibrary.WebAPI.Models.DTOs;

/// <summary>
/// 更新用户DTO
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
    public string? Username { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddressEx(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [PhoneNumber(ErrorMessage = "手机号格式不正确")]
    public string? PhoneNumber { get; set; }

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

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
}

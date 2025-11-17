using CommonMethodLibrary.Core.Helpers;
using CommonMethodLibrary.WebAPI.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace CommonMethodLibrary.WebAPI.Controllers;

/// <summary>
/// 工具类演示控制器
/// 展示所有工具类的使用方法
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ToolsDemoController : ControllerBase
{
    private readonly ILogger<ToolsDemoController> _logger;
    private readonly HttpHelper _httpHelper;
    private readonly CacheHelper _cache;

    public ToolsDemoController(
        ILogger<ToolsDemoController> logger,
        HttpHelper httpHelper,
        CacheHelper cache)
    {
        _logger = logger;
        _httpHelper = httpHelper;
        _cache = cache;
    }

    #region StringHelper 示例

    /// <summary>
    /// 字符串工具演示
    /// </summary>
    [HttpGet("string")]
    public ActionResult<ApiResponse<object>> StringHelperDemo([FromQuery] string? input = "HelloWorld")
    {
        var result = new
        {
            Original = input,
            CamelCase = StringHelper.ToCamelCase(input ?? ""),
            PascalCase = StringHelper.ToPascalCase(input ?? ""),
            SnakeCase = StringHelper.ToSnakeCase(input ?? ""),
            Truncated = StringHelper.Truncate(input ?? "", 5),
            RandomString = StringHelper.GenerateRandomString(10, true, true),
            MaskedPhone = StringHelper.MaskSensitiveInfo("13800138000", 3, 4),
            Base64 = StringHelper.ToBase64(input ?? "")
        };

        return Ok(ApiResponse<object>.Ok(result, "字符串工具演示"));
    }

    #endregion

    #region DateTimeHelper 示例

    /// <summary>
    /// 日期时间工具演示
    /// </summary>
    [HttpGet("datetime")]
    public ActionResult<ApiResponse<object>> DateTimeHelperDemo()
    {
        var now = DateTime.Now;
        var birthDate = new DateTime(1990, 5, 15);

        var result = new
        {
            CurrentTimestamp = DateTimeHelper.GetCurrentTimestamp(),
            CurrentTimestampMs = DateTimeHelper.GetCurrentTimestampMilliseconds(),
            FriendlyTime = DateTimeHelper.GetFriendlyTimeSpan(now.AddHours(-2)),
            IsWorkday = DateTimeHelper.IsWorkday(now),
            StartOfWeek = DateTimeHelper.GetStartOfWeek(now),
            StartOfMonth = DateTimeHelper.GetStartOfMonth(now),
            EndOfMonth = DateTimeHelper.GetEndOfMonth(now),
            Age = DateTimeHelper.CalculateAge(birthDate),
            Iso8601 = DateTimeHelper.ToIso8601(now)
        };

        return Ok(ApiResponse<object>.Ok(result, "日期时间工具演示"));
    }

    #endregion

    #region ValidationHelper 示例

    /// <summary>
    /// 验证工具演示
    /// </summary>
    [HttpPost("validation")]
    public ActionResult<ApiResponse<object>> ValidationHelperDemo([FromBody] ValidationDemoInput input)
    {
        var result = new
        {
            IsValidEmail = ValidationHelper.IsValidEmail(input.Email ?? ""),
            IsValidPhone = ValidationHelper.IsValidPhoneNumber(input.Phone ?? ""),
            IsValidIdCard = ValidationHelper.IsValidIdCard(input.IdCard ?? ""),
            IsValidUrl = ValidationHelper.IsValidUrl(input.Url ?? ""),
            IsValidIP = ValidationHelper.IsValidIPAddress(input.IpAddress ?? ""),
            IsStrongPassword = ValidationHelper.IsStrongPassword(input.Password ?? ""),
            IsNumeric = ValidationHelper.IsNumeric(input.NumberString ?? ""),
            IsAlphaNumeric = ValidationHelper.IsAlphaNumeric(input.AlphaNumString ?? "")
        };

        return Ok(ApiResponse<object>.Ok(result, "验证工具演示"));
    }

    #endregion

    #region JsonHelper 示例

    /// <summary>
    /// JSON工具演示
    /// </summary>
    [HttpPost("json")]
    public ActionResult<ApiResponse<object>> JsonHelperDemo([FromBody] object input)
    {
        var json = JsonHelper.Serialize(input);
        var prettyJson = JsonHelper.Serialize(input, pretty: true);
        var isValid = JsonHelper.IsValidJson(json);
        var cloned = JsonHelper.DeepClone(input);

        var result = new
        {
            Serialized = json,
            PrettyJson = prettyJson,
            IsValidJson = isValid,
            ClonedObject = cloned
        };

        return Ok(ApiResponse<object>.Ok(result, "JSON工具演示"));
    }

    #endregion

    #region EncryptionHelper 示例

    /// <summary>
    /// 加密工具演示
    /// </summary>
    [HttpPost("encryption")]
    public ActionResult<ApiResponse<object>> EncryptionHelperDemo([FromBody] EncryptionDemoInput input)
    {
        var plainText = input.Text ?? "Hello, World!";
        var key = "MySecretKey12345";
        var iv = "MyInitVector123";

        var result = new
        {
            Original = plainText,
            MD5 = EncryptionHelper.Md5(plainText),
            SHA256 = EncryptionHelper.Sha256(plainText),
            SHA512 = EncryptionHelper.Sha512(plainText),
            AESEncrypted = EncryptionHelper.AesEncrypt(plainText, key, iv),
            GUID = EncryptionHelper.GenerateGuid(),
            ShortGUID = EncryptionHelper.GenerateShortGuid(),
            RandomKey = EncryptionHelper.GenerateRandomKey(16),
            HmacSha256 = EncryptionHelper.HmacSha256(plainText, key)
        };

        return Ok(ApiResponse<object>.Ok(result, "加密工具演示"));
    }

    /// <summary>
    /// AES解密演示
    /// </summary>
    [HttpPost("encryption/decrypt")]
    public ActionResult<ApiResponse<object>> AesDecryptDemo([FromBody] AesDecryptInput input)
    {
        try
        {
            var decrypted = EncryptionHelper.AesDecrypt(
                input.CipherText,
                input.Key ?? "MySecretKey12345",
                input.IV ?? "MyInitVector123"
            );

            return Ok(ApiResponse<object>.Ok(new { Decrypted = decrypted }, "AES解密成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail($"解密失败: {ex.Message}"));
        }
    }

    #endregion

    #region CacheHelper 示例

    /// <summary>
    /// 缓存工具演示 - 设置缓存
    /// </summary>
    [HttpPost("cache/set")]
    public ActionResult<ApiResponse<object>> CacheSetDemo([FromBody] CacheSetInput input)
    {
        _cache.Set(input.Key, input.Value, TimeSpan.FromMinutes(input.ExpirationMinutes ?? 5));

        return Ok(ApiResponse<object>.Ok(new
        {
            Message = "缓存设置成功",
            Key = input.Key,
            ExpiresIn = $"{input.ExpirationMinutes ?? 5} 分钟"
        }));
    }

    /// <summary>
    /// 缓存工具演示 - 获取缓存
    /// </summary>
    [HttpGet("cache/get/{key}")]
    public ActionResult<ApiResponse<object>> CacheGetDemo(string key)
    {
        var value = _cache.Get<object>(key);

        if (value == null)
        {
            return NotFound(ApiResponse<object>.Fail("缓存不存在或已过期"));
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            Key = key,
            Value = value
        }));
    }

    /// <summary>
    /// 缓存工具演示 - 获取所有缓存键
    /// </summary>
    [HttpGet("cache/keys")]
    public ActionResult<ApiResponse<object>> CacheKeysDemo()
    {
        var keys = _cache.GetAllKeys().ToList();

        return Ok(ApiResponse<object>.Ok(new
        {
            TotalKeys = keys.Count,
            Keys = keys
        }));
    }

    #endregion

    #region FileHelper 示例

    /// <summary>
    /// 文件工具演示
    /// </summary>
    [HttpGet("file")]
    public ActionResult<ApiResponse<object>> FileHelperDemo()
    {
        var result = new
        {
            FriendlySize1KB = FileHelper.GetFriendlyFileSize(1024),
            FriendlySize1MB = FileHelper.GetFriendlyFileSize(1024 * 1024),
            FriendlySize1GB = FileHelper.GetFriendlyFileSize(1024 * 1024 * 1024),
            FileExtension = FileHelper.GetFileExtension("document.pdf"),
            FileNameWithoutExt = FileHelper.GetFileNameWithoutExtension("/path/to/document.pdf")
        };

        return Ok(ApiResponse<object>.Ok(result, "文件工具演示"));
    }

    #endregion
}

#region Input Models

public class ValidationDemoInput
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? IdCard { get; set; }
    public string? Url { get; set; }
    public string? IpAddress { get; set; }
    public string? Password { get; set; }
    public string? NumberString { get; set; }
    public string? AlphaNumString { get; set; }
}

public class EncryptionDemoInput
{
    public string? Text { get; set; }
}

public class AesDecryptInput
{
    public string CipherText { get; set; } = string.Empty;
    public string? Key { get; set; }
    public string? IV { get; set; }
}

public class CacheSetInput
{
    public string Key { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public int? ExpirationMinutes { get; set; }
}

#endregion

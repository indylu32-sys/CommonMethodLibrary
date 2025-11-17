using CommonMethodLibrary.WebAPI.Models.Common;
using CommonMethodLibrary.WebAPI.Models.DTOs;
using CommonMethodLibrary.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommonMethodLibrary.WebAPI.Controllers;

/// <summary>
/// 用户管理控制器
/// 演示完整的RESTful CRUD操作
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有用户
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(u => MapToDto(u)).ToList();

            return Ok(ApiResponse<List<UserDto>>.Ok(userDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有用户失败");
            return StatusCode(500, ApiResponse<List<UserDto>>.Fail("获取用户列表失败"));
        }
    }

    /// <summary>
    /// 分页获取用户
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<UserDto>>> GetPagedUsers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var (users, total) = await _userService.GetPagedUsersAsync(pageIndex, pageSize, searchTerm);
            var userDtos = users.Select(u => MapToDto(u)).ToList();

            return Ok(PagedResponse<UserDto>.Ok(userDtos, total, pageIndex, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取用户失败");
            return StatusCode(500, ApiResponse<List<UserDto>>.Fail("获取用户列表失败"));
        }
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.Fail("用户不存在", "USER_NOT_FOUND"));
            }

            return Ok(ApiResponse<UserDto>.Ok(MapToDto(user)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户失败: {UserId}", id);
            return StatusCode(500, ApiResponse<UserDto>.Fail("获取用户失败"));
        }
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            // 检查用户名是否已存在
            if (await _userService.UsernameExistsAsync(dto.Username))
            {
                return BadRequest(ApiResponse<UserDto>.Fail("用户名已存在", "USERNAME_EXISTS"));
            }

            // 检查邮箱是否已存在
            if (await _userService.EmailExistsAsync(dto.Email))
            {
                return BadRequest(ApiResponse<UserDto>.Fail("邮箱已存在", "EMAIL_EXISTS"));
            }

            var user = await _userService.CreateUserAsync(dto);
            var userDto = MapToDto(user);

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                ApiResponse<UserDto>.Ok(userDto, "用户创建成功")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户失败");
            return StatusCode(500, ApiResponse<UserDto>.Fail("创建用户失败"));
        }
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.Fail("用户不存在", "USER_NOT_FOUND"));
            }

            return Ok(ApiResponse<UserDto>.Ok(MapToDto(user), "用户更新成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新用户失败: {UserId}", id);
            return StatusCode(500, ApiResponse<UserDto>.Fail("更新用户失败"));
        }
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail("用户不存在", "USER_NOT_FOUND"));
            }

            return Ok(ApiResponse<object>.Ok(null, "用户删除成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除用户失败: {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("删除用户失败"));
        }
    }

    private static UserDto MapToDto(Models.Entities.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RealName = user.RealName,
            Age = user.Age,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}

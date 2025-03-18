using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Interfaces.Services;

namespace Rendezvous.API.Controllers;

public class AccountController(UserManager<AppUser> userManager,
    ITokenService tokenService, IMapper mapper) : BaseApiController
{
    // POST: /api/account/register
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username))
        {
            return BadRequest("Username is taken.");
        }

        var user = mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();

        var result = await userManager.CreateAsync(user, registerDto.Password);
        await userManager.AddToRoleAsync(user, "Member");

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = await tokenService.CreateTokenAsync(user)
        });
    }

    // POST: /api/account/login
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.Users
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == loginDto.Username.ToUpper());

        if (user == null || user.UserName == null)
        {
            return Unauthorized("Invalid username.");
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
        {
            return Unauthorized();
        }

        return Ok(new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = await tokenService.CreateTokenAsync(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
        });
    }

    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users
            .AnyAsync(u => u.NormalizedUserName == username.ToUpper());
    }
}

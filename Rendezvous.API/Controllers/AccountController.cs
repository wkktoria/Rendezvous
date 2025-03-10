using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.Data;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    // POST: /api/account/register
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username))
        {
            return BadRequest("Username is taken.");
        }

        using var hmac = new HMACSHA512();

        var user = mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = tokenService.CreateToken(user)
        });
    }

    // POST: /api/account/login
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == loginDto.Username.ToLower());

        if (user == null)
        {
            return Unauthorized("Invalid username.");
        }

        return Ok(new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
        });
    }

    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
    }
}

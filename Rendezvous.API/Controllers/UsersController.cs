using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rendezvous.API.DTOs;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    // GET: /api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();
        return Ok(users);
    }

    // GET: /api/users/bob
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberAsync(username);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    // PUT: /api/users
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (username == null)
        {
            return BadRequest("No username found in token.");
        }

        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return BadRequest("Could not find user.");
        }

        mapper.Map(memberUpdateDto, user);

        if (await userRepository.SaveAllAsync())
        {
            return NoContent();
        }

        return BadRequest("Failed to update the user.");
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.Data;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository) : BaseApiController
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
}

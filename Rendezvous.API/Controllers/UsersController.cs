using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Extensions;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Controllers;

[Authorize]
public class UsersController(
    IUserRepository userRepository,
    IMapper mapper, IPhotoService photoService) : BaseApiController
{
    // GET: /api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        var users = await userRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(users);
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
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

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

    // POST: /api/users/add-photo
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Cannot update user.");
        }

        var result = await photoService.AddPhotoAsync(file);

        if (result.Error != null)
        {
            return BadRequest(result.Error.Message);
        }

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        if (await userRepository.SaveAllAsync())
        {
            return CreatedAtAction(nameof(GetUser),
                new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
        }

        return BadRequest("Problem with adding photo.");
    }

    // PUT: /api/users/set-main-photo/11
    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Could not find user.");
        }

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain)
        {
            return BadRequest("Cannot use this as main photo.");
        }

        var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);

        if (currentMain != null)
        {
            currentMain.IsMain = false;
        }

        photo.IsMain = true;

        if (await userRepository.SaveAllAsync())
        {
            return NoContent();
        }

        return BadRequest("Problem with setting main photo");
    }

    // DELETE: /api/users/delete-photo/11
    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain)
        {
            return BadRequest("This photo cannot be deleted.");
        }

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
        }

        user.Photos.Remove(photo);

        if (await userRepository.SaveAllAsync())
        {
            return Ok();
        }

        return BadRequest("Problem with deleting photo.");
    }
}

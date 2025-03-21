using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Extensions;
using Rendezvous.API.Helpers;
using Rendezvous.API.Interfaces;
using Rendezvous.API.Interfaces.Services;

namespace Rendezvous.API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork,
    IMapper mapper, IPhotoService photoService) : BaseApiController
{
    // GET: /api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(users);
        return Ok(users);
    }

    // GET: /api/users/bob
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var currentUsername = User.GetUsername();
        var user = await unitOfWork.UserRepository.GetMemberAsync(username, currentUsername == username);

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
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Could not find user.");
        }

        mapper.Map(memberUpdateDto, user);

        if (await unitOfWork.CompleteAsync())
        {
            return NoContent();
        }

        return BadRequest("Failed to update the user.");
    }

    // POST: /api/users/add-photo
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

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

        user.Photos.Add(photo);

        if (await unitOfWork.CompleteAsync())
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
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

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

        if (await unitOfWork.CompleteAsync())
        {
            return NoContent();
        }

        return BadRequest("Problem with setting main photo");
    }

    // DELETE: /api/users/delete-photo/11
    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);

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

        if (await unitOfWork.CompleteAsync())
        {
            return Ok();
        }

        return BadRequest("Problem with deleting photo.");
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rendezvous.API.Interfaces;
using Rendezvous.API.Entities;
using Rendezvous.API.Interfaces.Services;

namespace Rendezvous.API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork,
    IPhotoService photoService) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRolesAsync()
    {
        var users = await userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles))
        {
            return BadRequest("You must select at least one role.");
        }

        var selectedRoles = roles.Split(",").ToArray();
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded)
        {
            return BadRequest("Failed to add user to roles.");
        }

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded)
        {
            return BadRequest("Failed to remove user from roles.");
        }

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModerationAsync()
    {
        var photos = await unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync();
        return Ok(photos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId:int}")]
    public async Task<ActionResult> ApprovePhotoAsync(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);

        if (photo == null)
        {
            return BadRequest("Could not get photo from database.");
        }

        photo.IsApproved = true;

        var user = await unitOfWork.UserRepository.GetUserByPhotoIdAsync(photoId);

        if (user == null)
        {
            return BadRequest("Could not get user from database.");
        }

        if (!user.Photos.Any(p => p.IsMain))
        {
            photo.IsMain = true;
        }

        await unitOfWork.CompleteAsync();

        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId:int}")]
    public async Task<ActionResult> RejectPhotoAsync(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);

        if (photo == null)
        {
            return BadRequest("Could not get photo from database.");
        }

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Result == "ok")
            {
                unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
        }
        else
        {
            unitOfWork.PhotoRepository.RemovePhoto(photo);
        }

        await unitOfWork.CompleteAsync();

        return Ok();
    }
}

using Microsoft.AspNetCore.Mvc;
using Rendezvous.API.DTOs;
using Rendezvous.API.Entities;
using Rendezvous.API.Extensions;
using Rendezvous.API.Interfaces;

namespace Rendezvous.API.Controllers;

public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if (sourceUserId == targetUserId)
        {
            return BadRequest("You cannot like yourself.");
        }

        var existingLike = await likesRepository
            .GetUserLikeAsync(sourceUserId, targetUserId);

        if (existingLike == null)
        {
            var like = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };

            await likesRepository.AddLikeAsync(like);
        }
        else
        {
            likesRepository.DeleteLike(existingLike);
        }

        if (await likesRepository.SaveChangesAsync())
        {
            return Ok();
        }

        return BadRequest("Failed to update like.");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        return Ok(await likesRepository.GetCurrentUserLikeIdsAsync(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes(string predicate)
    {
        var users = await likesRepository.GetUserLikesAsync(predicate, User.GetUserId());
        return Ok(users);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rendezvous.API.Data;
using Rendezvous.API.Entities;

namespace Rendezvous.API.Controllers;

public class BuggyController(DataContext context) : BaseApiController
{
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetAuth()
    {
        return "secret text";
    }

    [HttpGet("not-found")]
    public async Task<ActionResult<AppUser>> GetNotFound()
    {
        var user = await context.Users.FindAsync(-1);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpGet("server-error")]
    public async Task<ActionResult<AppUser>> GetServerError()
    {
        var user = await context.Users.FindAsync(-1) ?? throw new Exception("An error occurred");
        return user;
    }

    [HttpGet("bad-request")]
    public ActionResult GetBadRequest()
    {
        return BadRequest("That was bad request");
    }
}

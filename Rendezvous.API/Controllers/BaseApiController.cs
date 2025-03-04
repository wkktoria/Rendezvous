using Microsoft.AspNetCore.Mvc;
using Rendezvous.API.Helpers;

namespace Rendezvous.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(LogUserActivity))]
public class BaseApiController : ControllerBase
{

}

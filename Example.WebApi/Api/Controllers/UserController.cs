using System.Security.Claims;
using Example.WebApi.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.WebApi.Api.Controllers;

[ApiController]
[Route("/api/v1/user")]
public class UserController
    : Controller
{
    [HttpGet]
    [Authorize("read:users")]
    public ActionResult GetCurrentUser()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        
        return Ok($"Current user has ID {identity!.GetSubjectId()} and name {identity!.GetGivenName()} {identity!.GetFamilyName()}!");
    }

    [HttpGet("{id:int}")]
    [Authorize("api:read:users")]
    public ActionResult GetUserById(int id)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        
        return Ok($"Returned User with ID {id} via Client Credentials token.");
    }

    [HttpPut("{id:int}")]
    [Authorize("api:write:users")]
    public ActionResult UpdateUser(int id)
    {
        return Ok($"User with id {id} updated successfully via Client Credentials token.");
    }
    
    [HttpPost]
    [Authorize("api:write:users")]
    public ActionResult CreateUser()
    {
        return Ok("User created successfully via Client Credentials token.");
    }
}

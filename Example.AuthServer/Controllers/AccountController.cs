using System.ComponentModel.DataAnnotations;
using Example.AuthServer.Handlers.Account;
using Example.AuthServer.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.AuthServer.Controllers;

public class AccountController
    : Controller
{
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login(
        [FromQuery(Name = "ClientId"), Required]
        string clientId,
        [FromQuery(Name = "ReturnUrl")] string? returnUrl = null)
    {
        // view only displays login form
        return View(new LoginViewModel
        {
            ClientId = clientId,
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] LoginViewModel model,
        [FromServices] IMediator mediator,
        CancellationToken token = default)
    {
        if (!ModelState.IsValid)
        {
            // when model state is not valid, return the view with the model
            return View(model);
        }

        try
        {
            var request = new AccountLoginRequest(
                HttpContext, 
                model.ClientId, model.Username, model.Password, 
                model.ReturnUrl);
            
            var response = await mediator.Send(request, token);
            if (response.RedirectUri is not null)
            {
                // when redirect URI is provided, redirect to the URI
                return Redirect(response.RedirectUri);
            }

            return Ok(); // just return OK, when nothing else necessary
        }
        catch (Exception ex)
        {
            // when there is some exception, display login form again
            return View(model with { ErrorMessage = ex.Message });
        }
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}

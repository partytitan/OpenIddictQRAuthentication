using System.Security.Claims;
using System.Threading.Tasks;
using Balosar.Server.Cache;
using Balosar.Server.Models;
using Balosar.Server.ViewModels.QRAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Balosar.Server.Controllers;

[Route("[controller]")]
public class QRAuthenticationController : Controller
{
    private readonly QRAuthenticationCache _qrAuthenticationCache;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public QRAuthenticationController(QRAuthenticationCache qrAuthenticationCache, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _qrAuthenticationCache = qrAuthenticationCache;
        _signInManager = signInManager;
        _userManager = userManager;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoQRAuthentication(DoQRLoginViewModel viewModel)
    {
        viewModel.ReturnUrl ??= Url.Content("~/");
        
        var jwt = _qrAuthenticationCache.GetJwtForDeviceCode(viewModel.DeviceCode);
        if (jwt == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid QR login attempt.");

            return LocalRedirect("/Identity/Account/Login");
        }
        
        var user = await _userManager.FindByIdAsync(jwt.Subject);
        if (user == null)
        {
            return LocalRedirect("/Identity/Account/Login");
        }

        if (!await _signInManager.CanSignInAsync(user))
        {
            return BadRequest();
        }
        
        // The amr claim should reference how the user was authenticated. Therefore we set it to qr.
        await _signInManager.SignInWithClaimsAsync(user, new AuthenticationProperties { IsPersistent = true },
            new[] { new Claim("amr", "qr") });
        

        return LocalRedirect(viewModel.ReturnUrl);
    }
}
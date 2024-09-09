using System.Security.Claims;
using FamilyCalendar.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FamilyCalendar.Tests.Pages;

public class LoginModelTests
{
  private readonly IAuthenticationService _authenticationService;
  private readonly HttpContext _httpContext;
  private readonly LoginModel _pageModel;
  private readonly string _administratorPassword = "admin";
  private readonly string _viewerPassword = "viewer";

  public LoginModelTests()
  {
    var settings = Options.Create(new FamilyCalendarSettings()
    {
      DynamoDbTable = "",
      EntriesByDisplayEndDateIndex = "",
      BaseHref = "",
      OpenAiApiKey = "",
      OpenAiModelId = "",
      ViewerPassword = _viewerPassword,
      AdministratorPassword = _administratorPassword,
    });

    _httpContext = Substitute.For<HttpContext>();
    _authenticationService = Substitute.For<IAuthenticationService>();

    var serviceProvider = Substitute.For<IServiceProvider>();
    serviceProvider.GetService(typeof(IAuthenticationService)).Returns(_authenticationService);
    _httpContext.RequestServices.Returns(serviceProvider);

    _pageModel = new LoginModel(settings)
    {
      PageContext = new PageContext
      {
        HttpContext = _httpContext
      },
      Input = null!,
    };
  }

  [Fact]
  public async Task OnPostAsync_PreventsLoginForInvalidModelState()
  {
    _pageModel.ModelState.AddModelError("Input.Password", "The Password field is required.");

    await _pageModel.OnPostAsync("");

    await _authenticationService.Received(0).SignInAsync(
      _httpContext,
      CookieAuthenticationDefaults.AuthenticationScheme,
      Arg.Any<ClaimsPrincipal>(),
      Arg.Any<AuthenticationProperties>());
  }

  [Fact]
  public async Task OnPostAsync_PreventsLoginForInvalidPassword()
  {
    _pageModel.Input = new LoginModel.InputModel
    {
      Password = "invalid password",
    };

    await _pageModel.OnPostAsync("");

    await _authenticationService.Received(0).SignInAsync(
      _httpContext,
      CookieAuthenticationDefaults.AuthenticationScheme,
      Arg.Any<ClaimsPrincipal>(),
      Arg.Any<AuthenticationProperties>());
  }

  [Fact]
  public async Task OnPostAsync_LoginsAsViewer()
  {
    _pageModel.Input = new LoginModel.InputModel
    {
      Password = _viewerPassword,
    };

    await _pageModel.OnPostAsync("redirect");

    await _authenticationService.Received(1).SignInAsync(
      _httpContext,
      CookieAuthenticationDefaults.AuthenticationScheme,
      Arg.Is<ClaimsPrincipal>(cp => cp.HasClaim(ClaimTypes.Role, "Viewer")),
      Arg.Any<AuthenticationProperties>());
  }

  [Fact]
  public async Task OnPostAsync_LoginsAsAdministrator()
  {
    _pageModel.Input = new LoginModel.InputModel
    {
      Password = _administratorPassword,
    };

    await _pageModel.OnPostAsync("redirect");

    await _authenticationService.Received(1).SignInAsync(
      _httpContext,
      CookieAuthenticationDefaults.AuthenticationScheme,
      Arg.Is<ClaimsPrincipal>(cp => cp.HasClaim(ClaimTypes.Role, "Administrator")),
      Arg.Any<AuthenticationProperties>());
  }

  [Fact]
  public async Task OnPostAsync_RedirectsAfterLogin()
  {
    var expectedRedirect = "expected redirect";
    _pageModel.Input = new LoginModel.InputModel
    {
      Password = _administratorPassword,
    };

    var result = await _pageModel.OnPostAsync(expectedRedirect);

    Assert.IsType<LocalRedirectResult>(result);
    Assert.Equal(expectedRedirect, ((LocalRedirectResult)result).Url);
  }
}

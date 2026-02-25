using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("applications")]
public class ApplicationController
{
    
}
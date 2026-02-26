using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("applications")]
public class ApplicationController(IArgoService argoService)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Application>))]
    public ValueTask<IEnumerable<Application>> List(CancellationToken cancellationToken)
    {
        return argoService.GetApplicationsAsync(cancellationToken);
    }
    
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Application> Get(string name, CancellationToken cancellationToken)
    {
        return argoService.GetApplicationAsync(name, cancellationToken);
    }
    
    [HttpGet("{name}/refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async Task Refresh(string name, CancellationToken cancellationToken)
    {
        await argoService.RefreshApplicationAsync(name, cancellationToken);
    }
    
    [HttpGet("{name}/sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async Task Sync(string name, CancellationToken cancellationToken)
    {
        await argoService.SyncApplicationAsync(name, cancellationToken);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Application> Post([FromBody] Application body, CancellationToken cancellationToken)
    {
        return argoService.CreateApplicationAsync(body, cancellationToken);
    }
    
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public ValueTask<Application> Delete(string name, CancellationToken cancellationToken)
    {
        return argoService.DeleteApplicationAsync(name, cancellationToken);
    }
}
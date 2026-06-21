using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("applications")]
public class ApplicationController(IArgoService argoService, ILogger<ApplicationController> logger)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Application>))]
    public async ValueTask<IEnumerable<Application>> List(CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.GetApplicationsAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing applications");
            throw;
        }
    }
    
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async ValueTask<Application> Get(string name, CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.GetApplicationAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting application {Name}", name);
            throw;
        }
    }
    
    [HttpGet("{name}/refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async Task Refresh(string name, CancellationToken cancellationToken)
    {
        try
        {
            await argoService.RefreshApplicationAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error refreshing application {Name}", name);
            throw;
        }
    }
    
    [HttpGet("{name}/sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async Task Sync(string name, CancellationToken cancellationToken)
    {
        try
        {
            await argoService.SyncApplicationAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error syncing application {Name}", name);
            throw;
        }
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async ValueTask<Application> Post([FromBody] Application body, CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.CreateApplicationAsync(body, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating application {Name}", body.Name);
            throw;
        }
    }
    
    [HttpPut("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Application))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async ValueTask<Application> Put(string name, [FromBody] Application body, CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.UpdateApplicationAsync(name, body, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating application {Name}", name);
            throw;
        }
    }
    
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public async ValueTask<Application> Delete(string name, CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.DeleteApplicationAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting application {Name}", name);
            throw;
        }
    }
}
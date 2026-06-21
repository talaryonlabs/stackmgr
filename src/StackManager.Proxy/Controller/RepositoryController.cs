using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("repositories")]
public class RepositoryController(IArgoService argoService, ILogger<RepositoryController> logger)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Repository>))]
    public async ValueTask<IEnumerable<Repository>> List(CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.GetRepositoriesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing repositories");
            throw;
        }
    }
    
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Repository))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async ValueTask<Repository> Get(string name, CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.GetRepositoryAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting repository {Name}", name);
            throw;
        }
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Repository))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public async ValueTask<Repository> Post([FromBody] Repository body, CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.CreateRepositoryAsync(body, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating repository {Name}", body.Name);
            throw;
        }
    }
    
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public async ValueTask<Repository> Delete(string name, CancellationToken cancellationToken)
    {
        try
        {
            return await argoService.DeleteRepositoryAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting repository {Name}", name);
            throw;
        }
    }
}
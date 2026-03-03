using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("repositories")]
public class RepositoryController(IArgoService argoService)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Repository>))]
    public ValueTask<IEnumerable<Repository>> List(CancellationToken cancellationToken)
    {
        return argoService.GetRepositoriesAsync(cancellationToken);
    }
    
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Repository))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Repository> Get(string name, CancellationToken cancellationToken)
    {
        return argoService.GetRepositoryAsync(name, cancellationToken);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Repository))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Repository> Post([FromBody] Repository body, CancellationToken cancellationToken)
    {
        return argoService.CreateRepositoryAsync(body, cancellationToken);
    }
    
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public ValueTask<Repository> Delete(string name, CancellationToken cancellationToken)
    {
        return argoService.DeleteRepositoryAsync(name, cancellationToken);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[Microsoft.AspNetCore.Mvc.Route("namespaces")]
[ApiController]
public class NamespaceController(IRancherService rancherService)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Namespace>))]
    public ValueTask<IEnumerable<Namespace>> List(CancellationToken cancellationToken)
    {
        return rancherService.GetNamespacesAsync(cancellationToken);
    }
    
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Namespace))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Namespace> Get(string name, CancellationToken cancellationToken)
    {
        return rancherService.GetNamespaceAsync(name, cancellationToken);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Namespace))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Namespace> Post([FromBody] Namespace body, CancellationToken cancellationToken)
    {
        return rancherService.CreateNamespaceAsync(body.Name, cancellationToken);
    }
    
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public ValueTask<Namespace> Delete(string name, CancellationToken cancellationToken)
    {
        return rancherService.DeleteNamespaceAsync(name, cancellationToken);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("namespaces")]
public class NamespaceController(IRancherService rancherService)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Namespace>))]
    public ValueTask<IEnumerable<Namespace>> List(CancellationToken cancellationToken)
    {
        try
        {
            return rancherService.GetNamespacesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Namespace))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Namespace> Get(string name, CancellationToken cancellationToken)
    {
        try
        {
            return rancherService.GetNamespaceAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Namespace))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]   
    public ValueTask<Namespace> Post([FromBody] Namespace body, CancellationToken cancellationToken)
    {
        try
        {
            return rancherService.CreateNamespaceAsync(body.Name, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public ValueTask<Namespace> Delete(string name, CancellationToken cancellationToken)
    {
        try
        {
            return rancherService.DeleteNamespaceAsync(name, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
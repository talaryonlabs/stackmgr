using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("volumes")]
public class VolumeController(ILonghornService longhornService, IRancherService rancherService)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Volume>))]
    public ValueTask<IEnumerable<Volume>> ListVolumes(CancellationToken cancellationToken)
    {
        return longhornService.GetVolumesAsync(cancellationToken);
    }

    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Volume))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<Volume> GetVolume(string name, CancellationToken cancellationToken)
    {
        return longhornService.GetVolumeAsync(name, cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Volume))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<Volume> PostVolume([FromBody] Volume body, CancellationToken cancellationToken)
    {
        return longhornService.CreateVolumeAsync(body, cancellationToken);
    }

    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Volume))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<Volume> DeleteVolume(string name, CancellationToken cancellationToken)
    {
        return longhornService.DeleteVolumeAsync(name, cancellationToken);
    }
    
    /**
     * PersistentVolume (PV)
     */
    [HttpGet("pv/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PersistentVolume>))]
    public ValueTask<IEnumerable<PersistentVolume>> ListPersistentVolumes(CancellationToken cancellationToken)
    {
        return rancherService.GetPersistentVolumesAsync(cancellationToken);
    }

    [HttpGet("pv/{pvName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersistentVolume))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<PersistentVolume> GetPersistentVolume(string pvName, CancellationToken cancellationToken)
    {
        return rancherService.GetPersistentVolumeAsync(pvName, cancellationToken);
    }

    [HttpPost("pv/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersistentVolume))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<PersistentVolume> PostPersistentVolume(
        [FromBody] PersistentVolume body,
        CancellationToken cancellationToken)
    {
        return rancherService.CreatePersistentVolumeAsync(body, cancellationToken);
    }

    [HttpDelete("pv/{pvName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersistentVolume))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<PersistentVolume> DeletePersistentVolume(string pvName, CancellationToken cancellationToken)
    {
        return rancherService.DeletePersistentVolumeAsync(pvName, cancellationToken);
    }
    
    /**
     * PersistentVolumeClaim (PVC)
     */
    [HttpGet("pvc/{namespaceName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PersistentVolumeClaim>))]
    public ValueTask<IEnumerable<PersistentVolumeClaim>> ListVolumeClaims(string namespaceName, CancellationToken cancellationToken)
    {
        return rancherService.GetVolumeClaimsAsync(namespaceName, cancellationToken);
    }

    [HttpGet("pvc/{namespaceName}/{claimName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersistentVolumeClaim))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<PersistentVolumeClaim> GetVolumeClaim(string namespaceName, string claimName, CancellationToken cancellationToken)
    {
        return rancherService.GetVolumeClaimAsync(namespaceName, claimName, cancellationToken);
    }

    [HttpPost("pvc/{namespaceName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersistentVolumeClaim))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<PersistentVolumeClaim> PostVolumeClaim(
        string namespaceName,
        [FromBody] PersistentVolumeClaim body,
        CancellationToken cancellationToken)
    {
        body.Namespace = namespaceName;
        return rancherService.CreateVolumeClaimAsync(namespaceName, body, cancellationToken);
    }

    [HttpDelete("pvc/{namespaceName}/{claimName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PersistentVolumeClaim))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public ValueTask<PersistentVolumeClaim> DeleteVolumeClaim(string namespaceName, string claimName, CancellationToken cancellationToken)
    {
        return rancherService.DeleteVolumeClaimAsync(namespaceName, claimName, cancellationToken);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Models;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.StackManager.Proxy.Utilities;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Controller;

[Authorize]
[ApiController]
[Route("volumes/{namespace}")]
public class VolumeController(ILonghornService longhornService, IRancherService rancherService)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Volume>))]
    public async ValueTask<IEnumerable<Volume>> ListVolumes(string @namespace, CancellationToken cancellationToken)
    {
        var ns = await rancherService.GetNamespaceAsync(@namespace, cancellationToken);
        var claims = await rancherService.GetVolumeClaimsAsync(ns.Name, cancellationToken);
        var volumes = await longhornService.GetVolumesAsync(cancellationToken);
        
        return volumes.Where(v => claims.Any(c => c.Name == v.Name));
    }

    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Volume))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public async ValueTask<Volume> GetVolume(string @namespace, string name, CancellationToken cancellationToken)
    {
        var ns = await rancherService.GetNamespaceAsync(@namespace, cancellationToken);
        var pvc = await rancherService.GetVolumeClaimAsync(ns.Name, name, cancellationToken);
        
        return await longhornService.GetVolumeAsync(pvc.VolumeName, cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Volume))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestError))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public async ValueTask<Volume> PostVolume(string @namespace, [FromBody] Volume body, CancellationToken cancellationToken)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(@namespace))
            throw new BadRequestError("Namespace cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.Name))
            throw new BadRequestError("Volume name cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.Size))
            throw new BadRequestError("Volume size cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.AccessMode))
            throw new BadRequestError("Access mode cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.Frontend))
            throw new BadRequestError("Frontend cannot be null or empty.");
        
        try
        {
            await rancherService.GetPersistentVolumeAsync(body.Name, cancellationToken);
            throw new ConflictError($"Volume with name '{body.Name}' already exists.");
        }
        catch (NotFoundError) {}

        Volume volume;
        try
        {
            volume = await longhornService.GetVolumeAsync(body.Name, cancellationToken);
        }
        catch (NotFoundError)
        {
            volume = await longhornService.CreateVolumeAsync(body, cancellationToken);
        }

        if (volume is null)
        {
            throw new InternalServerError($"Failed to create volume '{body.Name}'.");       
        }
        
        var ns = await rancherService.GetNamespaceAsync(@namespace, cancellationToken);
        var pv = await rancherService.CreatePersistentVolumeAsync(new PersistentVolume
        {
            AccessMode = body.AccessMode,
            Name = volume.Name,
            StorageSize = body.Size,
            VolumeHandle = volume.Name,
        }, cancellationToken);
        
        if(pv is null) throw new InternalServerError($"Failed to create persistent volume '{volume.Name}'.");      

        var pvc = await rancherService.CreateVolumeClaimAsync(ns.Name, new PersistentVolumeClaim
        {
            Name = pv.Name,
            VolumeName = pv.Name,
            AccessMode = pv.AccessMode,
            StorageSize = pv.StorageSize
        }, cancellationToken);
        
        return pvc is null ? throw new InternalServerError($"Failed to create persistent volume claim '{pv.Name}'.") : volume;
    }

    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Volume))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BadRequestError))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerError))]
    public async ValueTask<Volume> DeleteVolume(string @namespace, string name, CancellationToken cancellationToken)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(@namespace))
            throw new BadRequestError("Namespace cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestError("Volume name cannot be null or empty.");
        
        var ns = await rancherService.GetNamespaceAsync(@namespace, cancellationToken);
        var pvc = await rancherService.GetVolumeClaimAsync(ns.Name, name, cancellationToken);
        var pv = await rancherService.GetPersistentVolumeAsync(pvc.VolumeName, cancellationToken);

        await rancherService.DeleteVolumeClaimAsync(@namespace, pvc.Name, cancellationToken);
        await rancherService.DeletePersistentVolumeAsync(pv.Name, cancellationToken);
        
        return await longhornService.GetVolumeAsync(pvc.VolumeName, cancellationToken);       
    }
}
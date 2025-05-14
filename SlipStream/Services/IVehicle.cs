using System;
using SlipStream.DTOs;
using SlipStream.DTOs.Vehicle;

namespace SlipStream.Services;

public interface IVehicle
{
    Task<ReturnDTO> AddVehicleAsync(CreateVehicleDTO createVehicleDTO);
    Task<ReturnDTO> UpdateVehicleAsync(UpdateVehicleDTO updateVehicleDTO);
    Task<ReturnDTO> DeleteVehicleAsync(DeleteVehicleDTO deleteVehicleDTO);
    Task<ReturnDTO> RetrieveVehicleAsync(RetrieveVehicleDTO retrieveVehicleDTO);
    
}

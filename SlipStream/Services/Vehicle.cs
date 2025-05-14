using System;
using Microsoft.EntityFrameworkCore;
using SlipStream.Data;
using SlipStream.DTOs;
using SlipStream.DTOs.Vehicle;
// using SlipStream.Entities;

namespace SlipStream.Services;

public class Vehicle : IVehicle
{
    private readonly AppDbContext _dbContext;
    private readonly IJWTService _jwtService;
    public Vehicle(AppDbContext dbContext, IJWTService jwtService)
    {
        _jwtService = jwtService;
        _dbContext = dbContext;
    }
    public async Task<ReturnDTO> AddVehicleAsync(CreateVehicleDTO createVehicleDTO)
    {
        // if (!createVehicleDTO.ValidPassword){
        //     return new ReturnDTO{ 
        //         Message = "Password must be at least 8 characters long and contain at least one letter, one number, and one symbol.",
        //         StatusCode = 400
        //     };
        // }

        Entities.Vehicle? vehicle = await ExistsAsync( vehicleName: createVehicleDTO.Name );
        if (vehicle != null)
        {
            return new ReturnDTO
            {
                Message = "Vehicle name already exists",
                StatusCode = 400
            };
        }

        vehicle = new Entities.Vehicle
        {
            Name = createVehicleDTO.Name,
            Password = createVehicleDTO.Password,
        };

        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();

        return new ReturnDTO
        {
            Status = true,
            Message = "Vehicle created successfully",
            StatusCode = 201,
            Data = vehicle.Id
        };
    }

    public async Task<ReturnDTO> DeleteVehicleAsync(DeleteVehicleDTO deleteVehicleDTO)
    {
        Entities.Vehicle? vehicle = await ExistsAsync( vehicleId: deleteVehicleDTO.Id );
        if (vehicle == null)
        {
            return new ReturnDTO
            {
                Message = "Vehicle not found",
                StatusCode = 404
            };
        }
        _dbContext.Vehicles.Remove(vehicle);
        await _dbContext.SaveChangesAsync();
        return new ReturnDTO
        {
            Status = true,
            Message = "Vehicle deleted successfully",
            StatusCode = 200
        };
    }

    public async Task<ReturnDTO> RetrieveVehicleAsync(RetrieveVehicleDTO retrieveVehicleDTO){
        Entities.Vehicle? vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(
            v => v.Name == retrieveVehicleDTO.Name 
            && v.Password == retrieveVehicleDTO.Password 
        );

        if (vehicle == null)
        {
            return new ReturnDTO
            {
                Message = "Vehicle not found Name or Password is incorrect",
                StatusCode = 404
            };
        }

        int session;
        if (await _dbContext.Sessions.AnyAsync(s => s.VehicleId == vehicle.Id)){
            session = await _dbContext.Sessions
                .Where(s => s.VehicleId == vehicle.Id)
                .MaxAsync(s => s.Id);
        }
        else{
            session = -1;
        }

        string token = _jwtService.GenerateToken( VehicleId: vehicle.Id, 
                                                sessionId: session );
        return new ReturnDTO
        {
            Status = true,
            Message = "Vehicle retrieved successfully",
            StatusCode = 200,
            Data = new AuthSuccessVehicleDTO{
                Jwt = token,
                VehicleId = vehicle.Id,
                SessionId = session
            }
        };
    }

    public async Task<ReturnDTO> UpdateVehicleAsync(UpdateVehicleDTO updateVehicleDTO)
    {
        Entities.Vehicle? vehicle = await ExistsAsync( vehicleId: updateVehicleDTO.VehicleId );
        if (vehicle == null)
        {
            return new ReturnDTO
            {
                Message = "Vehicle not found",
                StatusCode = 404
            };
        }

        if (updateVehicleDTO.NewVehicleName is not null){
            vehicle.Name = updateVehicleDTO.NewVehicleName;
        }

        if (updateVehicleDTO.NewPassword is not null){
            if (!updateVehicleDTO.ValidPassword){
                return new ReturnDTO{ 
                    Message = "Password must be at least 8 characters long and contain at least one letter, one number, and one symbol.",
                    StatusCode = 400
                };
            }
            vehicle.Password = updateVehicleDTO.NewPassword;
        }
        _dbContext.Vehicles.Update(vehicle);
        await _dbContext.SaveChangesAsync();
        return new ReturnDTO
        {
            Status = true,
            Message = "Vehicle updated successfully",
            StatusCode = 204
        };
    }

    public async Task<Entities.Vehicle?> ExistsAsync(string vehicleName){
        Entities.Vehicle? vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Name == vehicleName);
    
        return vehicle;
    }

    public async Task<Entities.Vehicle?> ExistsAsync(int vehicleId){
        Entities.Vehicle? vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId);
                
        return vehicle;
    }

}

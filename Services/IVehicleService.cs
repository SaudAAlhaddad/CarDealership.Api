using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;

namespace CarDealership.Api.Services;

/// <summary>
/// Service interface for vehicle management operations
/// Handles vehicle browsing, creation, updates, and filtering
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Retrieves vehicles with optional filtering by make, model, year, and price
    /// Only returns available vehicles
    /// </summary>
    /// <param name="searchFilters">Optional filters for vehicle search</param>
    /// <returns>Collection of matching vehicles</returns>
    Task<IEnumerable<Vehicle>> BrowseVehiclesAsync(VehicleQueryDto searchFilters);
    
    /// <summary>
    /// Retrieves a specific vehicle by its ID
    /// </summary>
    /// <param name="vehicleId">The vehicle's unique identifier</param>
    /// <returns>Vehicle entity or null if not found</returns>
    Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
    
    /// <summary>
    /// Creates a new vehicle in the inventory
    /// </summary>
    /// <param name="newVehicleData">Vehicle information for creation</param>
    /// <returns>The created vehicle entity</returns>
    Task<Vehicle> CreateVehicleAsync(VehicleCreateDto newVehicleData);
    
    /// <summary>
    /// Updates an existing vehicle with OTP validation
    /// Requires admin authentication and OTP code
    /// </summary>
    /// <param name="vehicleId">ID of the vehicle to update</param>
    /// <param name="updateData">New vehicle data including OTP code</param>
    /// <param name="adminEmail">Email of the admin performing the update</param>
    /// <returns>Updated vehicle entity or null if not found</returns>
    Task<Vehicle?> UpdateVehicleAsync(int vehicleId, VehicleUpdateDto updateData, string adminEmail);
}


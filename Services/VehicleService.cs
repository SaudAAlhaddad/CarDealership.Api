using CarDealership.Api.Data;
using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;
using CarDealership.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Services;

/// <summary>
/// Service responsible for vehicle management operations
/// Handles browsing, creation, and updating of vehicle inventory
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly AppDbContext _dbContext;
    private readonly IOtpService _otpService;

    /// <summary>
    /// Initializes a new instance of the VehicleService
    /// </summary>
    /// <param name="dbContext">Database context for vehicle operations</param>
    /// <param name="otpService">OTP service for admin operation validation</param>
    public VehicleService(AppDbContext dbContext, IOtpService otpService)
    {
        _dbContext = dbContext;
        _otpService = otpService;
    }

    /// <summary>
    /// Retrieves vehicles with optional filtering by make, model, year, and price
    /// Only returns available vehicles
    /// </summary>
    /// <param name="searchFilters">Optional filters for vehicle search</param>
    /// <returns>Collection of matching vehicles sorted by make and model</returns>
    public async Task<IEnumerable<Vehicle>> BrowseVehiclesAsync(VehicleQueryDto searchFilters)
    {
        // Start with base query for only available vehicles
        var availableVehiclesQuery = _dbContext.Vehicles.AsQueryable().Where(vehicle => vehicle.IsAvailable);

        // Apply optional search filters if provided
        if (!string.IsNullOrWhiteSpace(searchFilters.Make)) 
            availableVehiclesQuery = availableVehiclesQuery.Where(vehicle => vehicle.Make == searchFilters.Make);
        if (!string.IsNullOrWhiteSpace(searchFilters.Model)) 
            availableVehiclesQuery = availableVehiclesQuery.Where(vehicle => vehicle.Model == searchFilters.Model);
        if (searchFilters.MinYear.HasValue) 
            availableVehiclesQuery = availableVehiclesQuery.Where(vehicle => vehicle.Year >= searchFilters.MinYear);
        if (searchFilters.MaxYear.HasValue) 
            availableVehiclesQuery = availableVehiclesQuery.Where(vehicle => vehicle.Year <= searchFilters.MaxYear);
        if (searchFilters.MinPrice.HasValue) 
            availableVehiclesQuery = availableVehiclesQuery.Where(vehicle => vehicle.Price >= searchFilters.MinPrice);
        if (searchFilters.MaxPrice.HasValue) 
            availableVehiclesQuery = availableVehiclesQuery.Where(vehicle => vehicle.Price <= searchFilters.MaxPrice);

        // Sort results by make and model for consistent ordering
        var filteredVehicles = await availableVehiclesQuery
            .OrderBy(vehicle => vehicle.Make)
            .ThenBy(vehicle => vehicle.Model)
            .ToListAsync();

        return filteredVehicles;
    }

    /// <summary>
    /// Retrieves a specific vehicle by its ID
    /// </summary>
    /// <param name="vehicleId">The vehicle's unique identifier</param>
    /// <returns>Vehicle entity or null if not found</returns>
    public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
    {
        return await _dbContext.Vehicles.FindAsync(vehicleId);
    }

    /// <summary>
    /// Creates a new vehicle in the inventory
    /// </summary>
    /// <param name="newVehicleData">Vehicle information for creation</param>
    /// <returns>The created vehicle entity with assigned ID</returns>
    public async Task<Vehicle> CreateVehicleAsync(VehicleCreateDto newVehicleData)
    {
        // Create new vehicle entity from provided data
        var newVehicle = new Vehicle
        {
            Make = newVehicleData.Make,
            Model = newVehicleData.Model,
            Year = newVehicleData.Year,
            Price = newVehicleData.Price,
            Color = newVehicleData.Color,
            Description = newVehicleData.Description
        };

        // Save to database and return with assigned ID
        _dbContext.Vehicles.Add(newVehicle);
        await _dbContext.SaveChangesAsync();

        return newVehicle;
    }

    /// <summary>
    /// Updates an existing vehicle with OTP validation
    /// Requires admin authentication and OTP code
    /// </summary>
    /// <param name="vehicleId">ID of the vehicle to update</param>
    /// <param name="updateData">New vehicle data including OTP code</param>
    /// <param name="adminEmail">Email of the admin performing the update</param>
    /// <returns>Updated vehicle entity or null if not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when OTP is invalid or expired</exception>
    public async Task<Vehicle?> UpdateVehicleAsync(int vehicleId, VehicleUpdateDto updateData, string adminEmail)
    {
        // Step 1: Find the vehicle to update
        var vehicleToUpdate = await _dbContext.Vehicles.FindAsync(vehicleId);
        if (vehicleToUpdate == null)
            return null;

        // Step 2: Validate OTP before allowing vehicle updates for security
        var isOtpValid = await _otpService.ValidateAsync(adminEmail, OtpPurpose.UpdateVehicle, updateData.OtpCode);
        if (!isOtpValid)
            throw new InvalidOperationException("Invalid or expired OTP for update.");

        // Step 3: Update vehicle properties with new data
        vehicleToUpdate.Make = updateData.Make;
        vehicleToUpdate.Model = updateData.Model;
        vehicleToUpdate.Year = updateData.Year;
        vehicleToUpdate.Price = updateData.Price;
        vehicleToUpdate.Color = updateData.Color;
        vehicleToUpdate.Description = updateData.Description;

        // Step 4: Save changes to database
        await _dbContext.SaveChangesAsync();
        return vehicleToUpdate;
    }
}


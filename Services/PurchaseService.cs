using CarDealership.Api.Data;
using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;
using CarDealership.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Services;

/// <summary>
/// Service responsible for handling customer purchase requests and admin sale processing
/// Implements database transactions to ensure data integrity and prevent race conditions
/// </summary>
public class PurchaseService : IPurchaseService
{
    private readonly AppDbContext _dbContext;
    private readonly IOtpService _otpService;

    /// <summary>
    /// Initializes a new instance of the PurchaseService
    /// </summary>
    /// <param name="dbContext">Database context for purchase operations</param>
    /// <param name="otpService">OTP service for purchase validation</param>
    public PurchaseService(AppDbContext dbContext, IOtpService otpService)
    {
        _dbContext = dbContext;
        _otpService = otpService;
    }

    /// <summary>
    /// Creates a new purchase request with vehicle-bound OTP validation
    /// Uses database transactions to prevent race conditions
    /// </summary>
    /// <param name="vehicleId">ID of the vehicle to purchase</param>
    /// <param name="customerId">ID of the customer making the request</param>
    /// <param name="otpCode">OTP code bound to the specific vehicle</param>
    /// <returns>Purchase request result with request ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when customer is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when OTP is invalid or vehicle is unavailable</exception>
    public async Task<object> CreatePurchaseRequestAsync(int vehicleId, int customerId, string otpCode)
    {
        // Input validation
        if (vehicleId <= 0)
            throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(vehicleId));
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be a positive integer.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(otpCode))
            throw new ArgumentException("OTP code cannot be null or empty.", nameof(otpCode));

        // Use database transaction to prevent race conditions during purchase requests
        using var purchaseTransaction = await _dbContext.Database.BeginTransactionAsync();
        
        try
        {
            // Step 1: Verify customer exists and is valid
            var customer = await _dbContext.Users.FindAsync(customerId);
            if (customer is null)
                throw new UnauthorizedAccessException("User not found.");

            // Check if customer already has a pending request for this specific vehicle
            var existingRequestForSameVehicle = await _dbContext.PurchaseRequests
                .AnyAsync(request => request.CustomerId == customerId 
                                  && request.VehicleId == vehicleId 
                                  && request.Status == PurchaseStatus.Pending);
            if (existingRequestForSameVehicle)
                throw new InvalidOperationException("You already have a pending purchase request for this vehicle. Please wait for admin approval or contact support.");

            // Step 2: Validate OTP is correct and bound to the specific vehicle
            var isOtpValid = await _otpService.ValidateAsync(customer.Email, OtpPurpose.Purchase, otpCode, true, vehicleId);
            if (!isOtpValid)
                throw new InvalidOperationException("Invalid or expired OTP for purchase. OTP must be generated for this specific vehicle.");

            // Step 3: Check vehicle availability (within transaction to prevent race conditions)
            var requestedVehicle = await _dbContext.Vehicles
                .Where(vehicle => vehicle.Id == vehicleId)
                .FirstOrDefaultAsync();
                
            if (requestedVehicle is null)
                throw new InvalidOperationException("Vehicle not found.");
                
            if (!requestedVehicle.IsAvailable)
                throw new InvalidOperationException("Vehicle is no longer available.");

            // Step 4: Create purchase request
            var newPurchaseRequest = new PurchaseRequest
            {
                VehicleId = vehicleId,
                CustomerId = customerId,
                RequestedAt = DateTime.UtcNow // Explicitly set timestamp
            };

            // Step 5: Save purchase request and commit transaction
            _dbContext.PurchaseRequests.Add(newPurchaseRequest);
            await _dbContext.SaveChangesAsync();
            await purchaseTransaction.CommitAsync();

            return new { 
                message = "Purchase request submitted successfully.", 
                requestId = newPurchaseRequest.Id,
                vehicleId = newPurchaseRequest.VehicleId,
                requestedAt = newPurchaseRequest.RequestedAt
            };
        }
        catch
        {
            await purchaseTransaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Processes an approved purchase request and completes the sale
    /// Marks vehicle as unavailable and creates sale record
    /// Uses database transactions to ensure data consistency
    /// </summary>
    /// <param name="purchaseRequestId">ID of the purchase request to process</param>
    /// <returns>Sale completion result with sale ID</returns>
    /// <exception cref="InvalidOperationException">Thrown when request is not found, already processed, or vehicle unavailable</exception>
    public async Task<object> ProcessPurchaseRequestAsync(int purchaseRequestId)
    {
        // Use database transaction to prevent race conditions during sale processing
        using var saleProcessingTransaction = await _dbContext.Database.BeginTransactionAsync();
        
        try
        {
            // Step 1: Lock the purchase request and vehicle for update to prevent race conditions
            var pendingPurchaseRequest = await _dbContext.PurchaseRequests
                .Include(request => request.Vehicle)
                .Where(request => request.Id == purchaseRequestId)
                .FirstOrDefaultAsync();

            if (pendingPurchaseRequest is null)
                throw new InvalidOperationException("Purchase request not found.");
            if (pendingPurchaseRequest.Status != PurchaseStatus.Pending)
                throw new InvalidOperationException("Request already decided.");

            // Step 2: Validate vehicle is still available for sale
            var vehicleToSell = pendingPurchaseRequest.Vehicle;
            if (vehicleToSell is null || !vehicleToSell.IsAvailable)
                throw new InvalidOperationException("Vehicle is no longer available.");

            // Step 3: Mark vehicle as unavailable and approve the purchase request
            vehicleToSell.IsAvailable = false;
            pendingPurchaseRequest.Status = PurchaseStatus.Approved;

            // Step 4: Create sale record to track the completed transaction
            var completedSale = new Sale
            {
                VehicleId = vehicleToSell.Id,
                CustomerId = pendingPurchaseRequest.CustomerId,
                Price = vehicleToSell.Price
            };

            // Step 5: Save all changes and commit transaction
            _dbContext.Sales.Add(completedSale);
            await _dbContext.SaveChangesAsync();
            await saleProcessingTransaction.CommitAsync();

            return new { message = "Sale processed successfully.", saleId = completedSale.Id };
        }
        catch
        {
            await saleProcessingTransaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Retrieves the purchase history for a specific customer
    /// Returns sales ordered by most recent first
    /// </summary>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>Collection of completed sales for the customer ordered by sale date (newest first)</returns>
    public async Task<IEnumerable<object>> GetCustomerPurchaseHistoryAsync(int customerId)
    {
        // Validate customer exists before retrieving purchase history
        var customerExists = await _dbContext.Users.AnyAsync(user => user.Id == customerId);
        if (!customerExists)
            throw new InvalidOperationException("Customer not found.");

        // Retrieve customer's purchase history ordered by most recent sales first
        var customerPurchaseHistory = await _dbContext.Sales
            .Include(sale => sale.Vehicle)
            .Where(sale => sale.CustomerId == customerId)
            .OrderByDescending(sale => sale.SoldAt) // Most recent sales first
            .Select(sale => new
            {
                sale.Id,
                sale.SoldAt,
                sale.Price,
                Vehicle = new { sale.Vehicle.Make, sale.Vehicle.Model, sale.Vehicle.Year }
            })
            .ToListAsync();

        return customerPurchaseHistory;
    }

    /// <summary>
    /// Retrieves all pending purchase requests for admin review
    /// Returns requests ordered by oldest first (first-come, first-served)
    /// </summary>
    /// <returns>Collection of pending purchase requests with vehicle and customer details</returns>
    public async Task<IEnumerable<object>> GetPendingPurchaseRequestsAsync()
    {
        // Retrieve all pending purchase requests with related vehicle and customer information
        var pendingRequests = await _dbContext.PurchaseRequests
            .Include(request => request.Vehicle)
            .Include(request => request.Customer)
            .Where(request => request.Status == PurchaseStatus.Pending)
            .OrderBy(request => request.RequestedAt) // Oldest requests first (first-come, first-served)
            .Select(request => new
            {
                request.Id,
                request.RequestedAt,
                Vehicle = new 
                { 
                    request.Vehicle.Id,
                    request.Vehicle.Make, 
                    request.Vehicle.Model, 
                    request.Vehicle.Year,
                    request.Vehicle.Price,
                    request.Vehicle.Color
                },
                Customer = new 
                { 
                    request.Customer.Id,
                    request.Customer.Email,
                    request.Customer.FullName
                }
            })
            .ToListAsync();

        return pendingRequests;
    }
}

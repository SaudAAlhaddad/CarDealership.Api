using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;

namespace CarDealership.Api.Services;

/// <summary>
/// Service interface for purchase and sale operations
/// Handles purchase requests, admin approvals, and transaction history
/// </summary>
public interface IPurchaseService
{
    /// <summary>
    /// Creates a new purchase request with vehicle-bound OTP validation
    /// Uses database transactions to prevent race conditions
    /// </summary>
    /// <param name="vehicleId">ID of the vehicle to purchase</param>
    /// <param name="customerId">ID of the customer making the request</param>
    /// <param name="otpCode">OTP code bound to the specific vehicle</param>
    /// <returns>Purchase request result with request ID</returns>
    /// <exception cref="ArgumentException">Thrown when input parameters are invalid</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when customer is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when OTP is invalid, vehicle is unavailable, or customer has pending request for same vehicle</exception>
    Task<object> CreatePurchaseRequestAsync(int vehicleId, int customerId, string otpCode);
    
    /// <summary>
    /// Processes an approved purchase request and completes the sale
    /// Marks vehicle as unavailable and creates sale record
    /// Uses database transactions to ensure data consistency
    /// </summary>
    /// <param name="purchaseRequestId">ID of the purchase request to process</param>
    /// <returns>Sale completion result with sale ID</returns>
    /// <exception cref="InvalidOperationException">Thrown when request is not found, already processed, or vehicle unavailable</exception>
    Task<object> ProcessPurchaseRequestAsync(int purchaseRequestId);
    
    /// <summary>
    /// Retrieves the purchase history for a specific customer
    /// Returns sales ordered by most recent first
    /// </summary>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>Collection of completed sales for the customer ordered by sale date (newest first)</returns>
    Task<IEnumerable<object>> GetCustomerPurchaseHistoryAsync(int customerId);
    
    /// <summary>
    /// Retrieves all pending purchase requests for admin review
    /// Returns requests ordered by oldest first (first-come, first-served)
    /// </summary>
    /// <returns>Collection of pending purchase requests with vehicle and customer details</returns>
    Task<IEnumerable<object>> GetPendingPurchaseRequestsAsync();
}


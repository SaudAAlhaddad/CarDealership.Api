using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;

namespace CarDealership.Api.Services;

/// <summary>
/// Service interface for user management operations
/// Handles registration, authentication, and user data retrieval
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new customer with OTP validation
    /// Returns JWT token for immediate authentication
    /// </summary>
    /// <param name="registrationData">User registration information including OTP</param>
    /// <returns>Authentication result with JWT token</returns>
    Task<AuthResultDto> RegisterAsync(RegisterDto registrationData);
    
    /// <summary>
    /// Authenticates an existing user with password and OTP
    /// Returns JWT token for session management
    /// </summary>
    /// <param name="loginCredentials">User login credentials including OTP</param>
    /// <returns>Authentication result with JWT token</returns>
    Task<AuthResultDto> LoginAsync(LoginDto loginCredentials);
    
    /// <summary>
    /// Retrieves all customer users (excludes admin users)
    /// </summary>
    /// <returns>Collection of customer user data</returns>
    Task<IEnumerable<object>> GetAllCustomersAsync();
    
    /// <summary>
    /// Finds a user by their unique identifier
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetUserByIdAsync(int userId);
    
    /// <summary>
    /// Finds a user by their email address
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetUserByEmailAsync(string email);
}


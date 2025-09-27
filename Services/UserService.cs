using CarDealership.Api.Data;
using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;
using CarDealership.Api.Security;
using CarDealership.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Services;

/// <summary>
/// Service responsible for user management operations
/// Handles registration, authentication, and user data retrieval
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IOtpService _otpService;
    private readonly IJwtService _jwtService;

    /// <summary>
    /// Initializes a new instance of the UserService
    /// </summary>
    /// <param name="dbContext">Database context for user operations</param>
    /// <param name="otpService">OTP service for validation</param>
    /// <param name="jwtService">JWT service for token generation</param>
    public UserService(AppDbContext dbContext, IOtpService otpService, IJwtService jwtService)
    {
        _dbContext = dbContext;
        _otpService = otpService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Registers a new customer with OTP validation and password hashing
    /// </summary>
    /// <param name="registrationData">User registration information including OTP</param>
    /// <returns>Authentication result with JWT token</returns>
    /// <exception cref="InvalidOperationException">Thrown when OTP is invalid or email is already registered</exception>
    public async Task<AuthResultDto> RegisterAsync(RegisterDto registrationData)
    {
        // Step 1: Validate OTP before proceeding with registration
        var isOtpValid = await _otpService.ValidateAsync(registrationData.Email, OtpPurpose.Register, registrationData.OtpCode);
        if (!isOtpValid)
            throw new InvalidOperationException("Invalid or expired OTP.");

        // Step 2: Check if email is already registered to prevent duplicates
        var emailAlreadyExists = await _dbContext.Users.AnyAsync(u => u.Email == registrationData.Email.ToLower());
        if (emailAlreadyExists)
            throw new InvalidOperationException("Email already registered.");

        // Step 3: Create new customer user with hashed password
        var newCustomer = new User
        {
            Email = registrationData.Email.ToLower(),
            PasswordHash = PasswordHasher.Hash(registrationData.Password),
            Role = UserRole.Customer,
            FullName = registrationData.FullName
        };

        // Step 4: Save customer to database
        _dbContext.Users.Add(newCustomer);
        await _dbContext.SaveChangesAsync();

        // Step 5: Generate JWT token for immediate login after registration
        var jwtToken = _jwtService.CreateToken(newCustomer);
        return new AuthResultDto(jwtToken, newCustomer.Role.ToString(), newCustomer.Email, newCustomer.FullName ?? "");
    }

    /// <summary>
    /// Authenticates user with password and OTP validation
    /// </summary>
    /// <param name="loginCredentials">User login credentials including OTP</param>
    /// <returns>Authentication result with JWT token</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when OTP is invalid or expired</exception>
    public async Task<AuthResultDto> LoginAsync(LoginDto loginCredentials)
    {
        // Step 1: Find user by email and validate password
        var authenticatedUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == loginCredentials.Email.ToLower());
        var isPasswordValid = authenticatedUser is not null && authenticatedUser.PasswordHash == PasswordHasher.Hash(loginCredentials.Password);
        
        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid credentials.");

        // Step 2: Validate OTP for additional security layer
        var isOtpValid = await _otpService.ValidateAsync(loginCredentials.Email, OtpPurpose.Login, loginCredentials.OtpCode);
        if (!isOtpValid)
            throw new InvalidOperationException("Invalid or expired OTP.");

        // Step 3: Generate JWT token for authenticated session
        var jwtToken = _jwtService.CreateToken(authenticatedUser);
        return new AuthResultDto(jwtToken, authenticatedUser.Role.ToString(), authenticatedUser.Email, authenticatedUser.FullName ?? "");
    }

    /// <summary>
    /// Retrieves all customer users (excludes admin users)
    /// </summary>
    /// <returns>Collection of customer user data without sensitive information</returns>
    public async Task<IEnumerable<object>> GetAllCustomersAsync()
    {
        var customerList = await _dbContext.Users
            .Where(user => user.Role == UserRole.Customer)
            .Select(user => new { user.Id, user.Email, user.FullName })
            .ToListAsync();

        return customerList;
    }

    /// <summary>
    /// Finds a user by their unique identifier
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>User entity or null if not found</returns>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }

    /// <summary>
    /// Finds a user by their email address
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>User entity or null if not found</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email.ToLower());
    }
}


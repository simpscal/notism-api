using AutoMapper;

using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;
using Notism.Shared.Models;

namespace Notism.Application.Auth.Register;

public class RegisterUseCase : IRequestHandler<RegisterRequest, Result<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public RegisterUseCase(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var userByEmailSpec = new UserByEmailSpecification(request.Email);
        var existingUser = await _userRepository.FindByExpressionAsync(userByEmailSpec);

        if (existingUser != null)
        {
            throw new ResultFailureException("User with this email already exists");
        }

        // 2. Create new user with hashed password
        var hashedPassword = _passwordService.HashPassword(request.Password);
        var user = User.Create(request.Email, request.Password);
        var userWithHashedPassword = user.WithHashedPassword(hashedPassword);

        await _userRepository.AddAsync(userWithHashedPassword);

        if (await _userRepository.SaveChangesAsync() < 1)
        {
            throw new ResultFailureException("There was an error creating the user");
        }

        // 3. Generate JWT token
        var token = await _tokenService.GenerateTokenAsync(userWithHashedPassword);

        // 4. Map to response using AutoMapper
        var response = _mapper.Map<RegisterResponse>(userWithHashedPassword);
        response.Token = token.Token;
        response.ExpiresAt = token.ExpiresAt;
        response.RefreshToken = token.RefreshToken;
        response.RefreshTokenExpiresAt = token.RefreshTokenExpiresAt;

        return Result<RegisterResponse>.Success(response);
    }
}
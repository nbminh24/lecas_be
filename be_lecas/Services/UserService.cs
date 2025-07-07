using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using be_lecas.Common;
using AutoMapper;

namespace be_lecas.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Avatar = request.Avatar,
                GoogleId = request.GoogleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<UserDto?> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrEmpty(request.Avatar))
                user.Avatar = request.Avatar;

            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserDto>(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await _userRepository.DeleteAsync(userId);
        }

        public Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            // TODO: Implement logic
            return Task.FromResult(ApiResponse.ErrorResult("Not implemented"));
        }

        public Task<ApiResponse<UserDto?>> AddAddressAsync(string userId, AddressRequest request)
        {
            // TODO: Implement logic
            return Task.FromResult(ApiResponse<UserDto?>.ErrorResult("Not implemented"));
        }

        public Task<ApiResponse> RemoveAddressAsync(string userId, string addressId)
        {
            // TODO: Implement logic
            return Task.FromResult(ApiResponse.ErrorResult("Not implemented"));
        }
    }
}


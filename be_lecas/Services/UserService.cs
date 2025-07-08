using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using be_lecas.Common;
using AutoMapper;
using MongoDB.Bson;

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

        public async Task<ApiResponse<UserDto?>> AddAddressAsync(string userId, AddressRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<UserDto?>.ErrorResult("User not found");

            // Map AddressRequest sang Address (model)
            var newAddress = new Address
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = request.Name,
                Phone = request.Phone,
                AddressLine = request.Address,
                City = request.City,
                District = request.District,
                Note = request.Note,
                IsDefault = request.IsDefault
            };

            // Nếu là địa chỉ mặc định, bỏ cờ IsDefault ở các địa chỉ khác
            if (newAddress.IsDefault)
            {
                foreach (var addr in user.Addresses)
                {
                    addr.IsDefault = false;
                }
            }

            user.Addresses.Add(newAddress);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto?>.SuccessResult(userDto, "Address added successfully");
        }

        public Task<ApiResponse> RemoveAddressAsync(string userId, string addressId)
        {
            // TODO: Implement logic
            return Task.FromResult(ApiResponse.ErrorResult("Not implemented"));
        }
    }
}


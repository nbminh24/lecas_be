using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace be_lecas.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public PromotionService(IPromotionRepository promotionRepository, IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<PromotionDto?> GetByIdAsync(string id)
        {
            var promo = await _promotionRepository.GetByIdAsync(id);
            return promo == null ? null : _mapper.Map<PromotionDto>(promo);
        }

        public async Task<List<PromotionDto>> GetAllAsync()
        {
            var promos = await _promotionRepository.GetAllAsync();
            return _mapper.Map<List<PromotionDto>>(promos);
        }

        public async Task<List<PromotionDto>> GetActivePromotionsAsync()
        {
            var promos = await _promotionRepository.GetActivePromotionsAsync();
            return _mapper.Map<List<PromotionDto>>(promos);
        }

        public async Task<PromotionDto> CreateAsync(CreatePromotionRequest request)
        {
            var promo = _mapper.Map<Promotion>(request);
            promo.Id = System.Guid.NewGuid().ToString();
            promo.IsActive = true;
            var created = await _promotionRepository.CreateAsync(promo);
            return _mapper.Map<PromotionDto>(created);
        }

        public async Task<PromotionDto?> UpdateAsync(string id, UpdatePromotionRequest request)
        {
            var promo = await _promotionRepository.GetByIdAsync(id);
            if (promo == null) return null;
            if (request.Name != null) promo.Name = request.Name;
            if (request.Description != null) promo.Description = request.Description;
            if (request.DiscountType != null) promo.DiscountType = request.DiscountType;
            if (request.DiscountValue.HasValue) promo.DiscountValue = request.DiscountValue.Value;
            if (request.StartDate.HasValue) promo.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) promo.EndDate = request.EndDate.Value;
            if (request.IsActive.HasValue) promo.IsActive = request.IsActive.Value;
            if (request.ProductIds != null) promo.ProductIds = request.ProductIds;
            var updated = await _promotionRepository.UpdateAsync(id, promo);
            return updated == null ? null : _mapper.Map<PromotionDto>(updated);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _promotionRepository.DeleteAsync(id);
        }
    }
}

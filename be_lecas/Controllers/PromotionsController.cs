using be_lecas.DTOs;
using be_lecas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var result = await _promotionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult> GetActive()
        {
            var result = await _promotionService.GetActivePromotionsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(string id)
        {
            var result = await _promotionService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreatePromotionRequest request)
        {
            var result = await _promotionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] UpdatePromotionRequest request)
        {
            var result = await _promotionService.UpdateAsync(id, request);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var success = await _promotionService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}

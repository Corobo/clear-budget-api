using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionsService.Models.DTO;
using TransactionsService.Services;


namespace TransactionsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "clear-budget")]
    public class TransactionsController : AuthorizedControllerBase
    {
        private readonly ITransactionService _transactionsService;

        public TransactionsController(ITransactionService transactionsService)
        {
            _transactionsService = transactionsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetAll()
        {
            var transactions = await _transactionsService.GetAllByUserIdAsync(UserId);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDTO>> GetById(Guid id)
        {

            var transaction = await _transactionsService.GetByIdAsync(id, UserId);
            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDTO>> Create(CreateTransactionDTO dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var created = await _transactionsService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateTransactionDTO dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var updated = await _transactionsService.UpdateAsync(id, dto, userId);
            if (!updated) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null) return Unauthorized();

            var deleted = await _transactionsService.DeleteAsync(id, userId);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}

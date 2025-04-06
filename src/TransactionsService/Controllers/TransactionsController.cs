using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth.Controllers;
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


        [HttpGet("by-user/{userId}")]
        [Authorize(Roles = "clear-budget-m2m")]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetAllByUser(Guid userId)
        {
            var transactions = await _transactionsService.GetAllByUserIdAsync(userId);
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
            var created = await _transactionsService.CreateAsync(dto, UserId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateTransactionDTO dto)
        {
            var updated = await _transactionsService.UpdateAsync(id, dto, UserId);
            if (!updated) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _transactionsService.DeleteAsync(id, UserId);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}

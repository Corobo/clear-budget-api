using AutoMapper;
using TransactionsService.Models.DB;
using TransactionsService.Models.DTO;
using TransactionsService.Repositories;

namespace TransactionsService.Services.Impl
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransactionDTO>> GetAllByUserIdAsync(string userId)
        {
            var entities = await _repository.GetAllByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<TransactionDTO>>(entities);
        }

        public async Task<TransactionDTO?> GetByIdAsync(Guid id, string userId)
        {
            var entity = await _repository.GetByIdAsync(id, userId);
            return entity == null ? null : _mapper.Map<TransactionDTO>(entity);
        }

        public async Task<TransactionDTO> CreateAsync(CreateTransactionDTO dto, string userId)
        {
            var entity = _mapper.Map<Transaction>(dto);
            entity.UserId = userId;

            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<TransactionDTO>(created);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTransactionDTO dto, string userId)
        {
            var existing = await _repository.GetByIdAsync(id, userId);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            existing.Id = id;
            existing.UserId = userId;

            return await _repository.UpdateAsync(existing);
        }

        public Task<bool> DeleteAsync(Guid id, string userId)
        {
            return _repository.DeleteAsync(id, userId);
        }
    }
}

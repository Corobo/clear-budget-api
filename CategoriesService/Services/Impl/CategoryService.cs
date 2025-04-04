using AutoMapper;
using global::CategoriesService.Models.DB;
using global::CategoriesService.Models.DTO;
using global::CategoriesService.Repositories;
using Shared.Messaging.Configuration;
using Shared.Messaging.EventBus;
using Shared.Messaging.Events;
using Microsoft.Extensions.Options;

namespace CategoriesService.Services.Impl
{



    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IEventBusProducer<CategoryEvent> _eventProducer;
        private readonly string _exchangeName;

        public CategoryService(ICategoryRepository repo, IMapper mapper, IEventBusProducer<CategoryEvent> eventProducer,
            IOptions<RabbitMQOptions> options)
        {
            _repo = repo;
            _mapper = mapper;
            _eventProducer = eventProducer;
            _exchangeName = options.Value.ExchangeNames["Category"];
        }

        public async Task<IEnumerable<CategoryDTO>> GetMergedCategoriesAsync(Guid userId)
        {
            var userCategories = await _repo.GetUserCategoriesAsync(userId);
            var adminCategories = await _repo.GetAdminCategoriesAsync();

            var merged = userCategories
                .Concat(adminCategories.Where(admin =>
                !userCategories.Any(user => user.Name == admin.Name)));

            return merged.Select(_mapper.Map<CategoryDTO>);
        }

        public async Task<IEnumerable<Guid>> GetAllCategories()
        {
            var categories = await _repo.GetAllCategoriesAsync();

            return categories.Select(x=> x.Id);
        }

        public async Task<IEnumerable<CategoryDTO>> GetAdminCategoriesAsync()
        {
            var categories = await _repo.GetAdminCategoriesAsync();
            return categories.Select(_mapper.Map<CategoryDTO>);
        }

        public async Task<CategoryDTO> CreateUserCategoryAsync(Guid userId, CategoryCreateDTO dto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Color = dto.Color,
                UserId = userId
            };

            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            await _eventProducer.PublishAsync(new CategoryEvent
            {
                EventType = "category.created",
                CategoryId = category.Id,
                UserId = category.UserId
            }, _exchangeName);


            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> CreateAdminCategoryAsync(CategoryCreateDTO dto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Color = dto.Color,
                UserId = null
            };

            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            await _eventProducer.PublishAsync(new CategoryEvent
            {
                EventType = "category.created",
                CategoryId = category.Id,
                UserId = null
            }, _exchangeName);

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<bool> UpdateCategoryColorAsync(Guid id, Guid userId, CategoryUpdateDTO dto)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null)
                return false;

            // Only allow update if:
            // - the category is owned by the user
            // - or it's a global category (admin-only updates should be guarded in controller)
            if (category.UserId != null && category.UserId != userId)
                return false;

            category.Color = dto.Color;

            await _repo.SaveChangesAsync();

            await _eventProducer.PublishAsync(new CategoryEvent
            {
                EventType = "category.updated",
                CategoryId = category.Id,
                UserId = category.UserId
            }, _exchangeName);

            return true;
        }

        public async Task<bool> DeleteUserCategoryAsync(Guid id, Guid userId)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null || category.UserId != userId)
                return false;

            await _repo.DeleteAsync(category);
            await _repo.SaveChangesAsync();

            await _eventProducer.PublishAsync(new CategoryEvent
            {
                EventType = "category.deleted",
                CategoryId = category.Id,
                UserId = category.UserId
            }, _exchangeName);


            return true;
        }

        public async Task<bool> DeleteAdminCategoryAsync(Guid id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null || category.UserId != null)
                return false;

            await _repo.DeleteAsync(category);
            await _repo.SaveChangesAsync();

            await _eventProducer.PublishAsync(new CategoryEvent
            {
                EventType = "category.deleted",
                CategoryId = category.Id,
                UserId = null
            }, _exchangeName);


            return true;
        }
    }

}

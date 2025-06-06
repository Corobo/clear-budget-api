﻿using AutoMapper;
using CategoriesService.Models.DB;
using CategoriesService.Models.DTO;

namespace CategoriesService.Profiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDTO>();
    }
}

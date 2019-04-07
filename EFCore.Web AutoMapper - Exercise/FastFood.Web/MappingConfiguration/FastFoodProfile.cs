namespace FastFood.Web.MappingConfiguration
{
    using AutoMapper;
    using FastFood.Web.ViewModels.Categories;
    using FastFood.Web.ViewModels.Employees;
    using FastFood.Web.ViewModels.Items;
    using FastFood.Web.ViewModels.Orders;
    using Models;

    using ViewModels.Positions;

    public class FastFoodProfile : Profile
    {
        public FastFoodProfile()
        {
            PositionsMapping();

            EmployeesMapping();

            CategoriesMapping();

            ItemsMapping();

            OrdersMapping();
        }

        private void OrdersMapping()
        {
            this.CreateMap<CreateOrderInputModel, Order>()
                .ForMember(x => x.EmployeeName, y => y.MapFrom(p => p.EmployeeName))
                .ForMember(x => x.ItemName, y => y.MapFrom(p => p.ItemName));

            this.CreateMap<Order, OrderAllViewModel>()
                .ForMember(x => x.OrderId, y => y.MapFrom(p => p.Id))
                .ForMember(x => x.Employee, y => y.MapFrom(p => p.Employee.Name))
                .ForMember(x => x.DateTime, y => y.MapFrom(p => p.DateTime.ToString("g")));
        }

        private void ItemsMapping()
        {
            this.CreateMap<Category, CreateItemViewModel>()
                .ForMember(x => x.CategoryName, y => y.MapFrom(p => p.Name));

            this.CreateMap<CreateItemInputModel, Item>();

            this.CreateMap<Item, ItemsAllViewModels>()
                .ForMember(x => x.Category, y => y.MapFrom(p => p.Category.Name));
        }

        private void CategoriesMapping()
        {
            this.CreateMap<CreateCategoryInputModel, Category>()
                .ForMember(x => x.Name, y => y.MapFrom(p => p.CategoryName));

            this.CreateMap<Category, CategoryAllViewModel>()
                .ForMember(x => x.Name, y => y.MapFrom(p => p.Name));
        }

        private void EmployeesMapping()
        {
            this.CreateMap<Position, RegisterEmployeeViewModel>()
                .ForMember(x => x.PositionName, y => y.MapFrom(p => p.Name));

            this.CreateMap<RegisterEmployeeInputModel, Employee>();

            this.CreateMap<Employee, EmployeesAllViewModel>()
                .ForMember(x => x.Position, y => y.MapFrom(p => p.Position.Name));
        }

        private void PositionsMapping()
        {
            this.CreateMap<CreatePositionInputModel, Position>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.PositionName));

            this.CreateMap<Position, PositionsAllViewModel>()
                .ForMember(x => x.Name, y => y.MapFrom(s => s.Name));
        }
    }
}
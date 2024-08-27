using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SalesSystem.Entity;
using SalesSystem.DTO;
using System.Globalization;

namespace SalesSystem.Utility
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            #region Rol
            CreateMap<Rol, RolDTO>().ReverseMap();
            #endregion

            #region Menu
            CreateMap<Menu, MenuDTO>().ReverseMap();
            #endregion

            #region Usuario
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(destination => destination.RolDescription, option => option.MapFrom(source => source.IdRolNavigation.Name))
                .ForMember(destination => destination.Status, option => option.MapFrom(source => source.Status == true ? 1 : 0));

            CreateMap<Usuario, SessionDTO>()
                .ForMember(destination => destination.RolDescription, option => option.MapFrom(source => source.IdRolNavigation.Name));

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(destination => destination.IdRolNavigation, option => option.Ignore())
                .ForMember(destination => destination.Status, option => option.MapFrom(source => source.Status == 1 ? true : false));
            #endregion

            #region Category
            CreateMap<Category, CategoryDTO>().ReverseMap();
            #endregion Category

            #region Product
            CreateMap<Product, ProductDTO>()
                .ForMember(destination => destination.CategoryDescription, option => option.MapFrom(source => source.IdCategoryNavigation.Name))
                .ForMember(destination => destination.Price, option => option.MapFrom(source => Convert.ToString(source.Price.Value, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.Status, option => option.MapFrom(source => source.Status == true ? 1 : 0));

            CreateMap<ProductDTO, Product>()
              .ForMember(destination => destination.IdCategoryNavigation, option => option.Ignore())
              .ForMember(destination => destination.Price, option => option.MapFrom(source => Convert.ToDecimal(source.Price, new CultureInfo("es-CR"))))
              .ForMember(destination => destination.Status, option => option.MapFrom(source => source.Status == 1 ? true : false));
            #endregion Product

            #region Sale
            CreateMap<Sale, SaleDTO>()
                .ForMember(destination => destination.Total_Text, option => option.MapFrom(source => Convert.ToString(source.Total.Value, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.DateRegistry, option => option.MapFrom(source => source.DateRegistry.HasValue ? source.DateRegistry.Value.ToString("dd/MM/yyyy") : string.Empty));

            CreateMap<SaleDTO, Sale>()
                .ForMember(destination => destination.Total, option => option.MapFrom(source => Convert.ToDecimal(source.Total_Text, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.DateRegistry, option => option.MapFrom(source =>
                    string.IsNullOrEmpty(source.DateRegistry) ? (DateTime?)null : DateTime.ParseExact(source.DateRegistry, "dd/MM/yyyy", CultureInfo.InvariantCulture)));
            #endregion Sale


            #region SaleDetail
            CreateMap<SaleDetail, SaleDetailDTO>()
                .ForMember(destination => destination.ProductDescription, option => option.MapFrom(source => source.IdProductNavigation.Name))
                .ForMember(destination => destination.Price, option => option.MapFrom(source => Convert.ToString(source.Price.Value, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.Total, option => option.MapFrom(source => Convert.ToString(source.Total, new CultureInfo("es-CR"))));

            CreateMap<SaleDetailDTO, SaleDetail>()
                .ForMember(destination => destination.Price, option => option.MapFrom(source => Convert.ToDecimal(source.Price, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.Total, option => option.MapFrom(source => Convert.ToDecimal(source.Total, new CultureInfo("es-CR"))));
            #endregion SaleDetail

            #region Report
            CreateMap<SaleDetail, ReportDTO>()
                .ForMember(destination => destination.DateRegistry, option => option.MapFrom(source => source.IdSaleNavigation.DateRegistry.Value.ToString("dd/MM/yyyy")))
                .ForMember(destination => destination.numberDocument, option => option.MapFrom(source => source.IdSaleNavigation.numberDocument))
                .ForMember(destination => destination.PaymentType, option => option.MapFrom(source => source.IdSaleNavigation.PaymentType))
                .ForMember(destination => destination.TotalSale, option => option.MapFrom(source => Convert.ToString(source.IdSaleNavigation.Total, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.Product, option => option.MapFrom(source => source.IdProductNavigation.Name))
                .ForMember(destination => destination.Price, option => option.MapFrom(source => Convert.ToString(source.Price.Value, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.quantity, option => option.MapFrom(source => Convert.ToString(source.Quantity.Value, new CultureInfo("es-CR"))))
                .ForMember(destination => destination.Total, option => option.MapFrom(source => Convert.ToString(source.Total.Value, new CultureInfo("es-CR"))));
            #endregion Report
        }
    }
}

using System;
using AutoMapper;
using DonorGateway.Domain;
using Heroic.AutoMapper;

namespace DonorGateway.Admin.ViewModels
{
    public class TaxItemViewModel : IMapFrom<TaxItem>, IHaveCustomMappings
    {
        public int Id { get; set; }
        public int TaxYear { get; set; }
        public DateTime? DonationDate { get; set; }
        public decimal Amount { get; set; }
        public bool? IsUpdated { get; set; }
        public int ConstituentId { get; set; }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<TaxItem, TaxItemViewModel>()
                .ForMember(d => d.TaxYear, opt => opt.MapFrom(s => s.TaxYear))
                .ReverseMap();
        }
    }
}

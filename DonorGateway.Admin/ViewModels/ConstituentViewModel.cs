using System.Collections.Generic;
using AutoMapper;
using DonorGateway.Domain;
using Heroic.AutoMapper;

namespace DonorGateway.Admin.ViewModels
{
    public class ConstituentViewModel : IMapFrom<Constituent>, IHaveCustomMappings
    {
        public ConstituentViewModel()
        {
            TaxItems = new List<TaxItem>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string LookupId { get; set; }
        public string FinderNumber { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public List<TaxItem> TaxItems { get; set; }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<Constituent, ConstituentViewModel>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email)).ReverseMap()
                ;
        }
    }
}
using System;
using AutoMapper;
using DonorGateway.Domain;
using Heroic.AutoMapper;

namespace DonorGateway.Admin.ViewModels
{
    public class DemographicViewModel : IMapFrom<DemographicChange>, IHaveCustomMappings
    {
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
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Source { get; set; }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<DemographicChange, DemographicViewModel>()
                .ForMember(d => d.Source, opt => opt.MapFrom(s => s.Source.ToString()))
                ;
        }

        //public static Source MapSource(string source)
        //{
        //    //TODO: function to map a string to a SchoolGradeDTO
        //    return EnumHelper<SchoolGradeDTO>.Parse(grade);
        //}

    }
}
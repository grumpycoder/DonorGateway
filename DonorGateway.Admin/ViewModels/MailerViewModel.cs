using System;
using AutoMapper;
using DonorGateway.Domain;
using Heroic.AutoMapper;

namespace DonorGateway.Admin.ViewModels
{
    public class MailerViewModel : IMapFrom<Mailer>, IHaveCustomMappings
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string SourceCode { get; set; }
        public string FinderNumber { get; set; }
        public bool? Suppress { get; set; }
        public int? CampaignId { get; set; }
        public int? ReasonId { get; set; }
        public string Reason { get; set; }
        public string Campaign { get; set; }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<Mailer, MailerViewModel>()
                   .ForMember(d => d.Reason, opt => opt.MapFrom(s => s.Reason.Name))
                   .ForMember(d => d.Campaign, opt => opt.MapFrom(s => s.Campaign.Name))
                   ;
        }
    }
}
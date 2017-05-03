using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper;
using DonorGateway.Domain;
using DonorGateway.Domain.Helpers;
using FluentValidation;
using FluentValidation.Attributes;
using Heroic.AutoMapper;

namespace DonorGateway.RSVP.ViewModels
{
    [Validator(typeof(RegisterFormViewModelValidator))]
    public class RegisterFormViewModel : IMapFrom<Guest>, IHaveCustomMappings
    {
        public int GuestId { get; set; }

        public string LookupId { get; set; }

        public string PromoCode { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zipcode { get; set; }

        public string Comment { get; set; }

        public int? TicketCount { get; set; }

        public int? EventTicketAllowance { get; set; }

        public bool? IsAttending { get; set; }
        public bool? IsWaiting { get; set; }

        public DateTime? ResponseDate { get; set; }
        public DateTime? WaitingDate { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; }

        public bool IsRegistered => ResponseDate != null;

        public Template Template { get; set; }


        public void ProcessMessages()
        {
            var properties = typeof(FinishFormViewModel).GetProperties().Where(p => p.PropertyType == typeof(string));
            foreach (var prop in properties)
            {
                ProcessField(prop);
            }
        }

        private void ProcessField(PropertyInfo field)
        {
            if (field.GetValue(this, null) == null) return;

            var text = field.GetValue(this, null).ToString();

            var propertyInfos = typeof(FinishFormViewModel).GetProperties().Where(p => p.PropertyType == typeof(string));

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetValue(this, null) == null) continue;
                var propValue = propertyInfo.GetValue(this, null).ToString();

                if (string.IsNullOrWhiteSpace(propValue)) continue;
                text = ReplaceText(text, propertyInfo.Name, propValue);
            }

            field.SetValue(this, Convert.ChangeType(text, field.PropertyType), null);

        }

        private static string ReplaceText(string stringToReplace, string fieldName, string fieldValue)
        {
            var pattern = "@{" + fieldName + "}";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(stringToReplace);

            return matches.Replace(stringToReplace, fieldValue);

        }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<RegisterFormViewModel, Guest>()
                .ForMember(d => d.FinderNumber, opt => opt.MapFrom(o => o.PromoCode))
                .ForMember(d => d.Id, opt => opt.MapFrom(o => o.GuestId));

            configuration.CreateMap<Guest, RegisterFormViewModel>()
                .ForMember(vm => vm.GuestId, map => map.MapFrom(m => m.Id))
                .ForMember(vm => vm.PromoCode, map => map.MapFrom(m => m.FinderNumber))
                .ForMember(vm => vm.Template, map => map.MapFrom(m => m.Event.Template));
        }
    }

    public class RegisterFormViewModelValidator : AbstractValidator<RegisterFormViewModel>
    {
        public RegisterFormViewModelValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("Name is required.");
            RuleFor(x => x.Address).NotNull().WithMessage("Address is required.");
            RuleFor(x => x.City).NotNull().WithMessage("City is required.");
            RuleFor(x => x.State).NotNull().WithMessage("State is required.");
            RuleFor(x => x.Zipcode).NotNull().WithMessage("Zipcode is required.");
            RuleFor(x => x.Phone).NotNull().WithMessage("Phone is required.");
            RuleFor(x => x.Email).NotNull().WithMessage("Email is required.");
            RuleFor(x => x.IsAttending).NotNull().WithMessage("Please select Yes or No if you are attending.");
        }

    }
}
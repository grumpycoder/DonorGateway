using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DonorGateway.Domain;
using DonorGateway.Domain.Helpers;

namespace DonorGateway.RSVP.Helpers
{
    public class TemplateHelper
    {
        public static Template ParseGuestTemplate(Guest guest, Template template)
        {
            ParseTemplate(guest, template);
            return template;
        }

        private static void ParseTemplate(Guest guest, Template template)
        {
            var properties = typeof(Template).GetProperties().Where(p => p.PropertyType == typeof(string));

            foreach (var prop in properties)
            {
                if (prop.GetValue(template, null) == null) continue;

                var propValue = guest.Parse(prop.GetValue(template, null).ToString());
                if (string.IsNullOrWhiteSpace(propValue)) continue;

                prop.SetValue(template, Convert.ChangeType(propValue, prop.PropertyType), null);

            }

        }

    }
}

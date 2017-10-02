using DonorGateway.Domain.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DonorGateway.Domain
{
    public class Event : BaseEntity
    {
        public Event()
        {
            Guests = new List<Guest>();
        }

        public string Initiative { get; set; }
        public string NameUrl { get; set; }
        public string DisplayName { get; set; }
        public string EventCode { get; set; }
        public string Speaker { get; set; }
        public string Venue { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public int Capacity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? VenueOpenDate { get; set; }
        public DateTime? RegistrationCloseDate { get; set; }
        public int? TicketAllowance { get; set; }
        public bool IsCancelled { get; set; }
        public int? TemplateId { get; set; }

        public int GuestWaitingCount { get; set; }
        public int GuestAttendanceCount { get; set; }
        public int TicketMailedCount { get; set; }

        [NotMapped]
        public int TicketRemainingCount
        {
            get
            {
                var remaining = Capacity - GuestAttendanceCount;
                if (remaining < 0) remaining = 0;
                return remaining;
            }
        }

        [NotMapped]
        public bool IsAtCapacity => TicketRemainingCount <= 0;

        public ICollection<Guest> Guests { get; set; }
        public virtual Template Template { get; set; }

        public void CancelRegistration(Guest guest)
        {
            if (guest.IsMailed) TicketMailedCount -= guest.TicketCount ?? 0;
            if (guest.IsWaiting) GuestWaitingCount -= guest.TicketCount ?? 0;

            GuestAttendanceCount -= guest.TicketCount ?? 0;

            guest.ResponseDate = null;
            guest.IsAttending = false;
            guest.IsMailed = false;
            guest.IsWaiting = false;
            guest.WaitingDate = null;
            guest.TicketCount = null;

        }

        public void RegisterGuest(Guest guest)
        {
            guest.ResponseDate = DateTime.Now;
            guest.TicketCount = guest.TicketCount ?? 0;

            if (TicketRemainingCount - guest.TicketCount < 0)
            {
                guest.IsWaiting = true;
                guest.WaitingDate = DateTime.Now;
                GuestWaitingCount += guest.TicketCount ?? 0;
            }
            else
            {
                guest.IsWaiting = false;
                guest.WaitingDate = null;
                GuestAttendanceCount += guest.TicketCount ?? 0;
            }

        }

        public void MailTicket(Guest guest)
        {
            guest.IsMailed = true;
            guest.MailedDate = DateTime.Now;

            TicketMailedCount += guest.TicketCount ?? 0;
        }

        public void AddTickets(Guest guest, int additionalTickets)
        {
            guest.TicketCount = guest.TicketCount + additionalTickets;
            if (guest.IsMailed) TicketMailedCount += additionalTickets;
            GuestAttendanceCount += additionalTickets;
        }

        public void RemoveFromWaiting(Guest guest)
        {
            guest.IsWaiting = false;
            GuestWaitingCount -= guest.TicketCount ?? 0;
            GuestAttendanceCount += guest.TicketCount ?? 0;
        }

        public void MoveToMailQueue(Guest guest)
        {
            guest.IsWaiting = false;
            guest.IsAttending = true;
            guest.IsMailed = false;
            GuestWaitingCount = GuestWaitingCount - guest.TicketCount ?? 0;
            GuestAttendanceCount += guest.TicketCount ?? 0;
        }

        public async Task SendEmail(Guest guest)
        {
            if (string.IsNullOrWhiteSpace(guest.Email)) return;

            ParseTemplate();
            ParseTemplate(guest);

            string message = "<link crossorigin=anonymous href=https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css integrity=sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u rel=stylesheet><style>body{margin-top:20px;font-size:13px;font-family:verdana}section{padding-bottom:15px}p.well-sm img{margin-left:-130}</style><body class=container><div class=row><div class=col-md-12><p class=well-sm><img alt=\"\"src=https://donate.splcenter.org/image/splc_logo_ecard.jpg></div></div>";

            message += $"<div class=row><div class=col-md-12><section><div><p>{DateTime.Today:D}</p></div></section></div></div>";

            if (guest.IsAttending == true && guest.IsWaiting) message += Template.WaitingResponseText;
            if (guest.IsAttending == true && guest.IsWaiting == false) message += Template.YesResponseText;
            if (guest.IsAttending == false) message += Template.NoResponseText;

            message += "<div class=row><div class=\"col-md-12 col-sm-12\"><div><div style=float:left;width:121px><img src=http://donate.splcenter.org/image/morris_dees_photo.png alt=\"\"class=\"img-responsive inline-block\"></div><div style=margin-left:135px><p>Sincerely,<div style=height:93px><img src=http://donate.splcenter.org/image/morris_dees_sig2.png style=margin-top:10px;height:32px></div><span>Morris Dees<br>Founder, Southern Poverty Law Center</span></div></div></div></div><hr><div class=row><div class=\"col-md-12 col-sm-12\"><p class=small><i><a href=\"https://donate.splcenter.org/sslpage.aspx?pid=463&erid=3925852&trid=5cf6eac0-d290-4710-a6ee-7252ba7c10fa&efndnum=10196485410\">Make a contribution</a> to support our work fighting hate, teaching tolerance, and seeking justice.<br></i><p class=small><i>Take advantage of corporate matching gift opportunities. Contact your employer\'s HR department to see if they <a href=https://www.splcenter.org/support-us/employer-matching>will match your contributions to the SPLC.</a></i><p class=small><i>We welcome your feedback.<br>Contact us <a href=https://www.splcenter.org/contact-us>online</a>.<br>400 Washington Ave.<br>Montgomery, AL 36104</i></div></div>";
            message += "</body>";

            var html = AlternateView.CreateAlternateViewFromString(message, null, "text/html");

            var sendToAddress = new EmailAddress(guest.Email);
            var sendFromAddress = new EmailAddress(ConfigurationManager.AppSettings["SendFromAddress"], ConfigurationManager.AppSettings["SendFromDisplay"]);
            var subject = $"SPLC Event {DisplayName} Confirmation";

            var env = ConfigurationManager.AppSettings["Environment"];
            switch (env)
            {
                case "Prod":
                    break;
                default:
                    sendToAddress = new EmailAddress(ConfigurationManager.AppSettings["SendToOverride"]);
                    break;
            }

            var apiKey = ConfigurationManager.AppSettings["SENDGRID_APIKEY"];


            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage
            {
                From = sendFromAddress,
                Subject = subject,
                HtmlContent = message
            };

            msg.AddTo(sendToAddress);

            var response = await client.SendEmailAsync(msg);
        }

        public void ParseTemplate(Guest guest)
        {
            var properties = typeof(Template).GetProperties().Where(p => p.PropertyType == typeof(string));

            foreach (var prop in properties)
            {
                if (prop.GetValue(Template, null) == null) continue;

                var propValue = guest.Parse(prop.GetValue(Template, null).ToString());
                if (string.IsNullOrWhiteSpace(propValue)) continue;

                prop.SetValue(Template, Convert.ChangeType(propValue, prop.PropertyType), null);

            }

        }

        public void ParseTemplate()
        {
            var properties = typeof(Template).GetProperties().Where(p => p.PropertyType == typeof(string));

            foreach (var prop in properties)
            {
                if (prop.GetValue(this.Template, null) == null) continue;

                var propValue = Parse(prop.GetValue(this.Template, null).ToString());
                if (string.IsNullOrWhiteSpace(propValue)) continue;

                prop.SetValue(this.Template, Convert.ChangeType(propValue, prop.PropertyType), null);

            }
        }

        private string Parse(string message)
        {
            var properties = typeof(Event).GetProperties().Where(p => p.PropertyType == typeof(DateTime?) || p.PropertyType == typeof(string));
            foreach (var prop in properties)
            {
                if (prop.GetValue(this, null) == null) continue;

                var propValue = prop.GetValue(this, null).ToString();
                if (prop.PropertyType == typeof(DateTime?))
                {
                    propValue = Convert.ToDateTime(prop.GetValue(this, null)).ToString("dddd, MMMM d, yyyy @ h:mm tt");
                }

                if (string.IsNullOrWhiteSpace(propValue)) continue;
                message = ReplaceText(message, prop.Name, propValue);
            }
            return message;
        }

        private static string ReplaceText(string stringToReplace, string fieldName, string fieldValue)
        {
            var pattern = "@{" + fieldName + "}";

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(stringToReplace);

            return matches.Replace(stringToReplace, fieldValue);

        }

    }


}
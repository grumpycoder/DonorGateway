using System;

namespace DonorGateway.Admin.Helpers
{
    public static class DateExtensions
    {
        public static DateTime ToDateTime(this string s)
        {
            DateTime dtr;
            var tryDtr = DateTime.TryParse(s, out dtr);
            return tryDtr ? dtr : new DateTime();
        }


        public static int Year(this DateTime s)
        {
            return s.Year;
        }
    }
}

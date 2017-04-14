using System;
using System.Collections.Generic;

namespace DonorGateway.Admin.ViewModels
{
    public class PagerModel<T>
    {
        public int? Page { get; set; } = 0;
        public int? PageSize { get; set; } = 20;
        public string OrderBy { get; set; }
        public string OrderDirection { get; set; }

        public double TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public IEnumerable<T> Results { get; set; }
    }
}
using System;


namespace DonorGateway.Domain
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            CreatedDate = DateTime.Now;
        }

        
        public int Id { get; set; }
        
        public DateTime? CreatedDate { get; set; }
        
        public string CreatedBy { get; set; }
        
        public DateTime? UpdatedDate { get; set; }
        
        public string UpdatedBy { get; set; }

    }
}
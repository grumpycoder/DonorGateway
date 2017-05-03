using System;
using System.Collections.Generic;

namespace DonorGateway.Admin.Models
{
    public class OperationResult
    {
        public OperationResult()
        {
            Messages = new List<string>();
        }

        public OperationResult(bool success) : this()
        {
            Success = success;
        }

        public OperationResult(bool success, string message) : this(success)
        {
            Messages.Add(message);
        }

        public OperationResult(bool success, string message, TimeSpan totalTime) : this(success, message)
        {
            TotalTime = totalTime;
        }

        public bool Success { get; set; }
        public int Result { get; set; }
        public List<string> Messages { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
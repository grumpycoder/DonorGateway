using Microsoft.Azure.WebJobs;
using System.IO;

namespace ProcessAcquisition
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("acquisitionqueue")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }
    }
}

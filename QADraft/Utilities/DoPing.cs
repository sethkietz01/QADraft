using System;
using System.Net.NetworkInformation;

namespace QADraft.Utilities
{
    public class DoPing
    {

        public static void ping()
        {
            string host = "health.aws.amazon.com"; // Replace with your host or IP address

            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(host);

            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine($"Ping to {host} successful. Response time: {reply.RoundtripTime} ms");
            }
            else
            {
                Console.WriteLine($"Ping to {host} failed. Status: {reply.Status}");
            }
        }

    }
}

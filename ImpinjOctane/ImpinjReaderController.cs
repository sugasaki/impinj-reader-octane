using Impinj.OctaneSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpinjOctane
{
    public class ImpinjReaderController
    {
        //const string READER_HOSTNAME = "192.168.0.101";  // NEED to set to your speedway!

        private string hostName { get; set; } = "";


        private ImpinjReader reader = new ImpinjReader();

        public Action<string>? OnConnectionLostEvent;
        public Action<string>? OnKeepaliveReceivedEvent;
        public Action<TagModel>? OnTagReportedEvent;
        public Action? OnStartCompleted;
        public Action<string>? OnStartException;
        public Action? OnStopCompleted;
        public Action<string>? OnStopException;


        private void ConnectToReader(string host)
        {
            try
            {
                Console.WriteLine("Attempting to connect to {0} ({1}).", reader.Name, host);

                // The maximum number of connection attempts
                // before throwing an exception.
                //reader.MaxConnectionAttempts = 15;

                // Number of milliseconds before a 
                // connection attempt times out.
                reader.ConnectTimeout = 6000;
                // Connect to the reader.
                // Change the ReaderHostname constant in SolutionConstants.cs 
                // to the IP address or hostname of your reader.
                reader.Connect(host);
                Console.WriteLine("Successfully connected.");

                // Tell the reader to send us any tag reports and 
                // events we missed while we were disconnected.
                reader.ResumeEventsAndReports();
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Failed to connect.");
                throw e;
            }
        }

        public void Start(string host)
        {
            hostName = host;

            try
            {
                // Assign a name to the reader. 
                // This will be used in tag reports. 
                reader.Name = "My Reader #1";

                // Connect to the reader.
                ConnectToReader(host);

                // Get the default settings.
                // We'll use these as a starting point
                // and then modify the settings we're 
                // interested in.
                Settings settings = reader.QueryDefaultSettings();

                // Start the reader as soon as it's configured.
                // This will allow it to run without a client connected.
                settings.AutoStart.Mode = AutoStartMode.Immediate;
                settings.AutoStop.Mode = AutoStopMode.None;

                // Use Advanced GPO to set GPO #1 
                // when an client (LLRP) connection is present.
                settings.Gpos.GetGpo(1).Mode = GpoMode.LLRPConnectionStatus;

                // Tell the reader to include the timestamp in all tag reports.
                settings.Report.IncludeFirstSeenTime = true;
                settings.Report.IncludeLastSeenTime = true;
                settings.Report.IncludeSeenCount = true;

                // If this application disconnects from the 
                // reader, hold all tag reports and events.
                settings.HoldReportsOnDisconnect = true;

                // Enable keepalives.
                settings.Keepalives.Enabled = true;
                settings.Keepalives.PeriodInMs = 5000;

                // Enable link monitor mode.
                // If our application fails to reply to
                // five consecutive keepalive messages,
                // the reader will close the network connection.
                settings.Keepalives.EnableLinkMonitorMode = true;
                settings.Keepalives.LinkDownThreshold = 5;

                // Assign an event handler that will be called
                // when keepalive messages are received.
                reader.KeepaliveReceived += OnKeepaliveReceived;

                // Assign an event handler that will be called
                // if the reader stops sending keepalives.
                reader.ConnectionLost += OnConnectionLost;

                // Apply the newly modified settings.
                reader.ApplySettings(settings);

                // Save the settings to the reader's 
                // non-volatile memory. This will
                // allow the settings to persist
                // through a power cycle.
                reader.SaveSettings();

                // Assign the TagsReported event handler.
                // This specifies which method to call
                // when tags reports are available.
                reader.TagsReported += OnTagsReported;

                //// Wait for the user to press enter.
                //Console.WriteLine("Press enter to exit.");
                //Console.ReadLine();

                //// Stop reading.
                //reader.Stop();

                //// Disconnect from the reader.
                //reader.Disconnect();
                if (OnStartCompleted != null) OnStartCompleted();
            }
            catch (OctaneSdkException e)
            {
                var message = string.Format("Octane SDK exception: {0}", e.Message);
                // Handle Octane SDK errors.
                Console.WriteLine(message);
                if (OnStartException != null) OnStartException(message);
            }
            catch (Exception e)
            {
                var message = string.Format("Exception : {0}", e.Message);
                // Handle other .NET errors.
                Console.WriteLine("Exception : {0}", e.Message);
                if (OnStartException != null) OnStartException(message);
            }
        }

        public void Stop()
        {
            try
            {
                // Stop reading.
                reader.Stop();

                // Disconnect from the reader.
                reader.Disconnect();

                if (OnStopCompleted != null) OnStopCompleted();
            }
            catch (OctaneSdkException e)
            {
                var message = string.Format("Octane SDK exception: {0}", e.Message);
                // Handle Octane SDK errors.
                Console.WriteLine(message);
                if (OnStopException != null) OnStopException(message);
            }
            catch (Exception e)
            {
                var message = string.Format("Exception : {0}", e.Message);
                // Handle other .NET errors.
                Console.WriteLine("Exception : {0}", e.Message);
                if (OnStopException != null) OnStopException(message);
            }
        }


        private void OnConnectionLost(ImpinjReader reader)
        {
            var message = string.Format("Connection lost : {0} ({1})", reader.Name, reader.Address);

            // This event handler is called if the reader  
            // stops sending keepalive messages (connection lost).
            Console.WriteLine(message);

            // Cleanup
            reader.Disconnect();

            // Try reconnecting to the reader
            ConnectToReader(hostName);

            if (OnConnectionLostEvent != null) OnConnectionLostEvent(message);
        }

        private void OnKeepaliveReceived(ImpinjReader reader)
        {
            var message = string.Format("Keepalive received from {0} ({1})", reader.Name, reader.Address);

            // This event handler is called when a keepalive 
            // message is received from the reader.
            Console.WriteLine(message);

            if (OnKeepaliveReceivedEvent != null) OnKeepaliveReceivedEvent(message);
        }

        private void OnTagsReported(ImpinjReader sender, TagReport report)
        {
            // This event handler is called asynchronously 
            // when tag reports are available.
            // Loop through each tag in the report 
            // and print the data.
            foreach (Tag tag in report)
            {
                string epc = tag.Epc.ToHexString();
                string message = string.Format("EPC : {0} Timestamp : {1}", epc, tag.LastSeenTime);

                Console.WriteLine(message);

                var tagModel = new TagModel()
                {
                    Name = epc,
                    LastSeenTime = tag.LastSeenTime.ToString()
                };

                if (OnTagReportedEvent != null) OnTagReportedEvent(tagModel);
            }

        }

    }
}

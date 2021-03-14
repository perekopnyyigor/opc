using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Opc.Ua;
using Opc.Ua.Client;
using System.Threading;
using System.Threading.Tasks;

namespace OPC_UA.Model
{
    class UA_client
    {
        private static ApplicationConfiguration config = new ApplicationConfiguration();
        private static Session session;
        public static void configurate()
        {
            config.ApplicationName = "MyHomework";
            config.ApplicationUri = Utils.Format(@"urn:{0}:" + "MyHomework" + "", "127.0.0.1");

            config.ApplicationType = ApplicationType.Client;
            config.SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = Utils.Format(@"CN={0}, DC={1}", "MyHomework", "127.0.0.1") },
                TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                AutoAcceptUntrustedCertificates = true,
                AddAppCertToTrustedStore = true
            };
            config.TransportConfigurations = new TransportConfigurationCollection();
            config.TransportQuotas = new TransportQuotas { OperationTimeout = 15000 };
            config.ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 };
            config.TraceConfiguration = new TraceConfiguration();

            config.Validate(ApplicationType.Client);
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }

        }

        public static void create_server()
        {
            var selectedEndpoint = CoreClientUtils.SelectEndpoint("opc.tcp://" + "127.0.0.1" + ":" + "4840" + "", useSecurity: false);
            session = Session.Create(config, new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)), false, "", 60000, null, null).GetAwaiter().GetResult();

        }
        
        public static void browse()
        {
          

           Console.WriteLine("Step 3 - Browse the server namespace.");
            ReferenceDescriptionCollection refs;
            Byte[] cp;
            session.Browse(null, null, ObjectIds.ObjectsFolder, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out cp, out refs);
            Console.WriteLine("DisplayName: BrowseName, NodeClass, NodeID");


            foreach (var rd in refs)
            {
                string space = " ";
                Console.WriteLine("{0}: {1}, {2}, {3}", rd.DisplayName, rd.BrowseName, rd.NodeClass, rd.NodeId);
                obhod(session, rd, space);

            }

            
            
            var subscription = new Subscription(session.DefaultSubscription) { PublishingInterval = 10 };

           

            Data.list.Add(new MonitoredItem(subscription.DefaultItem) { DisplayName = "ServerStatusCurrentTime", StartNodeId = "i=2258" });

            foreach(string name in Data.tags.Keys)
            {
                Data.list.Add(new MonitoredItem(subscription.DefaultItem) { DisplayName = Data.tags[name].DisplayName, StartNodeId = Data.tags[name].NodeId});
                
            }
            


            Data.list.ForEach(i => i.Notification += OnNotification);
            subscription.AddItems(Data.list);

           
            session.AddSubscription(subscription);
            subscription.Create();

            


        }

       
        public static void read()
        {
            while(true)
            {
                for (int i = 0; i < Data.list.Count; i++)
                {

                    foreach (DataValue value in Data.list[i].DequeueValues())

                      if (value.Value != null && Data.tags.ContainsKey(Data.list[i].DisplayName))
                      {
                          Data.tags[Data.list[i].DisplayName].Value = value.Value.ToString();

                      }
                }
                Thread.Sleep(1000);
            }
       
        }
        public static void start()
        {
            Thread thread = new Thread(new ThreadStart(read));
            thread.Start();
        }

        public static void write()
        {
            WriteValue value = new WriteValue();
            value.NodeId = Data.tags["DigitConst"].NodeId;
            value.AttributeId = Attributes.Value;
            value.Value.Value = 10;

            WriteValueCollection valuesToWrite = new WriteValueCollection();
            valuesToWrite.Add(value);

            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            ResponseHeader responseHeader = session.Write(
                null,
                valuesToWrite,
                out results,
                out diagnosticInfos);

            ClientBase.ValidateResponse(results, valuesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);

            if (StatusCode.IsBad(results[0]))
            {
                throw ServiceResultException.Create(results[0], 0, diagnosticInfos, responseHeader.StringTable);
            }
        }
        private static void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {



            foreach (DataValue value in item.DequeueValues())
            {

                
                if (value.Value != null && Data.tags.ContainsKey(item.DisplayName))
                {
                    Data.tags[item.DisplayName].Value = value.Value.ToString();
                    value.GetValue(true);
                    value.Value = true;
                }
              
            }
                
       
            
            
        }
        private static void obhod(Session session, ReferenceDescription nextRd, string space)
        {
            ReferenceDescriptionCollection nextRefs2;
            byte[] nextCp2;
            space = space + " ";
            session.Browse(null, null, ExpandedNodeId.ToNodeId(nextRd.NodeId, session.NamespaceUris), 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out nextCp2, out nextRefs2);
            foreach (var nextRd2 in nextRefs2)
            {
                Console.WriteLine(space + "{0}: {1}, {2}, {3}", nextRd2.DisplayName, nextRd2.BrowseName, nextRd2.NodeClass, nextRd2.NodeId);
                
                if(!Data.tags.ContainsKey(nextRd2.DisplayName.ToString()))
                    Data.tags.Add(nextRd2.DisplayName.ToString(), new Tags(nextRd2.DisplayName.ToString(), nextRd2.BrowseName.ToString(), nextRd2.NodeClass.ToString(), nextRd2.NodeId.ToString()));

                obhod(session, nextRd2, space);

            }
        }

    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;

namespace OPC_UA.Model
{
    class Data
    {
        public static Dictionary<string,Tags> tags = new Dictionary<string, Tags>();
      
        public static List<MonitoredItem> list = new List<MonitoredItem>();
    }
    class Tags
    {
        public string DisplayName;
        public string BrowseName;
        public string NodeClass;
        public string NodeId;
        public string Value;
        public Tags(string DisplayName, string BrowseName, string NodeClass, string NodeId)
        {
            this.DisplayName = DisplayName;
            this.BrowseName = BrowseName;
            this.NodeClass = NodeClass;
            this.NodeId = NodeId;
        }
    }
  
}

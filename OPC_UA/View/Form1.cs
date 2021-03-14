using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using Opc.Ua.Client;
using Opc.Ua.Configuration;

using System.Threading;

namespace OPC_UA
{
    public partial class Form1 : Form
    {


        Present.Prersent prersent = new Present.Prersent(); 
        public Form1()
        {
            InitializeComponent();
            prersent.dataGridView = dataGridView1;




        }

        private void button1_Click(object sender, EventArgs e)
        {
            prersent.browse();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            prersent.connect();
        }
    }
}

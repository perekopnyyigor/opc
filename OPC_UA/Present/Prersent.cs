using OPC_UA.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace OPC_UA.Present
{
    class Prersent
    {
        public DataGridView dataGridView;
        private void fill()
        {
            dataGridView.Rows.Clear();
            int i = 0;
            foreach (string name in Data.tags.Keys)
            {
                if (Data.tags[name].Value != null)
                {
                    dataGridView.Rows.Add();
                    dataGridView.Rows[i].Cells["tag"].Value = Data.tags[name].DisplayName;
                    dataGridView.Rows[i].Cells["value"].Value = Data.tags[name].Value;
                    i++;
                }

            }
        }
        private void cycle()
        {

            while (true)
            {
                Action action = () =>
                {
                    //dataGridView.Rows.Clear();
                    for(int i=0; i<dataGridView.Rows.Count-1;i++)
                    {
                        string name = dataGridView.Rows[i].Cells["tag"].Value.ToString();
                        dataGridView.Rows[i].Cells["value"].Value = Data.tags[name].Value;
                    }
                    

                };
                dataGridView.Invoke(action);
                Thread.Sleep(100);
            }
        }
        public void browse()
        {
            fill();
            UA_client.start();
            
            Thread thread = new Thread(new ThreadStart(cycle));
            thread.Start();           

        }
        public void connect()
        {
            UA_client.configurate();
            UA_client.create_server();
            UA_client.browse();
            MessageBox.Show("Соединение установлено");
        }
    }
}

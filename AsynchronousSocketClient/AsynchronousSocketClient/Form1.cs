using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;

namespace AsynchronousSocketClient {
  public partial class Form1 : Form {

    AsynchronousSocketClient asyncClient = new AsynchronousSocketClient();

    public Form1() {
      InitializeComponent();
    }
     
    private void Form1_Load(object sender, EventArgs e) {
      //ClientThread = new Thread(new ThreadStart());
      //ClientThread.Start();
      asyncClient.StartClient();
    }

    private void button1_Click(object sender, EventArgs e) {
      //Char_Pos data = new Char_Pos();

      //data.x = Int32.Parse(textBox1.Text.ToString());
      //data.y = Int32.Parse(textBox2.Text.ToString());
      //data.z = Int32.Parse(textBox3.Text.ToString());

      //asyncClient.SendPacket(data, asyncClient.GetSocket);
    }


  }
}

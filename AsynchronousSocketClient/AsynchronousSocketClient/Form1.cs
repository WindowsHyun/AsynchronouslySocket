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
using static publicPacketProtocol.PacketProtocol;

namespace AsynchronousSocketClient {
  public partial class Form1 : Form {

    AsynchronousSocketClient asyncClient = new AsynchronousSocketClient();

    delegate void delegateProcessPacket(int packet_Type, byte[] buffer);
    // delegate 처리 함수
    public void DelegateProcessPacket(int packet_Type, byte[] buffer) {
      if (InvokeRequired) {
        delegateProcessPacket c = new delegateProcessPacket(DelegateProcessPacket);
        Invoke(c, new object[] { packet_Type, buffer });
      } else {
        ProcessPacket(packet_Type, buffer);
      }
    }

    public Form1() {
      InitializeComponent();
    }
     
    private void Form1_Load(object sender, EventArgs e) {
      asyncClient.StartClient();
      asyncClient.ClientGetPacket += DelegateProcessPacket;
    }

    private void ProcessPacket(int packet_Type, byte[] buffer) {

      switch (packet_Type) {
        case (int)packetType.char_pos:
          Char_Pos receiveclass = (Char_Pos)Packet.Deserialize(buffer);
          Console.WriteLine(receiveclass.x + ", " + receiveclass.y + ", " + receiveclass.z);
          listBox1.Items.Add("[Recv : packetType.char_pos ] " + receiveclass.x + ", " + receiveclass.y + ", " + receiveclass.z);
          break;

        case 2:
          break;

      }

    }

    private void button1_Click(object sender, EventArgs e) {
      Char_Pos char_Pos = new Char_Pos();
      char_Pos.x = Int32.Parse(textBox1.Text);
      char_Pos.y = Int32.Parse(textBox2.Text);
      char_Pos.z = Int32.Parse(textBox3.Text);
      char_Pos.packet_Type = (int)packetType.char_pos;

      asyncClient.Send(asyncClient.ClientSock, Packet.Serialize(char_Pos));
    }


  }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.IO;  // 파일 입출력 스트림
using System.Runtime.Serialization.Formatters.Binary;      // 바이너리 포맷
using static publicPacketProtocol.PacketProtocol;

namespace AsynchronousSocket {

  public partial class Form1 : Form {

    AsynchronousSocketServer asyncServer = new AsynchronousSocketServer();

    delegate void delegateProcessPacket(int packet_Type, byte[] buffer, int userNo);
    // delegate 처리 함수
    public void DelegateProcessPacket(int packet_Type, byte[] buffer, int userNo) {
      if (InvokeRequired) {
        delegateProcessPacket c = new delegateProcessPacket(DelegateProcessPacket);
        Invoke(c, new object[] { packet_Type, buffer, userNo });
      } else {
        ProcessPacket(packet_Type, buffer, userNo);
      }
    }

    public Form1() {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e) {
      asyncServer.StartServer();
      // 서버에서 받은 Packet을 Form에서 읽어서 처리 하기 위하여 Delegate를 설정한다.
      asyncServer.ServerGetPacket += DelegateProcessPacket;

    }

    private void ProcessPacket(int packet_Type, byte[] buffer, int userNO) {

      switch (packet_Type) {
        case (int)packetType.char_pos:

          // Char_Pos 를 직렬화 하여 가져온다.
          Char_Pos receiveclass = (Char_Pos)Packet.Deserialize(buffer);
          Console.WriteLine(receiveclass.x + ", " + receiveclass.y + ", " + receiveclass.z);

          // 리스트 박스에 해당 내용을 추가 한다.
          listBox1.Items.Add("(" + userNO + ")[Recv : packetType.char_pos ] " + receiveclass.x + ", " + receiveclass.y + ", " + receiveclass.z);

          // server_data에 추가를 하기 위하여 iter로 가져와서 set을 시켜준다.
          Game_ClientClass iter = asyncServer.server_data[userNO];
          iter.set_x(receiveclass.x);
          iter.set_y(receiveclass.y);
          iter.set_z(receiveclass.z);

          break;

        case 2:
          break;

      }

    }


    private void button1_Click_1(object sender, EventArgs e) {
      Char_Pos char_Pos = new Char_Pos();
      char_Pos.x = Int32.Parse(textBox1.Text);
      char_Pos.y = Int32.Parse(textBox2.Text);
      char_Pos.z = Int32.Parse(textBox3.Text);
      char_Pos.packet_Type = (int)packetType.char_pos;

      asyncServer.Send(Int32.Parse(UserNo.Text.ToString()), Packet.Serialize(char_Pos));
    }

  }
}

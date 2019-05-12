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
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.IO;  // 파일 입출력 스트림
using System.Runtime.Serialization.Formatters.Binary;      // 바이너리 포맷
using static publicPacketProtocol.PacketProtocol;

namespace AsynchronousSocket {

  public partial class Form1 : Form {

    public class StateObject {
      public Socket workSocket = null;
      public const int BufferSize = 1024;
      public byte[] buffer = new byte[BufferSize];
      public StringBuilder sb = new StringBuilder();
    }

    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    // Golobal Sockeet define for g_listener
    public static Socket g_listener = null;
    private static readonly int Port = 11000;
    private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");

    Thread ServerThread;

    public Form1() {
      InitializeComponent();
    }

    public void setSocket(Socket sock) {
      g_listener = sock;
    }

    private void Server_Start() {
      IPEndPoint localEndPoint = new IPEndPoint(iPAddress, Port);

      // Create a TCP/IP socket.  
      g_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

      // Bind the socket to the local endpoint and listen for incoming connections.  
      try {
        g_listener.Bind(localEndPoint);
        g_listener.Listen(100);

        while (true) {
          // Set the event to nonsignaled state.  
          allDone.Reset();

          // Start an asynchronous socket to listen for connections.  
          Console.WriteLine("Waiting for a connection...");
          g_listener.BeginAccept(
              new AsyncCallback(AcceptCallback),
              g_listener);

          // Wait until a connection is made before continuing.  
          allDone.WaitOne();
        }

      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }

      Console.WriteLine("\nPress ENTER to continue...");
      Console.Read();
    }

    public static void AcceptCallback(IAsyncResult ar) {
      // Signal the main thread to continue.  
      allDone.Set();

      // Get the socket that handles the client request.  
      g_listener = (Socket)ar.AsyncState;
      Socket handler = g_listener.EndAccept(ar);
      g_listener = handler;
      // Create the state object.  
      StateObject state = new StateObject();
      state.workSocket = handler;
      handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar) {
      String content = String.Empty;

      // Retrieve the state object and the handler socket  
      // from the asynchronous state object.  
      StateObject state = (StateObject)ar.AsyncState;
      Socket handler = state.workSocket;

      // Read data from the client socket.   
      int bytesRead = handler.EndReceive(ar);

      if (bytesRead > 0) {
        // There  might be more data, so store the data received so far.  
        state.sb.Append(Encoding.ASCII.GetString(
            state.buffer, 0, bytesRead));

        // Check for end-of-file tag. If it is not there, read   
        // more data.  
        content = state.sb.ToString();
        if (content.IndexOf("<EOF>") > -1) {
          // All the data has been read from the   
          // client. Display it on the console.  
          Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
          // Echo the data back to the client.  
          //Send(handler, content);

          handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);
          state.workSocket = handler;
        } else {
          // Not all data received. Get more.  
          handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);
        }
      }
    }

    private static void Send(Socket handler, String data) {
      // Convert the string data to byte data using ASCII encoding.  
      byte[] byteData = Encoding.ASCII.GetBytes(data);

      // Begin sending the data to the remote device.  
      handler.BeginSend(byteData, 0, byteData.Length, 0,
          new AsyncCallback(SendCallback), handler);
    }

    private static void Send(Socket handler, Byte[] data) {
      // Begin sending the data to the remote device.  
      handler.BeginSend(data, 0, data.Length, 0,
          new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar) {
      try {
        // Retrieve the socket from the state object.  
        Socket handler = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.  
        int bytesSent = handler.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to client.", bytesSent);


        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);

        //handler.Shutdown(SocketShutdown.Both);
        //handler.Close();

      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }

    private void Form1_Load(object sender, EventArgs e) {
      ServerThread = new Thread(new ThreadStart(Server_Start));
      ServerThread.Start();

    }

    private void button1_Click(object sender, EventArgs e) {

      Char_Pos char_Pos = new Char_Pos();
      char_Pos.x = 1;
      char_Pos.y = 1;
      char_Pos.z = 1;
      char_Pos.packet_Type = 1;

      Send(g_listener,  Packet.Serialize(char_Pos));

    }


  }
}

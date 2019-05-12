using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;  // 파일 입출력 스트림
using System.Runtime.Serialization.Formatters.Binary;      // 바이너리 포맷
using static publicPacketProtocol.PacketProtocol;


namespace AsynchronousSocket {
  public class StateObject {
    public Socket workSocket = null;
    public const int BufferSize = 1024;
    public byte[] buffer = new byte[BufferSize];
    public StringBuilder sb = new StringBuilder();
  }

  public class AsynchronousSocketServer {
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    // Golobal Sockeet define for g_listener
    public static Socket g_listener = null;
    public Socket ServerSock;
    private static readonly int Port = 11000;
    private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");

    Thread ServerThread;

    public delegate void delegateProcessPacket(int packet_Type, byte[] buffer);
    public event delegateProcessPacket ServerGetPacket;

    public void StartServer() {
      ServerThread = new Thread(new ThreadStart(Server_Start));
      ServerThread.Start();
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
          g_listener.BeginAccept( new AsyncCallback(AcceptCallback), g_listener);

          // Wait until a connection is made before continuing.  
          allDone.WaitOne();
        }

      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }

      Console.WriteLine("\nPress ENTER to continue...");
      Console.Read();
    }

    public void AcceptCallback(IAsyncResult ar) {
      // Signal the main thread to continue.  
      allDone.Set();

      // Get the socket that handles the client request.  
      g_listener = (Socket)ar.AsyncState;
      Socket handler = g_listener.EndAccept(ar);
      g_listener = handler;
      ServerSock = handler;
      // Create the state object.  
      StateObject state = new StateObject();
      state.workSocket = handler;
      handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);
    }

    public void ReadCallback(IAsyncResult ar) {
      String content = String.Empty;

      // Retrieve the state object and the handler socket  
      // from the asynchronous state object.  
      StateObject state = (StateObject)ar.AsyncState;
      Socket handler = state.workSocket;

      // Read data from the client socket.   
      int bytesRead = handler.EndReceive(ar);

      if (bytesRead > 0) {
        // 패킷 타입을 읽어온다.
        Packet receiveclass = (Packet)Packet.Deserialize(state.buffer);

        // Delegate Event에 패킷 타입, 버퍼를 전달 한다.
        ServerGetPacket?.Invoke(receiveclass.packet_Type, state.buffer);

        // Get the rest of the data.  
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
        state.workSocket = handler;
      } else {
        // Not all data received. Get more.  
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        new AsyncCallback(ReadCallback), state);
      }
    }

    public void Send(Socket handler, Byte[] data) {
      // Begin sending the data to the remote device.  
      handler.BeginSend(data, 0, data.Length, 0,
          new AsyncCallback(SendCallback), handler);
    }

    private void SendCallback(IAsyncResult ar) {
      try {
        // Retrieve the socket from the state object.  
        Socket handler = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.  
        int bytesSent = handler.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to client.", bytesSent);


        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }


  }
}

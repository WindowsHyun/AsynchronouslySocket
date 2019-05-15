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
    public int userNo = 0;
  }

  public class AsynchronousSocketServer {
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    // 소켓 및 아이피 포트 번호 선언
    public static Socket g_listener = null;
    //public Socket ServerSock;
    private static readonly int Port = 11000;
    private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");

    // 서버를 시작할 Thread 선언
    Thread ServerThread;

    // 유저를 관리할 Dictionary 선언
    public int userNo = 0;
    public Dictionary<int, Game_ClientClass> server_data = new Dictionary<int, Game_ClientClass>();

    public delegate void delegateProcessPacket(int packet_Type, byte[] buffer, int userNo);
    public event delegateProcessPacket ServerGetPacket;

    public void StartServer() {
      // 서버 유저 정보 초기화
      userNo = 0;
      server_data.Clear();

      // 서버 Thread 시작
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

      // 유저 정보를 Dictionary에 저장 시켜 준다.
      ++userNo;
      server_data.Add(userNo, new Game_ClientClass(userNo, handler, 0, 0, 0));

      // StateObject를 만들어서 ReadCallback에 넘겨준다.
      StateObject state = new StateObject();
      state.workSocket = handler;
      state.userNo = userNo;
      handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
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
        ServerGetPacket?.Invoke(receiveclass.packet_Type, state.buffer, state.userNo);

        // Get the rest of the data.  
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        state.workSocket = handler;
      } else {
        // Not all data received. Get more.  
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
      }
    }

    public void Send(int userNo, Byte[] data) {
      // userNo가 있는지 먼저 확인을 한다.
      if (server_data.ContainsKey(userNo) == false) {
        // userNO가 없을 경우
        Console.WriteLine("No User {0}", userNo);
      } else {

        // 전송할 Byte[], userNO를 받아서 userNo로 Socket을 가져와 해당 유저에게 패킷을 전달 한다.
        Socket handler = server_data[userNo].get_sock();

        StateObject state = new StateObject();
        state.workSocket = handler;
        state.userNo = userNo;

        handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), state);
      }
    }

    private void SendCallback(IAsyncResult ar) {
      try {

        // stateObject 값을 읽어 온다.
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Complete sending the data to the remote device.  
        int bytesSent = handler.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to client.", bytesSent);

        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }


  }
}

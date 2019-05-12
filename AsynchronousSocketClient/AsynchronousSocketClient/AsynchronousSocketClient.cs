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

namespace AsynchronousSocketClient {
  public class StateObject {
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024 * 4;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
  }
  public class AsynchronousSocketClient {
    // The port number for the remote device.  
    private const int port = 11000;
    private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
    public static Socket client;
    public Socket ClientSock;

    public delegate void SocketConnect(Socket sock);

    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    public delegate void delegateProcessPacket(int packet_Type, byte[] buffer);

    public event delegateProcessPacket ClientGetPacket;

    // The response from the remote device.  
    private static String response = String.Empty;

    public void StartClient() {
      // Connect to a remote device.  
      try {
        // Establish the remote endpoint for the socket.  
        // The name of the   
        // remote device is "host.contoso.com".  
        IPEndPoint remoteEP = new IPEndPoint(iPAddress, port);

        //// Create a TCP/IP socket.  
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ClientSock = client;

        // Connect to the remote endpoint.  
        client.BeginConnect(remoteEP,
            new AsyncCallback(ConnectCallback), client);
        connectDone.WaitOne();

        // Receive the response from the remote device.  
        Receive(client);

        // Write the response to the console.  
        Console.WriteLine("Response received : {0}", response);

      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }

    private void ConnectCallback(IAsyncResult ar) {
      try {
        // Retrieve the socket from the state object.  
        client = (Socket)ar.AsyncState;

        // Complete the connection.  
        client.EndConnect(ar);

        Console.WriteLine("Socket connected to {0}",
            client.RemoteEndPoint.ToString());

        // Signal that the connection has been made.  
        connectDone.Set();
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }

    private void Receive(Socket client) {
      try {
        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = client;

        // Begin receiving the data from the remote device.  
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReceiveCallback), state);
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }

    private void ReceiveCallback(IAsyncResult ar) {
      try {
        // Retrieve the state object and the client socket   
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        client = state.workSocket;

        // Read data from the remote device.  
        int bytesRead = client.EndReceive(ar);

        if (bytesRead > 0) {
          // There might be more data, so store the data received so far.  

          Packet receiveclass = (Packet)Packet.Deserialize(state.buffer);

          ClientGetPacket?.Invoke(receiveclass.packet_Type, state.buffer);

          // Get the rest of the data.  
          client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
              new AsyncCallback(ReceiveCallback), state);
        } else {
          // All the data has arrived; put it in response.  
          if (state.sb.Length > 1) {
            response = state.sb.ToString();
          }
          // Signal that all bytes have been received.  
          receiveDone.Set();
        }
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
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
        client = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.  
        int bytesSent = client.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to server.", bytesSent);

        // Signal that all bytes have been sent.  
        sendDone.Set();
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }


  }
}

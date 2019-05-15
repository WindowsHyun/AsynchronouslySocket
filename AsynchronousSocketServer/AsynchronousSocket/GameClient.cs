using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace AsynchronousSocket {
  public class Game_ClientClass {
    private Socket sock;  // 클라이언트 Socket
    private int id;             // 클라이언트 고유번호
    private int x;              // 클라이언트 x
    private int y;              // 클라이언트 y
    private int z;              // 클라이언트 z

    // Get 선언
    public Socket get_sock() { return this.sock; }
    public int get_id() { return this.id; }
    public int get_x() { return this.x; }
    public int get_y() { return this.y; }
    public int get_z() { return this.z; }

    // Set 선언
    public void set_sock(Socket value) { this.sock = value; }
    public void set_id(int value) { this.id = value; }
    public void set_x(int value) { this.x = value; }
    public void set_y(int value) { this.y = value; }
    public void set_z(int value) { this.z = value; }


    public Game_ClientClass(int id, Socket sock, int x, int y, int z) {
      this.id = id;
      this.sock = sock;
      this.x = x;
      this.y = y;
      this.z = z;
    }

  }
}

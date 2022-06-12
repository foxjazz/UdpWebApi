using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpWebApi;

public sealed class UdpListener
{
    private UdpClient client;
    private IPEndPoint sender;
    public static UdpController.UpdateInfoTxtDelegate UpdateInfoTxt;
    // public int port; // TODO add more dynamic capability for changing listener port. 
    bool stop;

    private int port;
    private static UdpListener instance = null;
    public static UdpListener GetInstance
    {
        get
        {
            if (instance == null)
                instance = new UdpListener();
            return instance;
        }
    }
    public void initUdp(int port)
    {
        try
        {
            this.port = port;

                client = new UdpClient(port); // listen on PORT 514 (syslog of Perle)
                sender = new IPEndPoint(IPAddress.Any, port);
                client.Client.Bind(sender);
                


        }
        catch (Exception ex)
        {
            // System.Windows.MessageBox.Show("Internal Exception occured while attempting to init UDP client and sender. :::: " + ex.ToString());
        }
        stop = false;
    }

    public void close()
    {
        this.stop = true;
        this.client.Close();
    }
    public void Listen()
    {
        Task t = new Task(ListenThread);
        t.Start(); 
    }
    private void ListenThread()
    {
        byte[] data = new byte[1024];
        string stringData;
        stop = false;
        while (!stop)
        {
            data = client.Receive(ref sender);
            stringData = Encoding.ASCII.GetString(data, 0, data.Length);
            /*client.Close();
            stop = true;*/
            UpdateInfoTxt(stringData);
        }
            
    }
}
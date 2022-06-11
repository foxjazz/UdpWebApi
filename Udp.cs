using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using UdpWebApi.Models;
using UdpWebApi.Registrations;


namespace UdpWebApi;
[ApiController]
[Route("[controller]")]
public class UdpController : ControllerBase
{

   public class msg
    {
        public string message { get; set; }
    }
    private IManager manager;
    public UdpController(IManager __manager)
    {
        manager = __manager;
    }
    // GET
    private IPEndPoint _endPoint;
    [HttpGet("Test")]
    public msg Function2()
    {
        var m = new msg();
        string date = DateTime.Now.ToString("mm:ss");
        m.message = "from server" + date;
        return m;
    }
    [HttpGet("IsAlive")]
    public string IsAlive()
    {
        return "alive";
    }

    
    [HttpGet("IP")]
    public string IP()
    {
        //var ip = request.HttpContext.Connection.RemoteIpAddress;
        
        // var data = Dns.GetHostEntry(Dns.GetHostName());
        var ip = HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        // string clientAddress = HttpContext.Current.Request.UserHostAddress;
        var ip2 = HttpContext.Request.Host.Value;
        return ip;
    }

    [HttpGet("Listen")]
    public string listen()
    {
        /*var c = new UdpClient();
        c.Client.Listen(52626);*/
        using (UdpClient socket = new UdpClient())
        {
            _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5005);
            socket.Connect(_endPoint);
            byte[] dataServer = socket.Receive(ref _endPoint);
            string recivedMessage = Encoding.UTF8.GetString(dataServer);
            return recivedMessage;
        }

        //konverter det til en string
        
    }

    [HttpGet("Send")]
    public string send(string message)
    {
        using (UdpClient socket = new UdpClient())
        {
            IPEndPoint EndPoint = null;
            byte[] data = Encoding.UTF8.GetBytes(message);

            //kalder send metoden der har de 4 parametre i sig
            //- Clienten vil gerne sende en besked til serveren
            int len = socket.Send(data, data.Length, "127.0.0.1", 5005);
            return len.ToString();
        }
    }

    [HttpGet("Search")]
    public Register search(string email)
    {
        return manager.search(email);
    }

    [HttpPost("register")]
    public msg register([FromBody] Register reg)
    { var m = new msg();
        try
        {
            manager.add(reg);
           
            m.message = "success";
            return m;
        }
        catch (Exception ex)
        {
            m.message = ex.Message;
            return m;
        }
    }
    
}
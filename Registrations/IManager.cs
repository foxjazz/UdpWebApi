using UdpWebApi.Models;

namespace UdpWebApi.Registrations;

public interface IManager
{
    bool add(Register reg);
    void save();
    Register search(string email);
    Register getLocal();

}
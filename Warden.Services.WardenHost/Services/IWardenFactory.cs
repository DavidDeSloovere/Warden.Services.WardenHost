namespace Warden.Services.WardenHost.Services
{
    public interface IWardenFactory
    {
         IWarden Create(string name);
    }
}
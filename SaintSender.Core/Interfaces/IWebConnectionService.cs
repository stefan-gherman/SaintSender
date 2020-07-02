namespace SaintSender.Core.Interfaces
{
    public interface IWebConnectionService
    {
        bool ConnectionCheck(string url);
        bool PingChek(string url);
        bool NLMAPICheck();
    }
}

namespace Fanex.ActivityTracking.Models
{
    public interface IUserSession
    {
        int AdminId { get; }

        string AdminName { get; }

        string SessionID { get; }

        int CustId { get; }

        string CustName { get; }
    }
}
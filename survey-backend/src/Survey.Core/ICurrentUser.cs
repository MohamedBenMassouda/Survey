namespace Survey.Core;

public interface ICurrentUser
{
    public Guid GetCurrentUserId();
    bool IsLoggedIn();
}
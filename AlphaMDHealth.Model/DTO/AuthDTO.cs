namespace AlphaMDHealth.Model;

public class AuthDTO : BaseDTO
{
    public AuthModel AuthenticationData { get; set; }
    public SessionModel Session { get; set; }
    public TempSessionModel TempSession { get; set; }
}
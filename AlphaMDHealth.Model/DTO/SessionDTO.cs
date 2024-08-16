namespace AlphaMDHealth.Model
{
    public class SessionDTO : BaseDTO
    {
        public SessionModel Session { get; set; }
        public object Request { get; set; }
        public string RequestSignature { get; set; }
    }
}
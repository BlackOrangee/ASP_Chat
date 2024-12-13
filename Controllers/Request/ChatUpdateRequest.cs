namespace ASP_Chat.Controllers.Request
{
    public class ChatUpdateRequest
    {
        public string? Tag { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}

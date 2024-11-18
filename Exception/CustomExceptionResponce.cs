using Newtonsoft.Json;

namespace ASP_Chat.Exception
{
    public class CustomExceptionResponce
    {
        public int Code { get; set; }
        public string Error { get; set; }

        public CustomExceptionResponce(int code, string error) => (Code, Error) = (code, error);

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

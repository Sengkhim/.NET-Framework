using ServerTest.Service;

namespace ServerTest.Implement
{
    public class ContactService : IContactService
    {
        public string GetEmail() => "akmsupport@gmail.com";
    }
}
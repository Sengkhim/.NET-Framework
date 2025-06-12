using System;
using System.Web;
using System.Web.UI;
using ServerTest.Attribute;
using ServerTest.Service;
using ServerTest.Utility;
using IServiceProvider = ServerTest.Extension.DIContainer.IServiceProvider;

namespace ServerTest.Pages
{
    public partial class Contact : Page
    {
        [TryInject] protected IContactService ContactService { get; set; }
        protected string DisplayEmail { get; private set; } = string.Empty;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            if (!(HttpContext.Current.Items["ServiceProvider"] is IServiceProvider serviceProvider)) return;
            
            PropertyInjector.PerformInjection(this, serviceProvider);
                
            DisplayEmail = ContactService.GetEmail();
        }
    }
}
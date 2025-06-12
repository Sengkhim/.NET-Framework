using System;
using System.Web;
using System.Web.UI;
using Shaper.Core.Attribute;
using Shaper.Core.Connection.Service;
using Shaper.Core.DependencyInjection.Service;
using Shaper.Utility;
using Web3.Service;
using IServiceProvider = Shaper.Core.DependencyInjection.Service.IServiceProvider;

namespace Web3.Pages
{
    public abstract class BasePage : Page
    {
        protected virtual void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            if (HttpContext.Current.Items["ServiceProvider"] is not IServiceProvider serviceProvider)
                return;
            
            PropertyInjector.PerformInjection(this, serviceProvider);
        }
    }
    
    public partial class Contact : BasePage
    {
        #region Service Dependency
        [TryInject] protected IConfiguration Configuration { get;  set; }
        [TryInject] protected IContactService ContactService { get; set; }
        [TryInject] protected IDbConnectionProvider ConnectionProvider { get;  set; }
        
        #endregion
        
        #region Properties
        protected string DisplayEmail { get; private set; } = string.Empty;
        protected string ConnectionString { get; private set; } = string.Empty;
        protected string LogLevel { get; private set; } = string.Empty;
        protected string SiteName { get; private set; } = string.Empty;
        
        #endregion
        
        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
            
            DisplayEmail = ContactService.GetEmail();
            ConnectionString = ConnectionProvider.GetConnection("Connection").ConnectionString;
            LogLevel = Configuration.GetValue("AppSettings:LogLevel");
            SiteName = Configuration.GetValue("AppSettings:SiteName");
        }
    }
}
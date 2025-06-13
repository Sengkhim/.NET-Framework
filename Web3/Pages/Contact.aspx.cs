using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Shaper.Core.Attribute;
using Shaper.Core.Connection.Service;
using Shaper.Extension;
using Shaper.Utility;
using Web3.Infrastructure.Repository;
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

        [TryInject] protected IDbConnectionProvider Provider { get; set; }

        [TryInject] protected IProductionRepository Repository { get; set; }
        
        #endregion
        
        #region Properties
        protected string ConnectionString { get; private set; } = string.Empty;

        #endregion

        #region Method

        private async Task TryLoadAsync()
        {
            var timeoutPolicy = ResiliencyPolicy.Timeout(TimeSpan.FromSeconds(2));

            await timeoutPolicy.ExecuteAsync(async () =>
            {
                var products = await Repository.GetAllAsync();
            
                ProductRepeater.DataSource = products.ToList();;
                ProductRepeater.DataBind();
                
            });
        }

        private async Task GetConnectionAsync()
        {
            var dapper = await Provider.GetConnection("Connection");
            ConnectionString = dapper.ConnectionString;
        }
        
        private async Task LoadAsync() => 
            await Task.WhenAll(GetConnectionAsync(), TryLoadAsync());

        #endregion
        
        protected override void Page_Load(object sender, EventArgs e)
        {
            try
            {
                base.Page_Load(sender, e);
                RegisterAsyncTask(new PageAsyncTask(LoadAsync));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
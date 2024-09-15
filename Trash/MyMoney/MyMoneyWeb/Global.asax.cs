using DataWorker;
using ServiceRequest.Money;
using ServiceResponse;
using ServiceWorker;
using System;
using System.Configuration;
using System.IO;
using System.Timers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MyMoneyWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private Timer _regularTaskRunTimer;
        private DateTime _regularTaskRunLastExecuteDate;
        private bool _regularTaskRunInProcess;

        protected void Application_Start()
        {
            Data.Migrations.Runner.Initialize(DataWorker.DbAccountWorker.GetConnectionString());
            Data.Migrations.Runner.MigrateToLatest();

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());

            _regularTaskRunLastExecuteDate = DateTime.Now.Date;
            MoneyWorker.RunRegularTask(new RunRegularTaskRequest(), null);
            _regularTaskRunInProcess = true;

            _regularTaskRunTimer = new Timer();
            _regularTaskRunTimer.Interval = 10000;
            _regularTaskRunTimer.Elapsed += TimerTick;
            _regularTaskRunTimer.Start();

            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["FilesStoragePath"]))
                {
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["FilesStoragePath"]);
                }
            }
            catch
            {
            }
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            if (_regularTaskRunInProcess)
            {
                return;
            }

            if (_regularTaskRunLastExecuteDate < DateTime.Now.Date)
            {
                _regularTaskRunInProcess = true;
                var result = MoneyWorker.RunRegularTask(new RunRegularTaskRequest(), null);
                if (result.Type == ResponseType.Success)
                {
                    _regularTaskRunLastExecuteDate = DateTime.Now.Date;
                }

                _regularTaskRunInProcess = false;
            }
        }
    }


    //https://stackoverflow.com/questions/14400643/accept-comma-and-dot-as-decimal-separator
    public class DecimalModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == null)
            {
                return base.BindModel(controllerContext, bindingContext);
            }
            var v = valueProviderResult.AttemptedValue.Replace('.', ',');
            if (v == "NaN" || v == "")
            {
                return null;
            }
            return Convert.ToDecimal(v);
        }
    }
}
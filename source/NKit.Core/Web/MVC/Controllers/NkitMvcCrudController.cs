//namespace NKit.Web.MVC.Controllers
//{
//    #region Using Directives

//    using System;
//    using System.Collections.Generic;
//    using System.Text;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.AspNetCore.Http;
//    using Microsoft.Extensions.Logging;
//    using Microsoft.Extensions.Options;
//    using NKit.Core.Data.DB.LINQ;
//    using NKit.Settings.Default;
//    using NKit.Utilities.Email;
//    using NKit.Web.MVC.Models;

//    #endregion //Using Directives

//    public abstract class NkitMvcCrudController<D, E> : NKitMvcController<D, E> where D : NKitDbContext where E : NKitEmailClientService
//    {
//        #region Constructors

//        public NkitMvcCrudController(
//            D dbContext, 
//            IHttpContextAccessor httpContextAccessor, 
//            IOptions<NKitGeneralSettings> generalOptions, 
//            IOptions<NKitWebApiControllerSettings> webApiControllerOptions, 
//            IOptions<NKitDbContextSettings> databaseOptions, 
//            IOptions<NKitEmailClientServiceSettings> emailOptions, 
//            IOptions<NKitLoggingSettings> loggingOptions, 
//            IOptions<NKitWebApiClientSettings> webApiClientOptions, 
//            E emailClientService, 
//            ILogger logger, 
//            IWebHostEnvironment environment) : 
//            base(dbContext, httpContextAccessor, generalOptions, webApiControllerOptions, databaseOptions, emailOptions, loggingOptions, webApiClientOptions, emailClientService, logger, environment)
//        {
//        }

//        #endregion //Constructors

//        #region Methods

//        public abstract FilterModel GetFilterViewModel<FilterModel, Model, Entity>(FilterModel model)
//            where FilterModel : FilterModelCore<Model>
//            where Model : NKitEntityModel<Model, Entity>
//            where Entity : class;

//        #endregion //Methods
//    }
//}

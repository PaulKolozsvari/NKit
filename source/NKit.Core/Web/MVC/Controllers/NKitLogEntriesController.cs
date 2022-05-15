namespace NKit.Web.MVC.Controllers
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Data;
    using NKit.Data.DB.LINQ;
    using NKit.Data.DB.LINQ.Models;
    using NKit.Settings.Default;
    using NKit.Utilities.Email;
    using NKit.Web.MVC.CsvModels;
    using NKit.Web.MVC.Models;
    using NKit.Web.MVC.ViewModels;

    #endregion //Using Directives

    public class NKitLogEntriesController<D, E> : NKitMvcController<D, E> where D : NKitDbContext where E : NKitEmailClientService
    {
        #region Constructors

        public NKitLogEntriesController(
            D dbContext, 
            IHttpContextAccessor httpContextAccessor, 
            IOptions<NKitGeneralSettings> generalOptions, 
            IOptions<NKitWebApiControllerSettings> webApiControllerOptions, 
            IOptions<NKitDbContextSettings> databaseOptions, 
            IOptions<NKitEmailClientServiceSettings> emailOptions, 
            IOptions<NKitLoggingSettings> loggingOptions, 
            IOptions<NKitWebApiClientSettings> webApiClientOptions, 
            E emailClientService, 
            ILogger logger, 
            IWebHostEnvironment environment) : base(dbContext, httpContextAccessor, generalOptions, webApiControllerOptions, databaseOptions, emailOptions, loggingOptions, webApiClientOptions, emailClientService, logger, environment)
        {
        }

        #endregion //Constructors

        #region Constants

        public const string FORM_RECORD_GRID_PARTIAL_VIEW_NAME = "_LogEntryGrid";
        public const string EDIT_FORM_RECORD_DIALOG_PARTIAL_VIEW_NAME = "_EditLogEntryDialog";

        #endregion //Constants

        #region Methods

        public virtual NKitLogEntryFilterViewModel GetNKitLogEntryFilterViewModel(NKitLogEntryFilterViewModel model)
        {
            Nullable<DateTime> startDate = null;
            Nullable<DateTime> endDate = null;
            if (!model.StartDate.HasValue || !model.EndDate.HasValue) //Has not been specified by the user on the page yet i.e. this is the first time the page is loading.
            {
                model.StartDate = startDate = DateTime.Today;
                model.EndDate = endDate = DateTime.Now;
            }
            else
            {
                if (model.StartDate > model.EndDate)
                {
                    ViewBag.ErrorMessage = $"{EntityReaderGeneric<FilterModelCore<NKitLogEntryViewModel>>.GetPropertyName(p => p.StartDate, true)} may not be later than {EntityReaderGeneric<FilterModelCore<NKitLogEntryViewModel>>.GetPropertyName(p => p.EndDate, true)}.";
                    return null;
                }
                startDate = model.StartDate.Value;
                endDate = model.EndDate.Value;
            }
            model.IsAdministrator = false;
            model.IsAdministrator = true;
            model.IsViewingAllowed = model.IsAdministrator;
            model.IsCreateAllowed = model.IsAdministrator;
            model.IsDeleteAllowed = model.IsAdministrator;
            model.IsEditAllowed = model.IsAdministrator;

            model.DataModel.Clear();
            model.DataModel = DbContext.GetNKitLogEntryViewModelsByPage(
                model.SearchText,
                model.PageSize,
                model.Page,
                model.NumberOfRecordsToSkipForCurrentPage,
                model.Sort,
                model.SortDirectionType,
                startDate,
                endDate,
                model.FilterByDateRange,
                out int totalFullDatasetRecordCount);
            model.TotalFullDatasetRecordCount = totalFullDatasetRecordCount;
            model.TotalTableCount = DbContext.GetTotalCountLong<NKitLogEntry>();
            return model;
        }

        #endregion //Methods

        #region Actions

        public ActionResult Index()
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                NKitLogEntryFilterViewModel model = GetNKitLogEntryFilterViewModel(new NKitLogEntryFilterViewModel());
                if (model == null) //There was an error and ViewBag.ErrorMessage has been set. So just return an empty model.
                {
                    return View(new NKitLogEntryFilterViewModel());
                }
                SetViewBagSearchFieldIdentifier<NKitLogEntryViewModel>(model);
                return View(model);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public ActionResult Index(NKitLogEntryFilterViewModel model)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                SetViewBagSearchFieldIdentifier<NKitLogEntryViewModel>(model);
                NKitLogEntryFilterViewModel resultModel = GetNKitLogEntryFilterViewModel(model);
                if (resultModel == null) //There was an error and ViewBag.ErrorMessage has been set. So just return an empty model.
                {
                    return PartialView(FORM_RECORD_GRID_PARTIAL_VIEW_NAME, new NKitLogEntryFilterViewModel());
                }
                return PartialView(FORM_RECORD_GRID_PARTIAL_VIEW_NAME, resultModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public ActionResult EditDialog(Nullable<Guid> logEntryId)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                if (!logEntryId.HasValue)
                {
                    return PartialView(EDIT_FORM_RECORD_DIALOG_PARTIAL_VIEW_NAME, new NKitLogEntryViewModel());
                }
                NKitLogEntryViewModel model = DbContext.GetNKitLogEntryViewModel(logEntryId.Value, false);
                PartialViewResult result = PartialView(EDIT_FORM_RECORD_DIALOG_PARTIAL_VIEW_NAME, model);
                return result;
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public ActionResult EditDialog(NKitLogEntryViewModel model)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                if (!model.IsValid(out string errorMessage))
                {
                    return GetJsonResult(false, errorMessage);
                }
                NKitLogEntry e = new NKitLogEntry();
                model.CopyPropertiesTo(e);
                DbContext.Save<NKitLogEntry>(e, e.DateCreated);
                return GetJsonResult(true);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return GetJsonResult(false, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Delete(Guid logEntryId)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                NKitLogEntry e = DbContext.GetEntityBySurrogateKey<NKitLogEntry>(logEntryId, false);
                DbContext.Delete<NKitLogEntry>(e, e.DateCreated);
                return GetJsonResult(true);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return GetJsonResult(false, ex.Message);
            }
        }

        public ActionResult ConfirmDeleteDialog(Guid identifier)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                NKitLogEntry e = DbContext.GetEntityBySurrogateKey<NKitLogEntry>(identifier, false);
                ConfirmationModel model = new ConfirmationModel();
                model.PostBackControllerAction = GetCurrentActionName();
                model.PostBackControllerName = GetCurrentControllerName();
                model.DialogDivId = CONFIRMATION_DIALOG_DIV_ID;
                if (e != null)
                {
                    model.Identifier = identifier;
                    model.ConfirmationMessage = $"Delete NKitLogEntry '{e.DateCreated}'?";
                }
                PartialViewResult result = PartialView(CONFIRMATION_DIALOG_PARTIAL_VIEW_NAME, model);
                return result;
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        public ActionResult ConfirmDeleteDialog(ConfirmationModel model)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                NKitLogEntry e = DbContext.GetEntityBySurrogateKey<NKitLogEntry>(model.Identifier, false);
                DbContext.Delete<NKitLogEntry>(e, e.DateCreated);
                return GetJsonResult(true);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return GetJsonResult(false, ex.Message);
            }
        }

        public ActionResult ConfirmDeleteAllDialog(string searchParametersString)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                ConfirmationModel model = new ConfirmationModel();
                model.PostBackControllerAction = GetCurrentActionName();
                model.PostBackControllerName = GetCurrentControllerName();
                model.DialogDivId = CONFIRMATION_DIALOG_DIV_ID;

                string[] searchParameters;
                string searchText;
                GetConfirmationModelFromSearchParametersString(
                    searchParametersString,
                    out searchParameters,
                    out searchText,
                    out Nullable<DateTime> startDate,
                    out Nullable<DateTime> endDate,
                    out bool filterByDateRange,
                    out Nullable<Guid> noticeFileId);
                model.SearchText = searchText;
                model.ConfirmationMessage = $"Delete all Log Entries currently loaded?";
                model.StartDate = startDate;
                model.EndDate = endDate;
                model.FilterByDateRange = filterByDateRange;
                model.ParentId = noticeFileId;
                PartialViewResult result = PartialView(CONFIRMATION_DIALOG_PARTIAL_VIEW_NAME, model);
                return result;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return GetJsonResult(false, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult ConfirmDeleteAllDialog(ConfirmationModel model)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                DbContext.DeleteNKitLogEntriesByFilter(model.SearchText, model.StartDate, model.EndDate, model.FilterByDateRange);
                return GetJsonResult(true);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public ActionResult DownloadCsvFile(string searchParametersString)
        {
            try
            {
                if (!IsRequestAuthenticated())
                {
                    return RedirectToHome();
                }
                string[] searchParameters;
                string searchText;
                GetConfirmationModelFromSearchParametersString(
                    searchParametersString,
                    out searchParameters,
                    out searchText,
                    out Nullable<DateTime> startDate,
                    out Nullable<DateTime> endDate,
                    out bool filterByDateRange);
                List<NKitLogEntryCsvModel> entities = DbContext.GetNKitLogEntryCsvModelsByFilter(searchText, startDate, endDate, filterByDateRange);
                EntityCacheGeneric<Guid, NKitLogEntryCsvModel> cache = new EntityCacheGeneric<Guid, NKitLogEntryCsvModel>();
                entities.ForEach(p => cache.Add(p.NKitLogEntryId, p));
                return GetCsvFileResult<NKitLogEntryCsvModel>(cache);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        #endregion //Actions
    }
}
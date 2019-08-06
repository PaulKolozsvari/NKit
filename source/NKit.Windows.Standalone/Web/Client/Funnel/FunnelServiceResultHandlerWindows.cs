namespace NKit.Web.Client.Funnel
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Utilities;

    #endregion //Using Directives

    public class FunnelServiceResultHandlerWindows
    {
        #region Methods

        public static bool HandleServiceResult(FunnelServiceResult result)
        {
            bool abort = false;
            switch (result.Code)
            {
                case FunnelServiceResultCode.Success:
                    break;
                case FunnelServiceResultCode.Information:
                    UIHelperWindows.DisplayInformation(result.Message);
                    break;
                case FunnelServiceResultCode.SpecialInstructions:
                    //using (SpecialInstructionsForm f = new SpecialInstructionsForm(result.Message))
                    //{
                    //    f.ShowDialog();
                    //}
                    throw new NotImplementedException("SpecialInstructions");
                case FunnelServiceResultCode.Warning:
                    UIHelperWindows.DisplayWarning(result.Message);
                    break;
                case FunnelServiceResultCode.OperationError:
                    UIHelperWindows.DisplayError(result.Message);
                    abort = true;
                    break;
                case FunnelServiceResultCode.FatalError:
                    throw new Exception(result.Message);
                default:
                    throw new Exception("Invalid service result.");
            }
            return abort;
        }

        #endregion //Methods
    }
}

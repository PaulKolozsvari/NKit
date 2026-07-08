using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NKit.Utilities.Enums
{
    public enum SignedAtOptions
    {
        [Description("Store")]
        Store,

        [Description("Site")]
        Site,

        [Description("Outside GeoFence")]
        OutsideGeoFence
    }
}

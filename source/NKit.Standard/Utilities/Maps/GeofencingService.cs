namespace NKit.Utilities.Maps
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class GeofencingService
    {
        #region Constants

        private const double EARTH_RADIUS_METERS = 6371000; // 6,371 km

        #endregion //Constants

        #region Methods

        /// <summary>
        /// Calculates whether a location is inside a target geolocation's radius.
        /// </summary>
        public static bool IsGeoLocationInTargetGeoFence(double? locationLatitude, double? locationLongitude, double? targetLatitude, double? targetLongitude, double? radiusInMeters)
        {
            double distance = CalculateDistanceBetweenGeolocations(locationLatitude, locationLongitude, targetLatitude, targetLongitude);
            return distance <= radiusInMeters;
        }

        /// <summary>
        /// Calculates the distance between two points on Earth using the Haversine formula: https://en.wikipedia.org/wiki/Haversine_formula
        /// </summary>
        public static double CalculateDistanceBetweenGeolocations(double? latitude1, double? longitude1, double? latitude2, double? longitude2)
        {
            // Convert degrees to radians
            double latitudeDelta = ToRadians(latitude2 - latitude1);
            double longitudeDelta = ToRadians(longitude2 - longitude1);

            double a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                       Math.Cos(ToRadians(latitude1)) * Math.Cos(ToRadians(latitude2)) *
                       Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);

            double centralAngle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EARTH_RADIUS_METERS * centralAngle; //Distance in meters
        }

        private static double ToRadians(double? val)
        {
            if (!val.HasValue)
            {
                return 0;
            }
            return (Math.PI / 180) * (double)val;
        }

        #endregion //Methods
    }
}

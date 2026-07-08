using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKit.Utilities.Maps
{
    public class GeofencingService
    {
        private const double EarthRadiusMeters = 6371000; // 6,371 km

        /// <summary>
        /// Checks if the driver is within the specified geofence radius of a store.
        /// </summary>
        public static bool IsDriverInFence(double? driverLat, double? driverLng, double? targetLat, double? targetLng, double? radiusInMeters)
        {
            double distance = CalculateDistance(driverLat, driverLng, targetLat, targetLng);
            return distance <= radiusInMeters;
        }

        /// <summary>
        /// Calculates the distance between two points on Earth using the Haversine formula.
        /// </summary>
        public static double CalculateDistance(double? lat1, double? lng1, double? lat2, double? lng2)
        {
            // Convert degrees to radians
            double dLat = ToRadians(lat2 - lat1);
            double dLng = ToRadians(lng2 - lng1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusMeters * c; // Distance in meters
        }

        private static double ToRadians(double? val)
        {
            if (!val.HasValue)
                return 0;

            return (Math.PI / 180) * (double)val;
        }
    }
}

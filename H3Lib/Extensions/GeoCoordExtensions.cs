using System;

namespace H3Lib.Extensions
{
    /// <summary>
    /// Operations for GeoCoord type
    /// </summary>
    public static class GeoCoordExtensions
    {
        /// <summary>
        /// Set the components of spherical coordinates in decimal degrees.
        /// </summary>
        /// <param name="gc">The spherical coordinates</param>
        /// <param name="latitude">The desired latitude in decimal degrees</param>
        /// <param name="longitude">The desired longitude in decimal degrees</param>
        /// <!--
        /// geoCoord.c
        /// void setGeoDegs
        /// -->
        public static GeoCoord SetDegrees(this GeoCoord gc, double latitude, double longitude)
        {
            return gc.SetGeoRads(latitude.DegreesToRadians(), longitude.DegreesToRadians());
        }

        /// <summary>
        /// Set the components of spherical coordinates in radians.
        /// </summary>
        /// <param name="gc">The spherical coordinates</param>
        /// <param name="latitude">The desired latitude in decimal radians</param>
        /// <param name="longitude">The desired longitude in decimal radians</param>
        public static GeoCoord SetRadians(this GeoCoord gc, double latitude, double longitude)
        {
            return gc.SetGeoRads(latitude, longitude);
        }

        
        /// <summary>
        /// Set the components of spherical coordinates in radians.
        /// </summary>
        /// <param name="gc">The spherical coordinates</param>
        /// <param name="latitudeRadians">The desired latitude in decimal radians</param>
        /// <param name="longitudeRadians">The desired longitude in decimal radians</param>
        /// <!--
        /// geoCoord.c
        /// void _setGeoRads
        /// -->
        public static GeoCoord SetGeoRads(this GeoCoord gc, double latitudeRadians, double longitudeRadians)
        {
            gc =  new GeoCoord(latitudeRadians, longitudeRadians);
            return gc;
        }

        /// <summary>
        /// Quick replacement for Latitude
        /// </summary>
        public static GeoCoord SetLatitude(this GeoCoord gc, double latitude)
        {
            return new GeoCoord(latitude, gc.Longitude);
        }

        /// <summary>
        /// Quick replacement for Longitude
        /// </summary>
        public static GeoCoord SetLongitude(this GeoCoord gc, double longitude)
        {
            return new GeoCoord(gc.Latitude, longitude);
        }

        /// <summary>
        /// The great circle distance in radians between two spherical coordinates.
        /// This function uses the Haversine formula.
        /// For math details, see:
        ///     https://en.wikipedia.org/wiki/Haversine_formula
        ///     https://www.movable-type.co.uk/scripts/latlong.html
        /// </summary>
        /// <param name="a">the first lat/lng pair (in radians)</param>
        /// <param name="b">the second lat/lng pair (in radians)</param>
        /// <returns>
        /// the great circle distance in radians between a and b
        /// </returns>
        /// <!--
        /// geoCoord.c
        /// double H3_EXPORT(pointDistRads)
        /// -->
        public static double DistanceToRadians(this GeoCoord a, GeoCoord b)
        {
            double sinLat = Math.Sin((b.Latitude - a.Latitude) / 2.0);
            double sinLng = Math.Sin((b.Longitude - a.Longitude) / 2.0);
            double p = sinLat * sinLat + Math.Cos(a.Latitude) *
                       Math.Cos(b.Latitude) * sinLng * sinLng;

            return 2 * Math.Atan2(Math.Sqrt(p), Math.Sqrt(1 - p));
        }

        /// <summary>
        /// The great circle distance in kilometers between two spherical coordinates
        /// </summary>
        /// <param name="a">the first lat/lng pair (in radians)</param>
        /// <param name="b">the second lat/lng pair (in radians)</param>
        /// <!--
        /// geoCoord.c
        /// double H3_EXPORT(pointDistKm)
        /// -->
        public static double DistanceToKm(this GeoCoord a, GeoCoord b)
        {
            return a.DistanceToRadians(b) * Constants.H3.EARTH_RADIUS_KM;
        }
        
        /// <summary>
        /// The great circle distance in meters between two spherical coordinates
        /// </summary>
        /// <param name="a">the first lat/lng pair (in radians)</param>
        /// <param name="b">the second lat/lng pair (in radians)</param>
        /// <!--
        /// geoCoord.c
        /// double H3_EXPORT(pointDistM)
        /// -->
        public static double DistanceToM(this GeoCoord a, GeoCoord b)
        {
            return a.DistanceToKm(b) * 1000;
        }

        /// <summary>
        /// Determines the azimuth to p2 from p1 in radians
        /// </summary>
        /// <param name="p1">The first spherical coordinates</param>
        /// <param name="p2">The second spherical coordinates</param>
        /// <returns>The azimuth in radians from p1 to p2</returns>
        /// <!--
        /// geoCoord.c
        /// double _geoAzimuthRads
        /// -->
        public static double AzimuthRadiansTo(this GeoCoord p1, GeoCoord p2)
        {
            return
                Math.Atan2
                    (
                     Math.Cos(p2.Latitude) * Math.Sin(p2.Longitude - p1.Longitude),
                     Math.Cos(p1.Latitude) * Math.Sin(p2.Latitude) -
                     Math.Sin(p1.Latitude) * Math.Cos(p2.Latitude) *
                     Math.Cos(p2.Longitude - p1.Longitude)
                    );
        }

        /// <summary>
        /// Computes the point on the sphere a specified azimuth and distance from
        /// another point.
        /// </summary>
        /// <param name="p1">The first spherical coordinates.</param>
        /// <param name="azimuth">The desired azimuth from p1.</param>
        /// <param name="distance">The desired distance from p1, must be non-negative.</param>
        /// <returns>The spherical coordinates at the desired azimuth and distance from p1.</returns>
        /// <!--
        /// geoCoord.c
        /// void _geoAzDistanceRads
        /// -->
        public static GeoCoord GetAzimuthDistancePoint(this GeoCoord p1, double azimuth, double distance)
        {
            if (distance < Constants.H3.EPSILON)
            {
                return p1;
            }

            azimuth = azimuth.NormalizeRadians();
            var p2 = new GeoCoord();

            // check for due north/south azimuth
            if (azimuth < Constants.H3.EPSILON ||
                Math.Abs(azimuth - Constants.H3.M_PI) < Constants.H3.EPSILON)
            {
                if (azimuth < Constants.H3.EPSILON) // due north
                {
                    p2 = p2.SetLatitude(p1.Latitude + distance);
                }
                else // due south
                {
                    p2 = p2.SetLatitude(p1.Latitude - distance);
                }

                if (Math.Abs(p2.Latitude - Constants.H3.M_PI_2) < Constants.H3.EPSILON) // north pole
                {
                    p2 = new GeoCoord(Constants.H3.M_PI_2, 0.0);
                }
                else if (Math.Abs(p2.Latitude + Constants.H3.M_PI_2) < Constants.H3.EPSILON) // south pole
                {
                    p2 = new GeoCoord(-Constants.H3.M_PI_2, 0.0);
                }
                else
                {
                    p2 = p2.SetLongitude(p1.Longitude.ConstrainLongitude());
                }
            }
            else // Not due north or south
            {
                double sinLatitude = Math.Sin(p1.Latitude) * Math.Cos(distance) +
                                Math.Cos(p1.Latitude) * Math.Sin(distance) * Math.Cos(azimuth);
                if (sinLatitude > 1.0)
                {
                    sinLatitude = 1.0;
                }

                if (sinLatitude < -1.0)
                {
                    sinLatitude = 1.0;
                }

                p2 = p2.SetLatitude(Math.Asin(sinLatitude));

                if (Math.Abs(p2.Latitude - Constants.H3.M_PI_2) < Constants.H3.EPSILON) // north pole
                {
                    p2 = new GeoCoord(Constants.H3.M_PI_2, 0.0);
                }
                else if (Math.Abs(p2.Latitude + Constants.H3.M_PI_2) < Constants.H3.EPSILON) // south pole
                {
                    p2 = new GeoCoord(-Constants.H3.M_PI_2, 0.0);
                }
                else
                {
                    double sinLongitude = Math.Sin(azimuth) * Math.Sin(distance) / Math.Cos(p2.Latitude);
                    double cosLongitude = (Math.Cos(distance) - Math.Sin(p1.Latitude) * Math.Sin(p2.Latitude)) /
                                    Math.Cos(p1.Latitude) / Math.Cos(p2.Latitude);
                    if (sinLongitude > 1.0)
                    {
                        sinLongitude = 1.0;
                    }

                    if (sinLongitude < -1.0)
                    {
                        sinLongitude = -1.0;
                    }

                    if (cosLongitude > 1.0)
                    {
                        cosLongitude = 1.0;
                    }

                    if (cosLongitude < -1.0)
                    {
                        cosLongitude = -1.0;
                    }

                    p2 = p2.SetLongitude
                        (
                         (p1.Longitude + Math.Atan2(sinLongitude, cosLongitude))
                        .ConstrainLongitude()
                        );
                }
            }

            return p2;            
        }

        /// <summary>
        /// Encodes a coordinate on the sphere to the FaceIJK address of the containing
        /// cell at the specified resolution.
        /// </summary>
        /// <param name="g">The spherical coordinates to encode.</param>
        /// <param name="res">The desired H3 resolution for the encoding.</param>
        /// <returns>The FaceIJK address of the containing cell at resolution res.</returns>
        /// <!--
        /// faceijk.c
        /// void _geoToFaceIjk
        /// -->
        public static FaceIjk ToFaceIjk(this GeoCoord g, int res)
        {
            // first convert to hex2d
            (int newFace, var v) = g.ToHex2d(res);
            var newCoord = v.ToCoordIjk();
           
            return new FaceIjk(newFace, newCoord);
        }

        /// <summary>
        /// Encodes a coordinate on the sphere to the corresponding icosahedral face and
        /// containing 2D hex coordinates relative to that face center.
        /// </summary>
        /// <param name="g">The spherical coordinates to encode.</param>
        /// <param name="res">The desired H3 resolution for the encoding.</param>
        /// <returns>
        /// Tuple
        /// Item1: The resulting face
        /// Item2: The 2D hex coordinates of the cell containing the point.
        /// </returns>
        /// <!--
        /// faceijk.c
        /// void _geoToHex2d
        /// -->
        public static (int, Vec2d) ToHex2d(this GeoCoord g, int res)
        {
            Vec3d v3d = g.ToVec3d();
            
            int newFace = 0;

            // determine the icosahedron face
            double sqd = v3d.PointSquareDistance(Constants.FaceIjk.FaceCenterPoint[0]);

            for (int f = 1; f < Constants.H3.NUM_ICOSA_FACES; f++)
            {
                double sqdT = v3d.PointSquareDistance(Constants.FaceIjk.FaceCenterPoint[f]);
                if (!(sqdT < sqd))
                {
                    continue;
                }
                newFace = f;
                sqd = sqdT;
            }
            
            // cos(r) = 1 - 2 * sin^2(r/2) = 1 - 2 * (sqd / 4) = 1 - sqd/2
            double r = Math.Acos(1 - sqd / 2.0);

            if (r < Constants.H3.EPSILON)
            {
                return (newFace, new Vec2d());
            }
            // now have face and r, now find CCW theta from CII i-axis
            double theta =
                (
                    Constants.FaceIjk.FaceAxesAzRadsCii[newFace, 0] -
                    Constants.FaceIjk.FaceCenterGeo[newFace].AzimuthRadiansTo(g)
                             .NormalizeRadians()
                ).NormalizeRadians();
            
            // adjust theta for Class III (odd resolutions)
            if (res.IsResClassIii())
            {
                theta = (theta - Constants.H3.M_AP7_ROT_RADS).NormalizeRadians();
            }

            // perform gnomonic scaling of r
            r = Math.Tan(r);

            // scale for current resolution length u
            r /= Constants.H3.RES0_U_GNOMONIC;
            for (var i = 0; i < res; i++)
            {
                r *= Constants.FaceIjk.MSqrt7;
            }
            
            // we now have (r, theta) in hex2d with theta ccw from x-axes
            // convert to local x,y
            return (newFace,
                    new Vec2d
                        (
                         r * Math.Cos(theta),
                         r * Math.Sin(theta)
                        ));
        }

        /// <summary>
        /// Calculate the 3D coordinate on unit sphere from the latitude and longitude.
        /// </summary>
        /// <param name="geo">The latitude and longitude of the point</param>
        /// <!--
        /// vec3d.c
        /// void _geoToVec3d
        /// -->
        public static Vec3d ToVec3d(this GeoCoord geo)
        {
            double r = Math.Cos(geo.Latitude);
            return new Vec3d
                (
                 Math.Cos(geo.Longitude) * r,
                 Math.Sin(geo.Longitude) * r,
                 Math.Sin(geo.Latitude)
                );
        }

        /// <summary>
        /// Encodes a coordinate on the sphere to the H3 index of the containing cell at
        /// the specified resolution.
        ///
        /// Returns 0 on invalid input.
        /// </summary>
        /// <param name="g">The spherical coordinates to encode.</param>
        /// <param name="res">The desired H3 resolution for the encoding.</param>
        /// <returns>The encoded H3Index (or H3_NULL on failure).</returns>
        /// <!--
        /// h3Index.c
        /// H3Index H3_EXPORT(geoToH3)
        /// -->
        public static H3Index ToH3Index(this GeoCoord g, int res)
        {
            if (res < 0 || res > Constants.H3.MAX_H3_RES)
            {
                return Constants.H3Index.H3_INVALID_INDEX;
            }
            
            if (!double.IsFinite(g.Latitude) || !double.IsFinite(g.Longitude))
            {
                return Constants.H3Index.H3_INVALID_INDEX;
            }

            return g.ToFaceIjk(res).ToH3(res);
        }

        /// <summary>
        /// returns an estimated number of hexagons that trace
        /// the cartesian-projected line
        /// </summary>
        /// <param name="origin">the origin coordinates</param>
        /// <param name="destination">the destination coordinates</param>
        /// <param name="res">the resolution of the H3 hexagons to trace the line</param>
        /// <returns>the estimated number of hexagons required to trace the line</returns>
        /// <!--
        /// bbox.c
        /// int lineHexEstimate
        /// -->
        public static int LineHexEstimate(this GeoCoord origin, GeoCoord destination, int res)
        {
            // Get the area of the pentagon as the maximally-distorted area possible
            var pentagons = res.GetPentagonIndexes();
            double pentagonRadiusKm = pentagons[0].HexRadiusKm();
            double dist = origin.DistanceToKm(destination);

            var estimate = (int) Math.Ceiling(dist / (2 * pentagonRadiusKm));
            if (estimate == 0)
            {
                estimate = 1;
            }
            return estimate;

        }
        
    }
}

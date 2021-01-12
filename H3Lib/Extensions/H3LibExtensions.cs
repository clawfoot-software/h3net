using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace H3Lib.Extensions
{
    public static class H3LibExtensions
    {
        /// <summary>
        /// Normalizes radians to a value between 0.0 and two PI.
        /// </summary>
        /// <param name="rads">The input radians value</param>
        /// <param name="limit">Default value of 2pi. _Can_ be changed, probably shouldn't</param>
        /// <returns>The normalized radians value</returns>
        /// <remarks>
        /// Originally part of geoCoord.c as  double _posAngleRads(double rads)
        /// 
        /// However, it's only used once in
        /// void _geoAzDistanceRads(const GeoCoord *p1, double az, double distance, GeoCoord *p2)
        /// 
        /// It's used multiple times in faceijk.c, _geoToHex2d and _hex2dToGeo
        /// 
        /// For now, let's isolate it and see if it needs to be folded in later.
        /// </remarks>
        /// <!--
        /// geoCoord.c
        /// double _posAngleRads
        /// -->
        public static double NormalizeRadians(this double rads, double limit = Constants.M_2PI)
        {
            if (rads < 0.0)
            {
                rads += Math.Ceiling(Math.Abs(rads / limit)) * limit;
            }

            while (rads >= limit)
            {
                rads -= limit;
            }

            return rads;
        }

        /// <summary>
        /// Makes sure latitudes are in the proper bounds
        /// </summary>
        /// <param name="latitude">The original lat value</param>
        /// <returns>The corrected lat value</returns>
        /// <!--
        /// geoCoord.c
        /// double constrainLat
        /// -->
        public static double ConstrainLatitude(this double latitude)
        {
            while (latitude > Constants.M_PI_2)
            {
                latitude -= Constants.M_PI;
            }

            return latitude;
        }
        
        /// <summary>
        /// Makes sure longitudes are in the proper bounds
        /// </summary>
        /// <param name="longitude">The origin lng value</param>
        /// <returns>The corrected lng value</returns>
        /// <!--
        /// geoCoord.c
        /// double constrainLng
        /// -->
        public static double ConstrainLongitude(this double longitude)
        {
            while (longitude > Constants.M_PI)
            {
                longitude -= 2 * Constants.M_PI;
            }

            while (longitude < -Constants.M_PI)
            {
                longitude += 2 * Constants.M_PI;
            }

            return longitude;
        }


        /// <summary>
        /// Convert from decimal degrees to radians.
        /// </summary>
        /// <param name="degrees">The decimal degrees</param>
        /// <returns>The corresponding radians</returns>
        /// <!--
        /// geoCoord.c
        /// double H3_EXPORT(degsToRads)
        /// -->
        public static double DegreesToRadians(this double degrees)
        {
            return degrees * Constants.M_PI_180;
        }

        /// <summary>
        /// Convert from radians to decimal degrees.
        /// </summary>
        /// <param name="radians">The radians</param>
        /// <returns>The corresponding decimal degrees</returns>
        /// <!--
        /// geoCoord.c
        /// double H3_EXPORT(radsToDegs)
        /// -->
        public static double RadiansToDegrees(this double radians)
        {
            return radians * Constants.M_180_PI;
        }

        /// <summary>
        /// Does integer exponentiation efficiently. Taken from StackOverflow.
        ///
        /// An example of this can be found at:
        /// https://stackoverflow.com/questions/101439/the-most-efficient-way-to-implement-an-integer-based-power-function-powint-int
        /// </summary>
        /// <param name="baseValue">the integer base (can be positive or negative)</param>
        /// <param name="power">the integer exponent (should be nonnegative)</param>
        /// <returns>the exponentiated value</returns>
        /// <!--
        /// mathExtensions.c
        /// int64_t _ipow
        /// -->
        public static long Power(this long baseValue, long power)
        {
            long result = 1;

            while (power > 0)
            {
                if ((power & 1) != 0)
                {
                    result *= baseValue;
                }
                power >>= 1;
                baseValue *= baseValue;
            }

            return result;
        }

        /// <summary>
        /// Number of unique valid H3Indexes at given resolution.
        /// </summary>
        /// <param name="res">Resolution to get count of cells</param>
        /// <!--
        /// geoCoord.c
        /// int64_t H3_EXPORT(numHexagons)
        /// -->
        public static long NumHexagons(int res)
        {
            return 2 + 120 * 7L.Power(res);
        }
        
        /// <summary>
        /// Returns whether or not a resolution is a Class III grid. Note that odd
        ///  resolutions are Class III and even resolutions are Class II.
        /// </summary>
        /// <param name="res">The H3 resolution</param>
        /// <returns>Returns true if the resolution is class III grid, otherwise false.</returns>
        /// <!--
        /// h3Index.c
        /// int H3_EXPORT(h3IsResClassIII)
        /// -->
        public static bool IsResClassIii(this int res) => res % 2 == 1;


        /// <summary>
        /// Square of a number
        /// </summary>
        /// <param name="x">The input number</param>
        /// <returns>The square of the input number</returns>
        /// <!--
        /// vec3d.c
        /// double _square
        /// -->
        public static double Square(this double x)
        {
            return x * x;
        }

        /// <summary>
        /// Converts a string representation of an H3 index into an H3 index.
        /// </summary>
        /// <param name="s"> The string representation of an H3 index.</param>
        /// <returns>
        /// The H3 index corresponding to the string argument, or 0 if invalid.
        /// </returns>
        /// <!--
        /// h3Index.c
        /// H3Index H3_EXPORT(stringToH3)
        /// -->
        public static H3Index ToH3Index(this string s)
        {
            // A small risk, but for the most part, we're dealing with hex
            // numbers, so let's use that as our default.
            if (ulong.TryParse(s, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong ul1))
            {
                return new H3Index(ul1);
            }
            // If failed, try parsing as a decimal based number
            return ulong.TryParse(s, out ulong ul2)
                       ? new H3Index(ul2)
                       : 0;
        }

        /// <summary>
        /// Determines whether one resolution is a valid child resolution of another.
        /// Each resolution is considered a valid child resolution of itself.
        /// </summary>
        /// <param name="parentRes">int resolution of the parent</param>
        /// <param name="childRes">int resolution of the child</param>
        /// <returns>The validity of the child resolution</returns>
        /// <!--
        /// h3Index.c
        /// static bool _isValidChildRes
        /// -->
        public static bool IsValidChildRes(this int parentRes, int childRes)
        {
            return childRes >= parentRes &&
                   childRes <= Constants.MAX_H3_RES;
        }

        /// Generates all pentagons at the specified resolution
        ///
        /// <param name="res">The resolution to produce pentagons at.</param>
        /// <returns>Output List.</returns>
        /// <!--
        /// h3Index.c
        /// void H3_EXPORT(getPentagonIndexes)
        /// -->
        public static List<H3Index> GetPentagonIndexes(this int res)
        {
            var cells = Enumerable
                       .Range(0, Constants.NUM_BASE_CELLS)
                       .Where(t => t.IsBaseCellPentagon());
            
            return cells
                  .Select(cell => new H3Index().SetIndex(res, cell, Direction.CENTER_DIGIT))
                  .ToList();
        }

        /// <summary>
        /// compact takes a set of hexagons all at the same resolution and compresses
        /// them by pruning full child branches to the parent level. This is also done
        /// for all parents recursively to get the minimum number of hex addresses that
        /// perfectly cover the defined space.
        /// </summary>
        /// <param name="h3Set"> Set of hexagons</param>
        /// <returns>status code and compacted hexes</returns>
        /// <remarks>
        /// Gonna do this a bit differently, allowing for varying
        /// resolutions in input data
        /// </remarks>
        /// <!--
        /// h3index.c
        /// int H3_EXPORT(compact)
        /// -->
        public static (int, List<H3Index>) Compact(List<H3Index> h3Set)
        {
            if (h3Set == null || h3Set.Count == 0)
            {
                return (StaticData.H3Index.COMPACT_SUCCESS, new List<H3Index>());
            }

            if (h3Set.All(h => h.Resolution == 0))
            {
                // No compaction possible, just copy the set to output
                return (StaticData.H3Index.COMPACT_SUCCESS, new List<H3Index>(h3Set));
            }

            //  Here's our results
            var results = new HashSet<ulong>();
            var copy = new List<H3Index>(h3Set);
            //  Move the 0 res to the results.
            if (h3Set.Any(h => h.Resolution == 0))
            {
                results.UnionWith(h3Set.Where(h => h.Resolution == 0).Select(s=>s.H3Value));
                copy.RemoveAll(r => r.Resolution == 0);
            }

            //  What our highest resolution is.
            int currentRes = copy.Select(r => r.Resolution).Max();
            //  Get the remaining set with all the dupes stripped out.
            var baseNumbers = new HashSet<ulong>(copy.Select(s => s.H3Value)).ToList();

            //  Now we've got our base cells
            var pool = new HashSet<H3Index>(baseNumbers.Select(h=>(H3Index) h));

            while (pool.Count > 0)
            {
                var cluster = new Dictionary<ulong, List<ulong>>();
                //  Get the cells at the current resolution and collect in parents.
                foreach (var index in pool.Where(p => p.Resolution == currentRes))
                {
                    var parent = index.ToParent(currentRes - 1);
                    if (!cluster.ContainsKey(parent.H3Value))
                    {
                        cluster[parent.H3Value] = new List<ulong>();
                    }

                    cluster[parent.H3Value].Add(index);
                }
                
                //  Check the parent keys for the amount of children
                foreach (ulong key in cluster.Keys)
                {
                    if (!((H3Index) key).IsValid())
                    {
                        return (StaticData.H3Index.COMPACT_BAD_DATA, new List<H3Index>());
                    }
                    int countChildren = cluster[key].Count;
                    
                    if (((H3Index) key).IsPentagon())
                    {
                        //  Complete set of children, dump kids, add parent to pool
                        if (countChildren == 6)
                        {
                            pool.Add(key);
                        }
                        else
                        {
                            results.UnionWith(cluster[key]);
                            pool.ExceptWith(cluster[key].Select(h=>(H3Index) h));
                        }
                    }
                    else
                    {
                        //  Complete set of children, dump kids, add parent to pool
                        if (countChildren == 7)
                        {
                            pool.Add(key);
                        }
                        else
                        {
                            results.UnionWith(cluster[key]);
                            pool.ExceptWith(cluster[key].Select(h=>(H3Index) h));
                        }
                    }
                }

                //  Let's get ready for the next round.
                cluster.Clear();
                currentRes--;
                if (currentRes != 0)
                {
                    continue;
                }

                //  We're down to res 0, clear the p0ol to jump out.
                results.UnionWith(pool.Select(p => p.H3Value));
                pool.Clear();
            }

            return (StaticData.H3Index.COMPACT_SUCCESS,
                    results.Select(r => (H3Index) r).ToList());
        }

        /// <summary>
        /// Maximum number of cells that result from the kRing algorithm with the given
        /// k. Formula source and proof: https://oeis.org/A003215
        /// </summary>
        /// <param name="k">k value, k &gt;= 0.</param>
        /// <!--
        /// algos.c
        /// int H3_EXPORT(maxKringSize)
        /// -->
        public static int MaxKringSize(this int k)
        {
            return 3 * k * (k + 1) + 1;
        }

        /// <summary>
        /// Normalize longitude, dealing with transmeridian arcs
        /// </summary>
        /// <!--
        /// polygonAlgos.h
        /// #define NORMALIZE_LON
        /// -->
        public static double NormalizeLongitude(this double longitude, bool isTransmeridian)
        {
            return isTransmeridian && longitude < 0
                       ? longitude + Constants.M_2PI
                       : longitude;
        }

    }
}

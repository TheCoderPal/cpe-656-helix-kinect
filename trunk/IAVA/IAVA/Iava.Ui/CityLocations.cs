﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Client.Geometry;

namespace Iava.Ui
{
    /// <summary>
    /// Loads in and maps a city name to a latitude longitude coordinate.
    /// </summary>
    public class CityLocations
    {
        private const string CityLocationPattern = LatLonPattern + @"\s+(.+),(.+)";

        private const string LatLonPattern = @"(-?[0-9]+(?:\.[0-9]*)?)\s+(-?[0-9]+(?:\.[0-9]*)?)";

        private const string CityLocationsFilePath = @".\Resources\CityLocationsData.txt";

        private static readonly Dictionary<string, MapPoint> cityToLocationMap = new Dictionary<string, MapPoint>();

        static CityLocations()
        {
            LoadCityLocationsFile();
        }

        private static void LoadCityLocationsFile()
        {
            using (StreamReader reader = new StreamReader(CityLocationsFilePath))
            {
                Regex regex = new Regex(CityLocationPattern);

                const int groupCount = 5;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Groups.Count == groupCount)
                    {
                        double lat = double.Parse(match.Groups[1].ToString());
                        double lon = double.Parse(match.Groups[2].ToString());

                        // TODO: Change lat/lon to correct format for map type!

                        string cityNameKey = match.Groups[3].ToString() + " " + match.Groups[4].ToString();
                        cityNameKey = cityNameKey.ToLower();
                        if (!cityToLocationMap.ContainsKey(cityNameKey))
                        {
                            cityToLocationMap.Add(cityNameKey, new MapPoint(lon, lat));
                        }
                    }
                }
            }
        }

        public static MapPoint GetCityLocation(string cityName)
        {
            MapPoint rv = null;
            cityToLocationMap.TryGetValue(cityName, out rv);

            return rv;
        }
    }
}

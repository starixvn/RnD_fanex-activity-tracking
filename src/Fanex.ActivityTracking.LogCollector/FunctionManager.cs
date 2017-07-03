////namespace Fanex.Tarus.ActivityTracking.Services
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Linq;
////    using Fanex.ActivityTracking.Configurations;
////    using Fanex.ActivityTracking.Models;
////    using User.DataAccess.Sql;
////    using User.Entities;

////    /// <summary>
////    /// This class must be moved to log collector.
////    /// </summary>
////    public static class FunctionManager
////    {
////        private readonly static IDictionary<string, IEnumerable<string>> functionWithUrls = new Dictionary<string, IEnumerable<string>>();
////        private readonly static IList<TrackingItem> trackingItems = AppSettings.ActivityTrackingConfig.TrackingItems.ToList();

////        private readonly static object LockObject = new object();

////        static FunctionManager()
////        {
////            LoadFunctionWithUrls();
////            UpdateFunctionTrackingItemByUrls();
////        }

////        public static void LoadFunctionWithUrls()
////        {
////            if (!functionWithUrls.Keys.Any())
////            {
////                lock (LockObject)
////                {
////                    if (!functionWithUrls.Keys.Any())
////                    {
////                        var functionAccess = new FunctionAccess();
////                        var funtions = functionAccess.GetFunctionList();

////                        foreach (var function in funtions)
////                        {
////                            if (!string.IsNullOrEmpty(function.Files))
////                            {
////                                functionWithUrls[function.Name] = AddFunctionUrlsRecursively(function);
////                            }
////                        }
////                    }
////                }
////            }
////        }

////        private static IList<string> AddFunctionUrlsRecursively(FunctionBO function)
////        {
////            if(string.IsNullOrEmpty(function.Files))
////            {
////                return new List<string>();
////            }

////            var functionUrls = function
////                .Files
////                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
////                .Select(url => url.ToLowerInvariant().Trim())
////                .ToList();

////            if(function.SubFuncs != null 
////                && function.SubFuncs.Any())
////            {
////                foreach (var subFunction in function.SubFuncs)
////                {
////                    var subFunctionUrls = AddFunctionUrlsRecursively(subFunction);
////                    functionWithUrls[subFunction.Name] = subFunctionUrls;
////                    functionUrls.AddRange(subFunctionUrls);
////                }
////            }

////            return functionUrls;
////        }

////        public static void UpdateFunctionTrackingItemByUrls()
////        {
////            if(functionWithUrls.Any() && trackingItems.Any())
////            {
////                foreach (var trackingItem in trackingItems)
////                {
////                    if(!string.IsNullOrEmpty(trackingItem.Function)
////                        && FunctionWithUrls.ContainsKey(trackingItem.Function)
////                        && FunctionWithUrls[trackingItem.Function] != null)
////                    {
////                        trackingItem.Urls = trackingItem.Urls == null 
////                            ? FunctionWithUrls[trackingItem.Function] 
////                            : trackingItem.Urls.Concat(FunctionWithUrls[trackingItem.Function]);
////                    }
////                }
////            }
////        }

////        public static IDictionary<string, IEnumerable<string>> FunctionWithUrls => functionWithUrls;

////        public static IList<TrackingItem> TrackingItems => trackingItems;
////    }
////}
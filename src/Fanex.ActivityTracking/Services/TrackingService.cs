namespace Fanex.ActivityTracking.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configurations;
    using Models;

    public interface TrackingService
    {
        TrackingItem GetTrackingItem(string url, IEnumerable<TrackingItem> trackingItems, IEnumerable<string> ignoredUrls);
    }

    public class TrackingServiceImpl : TrackingService
    {
        protected readonly bool enabledTrackAllRequests;
        protected readonly ActivityTrackingConfig activityTrackingConfig;

        public TrackingServiceImpl(ActivityTrackingConfig activityTrackingConfig = null)
        {
            this.activityTrackingConfig = activityTrackingConfig ?? AppSettings.ActivityTrackingConfig;
            this.enabledTrackAllRequests = this.activityTrackingConfig.EnabledTrackingAllRequests;
        }

        public virtual TrackingItem GetTrackingItem(string url, IEnumerable<TrackingItem> trackingItems, IEnumerable<string> ignoredUrls)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (trackingItems != null
                && trackingItems.Any())
            {
                var urlToLower = url.ToLowerInvariant();
                foreach (var trackingItem in trackingItems)
                {
                    if (trackingItem.Urls != null
                        && trackingItem.Urls.Any())
                    {
                        foreach (var trackingUrl in trackingItem.Urls)
                        {
                            if (!string.IsNullOrWhiteSpace(trackingUrl)
                                && urlToLower.Contains(trackingUrl))
                            {
                                return trackingItem;
                            }
                        }
                    }
                }
            }

            if (enabledTrackAllRequests)
            {
                return CheckIgnoredUrl(url, ignoredUrls);
            }

            return TrackingItem.NoTrackingItem;
        }

        private TrackingItem CheckIgnoredUrl(string url, IEnumerable<string> ignoredUrls)
        {
            if (ignoredUrls != null
                && ignoredUrls.Any())
            {
                var urlToLower = url.ToLowerInvariant();
                foreach (var ignoredUrl in ignoredUrls)
                {
                    if (!string.IsNullOrWhiteSpace(ignoredUrl)
                        && urlToLower.Contains(ignoredUrl))
                    {
                        return TrackingItem.NoTrackingItem;
                    }
                }
            }

            return TrackingItem.BasicTrackingItem;
        }
    }
}
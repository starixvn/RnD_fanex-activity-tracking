namespace Fanex.ActivityTracking.Services
{
    using System.Linq;
    using System.Collections.Generic;
    using Models;

    public interface TrackingService
    {
        TrackingItem GetTrackingItem(string url, IEnumerable<TrackingItem> trackingItems);
    }

    public class TrackingServiceImpl : TrackingService
    {
        private readonly bool enabledTrackAllRequests;

        public TrackingServiceImpl(bool enabledTrackAllRequests)
        {
            this.enabledTrackAllRequests = enabledTrackAllRequests;
        }

        public virtual TrackingItem GetTrackingItem(string url, IEnumerable<TrackingItem> trackingItems)
        {
            if (enabledTrackAllRequests)
            {
                return TrackingItem.MiddleLevelTrackingItem;
            }

            if (trackingItems.Any())
            {
                var urlToLower = url.ToLowerInvariant();
                foreach (var item in trackingItems)
                {
                    foreach (var itemUrl in item.Urls)
                    {
                        if (!string.IsNullOrWhiteSpace(itemUrl)
                            && urlToLower.Contains(itemUrl))
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }
    }
}
namespace BaristaLabs.Skrapr.ChromeDevTools.ApplicationCache
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NetworkStateUpdatedEvent : IEvent
    {
    
        
        /// <summary>
        /// Gets or sets the isNowOnline
        /// </summary>
        
        public bool IsNowOnline
        {
            get;
            set;
        }
    
    }
}
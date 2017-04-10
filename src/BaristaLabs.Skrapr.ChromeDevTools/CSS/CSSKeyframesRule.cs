namespace BaristaLabs.Skrapr.ChromeDevTools.CSS
{
    /// <summary>
    /// CSS keyframes rule representation.
    /// </summary>
    public sealed class CSSKeyframesRule
    {
    
        /// <summary>
        /// Animation name.
        ///</summary>
        public Value AnimationName
        {
            get;
            set;
        }
    
        /// <summary>
        /// List of keyframes.
        ///</summary>
        public CSSKeyframeRule[] Keyframes
        {
            get;
            set;
        }
    
    }
}
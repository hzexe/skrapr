namespace BaristaLabs.Skrapr.ChromeDevTools.Debugger
{
    using Newtonsoft.Json;

    /// <summary>
    /// Edits JavaScript source live.
    /// </summary>
    public sealed class SetScriptSourceCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Debugger.setScriptSource";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    
        
        /// <summary>
        /// Id of the script to edit.
        /// </summary>
        
        [JsonProperty("scriptId")]
        public string ScriptId
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// New content of the script.
        /// </summary>
        
        [JsonProperty("scriptSource")]
        public string ScriptSource
        {
            get;
            set;
        }
    
        
        /// <summary>
        ///  If true the change will not actually be applied. Dry run may be used to get result description without actually modifying the code.
        /// </summary>
        
        [JsonProperty("dryRun")]
        public bool DryRun
        {
            get;
            set;
        }
    
    }

    public sealed class SetScriptSourceCommandResponse : ICommandResponse<SetScriptSourceCommand>
    {
    
        
        /// <summary>
        /// New stack trace in case editing has happened while VM was stopped.
        ///</summary>
        
        [JsonProperty("callFrames")]
        public CallFrame[] CallFrames
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Whether current call stack  was modified after applying the changes.
        ///</summary>
        
        [JsonProperty("stackChanged")]
        public bool StackChanged
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Async stack trace, if any.
        ///</summary>
        
        [JsonProperty("asyncStackTrace")]
        public BaristaLabs.Skrapr.ChromeDevTools.Runtime.StackTrace AsyncStackTrace
        {
            get;
            set;
        }
    
        
        /// <summary>
        /// Exception details if any.
        ///</summary>
        
        [JsonProperty("exceptionDetails")]
        public BaristaLabs.Skrapr.ChromeDevTools.Runtime.ExceptionDetails ExceptionDetails
        {
            get;
            set;
        }
    
    }
}
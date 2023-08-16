using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction.ArtemisEngineeringPresets
{
    [Serializable]
    public class InvalidPresetFileException : Exception
    {
        public InvalidPresetFileException() : base("Preset file is invalid") { }
        public InvalidPresetFileException(string message) : base(message) { }
        public InvalidPresetFileException(string message, string file) : base(string.Format("File: {0}: {1}", file, message)) { }
        protected InvalidPresetFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public InvalidPresetFileException(string message, Exception ex) : base(message, ex) { }
    }
}

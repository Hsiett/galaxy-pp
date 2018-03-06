using System;

namespace SharedClasses
{
    [Serializable]
    public class MyErrorMessage
    {
        public Exception Error;
        public string UserMessage;
        public string ProgramVersion;
        public byte[] ZippedProject;

        public MyErrorMessage(string userMessage, Exception error, string version, byte[] zippedProject)
        {
            UserMessage = userMessage;
            Error = error;
            ProgramVersion = version;
            ZippedProject = zippedProject;
        }
    }
}

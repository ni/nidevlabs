using System.Collections.Generic;

namespace NationalInstruments.WebServiceRunner
{
    /// <summary>
    /// Manages the list of registered executables (web service VIs).
    /// </summary>
    internal class ConnectionTypeManager
    {
        private readonly List<RegisteredExecutable> _registeredCommectionTypes = new List<RegisteredExecutable>();

        public ConnectionTypeManager(GllManager gllManager)
        {
            GLLManager = gllManager;
        }

        public void AddConnectionType(RegisteredExecutable connectionType)
        {
            _registeredCommectionTypes.Add(connectionType);
        }

        public GllManager GLLManager { get; }
    }
}
using System.Collections.Generic;

namespace ExtCS.Debugger.ListCommandHelper
{
    public abstract class DataHelper
    {
        public string ProcessData(ICommand command)
        {
            return command.Args;
        }
        public abstract bool SaveData(ICommand command);
        public abstract IList<ICommand> GetRecentCommands();
        public abstract IList<ICommand> GetContainers();
        public abstract IList<ICommand> GetScriptsFromContainer(string containername);
        public abstract IList<ICommand> Search(string searchString, string containerName);
        public abstract IList<ICommand> Search(string searchString);
    }
}

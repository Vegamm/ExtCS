namespace ExtCS.Debugger.ListCommandHelper
{
    public interface ICommand
    {
         string Args { get; }

         string ScriptName { get; }

        IResult Execute(params string[] args);

    }
}

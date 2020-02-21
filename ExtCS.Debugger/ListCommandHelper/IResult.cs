using System;

namespace ExtCS.Debugger.ListCommandHelper
{
    public interface IResult
    {
        bool IsSuccess{get;set;}
        Exception LastError { get; set; }
        Object Value { get; set; }
    }
}

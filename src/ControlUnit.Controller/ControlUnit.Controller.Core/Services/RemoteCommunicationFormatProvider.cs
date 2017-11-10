using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ControlUnit.Controller.Core.Services
{
    /// <summary>
    /// Standard format provider for the communication with the arduino motorengine
    /// </summary>
    public class RemoteCommunicationFormatProvider : IRemoteCommunicationFormatProvider
    {
        public object BuildRequestObject(MethodInfo method, Dictionary<string, object> parameters)
        {
            var serializableParams = new List<string>();
            var sb = new StringBuilder();

            foreach (var param in parameters)
            {
                serializableParams.Add($"{param.Key}: {param.Value}");
                sb.Append(param.Value + ",");
            }

            var request = $"<TX>{method.Name}({sb.ToString().TrimEnd(',')})</TX>";
            return request;
            //return new { Method = method.Name, Params = serializableParams };
        }
    }
}

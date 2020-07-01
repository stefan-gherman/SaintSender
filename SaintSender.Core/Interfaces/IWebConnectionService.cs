using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintSender.Core.Interfaces
{
    public interface IWebConnectionService
    {
        bool ConnectionCheck(string url);
        bool PingChek(string url);
        bool NLMAPICheck();
    }
}

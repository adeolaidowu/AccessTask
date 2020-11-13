using AccessBankTask.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccessBankTask.Services
{
    public interface ILogActivityRepository
    {
        Task<bool> AddLogActivity(LogActivity model);
        Task<bool> UpdateLogActivity(LogActivity model);
        Task<bool> DeleteLogActivity(LogActivity model);
        Task<LogActivity> GetLogActivity(string userId);
        Task<LogActivity> GetActiveLogActivity(string userId);
        Task<List<LogActivity>> GetAllLogActivity();
        Task<LogActivity> GetLogActivityId(int Id);
        //Task<bool> GetIpAddress(string id, string deviceIp);
    }
}

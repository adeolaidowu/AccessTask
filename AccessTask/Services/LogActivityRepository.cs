using AccessBankTask.Data;
using AccessBankTask.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccessBankTask.Services
{
    public class LogActivityRepository : ILogActivityRepository
    {
        private readonly AppDbContext _ctx;

        public LogActivityRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<bool> AddLogActivity(LogActivity model)
        {
            await _ctx.LogActivities.AddAsync(model);
            return await _ctx.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateLogActivity(LogActivity model)
        {
            _ctx.LogActivities.Update(model);
            return await _ctx.SaveChangesAsync() > 0;            
        }
        public async Task<bool> DeleteLogActivity(LogActivity model)
        {
            _ctx.LogActivities.Remove(model);
            return await _ctx.SaveChangesAsync() > 0;
        }
        public async Task<LogActivity> GetLogActivity(string userId)
        {
            return await _ctx.LogActivities.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<LogActivity> GetLogActivityId(int Id)
        {
            return await _ctx.LogActivities.FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<LogActivity> GetActiveLogActivity(string userId)
        {
            return await _ctx.LogActivities.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive == true);
        }

        public async Task<List<LogActivity>> GetAllLogActivity()
        {
            return await _ctx.LogActivities.ToListAsync();
        }


        //public async Task<bool> GetIpAddress(string id, string deviceIp)
        //{
        //    var result = await GetLogActivity(id);
        //    if (result.DeviceIp == null) return true;
        //    return false;
        //}
    }
}

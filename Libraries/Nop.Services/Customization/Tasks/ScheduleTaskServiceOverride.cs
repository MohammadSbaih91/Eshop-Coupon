using System.Collections.Generic;
using Nop.Services.Tasks;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Tasks;
using Nop.Core.Configuration;

namespace Nop.Services.Customization.Tasks
{
    public class ScheduleTaskServiceOverride : ScheduleTaskService
    {
        private readonly IRepository<ScheduleTask> _taskRepository;
        private readonly NopConfig _nopConfig;

        public ScheduleTaskServiceOverride(IRepository<ScheduleTask> taskRepository,
            NopConfig nopConfig) 
            : base(taskRepository)
        {
            this._taskRepository = taskRepository;
            this._nopConfig = nopConfig;
        }

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Tasks</returns>
        public override IList<ScheduleTask> GetAllTasks(bool showHidden = false)
        {
            var query = _taskRepository.Table;
            if (!showHidden)
            {
                query = query.Where(t => t.Enabled);
            }

            if (_nopConfig.SchedultasksList != null && _nopConfig.SchedultasksList.Length > 0)
                query = query.Where(p => _nopConfig.SchedultasksList.Contains(p.Name));

            query = query.OrderByDescending(t => t.Seconds);

            var tasks = query.ToList();
            return tasks;
        }
    }
}

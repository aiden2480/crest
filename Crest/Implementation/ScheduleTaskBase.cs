using Crest.Integration;
using Quartz;

namespace Crest.Implementation
{
    public abstract class ScheduleTaskBase : IScheduleTask
    {
		public abstract string Name { get; }
        //public virtual string Name
        //    => GetType().Name;

        public IJobDetail Job => JobBuilder.Create()
            .OfType(GetType())
            .WithIdentity(Name, "crest")
            .Build();

        public ITrigger Trigger => TriggerBuilder.Create()
            .WithIdentity(Name, "crest")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever())
            .Build();

        public abstract void OneTimeSetup();

        public abstract Task Execute(IJobExecutionContext context);
    }
}

using Crest.Integration;
using Moq;
using NUnit.Framework;
using Quartz;

namespace Crest.Test
{
	public class StateTests
	{
		static readonly string ProgramDataLocation = "mockcrest.programdata";

		static IEnumerable<object> TestValues() => new List<object>
		{
			new DateTime(2024, 02, 02),
			new List<int>() { 15, 16, 17 },
			new List<Guid>(),

			"exampleDefaultStringState",
			"anotherStringExample",

			2.7182f,
			3.14159,
			15,
		};

		[TestCaseSource(nameof(TestValues))]
		public void TestReturnsDefaultWhenStateIsNotSet<TState>(TState expectedDefaultValue)
		{
			// Arrange
			var task = GetMockScheduleTask(Guid.NewGuid().ToString(), "tasktype");

			// Act & Assert
			Assert.That(task.GetState(expectedDefaultValue), Is.EqualTo(expectedDefaultValue));
		}

		[TestCaseSource(nameof(TestValues))]
		public void TestReturnsSetValueWhenStateIsSet<TState>(TState setAndExpectedReturnValue)
		{
			// Arrange
			var task = GetMockScheduleTask(Guid.NewGuid().ToString(), "tasktype");

			// Act
			task.SetState(setAndExpectedReturnValue);

			// Assert
			Assert.That(task.GetState<TState>(), Is.EqualTo(setAndExpectedReturnValue));
		}

		[Test]
		public void TestReturnsDefaultWhenStateIsInvalid()
		{
			// Arrange
			var task = GetMockScheduleTask(Guid.NewGuid().ToString(), "tasktype");
			var expectedDefault = 15;

			// Act
			task.SetState("stringValue");

			// Assert
			Assert.That(task.GetState(expectedDefault), Is.EqualTo(expectedDefault));
		}

		#region Helpers
		
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			if (File.Exists(ProgramDataLocation))
			{
				File.Delete(ProgramDataLocation);
			}
		}

		static ScheduleTask<TerrainApprovalsTaskConfig> GetMockScheduleTask(string jobName, string jobGroup)
		{
			var mock = new Mock<ScheduleTask<TerrainApprovalsTaskConfig>>();
			var mockContext = GetMockContext(jobName, jobGroup);

			mock.SetupGet(m => m.StatePath).Returns(ProgramDataLocation);
			mock.SetupGet(m => m.Context).Returns(mockContext);

			return mock.Object;
		}

		static IJobExecutionContext GetMockContext(string jobName, string jobGroup)
		{
			var mockContext = new Mock<IJobExecutionContext>();
			var mockTrigger = new Mock<ITrigger>();
			var mockKey = new TriggerKey(jobName, jobGroup);

			mockTrigger.SetupGet(t => t.Key).Returns(mockKey);
			mockContext.SetupGet(c => c.Trigger).Returns(mockTrigger.Object);

			return mockContext.Object;
		}

		#endregion
	}
}

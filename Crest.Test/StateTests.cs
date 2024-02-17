using Crest.Integration;
using Crest.Test.Utilities;
using Moq;
using NUnit.Framework;

namespace Crest.Test;

public class StateTests : DeleteProgramDataBeforeTest
{
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
	
	static ScheduleTask<TerrainApprovalsTaskConfig> GetMockScheduleTask(string jobName, string jobGroup)
	{
		var mock = new Mock<ScheduleTask<TerrainApprovalsTaskConfig>>();
		var stateKey = $"{jobName}-{jobGroup}";

		mock.SetupGet(m => m.StatePath).Returns(ProgramDataLocation);
		mock.SetupGet(m => m.StateKey).Returns(stateKey);

		return mock.Object;
	}

	#endregion
}

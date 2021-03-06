What?
====

DillPickle is a slim Gherkin-compliant BDD story runner for .NET.

It has borrowed (or stolen?) most of its functionality from NBehave, but added the hooks I needed and some more stuff.

More info coming soon at http://mookid.dk/oncode/dillpickle

One day, maybe I'll tweet something as well... [@mookid8000][2]

Getting started
====
Create a Class Library that will act as your test project.

Include the DillPickle executable `dill.exe` with your favorite NuGet interface.

Create a feature file, e.g. `my_first.feature`. Put something like this in there:

	Feature: This is my first feature
		This is a textual description that will be output when the feature is run

		Scenario: Do some stuff
			Given I want to do some testing
			When I include the DillPickle package
			Then my happiness got increased by 10 %

Now, create a matching action steps file - its name should be an idiomatic C# class name derived from the feature file 
name. I.e. `my_first.feature` becomes `MyFirst`. Also, the class should be decorated with the `[ActionSteps]` attribute
- therefore:

	[ActionSteps]
	public class MyFirst
	{
		// we'll put something in here in a moment
	}


Now, build the project and go to a command prompt. `CD` your way into the output folder of your test project, e.g.

	CD c:\temp\TestingDillPicle\TestingDillPickle.Tests\bin\Debug

Invoke DillPickle and specify the test assembly and the feature files to run - e.g. like so:

	dill TestingDillPickle.Tests.dll ..\..\my_first.feature

This should result in a couple of YELLOW "pending" action steps being output to the console. Now, make `MyFirst` implement
`IDisposable` and add something like the following body to the `MyFirst` action steps class:

	VisualStudioAutomation visualStudioAutomation = new VisualStudioAutomation();
	HappinessMeasurement happinessBefore;

	public MyFirst()
	{
		visualStudioAutomation.StartVisualStudio();
	}

	[Given("I want to do some testing")]
	public void WantingToTestStuff()
	{
		visualStudioAutomation
			.CreateEmptySolution("SomeSolution")
			.CreateClassLibrary("SomeProject")
			.CreateClassLibrary("SomeProject.Tests");

		visualStudioAutomation.SetActiveProject("SomeProject.Tests");
	}

	[When("I include the $packageName package")]
	public void IncludePackage(string packageName)
	{
		happinessBefore = HappinessMeasurement.CurrentValue;

		visualStudioAutomation
			.GetActiveProject()
			.IncludePackage(packageName);
	}

	[Then("my happiness got increased by $percentage %")]
	public void AssertHappinessIncrease(double percentage)
	{
		var happinessNow = HappinessMeasurement.CurrentValue;
		var experiencedIncreaseInPercent = 100 * happinessNow / happinessBefore;

		if (experiencedIncreaseInPercent < percentage)
		{
			var msg = string.Format("Expected {0:0.0} % increase, but happiness only rose from {1} to {2}! ({3:0.0} %)",
				percentage, happinessBefore, happinessNow, experiencedIncreaseInPercent);

			throw new AssertionException(msg);
		}
	}

	public void Dispose()
	{
		visualStudioAutomation.ShutDownVisualStudio();
		visualStudioAutomation.Dispose();
	}

Now, if you add some empty stubs for `VisualStudioAutomation` and `HappinessMeasurement`, including a few methods and properties, then
it should be possible to compile & run the scenario. All steps should be green, except the last one (depending on the value returned by 
`HappinessMeasurement.CurrentValue` of course).

This demonstrates how testing can be done on a high level of abstraction, in a specification language that is understandable by
business people. It also demonstrates that _your job_ is to write this stuff, and then build the bridge between the action steps in the
feature file and the actual business logic. In my experience, this is done best with the action steps as mediators, and then some
custom built bridge that automates your application or whatever module you want to test.

Now, go do the actual implementation of `VisualStudioAutomation` and `HappinessMeasurement`... :)

License
====

DillPickle is licensed under [Apache License, Version 2.0][1]. Basically, this licens grants you the right to use DillPickle in every way you see fit.

[1]: http://www.apache.org/licenses/LICENSE-2.0.html
[2]: http://twitter.com/#!/mookid8000
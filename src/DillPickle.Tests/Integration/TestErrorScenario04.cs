using DillPickle.Framework;
using NUnit.Framework;

namespace DillPickle.Tests.Integration
{
    [TestFixture]
    public class TestErrorScenario04 : FixtureBase
    {
        [Test]
        public void WhyDoesntItStop()
        {
            var text = @"
Story: Forecaster Regression tests
	Scenario: PHM.SUP.01.02 Forecast of Production Units

		Given the following lus
			| Type				| Alias			| MinPower	| MaxPower  | RampUp	| RampDown	| ActivationMode	| ActivationType		| ObservableMin | ObservableMax | EstimatedMaxCapacity	| Efficiency	| Disturbance	| ProductionConsumptionCost	|
			| AnalogHydro		| REG_T1_001	| 50 kW		| 350 kW	| 30 s		| 30 s		| Auto				| AnalogueProduction	| 24.6			| 25			| 40000 kW				| 0.8			| 0 kW			| 1							|
			| AnalogHydro		| REG_T1_002	| 50 kW		| 350 kW	| 30 s		| 30 s		| Auto				| AnalogueProduction	| 24.6			| 25			| 40000 kW				| 0.8			| 0 kW			| 2							|
			| AnalogHydro		| REG_T1_003	| 50 kW		| 350 kW	| 30 s		| 30 s		| Auto				| AnalogueProduction	| 24.6			| 25			| 40000 kW				| 0.8			| 0 kW			| 4							|
			
		Given the following lu Pmin and Pmax forecast
			| Alias			| Pmin		| Pmax		|
			| REG_T1_001	| 100 kW	| 300 kW	| 
			| REG_T1_002	| 0 kW		| 200 kW	| 
			| REG_T1_003	| 50 kW		| 350 kW	| 
		
		Given the following owner settings
			| Alias			| BaseLoad	| EnableBaseLoad	| PriceUF	| PriceOF	|
			| REG_T1_001	| 0 kW		| False				| 0.0		| 0.0		|
			| REG_T1_002	| 0 kW		| False				| 0.0		| 0.0		|
			| REG_T1_003	| 100 kW	| True				| 0.0		| 0.0		|
	
		When we wait 310 s
		
		Then the following Forecast should have been made
			| Alias				| Pmin		| Pmax		| EMin		| EMax		| Lpk0	| Lpk1	| Qpk0	| Qpk1	| Qpk2	| Disturbance	|
			| RegressionTest	| 200 kW	| 600 kW	| 0 kWh		| 96000 kWh	| 0		| 0		| 500	| -500	| 2500	| 200 kW		|

";

            var program = new Program();

        }
    }
}
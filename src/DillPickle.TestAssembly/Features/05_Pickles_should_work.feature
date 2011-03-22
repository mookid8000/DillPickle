Feature: DillPickle should work
  As a duderino
  In order to stop execution when something fails
  I would like to be able to run tests that stop when an error is encountered
  
  Scenario: Should stop after a few steps
	Given something that works
		and something that works
	
	When something fails
	
	Then this should never be executed
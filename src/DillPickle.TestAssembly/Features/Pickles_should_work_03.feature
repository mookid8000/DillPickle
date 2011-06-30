@tag
Feature: DillPickle should work
  As a software developer with serious ambition
  I want to implement a calculator
  That just works
  
  @another_tag
  Scenario outline:
    Given I type <number1>
    When I press + followed by <number2>
      and I press =
    Then I see <number3> in the display
    
    Examples:
      | number1 | number2 | number3 |
      | 20      | 39      | 59      |
      | 50      | 50      | 100     |

  @yet_another_tag
  Scenario outline:
    Given I type <number1>
    When I press + followed by <number2>
      and I press =
    Then I see <number3> in the display
    
    Examples:
      | number1 | number2 | number3 |
      | 20      | 39      | 59      |
      | 50      | 50      | 100     |


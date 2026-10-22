Feature: WiFi Portal Sign Up Validation
    As a user
    I want to see validation messages when I enter invalid sign up details
    So that I know what to correct

    Scenario: Empty first name shows required validation
        Given I accept the cookie banner on the WiFi portal
        And I navigate to the sign up page
        When I clear the first name field and tab away
        Then I should see a first name validation error

    Scenario: Empty last name shows required validation
        Given I accept the cookie banner on the WiFi portal
        And I navigate to the sign up page
        When I clear the last name field and tab away
        Then I should see a last name validation error

    Scenario: Empty date of birth shows required validation
        Given I accept the cookie banner on the WiFi portal
        And I navigate to the sign up page
        When I clear the date of birth field and tab away
        Then I should see a date of birth validation error

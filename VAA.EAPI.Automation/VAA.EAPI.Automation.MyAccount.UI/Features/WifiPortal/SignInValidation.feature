Feature: WiFi Portal Sign In Validation
    As a user
    I want to see validation messages when I enter invalid sign in details
    So that I know what to correct

    Scenario: Empty email shows required validation
        Given I accept the cookie banner on the WiFi portal
        When I tab out of the email field without entering anything
        Then I should see an email validation error

    Scenario: Invalid email shows validation error
        Given I accept the cookie banner on the WiFi portal
        When I enter an invalid email address
        And I tab out of the email field
        Then I should see an email validation error

    Scenario: Empty password shows required validation
        Given I accept the cookie banner on the WiFi portal
        When I enter a valid email address
        And I clear the password field and tab away
        Then I should see a password validation error

    Scenario: Non-existent account shows error
        Given I accept the cookie banner on the WiFi portal
        When I enter a non-existent email and password
        And I click Log In
        Then I should see an account not found error

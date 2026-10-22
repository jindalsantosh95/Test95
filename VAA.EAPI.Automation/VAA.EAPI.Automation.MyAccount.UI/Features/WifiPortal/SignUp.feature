Feature: WiFi Portal Sign Up
    As a new customer
    I want to sign up on the WiFi portal
    So that I can connect to the in-flight internet

    Scenario: Adult sign up with OTP verification
        Given I accept the cookie banner on the WiFi portal
        And I navigate to the sign up page
        When I enter my first name, last name and date of birth as an adult
        And I click Next
        And I enter my email address
        And I submit the sign up form
        And I verify with the signup OTP code
        Then I should see the connected message after sign up

    Scenario: Under 18 sign up skips OTP
        Given I accept the cookie banner on the WiFi portal
        And I navigate to the sign up page
        When I enter my first name, last name and date of birth as under 18
        And I click Next
        And I click Connect Now
        Then I should see the connected message after sign up

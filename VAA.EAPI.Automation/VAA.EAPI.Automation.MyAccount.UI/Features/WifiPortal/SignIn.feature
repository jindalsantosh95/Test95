Feature: WiFi Portal Sign In
    As a Flying Club member
    I want to sign in to the WiFi portal
    So that I can connect to the in-flight internet

    Scenario: Sign in with OTP verification
        Given I accept the cookie banner on the WiFi portal
        And I have valid OTP sign-in credentials
        When I enter my email and password
        And I click Log In
        And I verify with the OTP code
        Then I should see the connected message

    Scenario: Sign in with email and no OTP
        Given I accept the cookie banner on the WiFi portal
        And I have a Gold loyalty account
        When I enter my email and password
        And I click Log In
        Then I should see the connected message

    Scenario: Sign in with loyalty account number and no OTP
        Given I accept the cookie banner on the WiFi portal
        And I have a Gold loyalty account
        When I enter my loyalty account number and password
        And I click Log In
        Then I should see the connected message

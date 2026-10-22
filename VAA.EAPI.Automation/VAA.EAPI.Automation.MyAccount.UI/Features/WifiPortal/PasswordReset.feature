Feature: WiFi Portal Password Reset
    As a Flying Club member
    I want to reset my password on the WiFi portal
    So that I can regain access to my account

    Scenario: Password reset with OTP verification
        Given I accept the cookie banner on the WiFi portal
        And I have a Gold loyalty account
        When I navigate to the password reset page
        And I enter my email for password reset
        And I submit the password reset email
        And I verify with the OTP code
        And I enter a new password
        And I confirm the new password
        And I click Change Password
        Then I should see the connected message

Feature: WiFi Portal Password Reset Validation
    As a user
    I want to see password validation rules update in real time
    So that I can create a valid password

    Scenario: Password too short fails length rule
        Given I am on the password reset entry screen
        When I enter "Ab1!" as the new password
        Then the length rule should not be met
        And the number rule should be met
        And the uppercase rule should be met
        And the lowercase rule should be met
        And the special character rule should be met

    Scenario: Password without number fails number rule
        Given I am on the password reset entry screen
        When I enter "Password!" as the new password
        Then the number rule should not be met
        And the length rule should be met

    Scenario: Password without uppercase fails uppercase rule
        Given I am on the password reset entry screen
        When I enter "password1!" as the new password
        Then the uppercase rule should not be met
        And the length rule should be met

    Scenario: Password without lowercase fails lowercase rule
        Given I am on the password reset entry screen
        When I enter "PASSWORD1!" as the new password
        Then the lowercase rule should not be met
        And the length rule should be met

    Scenario: Password with too many special characters fails special character rule
        Given I am on the password reset entry screen
        When I enter "Pa1!@#$%" as the new password
        Then the special character rule should not be met
        And the length rule should be met

    Scenario: Mismatched passwords fails match rule
        Given I am on the password reset entry screen
        When I enter "Password1!" as the new password
        And I enter "Different1!" as the confirm password
        Then the passwords match rule should not be met

    Scenario: All rules met with valid password
        Given I am on the password reset entry screen
        When I enter "Password1!" as the new password
        And I enter "Password1!" as the confirm password
        Then all password rules should be met

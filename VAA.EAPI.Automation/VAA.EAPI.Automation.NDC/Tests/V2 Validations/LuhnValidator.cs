

public static class LuhnValidator
{
    public static bool IsValidLuhn(string cardNumber, out string message)
    {
        // Check if the card number array is null or empty
        if (cardNumber == null || cardNumber.Length == 0)
        {
            message = "Card number cannot be null or empty.";
            return false;
        }

        int s1 = 0, s2 = 0;
        bool isOddPosition = true; // Start from the rightmost position

        // Process each character from the rightmost side
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(cardNumber[i]))
            {
                message = "Card number should contain only digits.";
                return false;
            }

            int digit = cardNumber[i] - '0';

            if (isOddPosition)
            {
                // Sum digits in non-doubled (odd) positions
                s1 += digit;
            }
            else
            {
                // Double the digit in even positions and subtract 9 if > 9
                int doubled = digit * 2;
                s2 += (doubled > 9) ? doubled - 9 : doubled;
            }

            // Toggle position flag for each step
            isOddPosition = !isOddPosition;
        }

        // Total sum of s1 and s2
        int totalSum = s1 + s2;

        // Check if the total modulo 10 is zero for Luhn validation
        if (totalSum % 10 == 0)
        {
            message = "Card number is valid.";
            return true;
        }
        else
        {
            message = "Card number is invalid.";
            return false;
        }
    }

}

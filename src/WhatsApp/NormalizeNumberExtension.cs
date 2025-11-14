namespace Devlooped.WhatsApp;

static class NormalizeNumberExtension
{
    extension(string number)
    {
        /// <summary>
        /// Normalizes a WhatsApp phone number by ensuring it does not start with a '+' and 
        /// has the right format for Argentina numbers.
        /// </summary>
        public string NormalizeNumber()
        {
            var result = number.TrimStart('+');

            // On the web, we don't get the 9 after 54 \o/
            // so for Argentina numbers, we need to remove the 9.
            if (result.StartsWith("549", StringComparison.Ordinal))
                result = "54" + number[3..];

            return result;
        }
    }
}

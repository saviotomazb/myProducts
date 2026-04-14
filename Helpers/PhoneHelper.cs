using System.Text.RegularExpressions;

namespace myProducts.Helpers
{
    public static class PhoneHelper
    {
        public static string? Clean(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            return Regex.Replace(phone, @"\D", "");
        }
    }
}

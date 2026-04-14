using Microsoft.AspNetCore.Mvc.Rendering;

namespace myProducts.Helpers
{
    public static class StateHelper
    {
        public static readonly string[] All =
        {
            "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA",
            "MT","MS","MG","PA","PB","PR","PE","PI","RJ","RN",
            "RS","RO","RR","SC","SP","SE","TO"
        };

        public static List<SelectListItem> GetSelectList()
        {
            return All
                .Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s
                })
                .Prepend(new SelectListItem { Value = "", Text = "Selecione" })
                .ToList();
        }

        public static bool IsValid(string? state)
        {
            if (string.IsNullOrWhiteSpace(state))
                return true;

            return All.Contains(state.ToUpperInvariant());
        }

        public static string? Normalize(string? state)
        {
            return string.IsNullOrWhiteSpace(state)
                ? null
                : state.Trim().ToUpperInvariant();
        }
    }
}

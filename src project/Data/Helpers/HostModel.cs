using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorServerMyMongo.Data.Helpers
{
    public class HostModel : PageModel
    {
        private readonly AppData appData;

        public HostModel(AppData appData)
        {
            this.appData = appData;
        }

        public void OnGet()
        {
            var theme = HttpContext.Request.Cookies["theme"];
            if (string.IsNullOrEmpty(theme))
                theme = "Dark";

            appData.Theme = theme;
        }
    }
}

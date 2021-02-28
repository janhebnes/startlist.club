using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Helpers
{
    public static class HtmlHelpers
    {

        public static string InUseCannotDeleteAlert()
        {
            var msg = Internationalization.GetText("In use", Internationalization.LanguageCode);
            var tooltip = Internationalization.GetText("Cannot be deleted", Internationalization.LanguageCode);
            return $"<span class='label label-warning' title='{tooltip}'>{msg}</span>";
        }

        public static string DoNotChangeSemanticsAlert()
        {
            var msg = Internationalization.GetText("Do not change the semantics or meaning of this item - it is already in use in training flights", Internationalization.LanguageCode);
            return $"<div class='alert alert-danger'>{msg}</div>";
        }

}
}
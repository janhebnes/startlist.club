<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="FlightJournal.Web.Models" %>
<%
if ((Model!=null) && (Model is string))
{
    var input = Model as String;
    if (!string.IsNullOrWhiteSpace(input) && input.Length > 40)
    {
        Response.Write(string.Format("{0} <span title=\"[...]{1}\">[...]</span>", input.Substring(0, 37), input.Substring(37)));
    }
    else
    {
        Response.Write(input);
    }
}
%>

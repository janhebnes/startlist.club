<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="FlightJournal.Web.Models" %>
<%
if ((Model!=null) && (Model is StartType))
{
    Response.Write(((StartType)Model).ShortName);
}
%>

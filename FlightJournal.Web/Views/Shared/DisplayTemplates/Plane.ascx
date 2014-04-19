<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="FlightJournal.Web.Models" %>
<%
if ((Model!=null) && (Model is Plane))
{
    Response.Write(((Plane)Model).Registration);
}
%>

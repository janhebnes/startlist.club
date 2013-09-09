<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="FlightLog.Models" %>
<%
if ((Model!=null) && (Model is StartType))
{
    Response.Write(((StartType)Model).ShortName);
}
%>

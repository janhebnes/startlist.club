<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%= (Model!=null)?Html.Encode(String.Format("{0:d}", Model)):string.Empty%> 
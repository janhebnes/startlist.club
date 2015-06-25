using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Extensions
{
    public class ExtendedSelectListItem : SelectListItem
    {
        public object htmlAttributes { get; set; }
    }

    /// <summary>
    /// http://stackoverflow.com/questions/1829566/asp-net-mvc-putting-custom-attributes-into-option-tag-in-select-list
    /// http://stackoverflow.com/questions/7536631/adding-html-class-tag-under-option-in-html-dropdownlist/7537628#7537628
    /// </summary>
    public static partial class HtmlHelperExtension
    {
        public static MvcHtmlString ExtendedDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, IEnumerable<ExtendedSelectListItem> selectList)
        {
            return SelectInternal(htmlHelper, null, ExpressionHelper.GetExpressionText(expression), selectList, false /* allowMultiple */, HtmlHelper.AnonymousObjectToHtmlAttributes(new { }));
        }

        public static MvcHtmlString ExtendedDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, IEnumerable<ExtendedSelectListItem> selectList,
            string optionLabel)
        {
            return SelectInternal(htmlHelper, optionLabel, ExpressionHelper.GetExpressionText(expression), selectList, false /* allowMultiple */, HtmlHelper.AnonymousObjectToHtmlAttributes(new {}));    
        }

        public static MvcHtmlString ExtendedDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<ExtendedSelectListItem> selectList, string optionLabel, object htmlAttributes)
        {
            return SelectInternal(htmlHelper, optionLabel, ExpressionHelper.GetExpressionText(expression), selectList, false /* allowMultiple */, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        private static MvcHtmlString SelectInternal(this HtmlHelper htmlHelper, string optionLabel, string name, IEnumerable<ExtendedSelectListItem> selectList, bool allowMultiple, IDictionary<string, object> htmlAttributes)
        {
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (String.IsNullOrEmpty(fullName))
                throw new ArgumentException("No name");

            if (selectList == null)
                throw new ArgumentException("No selectlist");

            object defaultValue = (allowMultiple) ? GetModelStateValue(htmlHelper, fullName, typeof(string[])) : GetModelStateValue(htmlHelper, fullName, typeof(string));

            // If we haven't already used ViewData to get the entire list of items then we need to
            // use the ViewData-supplied value before using the parameter-supplied value.
            if (defaultValue == null)
                defaultValue = htmlHelper.ViewData.Eval(fullName);

            if (defaultValue != null)
            {
                IEnumerable defaultValues = (allowMultiple) ? defaultValue as IEnumerable : new[] { defaultValue };
                IEnumerable<string> values = from object value in defaultValues select Convert.ToString(value, CultureInfo.CurrentCulture);
                HashSet<string> selectedValues = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
                List<ExtendedSelectListItem> newSelectList = new List<ExtendedSelectListItem>();

                foreach (ExtendedSelectListItem item in selectList)
                {
                    item.Selected = (item.Value != null) ? selectedValues.Contains(item.Value) : selectedValues.Contains(item.Text);
                    newSelectList.Add(item);
                }
                selectList = newSelectList;
            }

            // Convert each ListItem to an <option> tag
            StringBuilder listItemBuilder = new StringBuilder();

            // Make optionLabel the first item that gets rendered.
            if (optionLabel != null)
                listItemBuilder.Append(ListItemToOption(new ExtendedSelectListItem() { Text = optionLabel, Value = String.Empty, Selected = false }));

            foreach (ExtendedSelectListItem item in selectList)
            {
                listItemBuilder.Append(ListItemToOption(item));
            }

            TagBuilder tagBuilder = new TagBuilder("select")
            {
                InnerHtml = listItemBuilder.ToString()
            };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", fullName, true /* replaceExisting */);
            tagBuilder.GenerateId(fullName);
            if (allowMultiple)
                tagBuilder.MergeAttribute("multiple", "multiple");

            // If there are any errors for a named field, we add the css attribute.
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullName, out modelState))
            {
                if (modelState.Errors.Count > 0)
                {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            tagBuilder.MergeAttributes(htmlHelper.GetUnobtrusiveValidationAttributes(name));

            return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
        }

        internal static string ListItemToOption(ExtendedSelectListItem item)
        {
            TagBuilder builder = new TagBuilder("option")
            {
                InnerHtml = HttpUtility.HtmlEncode(item.Text)
            };
            if (item.Value != null)
            {
                builder.Attributes["value"] = item.Value;
            }
            if (item.Selected)
            {
                builder.Attributes["selected"] = "selected";
            }
            builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(item.htmlAttributes));
            return builder.ToString(TagRenderMode.Normal);
        }
        /// <summary>
        /// http://stackoverflow.com/questions/6967148/not-able-to-access-getmodelstatevalue-in-custom-control-in-asp-net-mvc2
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="key"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        static object GetModelStateValue(HtmlHelper htmlHelper, string key, Type destinationType)
        {
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(key, out modelState))
            {
                if (modelState.Value != null)
                {
                    return modelState.Value.ConvertTo(destinationType, null /* culture */);
                }
            }
            return null;
        }

    }
}
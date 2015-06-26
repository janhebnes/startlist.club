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
            {
                throw new ArgumentException("no name");
            }

            bool usedViewData = false;

            // If we got a null selectList, try to use ViewData to get the list of items.
            if (selectList == null)
            {
                selectList = (IEnumerable<ExtendedSelectListItem>) GetSelectData(htmlHelper, name);
                usedViewData = true;
            }

            object defaultValue = (allowMultiple) ? htmlHelper.GetModelStateValue(fullName, typeof(string[])) : htmlHelper.GetModelStateValue(fullName, typeof(string));

            // If we haven't already used ViewData to get the entire list of items then we need to
            // use the ViewData-supplied value before using the parameter-supplied value.
            if (defaultValue == null && !String.IsNullOrEmpty(name))
            {
                if (!usedViewData)
                {
                    defaultValue = htmlHelper.ViewData.Eval(name);
                }
            }

            // Convert each ListItem to an <option> tag
            StringBuilder listItemBuilder = new StringBuilder();

            // Make optionLabel the first item that gets rendered.
            if (optionLabel != null)
                listItemBuilder.Append(ListItemToOption(new ExtendedSelectListItem() { Text = optionLabel, Value = string.Empty, Selected = false }));

            foreach (ExtendedSelectListItem item in selectList)
            if (defaultValue != null)
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
            {
                tagBuilder.MergeAttribute("multiple", "multiple");
            }

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
        /// private /src/System.Web.Mvc/System.Web.Mvc/HtmlHelper.cs copied here
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="key"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        private static object GetModelStateValue(this HtmlHelper htmlHelper, string key, Type destinationType)
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

        /// <summary>
        /// private /src/System.Web.Mvc/Html/SelectExtensions.cs copied here
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IEnumerable<SelectListItem> GetSelectData(HtmlHelper htmlHelper, string name)
        {
            object o = null;
            if (htmlHelper.ViewData != null)
            {
                o = htmlHelper.ViewData.Eval(name);
            }
            if (o == null)
            {
                throw new InvalidOperationException("Missing IEnumerable<SelectListItem>");
            }
            IEnumerable<SelectListItem> selectList = o as IEnumerable<SelectListItem>;
            if (selectList == null)
            {
                throw new InvalidOperationException("WrongSelectDataType IEnumerable<SelectListItem>");
            }
            return selectList;
        }

        

    }
}
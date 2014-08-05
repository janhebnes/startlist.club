using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FakeItEasy;
using FlightJournal.Web;
using FlightJournal.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlightJournal.Tests
{
    [TestClass]
    public class RouteFilters
    {
        [TestInitialize]
        public void Prepare()
        {
            RouteTable.Routes.Clear();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_defaults_to_report_controller_index_action()
        {
            should_return_expected_controller_and_action("~/", "Report", "Index");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_defaults_to_no_club_filter()
        {
            should_return_expected_club("~/", null);
        }
        
        [TestMethod]
        [TestCategory("Routes")]
        public void starttype_default_url_maps_to_starttype_controller_index_action()
        {
            should_return_expected_controller_and_action("~/StartType", "StartType", "Index");
        }
        

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_any_date_part_should_send_to_report_controller_index_action()
        {
            should_return_expected_controller_and_action("~/2014", "Report", "Index");
            should_return_expected_controller_and_action("~/2014-03", "Report", "Index");
            should_return_expected_controller_and_action("~/2014-03-22", "Report", "Index");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_date_as_year_should_set_routedata_date_to_incomming_path_information()
        {
            var routeData = GetRouteData("~/2014");
            Assert.IsNotNull(routeData.Values["date"]);
            Assert.AreEqual(routeData.Values["date"].ToString().ToLower(), "2014");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_date_as_year_month_should_set_routedata_date_to_incomming_path_information()
        {
            var routeData1 = GetRouteData("~/2014-03");
            Assert.IsNotNull(routeData1.Values["date"]);
            Assert.AreEqual(routeData1.Values["date"].ToString().ToLower(), "2014-03");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_date_as_year_month_day_should_set_routedata_date_to_incomming_path_information()
        {
            var routeData1 = GetRouteData("~/2014-03-22");
            Assert.IsNotNull(routeData1.Values["date"]);
            Assert.AreEqual(routeData1.Values["date"].ToString().ToLower(), "2014-03-22");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_a_club_should_send_to_report_controller_and_index_action()
        {
            should_return_expected_controller_and_action("~/ØSF", "Report", "Index");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_a_club_should_set_currentclub_to_matching_shortname()
        {
            // HACK: This is failing because of the Mockup Framework Issues, the actual code works
            should_return_expected_club("~/AASVK/", "AASVK");
            should_return_expected_club("~/ØSF/", "ØSF");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_a_non_existing_club_should_set_currentclub_to_non()
        {
            should_return_expected_club("~/MISSINGCLUB", null);
        }
        
        [TestMethod]
        [TestCategory("Routes")]
        public void Root_and_a_non_existing_club_should_send_to_non_existing_controller_and_index_action()
        {
            // Will send through the route pipeline and land on the controller default route (missingclub does not exist and will result in a 404)
            should_return_expected_controller_and_action("~/MISSINGCLUB", "missingclub", "Index");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_any_date_part_should_match_currentclub()
        {
            should_return_expected_club("~/ØSF/2014", "ØSF");
            should_return_expected_club("~/AASVK/2014-03", "AASVK");
            should_return_expected_club("~/AASVK/2014-06-21", "AASVK");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void RouteConstrint_Club_validator_test()
        {
            var validator = new ClubValidator();

            // Valid Country
            Assert.IsTrue(validator.IsValid("ØSF"), "ØSF should be valid");

            // Invalid Country
            Assert.IsFalse(validator.IsValid("OST"), "OSF should not validate");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void RouteConstrint_DateRoute_validator_test()
        {
            var validator = new DatePathValidator();

            // Valid Country
            Assert.IsTrue(validator.IsValid("2014"), "2014 is a valid format");
            Assert.IsTrue(validator.IsValid("2014-03"), "2014-03 is a valid format");
            Assert.IsTrue(validator.IsValid("2014-03-03"), "2014-03-03 is a valid format");

            // Invalid formats
            Assert.IsFalse(validator.IsValid("ØSF"), "ØSF should not be valid");
            Assert.IsFalse(validator.IsValid("201403"), "201403 should not be valid");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_date_as_year_date_part_should_send_to_report_controller_index_action()
        {
            should_return_expected_controller_and_action("~/ØSF/2014", "Report", "Index");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_date_as_year_month_should_send_to_report_controller_index_action()
        {
            should_return_expected_controller_and_action("~/ØSF/2014-03", "Report", "Index");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_date_as_year_month_day_should_send_to_report_controller_index_action()
        {
            should_return_expected_controller_and_action("~/ØSF/2014-03-22", "Report", "Index");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_date_as_year_should_set_routedata_date_to_incomming_path_information()
        {
            var routeData = GetRouteData("~/ØSF/2014");
            Assert.IsNotNull(routeData.Values["date"]);
            Assert.AreEqual(routeData.Values["date"].ToString().ToLower(), "2014");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_date_as_year_should_set_routedata_club_to_incomming_path_information()
        {
            var routeData = GetRouteData("~/ØSF/2014");
            Assert.IsNotNull(routeData.Values["club"]);
            Assert.AreEqual(routeData.Values["club"].ToString().ToLower(), "øsf");
        }

        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_date_as_year_month_day_should_set_routedata_date_to_incomming_path_information()
        {
            var routeData1 = GetRouteData("~/ØSF/2014-03-22");
            Assert.IsNotNull(routeData1.Values["date"]);
            Assert.AreEqual(routeData1.Values["date"].ToString().ToLower(), "2014-03-22");
        }
        
        [TestMethod]
        [TestCategory("Routes")]
        public void Root_club_and_date_as_year_month_day_should_set_routedata_club_to_incomming_path_information()
        {
            var routeData1 = GetRouteData("~/ØSF/2014-03-22");
            Assert.IsNotNull(routeData1.Values["club"]);
            Assert.AreEqual(routeData1.Values["club"].ToString().ToLower(), "øsf");
        }

        public RouteData GetRouteData(string path)
        {
            var httpContext = A.Fake<HttpContextBase>();
            A.CallTo(() => httpContext.Request.AppRelativeCurrentExecutionFilePath).Returns(path);
            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            Assert.IsNotNull(routeData);
            return routeData;
        }

        public void should_return_expected_controller_and_action(string path, string expectedController, string expectedAction)
        {
            var httpContext = A.Fake<HttpContextBase>();
            A.CallTo(() => httpContext.Request.AppRelativeCurrentExecutionFilePath).Returns(path);
            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            Assert.IsNotNull(routeData);
            Assert.AreEqual(expectedController.ToLower(), routeData.Values["controller"].ToString().ToLower());
            Assert.AreEqual(expectedAction.ToLower(), routeData.Values["action"].ToString().ToLower());
        }

        public void should_return_expected_club(string path, string expectedClub)
        {
            var httpContext = A.Fake<HttpContextBase>();
            A.CallTo(() => httpContext.Request.AppRelativeCurrentExecutionFilePath).Returns(path);
            A.CallTo(() => httpContext.Request.Url).Returns(new Uri("http://localhost/" + path.Replace("~/", string.Empty)));
            var routeData = RouteTable.Routes.GetRouteData(httpContext);
            Assert.IsNotNull(routeData);
            var currentClub = ClubController.GetCurrentClub(httpContext, routeData);
            Assert.IsNotNull(currentClub);
            Assert.AreEqual(expectedClub, currentClub.ShortName);
        }

        /// <summary>
        /// This method is keeped for learning purposes 
        /// - one should not try to validate content by path in MVC Unit Testing those are to different tests (Actions vs Filters)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="expectedContent"></param>
        public void should_return_expected_content(string path, string expectedContent)
        {
            throw new NotImplementedException("This method is keeped for learning purposes - one should not try to validate content by path in MVC Unit Testing those are to different tests (Actions vs Filters)");

            #region (Learning #0 About Testing MVC) one should not try to validate content by path in MVC Unit Testing those are to different tests (Actions vs Filters)
            //// We could have invoked Controller and Action and Used the ViewResult to accomplish this validation 
            //// But I found this nice argument as to why not go down that road. 
            //// http://stackoverflow.com/questions/1145296/controlleractioninvoker-to-invoke-action-with-paramters

            //// You shouldn't use the ControllerActionInvoker from within your unit tests. What are you actually trying to accomplish?
            //// If you're trying to test the behavior of your actions, just call them directly (they are just regular methods). 
            //// If you're trying to test the behavior of your filters, create a mock context for the filter and call its OnXxx() method.
            
            //// Why? 
            //// Because you're testing the MVC infrastructure at that point, which is Microsoft-owned code. 
            //// Normally when unit testing MVC applications, you only test the code that you have written, and you do this in isolation from any other parts of the infrastructure. 
            //// This allows you to track down problems in that code base easily. Testing the infrastructure - e.g. the interaction between your code and third-party code - is normally an integration test. 
            //// Of course you can test the invoker if you wish, but unit tests might break between different versions of MVC, and you might go crazy. :)

            ////var httpContext = A.Fake<HttpContextBase>();
            ////A.CallTo(() => httpContext.Request.AppRelativeCurrentExecutionFilePath).Returns(path);
            ////var routeData = RouteTable.Routes.GetRouteData(httpContext);
            ////Assert.IsNotNull(routeData);
            ////var controller = routeData.Values["controller"];
            ////var action = routeData.Values["action"];
            //// ## Arrange for this to be done dynamically, but found good argument for not to continue down this path
            ////HomeController controller = new HomeController();
            ////ViewResult result = controller.Administration() as ViewResult;
            ////Assert.IsNotNull(result);
            #endregion

            #region (Learning #1 about using FakeItEasy) Trying to use the Fake response through Fake httpContext gives a fake object back... dooh!

            ////var sb = new StringBuilder();
            ////var httpContext = A.Fake<HttpContextBase>();
            ////A.CallTo(() => httpContext.Request.AppRelativeCurrentExecutionFilePath).Returns(path);
            ////var response = A.Fake<HttpResponseBase>();
            ////A.CallTo(() => response.Write(A<string>.Ignored)).Invokes((string x) => sb.Append(x));
            ////// Throws exception... 
            ////var stream = new StreamReader(httpContext.Response.OutputStream);
            ////var output = stream.ReadToEnd();
            ////Assert.IsTrue(output.Contains(expectedContent));

            #endregion

            #region (Learning #2 about using any kind of Mockup Framework) Trying to use a mockHttpContext with request and response to request actual content (fails - because everything is fake...)

            ////var sb = new StringBuilder();

            ////var formCollection = new NameValueCollection();
            ////formCollection.Add("MyPostedData", "Boo");

            ////var request = A.Fake<HttpRequestBase>();
            ////A.CallTo(() => request.HttpMethod).Returns("POST");
            ////A.CallTo(() => request.Headers).Returns(new NameValueCollection());
            ////A.CallTo(() => request.Form).Returns(formCollection);
            ////A.CallTo(() => request.QueryString).Returns(new NameValueCollection());
            ////A.CallTo(() => request.Path).Returns(path);

            ////var response = A.Fake<HttpResponseBase>();
            ////A.CallTo(() => response.Write(A<string>.Ignored)).Invokes((string x) => sb.Append(x));

            ////var mockHttpContext = A.Fake<HttpContextBase>();
            ////A.CallTo(() => mockHttpContext.Request).Returns(request);
            ////A.CallTo(() => mockHttpContext.Response).Returns(response);
            ////Assert.IsTrue(sb.ToString().Contains(expectedContent));

            #endregion

        }

    }  
}

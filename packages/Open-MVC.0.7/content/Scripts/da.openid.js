(function() {
  var OAuthPopUp, VKAuth;
  $(document).ready(function() {
    var item, linkPhrase, links;
    console.log("ready");
    item = null;
    links = null;
    linkPhrase = $("#oa_action span").text();
    $(".logon-button").click(function(e) {
      var servName, servUrl;
      servName = $(this).attr("data-serv-name");
      servUrl = null;
      if (servName === "openid") {
        servUrl = "/Account/OpenId?ServiceUrl=" + ($("#oi_prefix").text() + $("#oi_input").val() + $("#oi_postfix").text()) + "&returnUrl=" + ($("#return_url").val());
      } else {
        servUrl = $(this).attr("data-serv-url");
      }
      OAuthPopUp(servUrl, 600, 400);
      return e.preventDefault();
    });
    $(".oa-button").click(function(e) {
      var link, oiUrl, r, servName;
      e.preventDefault();
      if (item) {
        item.removeClass("oa-selected");
      }
      if (links) {
        links.hide();
      }
      item = $(this).addClass("oa-selected");
      oiUrl = item.attr("data-oi-url");
      if (oiUrl) {
        r = (" " + oiUrl + " ").split("$");
        $("#oi_prefix").text($.trim(r[0]));
        $("#oi_postfix").text($.trim(r[1]));
        links = $("#oi_action");
      } else {
        servName = item.attr("data-serv-name");
        links = $("#oa_action, #oa_action [data-serv-name = " + servName + "]");
        link = $(links[1]);
        $("#oa_action span").text(linkPhrase.replace("$", link.attr("data-serv-caption")));
      }
      return links.show();
    });
    $("#vk_button").click(function() {
      return VK.Auth.login(VKAuth);
    });
    return window.setTimeout((function() {
      console.log("vkontakte most wrecked oauth service ever concieved");
      return VK.init({
        apiId: $("#vk_app_id").val()
      });
    }), 1000);
  });
  OAuthPopUp = function(url, width, height) {
    var h, l, t, w;
    if (navigator.userAgent.toLowerCase().indexOf("opera") !== -1) {
      w = document.body.offsetWidth;
      h = document.body.offsetHeight;
    } else {
      w = screen.width;
      h = screen.height;
    }
    t = Math.floor((h - height) / 2 - 14);
    l = Math.floor((w - width) / 2 - 5);
    url += "&extWindow=true";
    return window.open(url, "", "status=no,scrollbars=yes,resizable=yes,width=" + width + ",height=" + height + ",top=" + t + ",left=" + l);
  };
  VKAuth = function(response) {
    var data;
    if (response.session) {
      data = response.session;
      data.ServiceName = "VKontakte";
      return $.post("/Account/OAuth", data, function(ret) {
        if (ret.success) {
          return window.location = $("#return_url").val();
        }
      });
    }
  };
}).call(this);

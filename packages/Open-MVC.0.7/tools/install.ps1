param($installPath, $toolsPath, $package, $project)

Write-Output("open-mvc nuget package contet successfully installed!")
Write-Output("Script is applying to source files...")
Write-Output("In the case of the errors during scripts applying please visit nuget web page.")

#$path = "C:\dev\tests\MvcApplication4"#$installPath
#$namespace = "MvcApplication4"#$project.Object.Namespace
$mvcVersion = 4

$path = [System.IO.Path]::GetDirectoryName($project.FullName) 
$namespace = $project.Properties.Item("RootNamespace").Value.ToString()
$mvcVersion = 0

	if ($project.Object.References.Find("System.Web.Mvc").Version -eq "3.0.0.0")
	{
		$mvcVersion = 3
	}
	
	if ($project.Object.References.Find("System.Web.Mvc").Version -eq "4.0.0.0")
	{
		$mvcVersion = 4
	}
	
	if ($mvcVersion -eq 0)
	{
		Write-Error("MVC version of project neither 3 nor 4. Can't proceed")
		
		return
	}


	$fullPath = $path + "\Controllers\AccountController.cs"

	if (![System.IO.File]::Exists($fullPath)){
		Write-Error("1. Can't find file " + $fullPath + "! You have to manually derive your AccountController from OAuthAccountContoller.")
	} 
	else
	{
		$text = [System.IO.File]::ReadAllText($fullPath)
		
		$regexp = "OAuthAccountController"
		$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
		
		if ($match.Success)
		{
			Write-Warning("1. Looks like OAuthAccountController already inserted into AccountController file")
		}
		else
		{
			$regexp = "public[`\s]+class[`\s]+AccountController[`\s]+:[`\s]+Controller[`\s]`*`{"
			$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
			$modify = "public class AccountController : DataAvail.Mvc.Account.OAuthAccountController`n`t`{`n`t`t"
			if ($mvcVersion -eq 4)
			{
				$modify = $modify + "protected override string GetErrorCodeFromString(MembershipCreateStatus createStatus)`n`t`t{`n`t`t`t`treturn ErrorCodeToString(createStatus);`n`t`t`}`n"
				$modify = $modify + "`n`t`t[AllowAnonymous]`n`t`tpublic override ActionResult OpenId(string serviceUrl){return base.OpenId(serviceUrl);}`n"
				$modify = $modify + "`n`t`t[AllowAnonymous]`n`t`tpublic override ActionResult OAuth(string serviceName){return base.OAuth(serviceName);}"
			}
			
			
			if ($match.Success)
			{
				$s = [System.Text.RegularExpressions.Regex]::Replace($text, $regexp, $modify)

				$s = [System.Text.RegularExpressions.Regex]::Replace($s, "public[`\s]+ActionResult[`\s]+LogOn`\(`\)[`\s]`*`{",
				    "public ActionResult LogOn()`n`t`t{`n`t`t`tbase.OAuthBeforeLogOn();`n")
					
				$s | Set-Content -path $fullPath
				
				Write-Output("1. AccountController successfully derived from OAuthAccountController.")
			}
			else
			{
				Write-Error("1. Can't find AccountController class in " + $fullPath + "! You have to manually derive your AccountController from OAuthAccountContoller.")
			}
		}
	}


	$fullPath = $path + "\Views\Shared\_Layout.cshtml"

	if (![System.IO.File]::Exists($fullPath)){
		Write-Error("2. Can't find file " + $fullPath + "! You have to manually add da.openid.css to your layout file")
	} 
	else
	{
		$text = [System.IO.File]::ReadAllText($fullPath)
		
		$regexp = "~/Content/da.openid.css"
		$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
		
		if ($match.Success)
		{
			Write-Warning("2. Looks like da.openid.css already inserted into layout file")
		}
		else
		{
			$regexp = "</title>"
			$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
			
			if ($match.Success)
			{
				$text = [System.IO.File]::ReadAllText($fullPath);

				$s = [System.Text.RegularExpressions.Regex]::Replace($text, "</title>", "</title>`n`t`t<link href=`"@Url.Content`(`"~/Content/da.openid.css`"`)`" rel=`"stylesheet`" type=`"text`/css`" />")

				$s | Set-Content -path $fullPath
				
				Write-Output("2. da.openid.css successfully added into layout file.")
			}
			else
			{
				Write-Error("2. Can't add da.openid.css to " + $fullPath + "! You have to add da.openid.css into layout file manually.")
			}
		}
	}

	$fullPath = $path + "\Views\Account\LogOn.cshtml"

	if (![System.IO.File]::Exists($fullPath)){
		Write-Error("3. Can't find file " + $fullPath + "! You have to manually add @Html.Partial(`"_LogOnOAuth`"); to your LogOn.cshtml file.")
	} 
	else
	{
		$text = [System.IO.File]::ReadAllText($fullPath)
		
		$regexp = "_LogOnOAuth"
		$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
		
		if ($match.Success)
		{
			Write-Warning("3. Looks like _LogOnOAuth already inserted into the LogOn.cshtml file")
		}
		else
		{
			$regexp = "`}[`\s`\n]*`$`$"
			$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
			
			if ($match.Success)
			{
				$s = [System.Text.RegularExpressions.Regex]::Replace($text, $regexp, "`t@Html.Partial`(`"_LogOnOAuth`"`);`n`}")

				$s | Set-Content -path $fullPath
				
				Write-Output("3. _LogOnOAuth successfully added to your LogOn.cshtml file.")
			}
			else
			{
				Write-Error("3. Can't add _LogOnOAuth partial view to your logon view! You have to manually add @Html.Partial(`"_LogOnOAuth`"); to your LogOn.cshtml file.")
			}
		}
	}

	$fullPath = $path + "\Views\Shared\_LogOnPartial.cshtml"

	if (![System.IO.File]::Exists($fullPath)){
		Write-Error("4. Can't find file " + $fullPath + "! You have to manually add @Html.Partial(`"_LogInOAuth`"); to your _LogOnPartial.cshtml file.")
	} 
	else
	{
		$text = [System.IO.File]::ReadAllText($fullPath)
		
		$regexp = "_LogInOAuth"
		$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
		
		if ($match.Success)
		{
			Write-Warning("4. Looks like _LogInOAuth already inserted into the _LogOnPartial.cshtml file")
		}
		else
		{
			
			$regexp = "`<text`>Welcome.+!"
			$modify = "<text>Welcome @Html.Partial`(`"_LogInOAuth`"`) !"
			if ($mvcVersion -eq 4)
			{
				$regexp = "Hello.+!"
				$modify = "Hello, @Html.Partial`(`"_LogInOAuth`"`) !"
			}
			
			$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
			
			if ($match.Success)
			{
				$s = [System.Text.RegularExpressions.Regex]::Replace($text, $regexp, $modify);

				$s | Set-Content -path $fullPath
				
				Write-Output("4. _LogInOAuth successfully added to your _LogOnPartial.cshtml file.")
			}
			else
			{
				Write-Error("4. Can't add _LogInOAuth partial view to your logon view! You have to manually add @Html.Partial(`"_LogInOAuth`"); to your _LogOnPartial.cshtml file")
			}
		}
	}

	$fullPath = $path + "\Web.config"

	if (![System.IO.File]::Exists($fullPath)){
		Write-Error("5. Can't find file " + $fullPath + "! You have to manually inherits your default profile provider from " + $namespace + ".Account")
	} 
	else
	{
		$text = [System.IO.File]::ReadAllText($fullPath)
		
		$regexp = "AccountProfile"
		$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
		
		if ($match.Success)
		{
			Write-Warning("5. Looks like AccountProfile already inserted into the Web.config file")
		}
		else
		{
			#$regexp = "defaultProvider=`"DefaultProfileProvider`""
			$regexp = "`<profile"
			$match = [System.Text.RegularExpressions.Regex]::Match($text, $regexp)
			
			if ($match.Success)
			{
				$s = [System.Text.RegularExpressions.Regex]::Replace($text, $regexp, "`<profile inherits=`""+$namespace+".AccountProfile`"")

				$s | Set-Content -path $fullPath
				
				Write-Output("5. DefaultProfileProvider successfully inherited from  AccountProfile.")
			}
			else
			{
				Write-Error("5. Can't inherits DefaultProfileProvider from  AccountProfile! You have to manually inherits your default profile provider from " + $namespace + ".AccountProfile in your Web.Config file")
			}
		}
	}
	
	Write-Output("open-mvc installed!")



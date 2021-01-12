@echo off 
rem --------------------------
rem - REQUIRED - INSTALL NuGet packages\Gettext.Tools.0.19.8.1 (is included in test project for simple package restore)
rem -------------------------- 

rem 1) Generate inputfiles list used by xgettext
echo Building list of files...

goto modelC

:modelA
rem MODEL A (absolute path)
dir FlightJournal.Web\*.cs /S /B > translation-inputfiles.txt
dir FlightJournal.Web\*.cshtml /S /B >> translation-inputfiles.txt
goto start

:modelB
rem MODEL B Specific folders - removing obj and the like (absolute path) 
dir FlightJournal.Web\Controllers\*.cs /S /B > translation-inputfiles.txt
dir FlightJournal.Web\Models\*.cs /S /B >> translation-inputfiles.txt
dir FlightJournal.Web\Views\*.cs /S /B >> translation-inputfiles.txt
dir FlightJournal.Web\Views\*.cshtml /S /B >> translation-inputfiles.txt
goto start

:modelC
rem MODEL C Relative paths - but without obj folders and package folders
@echo on>translation-inputfiles.txt
@echo off 
setlocal EnableDelayedExpansion
for /L %%n in (1 1 500) do if "!__cd__:~%%n,1!" neq "" set /a "len=%%n+1"
setlocal DisableDelayedExpansion
for /r . %%g in (*.cs, *.cshtml) do (
  set "absPath=%%g"

  echo "%%g" |findstr /i "\obj\ \packages\ Samples">nul ||(
    setlocal EnableDelayedExpansion
    set "relPath=!absPath:~%len%!"
    echo(!relPath! >> translation-inputfiles.txt
    endlocal
  )
)

goto start


:start

rem Create a new .pot from source, place it in the Language folder, and merge with the existing .po file
echo Regenerating FlightJournal.Web\Translations\messages.pot po Template file

REM _ is for 'regular' use of _("SomeString"), __ is for use in Javascript and html attributes (where an extra enclosing quote is required, as xgettext apparently does not iside strings (which makes sense) )
packages\Gettext.Tools.0.19.8.1\tools\bin\xgettext.exe -k -k_ -k__  -kLocalizedDisplayName --msgid-bugs-address=jan.hebnes@gmail.com --package-name=startlist.club --from-code=UTF-8 -L C# -o FlightJournal.Web\Translations\messages.pot -f translation-inputfiles.txt

echo Updating Translations\en\LC_MESSAGES\messages.po with po template
packages\Gettext.Tools.0.19.8.1\tools\bin\msgmerge.exe --backup=none --lang=en -U FlightJournal.Web\Translations\en\LC_MESSAGES\messages.po FlightJournal.Web\Translations\messages.pot 

echo Updating Translations\da\LC_MESSAGES\messages.po with po template
packages\Gettext.Tools.0.19.8.1\tools\bin\msgmerge.exe --backup=none --lang=da -U FlightJournal.Web\Translations\da\LC_MESSAGES\messages.po FlightJournal.Web\Translations\messages.pot  

echo Updating Translations\no\LC_MESSAGES\messages.po with po template
packages\Gettext.Tools.0.19.8.1\tools\bin\msgmerge.exe --backup=none --lang=no -U FlightJournal.Web\Translations\no\LC_MESSAGES\messages.po FlightJournal.Web\Translations\messages.pot  

echo Updating Translations\sv\LC_MESSAGES\messages.po with po template
packages\Gettext.Tools.0.19.8.1\tools\bin\msgmerge.exe --backup=none --lang=sv -U FlightJournal.Web\Translations\sv\LC_MESSAGES\messages.po FlightJournal.Web\Translations\messages.pot  

rem begin new language by copying 
rem 		FlightJournal.Web\Translations\messages.pot 
rem 	to 	FlightJournal.Web\Translations\LANG\LC_MESSAGES\messages.po
rem 	and add line for msgmerge.exe that handles merging changes to pot into existing LANG messages.po

rem Notice that omitting to set charset e.g. "Content-Type: text/plain; charset=iso-8859-1\n" in the target po file might erase special charaters when running msgmerge 

pause
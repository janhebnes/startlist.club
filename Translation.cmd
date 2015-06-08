@echo off 
rem --------------------------
rem - REQUIRED - INSTALL NuGet packages\Gettext.Tools.0.19.4
rem -------------------------- 

rem 1) Generate inputfiles list used by xgettext

rem MODEL A (absolute path)
rem dir FlightJournal.Web\*.cs /S /B > translation-inputfiles.txt
rem dir FlightJournal.Web\*.cshtml /S /B >> translation-inputfiles.txt

rem MODEL B Specific folders - removing obj and the like (absolute path) 
rem dir FlightJournal.Web\Controllers\*.cs /S /B > translation-inputfiles.txt
rem dir FlightJournal.Web\Models\*.cs /S /B > translation-inputfiles.txt
rem dir FlightJournal.Web\Views\*.cs /S /B > translation-inputfiles.txt
rem dir FlightJournal.Web\Views\*.cshtml /S /B >> translation-inputfiles.txt

rem MODEL C Relative paths - but with obj folders and package folders (anyone fresh for a fix ?)
@echo on>translation-inputfiles.txt
@echo off 
setlocal EnableDelayedExpansion
for /L %%n in (1 1 500) do if "!__cd__:~%%n,1!" neq "" set /a "len=%%n+1"
setlocal DisableDelayedExpansion
for /r . %%g in (*.cs,*.cshtml) do (
  set "absPath=%%g"
  setlocal EnableDelayedExpansion
  set "relPath=!absPath:~%len%!"
  echo(!relPath! >> translation-inputfiles.txt
  endlocal
)

rem Create a new .pot from source, place it in the Language folder, and merge with the existing .po file
packages\Gettext.Tools.0.19.4\tools\xgettext.exe -k -k_ -kLocalizedDisplayName --msgid-bugs-address=jan.hebnes@gmail.com --package-name=startlist.club --from-code=UTF-8 -L C# -o FlightJournal.Web\Translations\messages.pot -f translation-inputfiles.txt 
packages\Gettext.Tools.0.19.4\tools\msgmerge.exe --backup=none -U FlightJournal.Web\Translations\en\LC_MESSAGES\messages.po FlightJournal.Web\Translations\messages.pot rem EN must always match pot file
packages\Gettext.Tools.0.19.4\tools\msgmerge.exe --backup=none --lang=da -U FlightJournal.Web\Translations\da\LC_MESSAGES\messages.po FlightJournal.Web\Translations\messages.pot  

rem begin new language by copying 
rem 		FlightJournal.Web\Translations\messages.pot 
rem 	to 	FlightJournal.Web\Translations\LANG\LC_MESSAGES\messages.po
rem 	and add line for msgmerge.exe that handles merging changes to pot into existing LANG messages.po

rem Notice that omitting to set charset e.g. "Content-Type: text/plain; charset=iso-8859-1\n" in the target po file might erase special charaters when running msgmerge 

pause
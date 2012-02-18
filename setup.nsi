!define PRODUCT_NAME "NZB-O-Matic+"
!define OUTPUT_NAME "NzbOMaticPlusSetup.exe"

!define INSTALLUTIL "$WINDIR\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe"

;--------------------------------
;Include Modern UI
	!include "MUI.nsh"
	!include "LogicLib.nsh"
	
;--------------------------------
;Compressor
	SetCompressor lzma

;--------------------------------
;General
  ;Name and file}
  
  Name "${PRODUCT_NAME}"
  OutFile "${OUTPUT_NAME}"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\${PRODUCT_NAME}"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\${PRODUCT_NAME}" ""
  
;--------------------------------
;Interface Settings
  !define MUI_ABORTWARNING
  
;--------------------------------
;Pages
  !define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES 
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
	!insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Callbacks
Function .onInit
	Call GetDotNet

FunctionEnd

Function .onInstSuccess
	
FunctionEnd

Function .onInstFailed
	
FunctionEnd

;--------------------------------
;Installer Sections

Section "Install Package" SecInstall
  CreateDirectory "$INSTDIR"
  SetOutPath "$INSTDIR"
  File "bin\Release\engine.dll"
  File "bin\Release\interface.dll"
  File "bin\Release\NZB-O-MaticPlus.exe"
  File "changelog.txt"
  File "readme.txt"
  

	CreateDirectory "$STARTMENU\Programs\NOMP"
	CreateShortcut "$STARTMENU\Programs\NOMP\NOMP.lnk" "$INSTDIR\NZB-O-MaticPlus.exe"

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  WriteRegStr HKLM "SOFTWARE\${PRODUCT_NAME}" "Directory" "$INSTDIR"
SectionEnd



;--------------------------------
;Uninstaller Section

Section "Uninstall"
  RMDir /r "$INSTDIR"
  DeleteRegKey HKCU "Software\${PRODUCT_NAME}"
SectionEnd

Function GetDotNet
	IfFileExists "${INSTALLUTIL}" NextStep
		MessageBox MB_ICONEXCLAMATION|MB_YESNO "${PRODUCT_NAME} requires that the .NET 2.0 Framework is installed which it appears you do not have installed.  If you want to install anyways click Yes." IDYES Skip
		abort
	Skip:
	NextStep:
FunctionEnd

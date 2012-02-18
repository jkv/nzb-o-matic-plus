
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3c.org/TR/1999/REC-html401-19991224/loose.dtd">
<HTML xmlns="http://www.w3.org/1999/xhtml">
<HEAD>
<link rel=stylesheet type="text/css" href="http://www.bunnyhug.net/nomp/styles-site.css">
</HEAD>
<BODY>

	<!-- <CONTAINER> -->
	<div id="container">
		<TABLE width="100%" CELLPADDING=0 CELLSPACING=0>
		<tr><td valign=top align=left>

			<!-- <HEADER> -->
			<div id="header">
				<table width=100%>
				<tr><td align=left>
					<div id="banner">
						<h1><a href="http://www.bunnyhug.net/nomp/" accesskey="1">NOMP - NZB-O-MATIC PLUS</a></h1>
						<h2>An extension from the <a href="http://nzb.wordtgek.nl/">NZB-O-MATIC</a> project by <i>over</i></h2>
					</div>
				</td></tr>
				</table>
			</div>
			<!-- </HEADER> -->

			<!-- <MENU> -->
			<UL id=navlist>
			</UL>
			<!-- </MENU> -->

		</td></tr><tr><td>
			<TABLE cellSpacing=0 cellPadding=0 width="100%">
			<TBODY>
			<TR>
			<TD id=center vAlign=top align=left>
				<!-- <CONTENT> -->
				<div class="content">
					<!-- <AD> -->
					<div id=ad>
						<center>
						<script type="text/javascript"><!--
						google_ad_client = "pub-3679344559703247";
						google_alternate_color = "FFFFFF";
						google_ad_width = 468;
						google_ad_height = 60;
						google_ad_format = "468x60_as";
						google_ad_channel ="2703929160";
						google_ad_type = "text_image";
						google_color_border = "FFFFFF";
						google_color_bg = "FFFFFF";
						google_color_link = "008000";
						google_color_url = "3366CC";
						google_color_text = "000000";
						//--></script>
						<script type="text/javascript"
						src="http://pagead2.googlesyndication.com/pagead/show_ads.js">
						</script>
						</center>
					</div>
					<!-- </AD> -->
<h3>NOMP</h3>

<TABLE class=test width="100%" CELLPADDING=0 CELLSPACING=0>
<tr><td valign=top align=left>

<!-- <desc> -->
<a name="description"></a>
<p>
<span class="header">Description</span><br>
<span class="description">
<b>NOMP requires the <a href="http://www.microsoft.com/downloads/details.aspx?FamilyID=0856EACB-4362-4B0D-8EDD-AAB15C5E04F5&displaylang=en">2.0 .NET Framework</a></b>
<BR><br>
NOMP (NZB-O-MATIC PLUS) is a extension from the <a href="http://nzb.wordtgek.nl/">NZB-O-MATIC</a> project started by Da_Teach and Azaril.
If you want to contact them check out the <a href="http://nzb.wordtgek.nl/">NZB-O-MATIC</a> page for their contact information.  So far all I
have done is add some small bug fixes and additional features.  Straight from the <a href="http://nzb.wordtgek.nl/">NZB-O-MATIC</a>
page here are some of the features:

<table width=80%><tr><td><i>
      <p>NZB-O-Matic is an easy to use, free, multi-server usenet download program based on NZB files. It requires the .NET framework v1.1 to run and probably contains a few bugs ;)</p>
      <p>Current features :</p>
      <ul>
        <li>Support multiple servers, it will first try to download from the first server, if the article cant be found it'll try the 2nd, and 3rd, and so on. There is no limit on servers, although each server-connection is a thread looking at that server's download queue, so processor power could come into play when you have a lot of connections.</li>
        <li>Multi-threaded downloading for multiple connections.</li>
        <li>Supports yEnc, UUEncode and Base-64 decoding. </li>
        <li>Imports NZB files that are downloaded from <a href="http://www.newzbin.com" target="_blank">www.newzbin.com</a>.</li>
      </ul>
</i>
</td></tr></table>
<BR>
NOMP is an open-source project.  If you would like to help with this project, report bugs or request features you can do so from the
<a href="http://developer.berlios.de/projects/nomp/">NOMP open-source development project page.</a>

</span></p>
<!-- </desc> -->


<!-- <changelog> -->
<a name="changelog"></a>
<p>
<span class="header">Change Log</span>
<p>
<span class="logheader">Version 0.53</span>
<ul class="changelog">
	
	
	<li>* Support newzbin2.es
</ul></p>

<p>
<span class="logheader">Version 0.52</span>
<ul class="changelog">
	
	
	<li>* Fixed minor delegate bug with status updates
	<li>* [Bug #5564] fixed the bug with article shown queued and never downloaded (modify FillQueues to add the articles beeing donloading)
	<li>* [Bug #4922] Fixed problem with pause or unpause who didn't effect immediately (rebuilding dl queue when pause, unpause or moving an aticle in the queue)
	<li>* Fixed a bug who let a file open when no space left on disk while writing (verify the space left on disk before opening file)(WMI should be replaced with DriveInfo when migration to .NET 2.0)
	<li>* Fixed a bug while decoding missformed yenc header (rewrite correctly the spaces in the line)
	<li>* Fixed connect retry : when connect is OK, the ConnectAttempts must be reseted
	<li>* fix divide by zero error
	<li>+ [Feature Request #1454] Added support for SSL servers 
	<li>+ Modify the directories : the cache directory and the config files are now in %APPDATA%\nomp (until this exist) and the default download directory is in "my documents". It's because we have not always the right to write in the pathtoexe directory (install made bye an administrator)
	<li>+ Modify the totalMB : only the articles to be downloaded (ie not the paused ) are treated in the count, so the remaining time is the real time until the download is ready
	<li>+ Modify the completedMB : only the segments to be downloaded are counted (ie not the paused), so the completed cant be greather than the total
	<li>+ Modify completion (same reasons as above)
	<li>+ Migrate to .NET 2.0
</ul></p>
<p>
<span class="logheader">Version 0.50</span>
<ul class="changelog">
	<li>* [Bug #4053] Fixed edit menu add/edit/delete to be disabled when connected
	<li>* [Bug #4072] Fixed the Disconnect on idle not being set properly
	<li>+ [Request #1105] Added option to delete prune from right click menu
	<li>+ [Request #1149] Added quick options for connect/disconnect
</ul></p>

<p>
<span class="logheader">Version 0.49</span>
<ul class="changelog">
	<li>+ Add option to pause/unpause selected files
	<li>+ Add option to pause all par files
	<li>+ Add option to decode all files in the queue
</ul></p>

<p>
<span class="logheader">Version 0.48</span>
<ul class="changelog">
	<li>* Fixed the file association so it will work when installed in a folder with a space
	<li>+ Added a button to set all incompletes back to queued
	<li>+ Added a feature to reconnect if user was disconnected from being idle and user imports a new nzb file
</ul></p>

<p>
<span class="logheader">Version 0.47</span>
<ul class="changelog">
	<li>* Fixed the '%z' so that it supports 'msgidlist_uid' files from newzbin as well
	<li>+ Added a '%y' option that is the same as '%z' but it replaces '_' with ' '
</ul></p>

<p>
<span class="logheader">Version 0.46</span>
<ul class="changelog">
	<li>* Fixed the monitor folder option to wait until the file has finished downloading.
</ul></p>

<p>
<span class="logheader">Version 0.45</span>
<ul class="changelog">
	<li>+ Added the option to monitor a folder for new NZB files.  In the
	preferences window you can select the folder which you want to monitor.
	Whenever a new nzb file is created in this folder it will automatically be
	imported into NOMP.
</ul></p>

<p>
<span class="logheader">Version 0.44</span>
<ul class="changelog">
	<li>* Fixed bug with importing a nzb file and then having the config files saved in the wrong folder
	<li>* Fixed bug with opening the window from the tray and having it set the window size to 0x0
	<li>* Fixed 'disconnect on idle' so that it saves it to the options file
	<li>+ If you import a nzb file while the form is minimized to the tray it won't bring the form to the front
	<li>+ Changed the import NZB functionality to allow importing multiple .nzb
		files at one time
	<li>+ Added a new download folder option '%z' which can be used for nzb files imported
		from newzbin (msgid_111111_Blah.nzb will become Blah).  If the nzb file
		doesn't follow the msgid_<number> format it will just create the folder
		the same way the '%i' does
	<li>+ Saves the window location now and whether or not the form is maximized
	<li>+ Changed the update check to point to my server instead
	<li>+ Added option to empty the cache
	<li>+ Added an attempt at a fix for when you import/delete entries while downloading and the current download stalls
</ul></p>

<!-- </changelog> -->

</td></tr></table>


				</div>
				<!-- </CONTENT> -->

			</td><td vAlign=top>

				<!-- <RIGHT> -->
				<div id="right">
					<!-- <SIDEBAR> -->
					<div class="sidebar">

					<h2>Downloads</h2>
					<ul>
					<li><a href="NzbOMaticPlusSetup-0.53.exe">Binary - Download (Version 0.53)</a></li>
					<li><a href="NzbOMaticPlusSetup.exe">Legacy .NET 1.1 version (Version 0.50)</a></li>
					</ul>

					<h2>Developers</h2>
					<ul>
					<li><a href="http://developer.berlios.de/projects/nomp/">Source</a>
					</ul>

					<H2>Information</H2>

					<ul>
					<!-- <header> -->
					<li>Original Release: February 10, 2005
					<li>Last Updated: February 10, 2012
					<li>Required: <a href="http://www.microsoft.com/downloads/details.aspx?FamilyID=0856EACB-4362-4B0D-8EDD-AAB15C5E04F5&displaylang=en">2.0 .NET Framework</a>
					</ul>

					<ul>
					<h2>Contact Information</h2>
					<li><a href="http://www.google.com/recaptcha/mailhide/d?k=01FgaKp0IRRqKxknpkUL6CSg==&amp;c=u6FZKxk_ny_f8Z8a3xdmqIXhX1OKPyBT4EcFLJkycdU=" title="Reveal this e-mail address">MailHide</a>
					</ul>

					</div>
					<!-- </SIDEBAR> -->
				</div>
				<!-- </RIGHT> -->
			</TD></TR></TBODY></TABLE>
		</td></tr></table>
	</div>
	<!-- </CONTAINER> -->

<script src="http://www.google-analytics.com/urchin.js" type="text/javascript">
</script>
<script type="text/javascript">
_uacct = "UA-368960-2";
urchinTracker();
</script>
</BODY>
</HTML>


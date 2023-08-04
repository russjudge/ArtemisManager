# Artemis Manager
Tool for managing modding and missions in Artemis SBS.  The scope of this application is that it is intended to run on all PCs that will run Artemis within a network.  One instance of the
application will act as a server while all the rest will be clients to that server.  This will be determined by settings set by the user within each instance of the application.

At least one, but up to all of these instances must be identified as a master.  The PC that contains the Artemis SBS server will always be a master.  All other instances will be slaves.  This has
no relationship to the client/server system.  Masters can submit commands that all instances will take, while slaves can only act on commands received.

So, basically, from a single master, all connected instances of the application can be instructed to install mods and missions, apply those mods and missions to the Artemis folder, start Artemis SBS,
unload those mods and missions to restore the Artemis Folder to vanilla state, and uninstall mods and missions, and finally process serveral tertiary commands.

Required disk space is around 3MB for the Artemis Manager application, plus the size of each installed mod and mission plus the size of the Artemis SBS application.

# Notes involving Steam:
Since I don't have the Steam version of Artemis, I'm not sure how well this will behave---> users will need a different Steam account for each pc.  Even though detecting
the Artemis install under Steam is encoded, detecting that install folder for Artemis SBS might be finicky (especially since I cannot test it), so you might have to manually browse to it.
Also, because Artemis Manager works by creating a copy of the Artemis SBS install folder under its own data location (generally
C:\Users\<username>\AppData\Roaming\Confederate In Blue Gaming\ArtemisManager\Activated), I'm not sure if Steam will like this--it's possible that the Steam DRM might prevent this
from running.

# Windows Firewall issues
If you have the Windows Firewall running, you WILL be prompted to allow Artemis Manager access on the network.  You must allow it for Artemis Manager to work on its peer-to-peer networking.
However, I've encountered issues with the Windows Firewall that may prevent proper function.  It seems that Microsoft isn't very good at creating a Firewall, at least in my opinion.
Artemis Manager's peer-to-peer network works by broadcasting a UDP message to all clients on the network subnet, on port 2012.  Any clients running Artemis Manager will receive this broadcast
and try to connect to the first client by TCP on port 2011 (which is configurable).  I THINK the issue the firewall is having is with the UDP broadcast.  
If you can't get the peer-to-peer network working, here are some troubleshooting tips you can try.  These are what I tried:

- Make sure your network is marked as "Private":

- Add an exception in Windows Firewall to allow ArtemisManager.exe through
- If all else fails, turn off Windows Firewall.

# Roadmap
There will be three phases of development for this application, and once complete, only bug-fixes will be performed, unless some enhancement within the scope of the application is determined.

To avoid scope creep and creating a monster, Artemis Manager will be limited to ONLY managing mods and missions for Artemis SBS. There will be no functionality added for generating Missions,
since the Mission Editor already.  DMX editing will not be implemented since there is already a tool for this.

Planned for release 0.5 (beta):
- UI for building a mod.  The first release for this will include an editor for vesselData.xml, as well as access to the non-localized settings in artemis.ini.
- Will review external tool Artemis Bridge Tools to determine if better functionality is needed for DMXCommands.xml.  If the functionality is needed, development
	will be planned, probably for version 0.7 (beta) or after version 1.0 when Artemis Manager leaves beta.

Planned for release 0.6 (beta):
- Full UI toolkit for pulling in files for building a mod (such as graphics and audio files).

Planned for release 1.0 (which should bring Artemis Manager out of beta):
- Linkage to website https://artemis.russjudge.com to allow submission of mods and missions to a central repository that Artemis Manager can query for offering mods and missions for
	direct download.


Stuff that will NOT be done:
-- Mission editor/scripting.  There is already an adaquate tool for this.
-- DMX editor, assuming my review of Artemis Bridge Tools shows this to be an adequate tool for DMX editing.
-- Graphics for Artemis SBS.  No modeling, no meshes, nothing of this.  This isn't where my expertise lies and would be nothing be frustration if I were to even try.


Once out of beta, examination of Artemis Cosmos will be performed.  If it is determined that Artemis Cosmos is similar enough to Artemis SBS, then Artemis Manager will be
modified to offer support for Artemis Cosmos as well--otherwise a new tool might be built to support Artemis Cosmos.  Based on preliminary information I've received, I think a new tool
will be likely as much of the support that Artemis Manager provides for Artemis SBS has been complete re-worked for Artemis Cosmos.

However, before work to support Artemis Cosmos is performed, examination of my application "The Big Red Button of Death" will be performed.  It has been reported that this
does not work anymore.  Coding for self-destruct is pretty straight-forward, so the plan is to create a new application that is cross-platform (Windows & Android) that will
provide self-destruct functionality.

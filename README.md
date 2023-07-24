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

Roadmap is subject to change.

Phase 1

-- Offer multiple versions of Artemis SBS updates.
	-- Store on artemis.russjudge.com.
	-- set unique Guid to each.
	-- Instructions: If installing a lower version, first uninstall Artemis SBS, then install the lowest base install you have (lower than the desired version to install), then install update.
	--    Installing 2.x requires first installing the 2.0 install.  Installing 1.x will need a full install version of 1.x that is lower than the desired version.

-- Upon release, document future plans, and request input for enhancements and for prioritizing, and for bug reports.  This first release version requires
	all of the above TODO items done first and will be noted as version 0.x and as a beta version.  Version 1.0 will be release upon completion of Phase 2, which will also
	note that an automated way of submitted mods is under development, and to submit mods to me via URL link in the forum post.


Phase 2
-- Add Engineering presets local edit and propagating to peers.
-- Add mechanism for editing artemis.ini file to restrict peers to specific consoles and ships and port (default 2010) (need to confirm this works).
	-- Since some mods include this file, the console restriction will need to be stored in the peer's settings, and applied to artemis.ini whenever a new mod is activated.
	-- The staging folder for mods and the vanilla artemis version will need to leave the artemis.ini file untouched.
-- Add mechanism for updating various settings in artemis.ini that might be useful (Touchscreen=1 or 0???).  Settings that might be useful for this will need tested.
-- Add mechanism to prevent the replacement of the DMXControl.xml file, since modification of this will be unique to each setting.
-- Add links to all useful tools for Artemis SBS (ship editor, Mission editor, DMX editor, etc.).  Include links to download and install these tools.
-- Establish a repository of mods and missons on a central internet website that the application can access and provide a means of automatic install of these mods and missions.  This
	repository will be maintained by the developer only.  The required information on Mods include: name, author, URL of the source (copy will be kept on website, however), version, date,
	required artemis version and any required mods.  missions require same data.

Phase 3
-- Establish an automated process for mod and mission creators to submit their creation for review and additon to the repository.
-- Add full mod creation functionality (for version 2.8):
	-- automatic noting of artemis verison being edited.
	-- Allow editing of ALL artemis.ini settings (excluding console restriction).  Warn of possible incompatibility of different versions.  
	-- Add editing of vesselData.xml file.
	-- add buttons for opening applications such as blender for editing the ship images.


Stuff that will NOT be done:
-- Mission editor/scripting.  There is already an adaquate tool for this.
-- DMX editor.  There appears to already be an adaquate tool for this, though I have not tested it.
-- Graphics for Artemis SBS.  No modeling, no meshes, nothing of this.  This isn't where my expertise lies and would be nothing be frustration if I were to even try.
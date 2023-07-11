# Artemis Manager
Tool for managing modding and missions in Artemis SBS.  The scope of this application is that it is intended to run on all PCs that will run Artemis within a network.  One instance of the
application will act as a server while all the rest will be clients to that server.  This will be determined by settings set by the user within each instance of the application.

At least one, but up to all of these instances must be identified as a master.  The PC that contains the Artemis SBS server will always be a master.  All other instances will be slaves.  This has
no relationship to the client/server system.  Masters can submit commands that all instances will take, while slaves can only act on commands received.

So, basically, from a single master, all connected instances of the application can be instructed to install mods and missions, apply those mods and missions to the Artemis folder, start Artemis SBS,
unload those mods and missions to restore the Artemis Folder to vanilla state, and uninstall mods and missions, and finally process serveral tertiary commands.

# Roadmap
There will be three phases of development for this application, and once complete, only bug-fixes will be performed, unless some enhancement within the scope of the application is determined.

To avoid scope creep and creating a monster, Artemis Manager will be limited to ONLY managing mods and missions for Artemis SBS.  It will NOT have anything to do with facilitating the creation
of mods and/or missions.

Roadmap is subject to change.

Phase 1
- Develop Network connectivity, including the processing of commands
- Manage the install and application of mods, both as a manual process and as a reaction to commands submitted over the network connectivity.

Current TODO:
-- Add retrieval of app settings from peers, including whether or not app is in start folder.
-- Add command to add app to start folder.
-- Add basic Update check and download and update apply.
-- Add local snapshotting of vanilla Artemis folder.  Include this process automatically at startup so long as there is no Artemis snapshot.  Include check for version change and creating a snapshot
	for each different version.  Make sure to warn that the vanilla Artemis MUST NOT have any mods applied, or there will be no way to return to vanilla artemis without full re-install of artemis.
-- Add process to restore to any available snapshot version of Artemis.
-- Add process to command peers to start Artemis automatically.
-- Add process to command peers to load a specific version snapshot of Artemis
-- Add process to manually install mods (i.e., not automatic download, but have a zip/compressed file).
	-- verify not already installed.  If already installed attempt to check for any version/date changes.
		-- if non-standard information required to identify version update, then generate standard data retention--base on newest file date in zip file.
	-- archive the original unmodified zip file.
	-- Create staging folder that contains the unpackaged mod and only that mod.
	-- Create a file that can be modified by user of details of this mod, that it is installed and the folder location.
	-- Details to retain include version number, date version was generated, name, author, required Artemis version.
-- Add process to command peers to install mod
	-- Peer must check to verify mod is already installed or not.  If it is already installed, include information to check if it needs updated.
		-- Check if the original zip is in the archive (since not deleted on uninstall) and that it is the correct version.
	-- If not installed or needs updated, transmit the zip (somehow) to the peer for processing.
	-- If required Artemis version is not installed on peer--transmit warning with instructions that the correct version of Artemis must be installed, and how to do it 
		(needs to make sure not to overlay existing version).
-- Add process to command peers to activate mod (or specific Artemis version)
	-- if peer does not have mod installed, peer requests the mod zip for install and provides update when install complete
	-- peer sends notice that mod is activated.
	-- if for specific version of Artemis and that version is not installed, peer sends alert with instructions to install.
	-- if the required version of Artemis for the mod is not installed, see previous.
	-- peer first activates required version of Artemis, then activates mod.
-- Add process to command peers to close Artemis process.
-- Add process to query current peer state: app settings, mods installed, mods activated, whether or not Artemis is running.
-- Add process to uninstall mods.
	-- If mod is active, restore to vanilla of matching version.
	-- delete uncompressed folder and update database.
	-- retain original zip file in archive.
-- In Setup, Develop process to optionally remove all staging and archived mods and vanilla artemis versions.
-- Create icon for main window and setup.
-- Create mechanism to generate a mod based on the currently active artemis.
-- Add button to open Windows Explorer to the active artemis folder that Artemis Manager uses to start Artemis (this will be different that the default install folder of Artemis).
-- Document possible issues with the Windows Firewall, and ways to fix Windows Firewall, including setting to being on Private network, adding rules, 
	and finally, when all else fails, disabling Windows Firewall.
-- Upon release, document future plans, and request input for enhancements and for prioritizing, and for bug reports.  This first release version requires
	all of the above TODO items done first and will be noted as version 0.x and as a beta version.  Version 1.0 will be release upon completion of Phase 2, which will also
	note that an automated way of submitted mods is under development, and to submit mods to me via URL link in the forum post.

Phase 2
-- Add mechanism for adding missions.  Include way to ensure mission is for correct version of Artemis (i.e., if the mission won't work for specific versions, this will need worked out).
	-- Keep missions independent of any version of artemis or from any mod, unless it was included with the mod.  Whenever a version of Artemis is activated, then ensure to apply all missions
		that work with that version of artemis automatically as part of the activation.
-- Add Master/Slave functionality for security control.  In other words, create a way that only one peer can be set to control all peers.  This would prevent any random peer from doing nasty stuff 
	to the other peers (particularly useful in a conference setup, or anywhere you might have random people using your setup).
	-- Thoughts on managing this:
		1. Controlled in Settings.  By default all are Master at install.  One Master can remotely set peers to be slave or secondary master.  A slave can become a master in the following situation:
			- All peers are slaves.  In this situation, all peers will auto switch to master, until a master connects to the network.  This will be a benefit for when the original master
			disappears permanently for any number of reasons.  Under this case, the setting won't change unless one peer makes another peer a slave--then the master peer will change it's setting to
			permanently store as master, while the slave peer will retain it's slave setting.  No password (see below) will be required under this condition because the password might not be known.
			- by entering a set password, which will be blank by default, but can be changed by the master.  This will allow the user to change which peer is the master if he is physically moving
			around.  This way he won't have to jump back to the original master peer to set a slave to a peer.
-- Document required disk space and how disk space will increase--so that users will be prepared on undersized disk spaced PCs.
-- Add Engineering presets local edit and propagating to peers.
-- Add mechanism for editing artemis.ini file to restrict peers to specific consoles and ships and port (default 2010) (need to confirm this works).
	-- Since some mods include this file, the console restriction will need to be stored in the peer's settings, and applied to artemis.ini whenever a new mod is activated.
	-- The staging folder for mods and the vanilla artemis version will need to leave the artemis.ini file untouched.
-- Add mechanism for updating various settings in artemis.ini that might be useful (Touchscreen=1 or 0???).  Settings that might be useful for this will need tested.
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
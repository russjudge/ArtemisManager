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
- Manage the install and application of mods and missions, both as a manual process and as a reaction to commands submitted over the network connectivity.

Phase 2
- Establish a repository of mods and missons on a central internet website that the application can access and provide a means of automatic install of these mods and missions.  This
repository will be maintained by the developer only.

Phase 3
- Establish an automated process for mod and mission creators to submit their creation for review and additon to the repository.


# Flow
1. Check for update (app.xaml onStartup)
2. Open Main.xaml window in ArtemisManagerUI.
3. Create TCP Listener
4. Broadcast on UDP to announce me.
5. Start UDP Listener for announcers.
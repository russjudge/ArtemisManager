using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ArtemisINI : INIFile
    {
        //Need attributes to handle what the default value is and whether or not to comment out the field in the ini file if the value is the default.

        //Need to keep comments with the setting.

        /*
         
; -------------------------------------------------------------------------------------
; STARTUP RESOLUTION SETTINGS (the following settings let you bypass the initial screen dialog by specifying the screen mode and resolution here)
; -------------------------------------------------------------------------------------
;gameWindowWidth=1024  (screen width  in pixels; your screen may not be able to show all resolutions, especially in Full Screen mode)
;gameWindowHeight=768 (screen height in pixels; your screen may not be able to show all resolutions, especially in Full Screen mode)
;gameWindowMode=2     ("Full Screen" = 1, "Windowed" = 2, "Full Screen Windowed" = 3)



; -------------------------------------------------------------------------------------
; CLIENT_SIDE SETTINGS (the following settings don't mean anything to the server)
; -------------------------------------------------------------------------------------

; sometimes you want this client to be "locked" into a specific console

; SETTING:  clientSide
; USE: forces this client to be associated with a specific ship
; ACCEPTABLE: 1 to 8, 1 = the artemis, the first ship on the ship choice list
;clientSide=1

; SETTING:  clientHelm, clientWeapon, clientEngineer, clientScience, clientComms
; USE: forces this client to be associated ONLY with one or more bridge stations
; ACCEPTABLE: 1 = yes, otherwise no
;clientMainScreen=1
;clientHelm=1
;clientWeapon=1
;clientEngineer=1
;clientScience=1
;clientComms=1        
;clientFighter=1
;clientData=1
;clientObserver=1
;clientCaptMap=1

         */

       public ArtemisINI() :base() { }

        public ArtemisINI(string file) : base()
        {
            LoadFile<ArtemisINI>(file);
        }

        public static ArtemisINI Merge(ArtemisINI localFile, ArtemisINI modFile)
        {
            ArtemisINI retVal = new();
            var properties = typeof(ArtemisINI).GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<LocalINISettingAttribute>();
                if (attr != null)
                {
                    property.SetValue(retVal, localFile);
                }
                else
                {
                    property.SetValue(retVal, modFile);
                }
            }
            return retVal;
        }
        public ArtemisINI Merge(ArtemisINI INIFile, bool ParmIsForLocal)
        {
            if (ParmIsForLocal)
            {
                return Merge(INIFile, this);
            }
            else
            {
                return Merge(this, INIFile);
            }
        }

        [INISetting(1, ClientServerType.Both)]
        public INISetting cameraPitch
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [INISetting(2, ClientServerType.Both)]
        public INISetting cameraDistance
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(3, ClientServerType.Both)]
        public INISetting networkPort
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        [INISetting(4, ClientServerType.Both)]
        public INISetting jumpTimeCoeff
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(5, ClientServerType.Both)]
        public INISetting useDebugConsoleWindow
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [INISetting(6, ClientServerType.Both)]
        public INISetting energyCostOfOneBeamShot
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [INISetting(7, ClientServerType.Both)]
        public INISetting gSoundOneShotVolume
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(8, ClientServerType.Both)]
        public INISetting musicObjectMasterVolume
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(9, ClientServerType.Both)]
        public INISetting commsObjectMasterVolume
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(10, ClientServerType.Both)]
        public INISetting gameWindowWidth
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(11, ClientServerType.Both)]
        public INISetting gameWindowHeight
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(12, ClientServerType.Both)]
        public INISetting gameWindowMode
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        public Size ScreenResolution
        {
            get
            {
                int height = 0;
                int width = 0;
                if (!gameWindowHeight.UsingDefault)
                {
                    height = gameWindowHeight.GetIntValue();
                }
                if (!gameWindowWidth.UsingDefault)
                {
                    width = gameWindowWidth.GetIntValue();
                }
                return new Size(width, height);
            }
            set
            {
                if (value.Width == 0)
                {
                    gameWindowWidth.UsingDefault = true;
                }
                else
                {
                    gameWindowWidth.SetValue(value.Width);
                }
                if (value.Height == 0)
                {
                    gameWindowHeight.UsingDefault = true;
                }
                else
                {
                    gameWindowHeight.SetValue(value.Height);
                }
                DoChanged();
            }
        }
        public bool CommentOutFixedScreenResolution
        {
            get
            {
                var winWidth = EnsureSettingExists(nameof(gameWindowWidth));
                var winHeight = EnsureSettingExists(nameof(gameWindowHeight));
                var windowMode = EnsureSettingExists(nameof(gameWindowMode));
                return winWidth.UsingDefault && winHeight.UsingDefault && windowMode.UsingDefault;
            }
            set
            {
                var winWidth = EnsureSettingExists(nameof(gameWindowWidth));
                var winHeight = EnsureSettingExists(nameof(gameWindowHeight));
                var windowMode = EnsureSettingExists(nameof(gameWindowMode));
                winWidth.UsingDefault = value;
                winHeight.UsingDefault = value;
                windowMode.UsingDefault = value;
                DoChanged();
            }
        }

        public VideoMode VideoMode
        {
            get
            {
                if (gameWindowMode.UsingDefault)
                {
                    return VideoMode.None;
                }
                else
                {
                    return (VideoMode)gameWindowMode.GetIntValue();
                }
            }
            set
            {
                if (value == VideoMode.None)
                {
                    gameWindowMode.UsingDefault = true;
                }
                else
                {
                    gameWindowMode.SetValue((int)value);
                }
                DoChanged();
            }
        }

        [LocalINISetting(13, ClientServerType.ClientOnly)]
        public INISetting clientSide
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        public Ship ClientShip
        {
            get
            {
                if (clientSide.UsingDefault)
                {
                    return Ship.None;
                }
                else
                {
                    return (Ship)clientSide.GetIntValue();
                }
            }

            set
            {
                if (value == Ship.None)
                {
                    clientSide.UsingDefault = true;
                }
                else
                {
                    clientSide.SetValue((int)value);
                }
                DoChanged();
            }
        }

        [LocalINISetting(14, ClientServerType.ClientOnly)]
        public INISetting clientMainScreen
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        [LocalINISetting(15, ClientServerType.ClientOnly)]
        public INISetting clientHelm
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        [LocalINISetting(16, ClientServerType.ClientOnly)]
        public INISetting clientWeapon
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(17, ClientServerType.ClientOnly)]
        public INISetting clientEngineer
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(18, ClientServerType.ClientOnly)]
        public INISetting clientScience
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(19, ClientServerType.ClientOnly)]
        public INISetting clientComms
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(20, ClientServerType.ClientOnly)]
        public INISetting clientFighter
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(21, ClientServerType.ClientOnly)]
        public INISetting clientData
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(22, ClientServerType.ClientOnly)]
        public INISetting clientObserver
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(23, ClientServerType.ClientOnly)]
        public INISetting clientCaptMap
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(24, ClientServerType.ClientOnly)]
        public INISetting useJoystick
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(25, ClientServerType.ClientOnly)]
        public INISetting touchScreen
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(26, ClientServerType.ClientOnly)]
        public INISetting forceAddress
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [INISetting(27, ClientServerType.ClientOnly)]
        public INISetting showVisTab
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(28, ClientServerType.ClientOnly)]
        public INISetting showScrnPopups
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(29, ClientServerType.ClientOnly)]
        public INISetting helmShipEditing
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(30, ClientServerType.ClientOnly)]
        public INISetting operatorLogoImage
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(31, ClientServerType.ClientOnly)]
        public INISetting operatorLogoImageX
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(33, ClientServerType.ClientOnly)]
        public INISetting operatorLogoImageY
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
       
        public Size OperatorLogoImageSize
        {
            get
            {
                int height = 0;
                int width = 0;
                if (!operatorLogoImageY.UsingDefault)
                {
                    height = operatorLogoImageY.GetIntValue();
                }
                if (!operatorLogoImageX.UsingDefault)
                {
                    width = operatorLogoImageX.GetIntValue();
                }
                return new Size(width, height);
            }
            set
            {
                if (value.Width == 0)
                {
                    operatorLogoImageX.UsingDefault = true;
                }
                else
                {
                    operatorLogoImageX.SetValue(value.Width);
                }
                if (value.Height == 0)
                {
                    operatorLogoImageY.UsingDefault = true;
                }
                else
                {
                    operatorLogoImageY.SetValue(value.Height);
                }
                DoChanged();
            }
        }

        [LocalINISetting(34, ClientServerType.ClientOnly)]
        public INISetting allowOptionButton
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        [LocalINISetting(35, ClientServerType.ClientOnly)]
        public INISetting allowVolumeControls
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        [INISetting(36, ClientServerType.ClientOnly)]
        public INISetting showWeaponArcToggle
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(37, ClientServerType.ClientOnly)]
        public INISetting tacticleViewType
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(38, ClientServerType.ClientOnly)]
        public INISetting damageEffectType
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        [LocalINISetting(39, ClientServerType.ClientOnly)]
        public INISetting damConTeamAutonomy
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        /*
        ; -------------------------------------------------------------------------------------
        ; SERVER_SIDE SETTINGS(the following settings don't mean anything to the clients)
        ; -------------------------------------------------------------------------------------
        */
        [LocalINISetting(40, ClientServerType.ServerOnly)]
        public INISetting ServerNetworkName
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        [INISetting(41, ClientServerType.ServerOnly)]
        public INISetting stationEnergy
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        //Thanks to ChatGPT for formatting below based on the artemis.ini file.
        // I got too lazy to cut-and-paste and manually editing.  ChatGPT
        // Did a good job.
        //
        // SETTING: playerShieldRechargeRate
        // USE: How fast player ship's shields recharge
        // ACCEPTABLE: 0.02 to 2.0
        [INISetting(42, ClientServerType.ServerOnly)]
        public INISetting playerShieldRechargeRate
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: enemyShieldRechargeRate
        // USE: How fast enemy ship's shields recharge
        // ACCEPTABLE: 0.02 to 2.0
        [INISetting(43, ClientServerType.ServerOnly)]
        public INISetting enemyShieldRechargeRate
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: stationSensorRange
        // USE: how many meters away can a station detect an enemy?
        // ACCEPTABLE: 5000 to 100000
        [INISetting(44, ClientServerType.ServerOnly)]
        public INISetting stationSensorRange
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: playerSensorRange
        // USE: how many meters away can a player detect an enemy?
        // ACCEPTABLE: 5000 to 100000
        [INISetting(45, ClientServerType.ServerOnly)]
        public INISetting playerSensorRange
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        // SETTING: torpedoTubeLoadingCoeff
        // USE: adjustment to base speed of torpedo loading over time
        // ACCEPTABLE: 0.2 to 3.0
        [INISetting(50, ClientServerType.ServerOnly)]
        public INISetting torpedoTubeLoadingCoeff
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyUseCoeff
        // USE: adjustment to total amount of energy the player ship uses over time
        // ACCEPTABLE: 0.2 to 3.0
        [INISetting(51, ClientServerType.ServerOnly)]
        public INISetting energyUseCoeff
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffBeams
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(52, ClientServerType.ServerOnly)]
        public INISetting energyCoeffBeams
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffTubes
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(53, ClientServerType.ServerOnly)]
        public INISetting energyCoeffTubes
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffTactical
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(54, ClientServerType.ServerOnly)]
        public INISetting energyCoeffTactical
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffManeuver
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(55, ClientServerType.ServerOnly)]
        public INISetting energyCoeffManeuver
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffImpulse
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(56, ClientServerType.ServerOnly)]
        public INISetting energyCoeffImpulse
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffWarp
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(57, ClientServerType.ServerOnly)]
        public INISetting energyCoeffWarp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffFrontShlds
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(58, ClientServerType.ServerOnly)]
        public INISetting energyCoeffFrontShlds
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffRearShlds
        // USE: coefficient of how much energy this system uses
        // ACCEPTABLE: 1.0 to 10.0
        [INISetting(59, ClientServerType.ServerOnly)]
        public INISetting energyCoeffRearShlds
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: energyCoeffJump
        // USE: coefficient of how much energy a jump uses (* distance)
        // ACCEPTABLE: 0.01 to 100.0
        [INISetting(60, ClientServerType.ServerOnly)]
        public INISetting energyCoeffJump
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }


        // SETTING: playerBeamDelay
        // USE: time delay between individual beams firing, so all beams don't fire at once
        // ACCEPTABLE: 0.0 to 1.0
        [INISetting(61, ClientServerType.ServerOnly)]
        public INISetting playerBeamDelay
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: overloadThreshold
        // USE: Systems heat up if the ship has more than this minimum of energy
        // ACCEPTABLE: 1000 to 6000
        [INISetting(62, ClientServerType.ServerOnly)]
        public INISetting overloadThreshold
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: overloadHeat
        // USE: Systems heat up this much if the ship has more than the overloadThreshold of energy
        // ACCEPTABLE: 0.0 to 5.0
        [INISetting(63, ClientServerType.ServerOnly)]
        public INISetting overloadHeat
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: torpEnergyValue
        // USE: energy you get for sacrificing a homing torp
        // ACCEPTABLE: 1-
        [INISetting(64, ClientServerType.ServerOnly)]
        public INISetting torpEnergyValue
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: torpEnergyCostValue
        // USE: energy you pay for creating a homing torp
        // ACCEPTABLE: 1-
        [INISetting(65, ClientServerType.ServerOnly)]
        public INISetting torpEnergyCostValue
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: beamDamageCap
        // USE: max damage an enemy beam can be, relative to the difficulty level
        // ACCEPTABLE: 3-10
        [INISetting(66, ClientServerType.ServerOnly)]
        public INISetting beamDamageCap
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: friendlyBeamDamageCap
        // USE: max damage an ai ship beam can be, relative to the difficulty level
        // ACCEPTABLE: 3-10
        [INISetting(67, ClientServerType.ServerOnly)]
        public INISetting friendlyBeamDamageCap
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: lowStartStationTorpHom
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(68, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpHom
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpHom
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(69, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpHom
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpHom
        // USE: damage caused by homing torpedoes
        // ACCEPTABLE: ?
        [INISetting(70, ClientServerType.ServerOnly)]
        public INISetting damageTorpHom
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpHom
        // USE: speed of homing torpedoes
        // ACCEPTABLE: ?
        [INISetting(71, ClientServerType.ServerOnly)]
        public INISetting speedTorpHom
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpHom
        // USE: turn rate of homing torpedoes
        // ACCEPTABLE: ?
        [INISetting(72, ClientServerType.ServerOnly)]
        public INISetting turnTorpHom
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpHom
        // USE: time taken to produce homing torpedoes
        // ACCEPTABLE: ?
        [INISetting(73, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpHom
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        // ----------------------- Nuke Torpedo Data --------------------------------
        // SETTING: lowStartStationTorpNuk
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(74, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpNuk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpNuk
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(75, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpNuk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpNuk
        // USE: damage caused by nuke torpedoes
        // ACCEPTABLE: ?
        [INISetting(76, ClientServerType.ServerOnly)]
        public INISetting damageTorpNuk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpNuk
        // USE: speed of nuke torpedoes
        // ACCEPTABLE: ?
        [INISetting(77, ClientServerType.ServerOnly)]
        public INISetting speedTorpNuk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpNuk
        // USE: turn rate of nuke torpedoes
        // ACCEPTABLE: ?
        [INISetting(78, ClientServerType.ServerOnly)]
        public INISetting turnTorpNuk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpNuk
        // USE: time taken to produce nuke torpedoes
        // ACCEPTABLE: ?
        [INISetting(79, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpNuk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        // ----------------------- Mine Torpedo Data --------------------------------
        // SETTING: lowStartStationTorpMin
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(80, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpMin
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpMin
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(81, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpMin
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpMin
        // USE: damage caused by mine torpedoes
        // ACCEPTABLE: ?
        [INISetting(82, ClientServerType.ServerOnly)]
        public INISetting damageTorpMin
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpMin
        // USE: speed of mine torpedoes
        // ACCEPTABLE: ?
        [INISetting(83, ClientServerType.ServerOnly)]
        public INISetting speedTorpMin
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpMin
        // USE: turn rate of mine torpedoes
        // ACCEPTABLE: ?
        [INISetting(84, ClientServerType.ServerOnly)]
        public INISetting turnTorpMin
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpMin
        // USE: time taken to produce mine torpedoes
        // ACCEPTABLE: ?
        [INISetting(85, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpMin
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // ----------------------- ECM Torpedo Data --------------------------------
        // SETTING: lowStartStationTorpEmp
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(86, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpEmp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpEmp
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(87, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpEmp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpEmp
        // USE: damage caused by ECM torpedoes
        // ACCEPTABLE: ?
        [INISetting(88, ClientServerType.ServerOnly)]
        public INISetting damageTorpEmp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpEmp
        // USE: speed of ECM torpedoes
        // ACCEPTABLE: ?
        [INISetting(89, ClientServerType.ServerOnly)]
        public INISetting speedTorpEmp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpEmp
        // USE: turn rate of ECM torpedoes
        // ACCEPTABLE: ?
        [INISetting(90, ClientServerType.ServerOnly)]
        public INISetting turnTorpEmp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpEmp
        // USE: time taken to produce ECM torpedoes
        // ACCEPTABLE: ?
        [INISetting(91, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpEmp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // ----------------------- Phase Shock Torpedo Data --------------------------------
        // SETTING: lowStartStationTorpShk
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(92, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpShk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpShk
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(93, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpShk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpShk
        // USE: damage caused by Phase Shock torpedoes
        // ACCEPTABLE: ?
        [INISetting(94, ClientServerType.ServerOnly)]
        public INISetting damageTorpShk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpShk
        // USE: speed of Phase Shock torpedoes
        // ACCEPTABLE: ?
        [INISetting(95, ClientServerType.ServerOnly)]
        public INISetting speedTorpShk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpShk
        // USE: turn rate of Phase Shock torpedoes
        // ACCEPTABLE: ?
        [INISetting(96, ClientServerType.ServerOnly)]
        public INISetting turnTorpShk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpShk
        // USE: time taken to produce Phase Shock torpedoes
        // ACCEPTABLE: ?
        [INISetting(97, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpShk
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // ----------------------- Buoy Torpedo Data --------------------------------
        // SETTING: lowStartStationTorpBoy
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(98, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpBoy
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpBoy
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(99, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpBoy
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpBea
        // USE: damage caused by Buoy torpedoes
        // ACCEPTABLE: ?
        [INISetting(100, ClientServerType.ServerOnly)]
        public INISetting damageTorpBea
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpBea
        // USE: speed of Buoy torpedoes
        // ACCEPTABLE: ?
        [INISetting(101, ClientServerType.ServerOnly)]
        public INISetting speedTorpBea
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpBea
        // USE: turn rate of Buoy torpedoes
        // ACCEPTABLE: ?
        [INISetting(102, ClientServerType.ServerOnly)]
        public INISetting turnTorpBea
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpBea
        // USE: time taken to produce Buoy torpedoes
        // ACCEPTABLE: ?
        [INISetting(103, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpBea
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        // ----------------------- Probe Torpedo Data --------------------------------
        // SETTING: lowStartStationTorpPro
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(104, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpPro
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpPro
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(105, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpPro
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpPro
        // USE: damage caused by Probe torpedoes
        // ACCEPTABLE: ?
        [INISetting(106, ClientServerType.ServerOnly)]
        public INISetting damageTorpPro
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpPro
        // USE: speed of Probe torpedoes
        // ACCEPTABLE: ?
        [INISetting(107, ClientServerType.ServerOnly)]
        public INISetting speedTorpPro
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpPro
        // USE: turn rate of Probe torpedoes
        // ACCEPTABLE: ?
        [INISetting(108, ClientServerType.ServerOnly)]
        public INISetting turnTorpPro
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpPro
        // USE: time taken to produce Probe torpedoes
        // ACCEPTABLE: ?
        [INISetting(109, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpPro
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // ----------------------- Tag Torpedo Data --------------------------------
        // SETTING: lowStartStationTorpTag
        // USE: least possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(110, ClientServerType.ServerOnly)]
        public INISetting lowStartStationTorpTag
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: highStartStationTorpTag
        // USE: most possible number of torps at each station at the start of the game
        // ACCEPTABLE: 0 to ?
        [INISetting(111, ClientServerType.ServerOnly)]
        public INISetting highStartStationTorpTag
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // Replaced editable torpedo values
        // SETTING: damageTorpTag
        // USE: damage caused by Tag torpedoes
        // ACCEPTABLE: ?
        [INISetting(112, ClientServerType.ServerOnly)]
        public INISetting damageTorpTag
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: speedTorpTag
        // USE: speed of Tag torpedoes
        // ACCEPTABLE: ?
        [INISetting(113, ClientServerType.ServerOnly)]
        public INISetting speedTorpTag
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: turnTorpTag
        // USE: turn rate of Tag torpedoes
        // ACCEPTABLE: ?
        [INISetting(114, ClientServerType.ServerOnly)]
        public INISetting turnTorpTag
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: minutesToProduceTorpTag
        // USE: time taken to produce Tag torpedoes
        // ACCEPTABLE: ?
        [INISetting(115, ClientServerType.ServerOnly)]
        public INISetting minutesToProduceTorpTag
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // ----------------------- Drone Data --------------------------------
        // SETTING: droneSpeed
        // USE: speed of a torgoth drone
        // ACCEPTABLE: positive value, around 2-3
        [INISetting(120, ClientServerType.ServerOnly)]
        public INISetting droneSpeed
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: droneDamageTorgoth
        // USE: damage caused by a torgoth drone. Internally, this value is multiplied by the port power of the torgoth launcher.
        // ACCEPTABLE: positive value, around 1
        [INISetting(121, ClientServerType.ServerOnly)]
        public INISetting droneDamageTorgoth
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: droneDamageSkaraan
        // USE: damage caused by a Skaraan drone.
        // ACCEPTABLE: positive value, around 1
        [INISetting(122, ClientServerType.ServerOnly)]
        public INISetting droneDamageSkaraan
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: droneDamageSegmentCount
        // USE: against player ships, this is the number of times a drone will cause internal damage
        // ACCEPTABLE: positive value, 1-10
        [INISetting(123, ClientServerType.ServerOnly)]
        public INISetting droneDamageSegmentCount
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
        // ----------------------- Sound FX Cues --------------------------------

        // SETTING: cueShieldsDown
        // USE: played on the main screen when shields are lowered by buttons on the helm and weapon stations
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(200, ClientServerType.Both)]
        public INISetting cueShieldsDown
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueShieldsDownVol
        // USE: volume of the cueShieldsDown sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(201, ClientServerType.Both)]
        public INISetting cueShieldsDownVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueShieldsUp
        // USE: played on the main screen when shields are raised by buttons on the helm and weapon stations
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(202, ClientServerType.Both)]
        public INISetting cueShieldsUp
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueShieldsUpVol
        // USE: volume of the cueShieldsUp sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(203, ClientServerType.Both)]
        public INISetting cueShieldsUpVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueAIShipExplode
        // USE: played on the main screen when an AIShip dies within 3000m of the Artemis
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(204, ClientServerType.Both)]
        public INISetting cueAIShipExplode
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueAIShipExplodeVol
        // USE: volume of the cueAIShipExplode sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(205, ClientServerType.Both)]
        public INISetting cueAIShipExplodeVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueRedAlert
        // USE: played on the main screen when the comms officer toggles the "red alert" state to on
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(206, ClientServerType.Both)]
        public INISetting cueRedAlert
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueRedAlertVol
        // USE: volume of the cueRedAlert sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(207, ClientServerType.Both)]
        public INISetting cueRedAlertVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueInternalDamageAlarm
        // USE: played on the main screen when an enemy damages an internal part of the player ship
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(208, ClientServerType.Both)]
        public INISetting cueInternalDamageAlarm
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueInternalDamageAlarmVol
        // USE: volume of the cueInternalDamageAlarm sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(209, ClientServerType.Both)]
        public INISetting cueInternalDamageAlarmVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueHullHit
        // USE: played on the main screen when an enemy damages an internal part of the player ship
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(210, ClientServerType.Both)]
        public INISetting cueHullHit
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueHullHitVol
        // USE: volume of the cueHullHit sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(211, ClientServerType.Both)]
        public INISetting cueHullHitVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueShieldHit
        // USE: played on the main screen when an enemy damages a shield of the player ship
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(212, ClientServerType.Both)]
        public INISetting cueShieldHit
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueShieldHitVol
        // USE: volume of the cueShieldHit sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(213, ClientServerType.Both)]
        public INISetting cueShieldHitVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cuePlayerBeam
        // USE: played on the main screen when the player ship fires a beam
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(214, ClientServerType.Both)]
        public INISetting cuePlayerBeam
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cuePlayerBeamVol
        // USE: volume of the cuePlayerBeam sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(215, ClientServerType.Both)]
        public INISetting cuePlayerBeamVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cuePlayerTorpedo
        // USE: played on the main screen when the player ship fires a torpedo of any type
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(216, ClientServerType.Both)]
        public INISetting cuePlayerTorpedo
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cuePlayerTorpedoVol
        // USE: volume of the cuePlayerTorpedo sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(217, ClientServerType.Both)]
        public INISetting cuePlayerTorpedoVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueFighterTorpedoVol
        // USE: volume of the cuePlayerTorpedo sound for fighter torpedoes
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(218, ClientServerType.Both)]
        public INISetting cueFighterTorpedoVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueStationDock
        // USE: played on the main screen when the player ship docks with a station
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(219, ClientServerType.Both)]
        public INISetting cueStationDock
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueStationDockVol
        // USE: volume of the cueStationDock sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(220, ClientServerType.Both)]
        public INISetting cueStationDockVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueEngineLoop
        // USE: looping sound.  The main screen plays two copies of this simultaneously, and pitches one according to the speed of the ship
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(221, ClientServerType.Both)]
        public INISetting cueEngineLoop
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueEngineLoopVol
        // USE: volume of the cueEngineLoop sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(222, ClientServerType.Both)]
        public INISetting cueEngineLoopVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueWarpFail
        // USE: played on the main screen when the warp jump fails
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(223, ClientServerType.Both)]
        public INISetting cueWarpFail
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueWarpFailVol
        // USE: volume of the cueWarpFail sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(224, ClientServerType.Both)]
        public INISetting cueWarpFailVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueWarpTravel
        // USE: played on the main screen when the warp jump succeeds
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(225, ClientServerType.Both)]
        public INISetting cueWarpTravel
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueWarpTravelVol
        // USE: volume of the cueWarpTravel sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(226, ClientServerType.Both)]
        public INISetting cueWarpTravelVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueJumpWarmup
        // USE: played on the main screen when the warp jump starts
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(227, ClientServerType.Both)]
        public INISetting cueJumpWarmup
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueJumpWarmupVol
        // USE: volume of the cueJumpWarmup sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(228, ClientServerType.Both)]
        public INISetting cueJumpWarmupVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // ----------------------- cueUI Sounds --------------------------------

        // SETTING: cueUI00
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(229, ClientServerType.Both)]
        public INISetting cueUI00
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI00Vol
        // USE: volume of the cueUI00 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(230, ClientServerType.Both)]
        public INISetting cueUI00Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI01
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(231, ClientServerType.Both)]
        public INISetting cueUI01
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI01Vol
        // USE: volume of the cueUI01 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(232, ClientServerType.Both)]
        public INISetting cueUI01Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI02
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(233, ClientServerType.Both)]
        public INISetting cueUI02
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI02Vol
        // USE: volume of the cueUI02 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(234, ClientServerType.Both)]
        public INISetting cueUI02Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI03
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(235, ClientServerType.Both)]
        public INISetting cueUI03
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI03Vol
        // USE: volume of the cueUI03 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(236, ClientServerType.Both)]
        public INISetting cueUI03Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI04
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(237, ClientServerType.Both)]
        public INISetting cueUI04
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI04Vol
        // USE: volume of the cueUI04 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(238, ClientServerType.Both)]
        public INISetting cueUI04Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI05
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(239, ClientServerType.Both)]
        public INISetting cueUI05
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI05Vol
        // USE: volume of the cueUI05 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(240, ClientServerType.Both)]
        public INISetting cueUI05Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI06
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(241, ClientServerType.Both)]
        public INISetting cueUI06
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI06Vol
        // USE: volume of the cueUI06 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(242, ClientServerType.Both)]
        public INISetting cueUI06Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI07
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(243, ClientServerType.Both)]
        public INISetting cueUI07
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI07Vol
        // USE: volume of the cueUI07 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(244, ClientServerType.Both)]
        public INISetting cueUI07Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI08
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(245, ClientServerType.Both)]
        public INISetting cueUI08
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI08Vol
        // USE: volume of the cueUI08 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(246, ClientServerType.Both)]
        public INISetting cueUI08Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI09
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(247, ClientServerType.Both)]
        public INISetting cueUI09
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI09Vol
        // USE: volume of the cueUI09 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(248, ClientServerType.Both)]
        public INISetting cueUI09Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI10
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(249, ClientServerType.Both)]
        public INISetting cueUI10
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI10Vol
        // USE: volume of the cueUI10 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(250, ClientServerType.Both)]
        public INISetting cueUI10Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI11
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(251, ClientServerType.Both)]
        public INISetting cueUI11
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI11Vol
        // USE: volume of the cueUI11 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(252, ClientServerType.Both)]
        public INISetting cueUI11Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI12
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(253, ClientServerType.Both)]
        public INISetting cueUI12
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI12Vol
        // USE: volume of the cueUI12 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(254, ClientServerType.Both)]
        public INISetting cueUI12Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI13
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(255, ClientServerType.Both)]
        public INISetting cueUI13
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI13Vol
        // USE: volume of the cueUI13 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(256, ClientServerType.Both)]
        public INISetting cueUI13Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI14
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(257, ClientServerType.Both)]
        public INISetting cueUI14
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI14Vol
        // USE: volume of the cueUI14 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(258, ClientServerType.Both)]
        public INISetting cueUI14Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI15
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(259, ClientServerType.Both)]
        public INISetting cueUI15
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI15Vol
        // USE: volume of the cueUI15 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(260, ClientServerType.Both)]
        public INISetting cueUI15Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI16
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(261, ClientServerType.Both)]
        public INISetting cueUI16
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI16Vol
        // USE: volume of the cueUI16 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(262, ClientServerType.Both)]
        public INISetting cueUI16Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI17
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(263, ClientServerType.Both)]
        public INISetting cueUI17
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI17Vol
        // USE: volume of the cueUI17 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(264, ClientServerType.Both)]
        public INISetting cueUI17Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI18
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(265, ClientServerType.Both)]
        public INISetting cueUI18
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI18Vol
        // USE: volume of the cueUI18 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(266, ClientServerType.Both)]
        public INISetting cueUI18Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI19
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(267, ClientServerType.Both)]
        public INISetting cueUI19
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI19Vol
        // USE: volume of the cueUI19 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(268, ClientServerType.Both)]
        public INISetting cueUI19Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI20
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(269, ClientServerType.Both)]
        public INISetting cueUI20
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI20Vol
        // USE: volume of the cueUI20 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(270, ClientServerType.Both)]
        public INISetting cueUI20Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI21
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(271, ClientServerType.Both)]
        public INISetting cueUI21
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI21Vol
        // USE: volume of the cueUI21 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(272, ClientServerType.Both)]
        public INISetting cueUI21Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI22
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(273, ClientServerType.Both)]
        public INISetting cueUI22
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI22Vol
        // USE: volume of the cueUI22 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(274, ClientServerType.Both)]
        public INISetting cueUI22Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI23
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(275, ClientServerType.Both)]
        public INISetting cueUI23
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI23Vol
        // USE: volume of the cueUI23 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(276, ClientServerType.Both)]
        public INISetting cueUI23Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI24
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(277, ClientServerType.Both)]
        public INISetting cueUI24
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI24Vol
        // USE: volume of the cueUI24 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(278, ClientServerType.Both)]
        public INISetting cueUI24Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI25
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(279, ClientServerType.Both)]
        public INISetting cueUI25
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI25Vol
        // USE: volume of the cueUI25 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(280, ClientServerType.Both)]
        public INISetting cueUI25Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI26
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(281, ClientServerType.Both)]
        public INISetting cueUI26
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueUI26Vol
        // USE: volume of the cueUI26 sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(282, ClientServerType.Both)]
        public INISetting cueUI26Vol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueOverheat
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(283, ClientServerType.Both)]
        public INISetting cueOverheat
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: cueOverheatVol
        // USE: volume of the cueOverheat sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(284, ClientServerType.Both)]
        public INISetting cueOverheatVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: fighterBeamShot
        // USE: played on consoles when buttons are pressed
        // ACCEPTABLE: a file name to a WAV file
        [INISetting(285, ClientServerType.Both)]
        public INISetting fighterBeamShot
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }

        // SETTING: fighterBeamShotVol
        // USE: volume of the fighterBeamShot sound
        // ACCEPTABLE: a floating-point value from 0.0 to 1.0
        [INISetting(286, ClientServerType.Both)]
        public INISetting fighterBeamShotVol
        {
            get
            {
                return EnsureSettingExists();
            }
            set
            {
                SetSetting(value);
            }
        }
    }
}


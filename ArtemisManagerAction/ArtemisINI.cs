using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ArtemisINI : INotifyPropertyChanged
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
        public event PropertyChangedEventHandler? PropertyChanged;
        void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            
        }
        //for release 0.3:
        int networkPort = 2010;
        int gameWindowWidth = 1024;
        int gameWindowHeight = 768;
        int gameWindowMode = 2;
        Ship clientSide= Ship.None;
        bool clientMainScreen = true;
        bool clientHelm = true;
        bool clientWeapon = true;
        bool clientEngineer = true;
        bool clientScience = true;
        bool clientComms = true;
        bool clientFighter = true;
        bool clientData = true;
        bool clientObserver = true;
        bool clientCaptMap = true;
        bool useJoystick = true;
        bool touchScreen = false;
        string forceAddress = IPAddress.Any.ToString();
        string operatorLogoImage = string.Empty; // "dat/operator-logo.png";
        bool allowOptionButton = true;
        //Remaining fields will not be controlled here, but cannot be lost.

        void LoadFile(string path)
        {

        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction.Tests
{
    [TestClass()]
    public class ModItemTests
    {
        [TestMethod()]
        public void GetModItemTest()
        {
            string data = "{\r\n  \"key\": \"\",\r\n  \"modIdentifier\": \"00000000-0000-0000-0000-000000000000\",\r\n  \"name\": \"Artemis SBS\",\r\n  \"description\": \"Base Artemis\",\r\n  \"author\": \"Thom Robertson\",\r\n  \"version\": \"2.8.0\",\r\n  \"releaseDate\": \"0001-01-01T00:00:00\",\r\n  \"requiredArtemisVersion\": \"2.8.0\",\r\n  \"compatibleArtemisVersions\": [],\r\n  \"isArtemisBase\": true,\r\n  \"installFolder\": \"C:\\\\Users\\\\russj\\\\AppData\\\\Roaming\\\\Confederate In Blue Gaming\\\\Artemis Manager\\\\InstalledMods\\\\ArtemisV2.8.0\",\r\n  \"localIdentifier\": \"01c37f58-c0bb-4d52-b2af-90a55b908c2a\",\r\n  \"isActive\": false,\r\n  \"stackOrder\": 0,\r\n  \"packageFile\": \"\",\r\n  \"saveFile\": \"\"\r\n}";
            var mod = ModItem.GetModItem(data);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AMCommunicator.Messages.Tests
{
    [TestClass()]
    public class NetworkMessageHeaderTests
    {


        [TestMethod()]
        public void GetItemTest()
        {
            /*
            string JSON = "{\r\n  \"Action\": 5,\r\n  \"Force\": true,\r\n  \"MessageVersion\": 0,\r\n  \"Source\": \"10.10.20.115\",\r\n  \"Unspecified\": \"\"\r\n}";
            
            var retVal = JsonSerializer.Deserialize<PCActionMessage>(JSON);
            */
            //PCActionMessage msg = new PCActionMessage(PCActions.SendClientInformation, true);
            //if (msg != null)
            //{
            //    var options = new JsonSerializerOptions
            //    {

            //        WriteIndented = true
            //    };

            //    var JSON = JsonSerializer.Serialize<PCActionMessage>(msg, options);
            //    options = new JsonSerializerOptions
            //    {
            //        Converters ={
            //            new JsonStringEnumConverter(null, true)
            //       }
            //    };

            //    var item = JsonSerializer.Deserialize<PCActionMessage>(JSON, options);
            //}


            //NetworkMessageHeader header = new NetworkMessageHeader(msg);
            //var result = header.GetItem<PCActionMessage>();


            var options = new JsonSerializerOptions
            {
                Converters ={
                        new JsonStringEnumConverter(null, true)
                   }
            };


            var testItem = new PCActionTest(PCActionsTest.SendClientInformation, true);

            var JSON = JsonSerializer.Serialize<PCActionTest>(testItem, options);

            var resultItem = JsonSerializer.Deserialize<PCActionTest>(JSON, options);
            if (resultItem == null)
            {

            }

        }
    }

    public class PCActionTest
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public PCActionTest() : base()
        {

        }
        public PCActionTest(PCActionsTest action, bool force) : base()
        {
            Force = force;
            Action = action;
        }

        public PCActionsTest Action { get; set; }
        public bool Force { get; set; }

    }
    public enum PCActionsTest : short
    {
        CloseApp,
        ShutdownPC,
        RestartPC,
        CheckForUpdate,
        DisconnectThisConnection,
        SendClientInformation,
    }
}
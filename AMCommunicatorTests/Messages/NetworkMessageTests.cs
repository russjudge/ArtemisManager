using Microsoft.VisualStudio.TestTools.UnitTesting;
using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages.Tests
{
    [TestClass()]
    public class NetworkMessageTests
    {
        [TestMethod()]
        public void GetJSONTest()
        {
            //HandshakeMessage msg = new Messages.HandshakeMessage();
            //string json = msg.GetJSON();

            //AlertMessage msg2 = new Messages.AlertMessage(AlertItems.MessageVersionMismatch, "test");
            //json = msg2.GetJSON();

            //var msg3 = new ClientInfoMessage(true, true, new string[0], new string[0], true, true, true);
            //json= msg3.GetJSON();
        }
    }
}
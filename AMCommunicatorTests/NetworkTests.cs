using Microsoft.VisualStudio.TestTools.UnitTesting;
using AMCommunicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Tests
{
    [TestClass()]
    public class NetworkTests
    {
        [TestMethod()]
        public void GetMyIPTest()
        {
            //Assert.Fail();

            Network.GetMyIP();
        }

        [TestMethod()]
        public void GetSubnetMaskTest()
        {
            var ip = Network.GetMyIP();
            if (ip !=null)
            {
                var mask = Network.GetSubnetMask(ip);
            }
            
        }
    }
}